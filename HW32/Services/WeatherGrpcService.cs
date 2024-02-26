using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using HW32.Grpc;

namespace HW32.Services;

public class WeatherGrpcService : Grpc.Weather.WeatherBase
{
    private readonly WeatherGetter _weatherGetter;

    public WeatherGrpcService(WeatherGetter weatherGetter)
    {
        _weatherGetter = weatherGetter;
    }

    public override async Task GetWeather(
        Empty request,
        IServerStreamWriter<WeatherEntry> responseStream,
        ServerCallContext context)
    {
        await foreach (var entry in _weatherGetter.GetWeather(TimeSpan.FromSeconds(1), context.CancellationToken))
        {
            await responseStream.WriteAsync(new WeatherEntry()
            {
                Time = Timestamp.FromDateTime(DateTime.SpecifyKind(entry.Time, DateTimeKind.Utc)),
                Temperature = entry.Temperature,
            });
        }
    }
}