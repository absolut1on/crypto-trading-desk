using Gateway.API.Auth;
using Gateway.API.Data;
using Gateway.API.Hubs;
using Gateway.API.Sagas;
using Gateway.API.Services;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:Connection"] ?? "localhost:6379"));
builder.Services.AddSingleton<RedisStreamService>();

builder.Services.AddSupabaseAuth(builder.Configuration);

builder.Services.AddSignalR();

builder.Services.AddHangfire(config =>
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseDefaultTypeSerializer()
        .UseMemoryStorage());
builder.Services.AddHangfireServer();

builder.Services.AddScoped<TradeSaga>();
builder.Services.AddHostedService<PriceStreamConsumer>();

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy => policy
        .WithOrigins(builder.Configuration["Client:Url"] ?? "http://localhost:3000")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    if (!db.CryptoPairs.Any())
    {
        var pairs = new[] { "BTCUSDT", "ETHUSDT", "BNBUSDT", "SOLUSDT", "XRPUSDT", "ADAUSDT", "DOGEUSDT", "DOTUSDT" };
        foreach (var symbol in pairs)
        {
            db.CryptoPairs.Add(new Gateway.API.Entities.CryptoPair
            {
                Id = Guid.NewGuid(),
                Symbol = symbol,
                BaseCurrency = symbol.Replace("USDT", ""),
                QuoteCurrency = "USDT",
                IsActive = true
            });
        }
        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseHangfireDashboard();

app.MapControllers();
app.MapHub<PriceHub>("/hubs/prices");
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "gateway" })).AllowAnonymous();

app.Run();
