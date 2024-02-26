using System.Text.Json.Serialization;

namespace HW32.Model;

public record HourlyWeather(
    DateTime[] Time,
    [property: JsonPropertyName("temperature_2m")]double[] Temperature);

public record WeatherUpdate(
    HourlyWeather Hourly);
    
public record WeatherEntryModel(
    DateTime Time, double Temperature);