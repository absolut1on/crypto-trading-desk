using Rates.API.Services;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:Connection"] ?? "localhost:6379"));

builder.Services.AddHostedService<BinanceWsConsumer>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "rates" }));

app.Run();
