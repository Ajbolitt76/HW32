using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using HW32.Grpc;

Console.WriteLine("Choose the task: [1,2]");
var res = int.Parse(Console.ReadLine() ?? string.Empty);
var builder = Host.CreateApplicationBuilder();

switch (res)
{
    case 1:
        builder.Services.AddHostedService<Worker>();
        break;
    case 2:
        builder.Services.AddHostedService<Worker2>();
        break;
}


var app = builder.Build();
app.Run();

public class Worker2 : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private string _token = "";
    private readonly GrpcChannel _channel;
    private readonly GrpcChannel _secureChannel;

    public Worker2(IConfiguration configuration, ILogger<Worker> logger)
    {
        _logger = logger;
        var url = configuration.GetConnectionString("WeatherService")!;
        ArgumentException.ThrowIfNullOrEmpty(url);
        
        _channel = GrpcChannel.ForAddress(url);
        var creds = CallCredentials.FromInterceptor(
            (context, metadata) =>
            {
                metadata.Add("Authorization", $"Bearer {_token}");
                return Task.CompletedTask;
            });
        _secureChannel = GrpcChannel.ForAddress(url, new GrpcChannelOptions
        {
            Credentials = ChannelCredentials.Create(ChannelCredentials.Insecure, creds),
            UnsafeUseInsecureChannelCallCredentials = true
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Authorize();
        var client = new HW32.Grpc.ProtectedDemo.ProtectedDemoClient(
            _secureChannel);
        var res = await client.PrivateAsync(new Empty());
        _logger.LogInformation($"Token created for: {res.Name} Secret: {res.Key}");
    }

    private async Task Authorize()
    {
        var client = new HW32.Grpc.ProtectedDemo.ProtectedDemoClient(
            _channel);
        var token = await client.PublicAsync(new Public.Types.Request()
        {
            Name = Environment.UserName ?? Random.Shared.NextInt64(0, Int64.MaxValue).ToString()
        });

        _token = token.Token;
        _logger.LogInformation("Authorizing...");
    }
}

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