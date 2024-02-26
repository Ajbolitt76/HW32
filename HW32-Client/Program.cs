using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;

var builder = Host.CreateApplicationBuilder();

builder.Services.AddHostedService<Worker>();

var app = builder.Build();
app.Run();

public partial class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly string _url;

    public Worker(IConfiguration configuration, ILogger<Worker> logger)
    {
        _logger = logger;
        _url = configuration.GetConnectionString("WeatherService")!;
        ArgumentException.ThrowIfNullOrEmpty(_url);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var client = new HW32.Grpc.Weather.WeatherClient(
            GrpcChannel.ForAddress(_url));

        var stream = client.GetWeather(new Empty())
            .ResponseStream
            .ReadAllAsync(cancellationToken: stoppingToken);
        
        await foreach (var entry in stream)
        {
            LogInfo(TimeOnly.FromDateTime(DateTime.Now), entry.Time.ToDateTime(), entry.Temperature);
        }
    }

    [LoggerMessage(
        eventId: 1234,
        level: LogLevel.Information,
        message: "{receivedAt:T} weather at {dataDate} = {temperature}C"
    )]
    private partial void LogInfo(TimeOnly receivedAt, DateTime dataDate, double temperature);
}