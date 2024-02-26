using System.Runtime.CompilerServices;
using HW32.Model;

namespace HW32.Services;

public class WeatherGetter
{
    private readonly HttpClient _httpClient = new();
    public async IAsyncEnumerable<WeatherEntryModel> GetWeather(TimeSpan pause, [EnumeratorCancellation] CancellationToken token)
    {
        var end = DateTime.Now.ToString("yyyy-MM-dd");
        var start = DateTime.Now.Subtract(TimeSpan.FromDays(60)).ToString("yyyy-MM-dd");
        var data = await _httpClient.GetAsync(
            $"https://api.open-meteo.com/v1/forecast?latitude=52.52&longitude=13.41&hourly=temperature_2m&start_date={start}&end_date={end}",
            token);
        var hourly = (await data.Content.ReadFromJsonAsync<WeatherUpdate>(cancellationToken: token))?.Hourly;
        
        if(hourly is null)
            yield break;

        foreach (var entry in hourly.Time
                     .Zip(hourly.Temperature, (time, d) => new WeatherEntryModel(time, d))
                     .Where((_, i) => i % 2 == 0))
        {
            token.ThrowIfCancellationRequested();
            yield return entry;
            await Task.Delay(pause, token);
        }
    } 
}