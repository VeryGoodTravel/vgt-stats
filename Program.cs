using NLog;
using NLog.Extensions.Logging;
using RabbitMQ.Client.Exceptions;
using vgt_saga_orders.Orchestrator;
using vgt_saga_orders.OrderService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
try
{
    builder.Configuration.AddJsonFile("appsettings.json", false, true).AddEnvironmentVariables().Build();
}
catch (InvalidDataException e)
{
    Console.WriteLine(e);
    Environment.Exit(0);
}


try
{
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    var options = new NLogProviderOptions
    {
        AutoShutdown = true
    };
    options.Configure(builder.Configuration.GetSection("NLog"));
    builder.Logging.AddNLog(options);
}
catch (InvalidDataException e)
{
    Console.WriteLine(e);
    Environment.Exit(0);
}

var app = builder.Build();

var logger = LogManager.GetCurrentClassLogger();

logger.Info("Hello word");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

Orchestrator? orchestrator = null;
OrderService? orderService = null;

AppDomain.CurrentDomain.ProcessExit += CurrentDomainProcessExit;
AppDomain.CurrentDomain.DomainUnload += CurrentDomainProcessExit;
AppDomain.CurrentDomain.UnhandledException += CurrentDomainProcessExit;

try
{
    orchestrator = new Orchestrator(app.Configuration);
    orderService = new OrderService(app.Configuration);
}
catch (BrokerUnreachableException)
{
    CurrentDomainProcessExit(null, EventArgs.Empty);
}
catch (ArgumentException)
{
    CurrentDomainProcessExit(null, EventArgs.Empty);
}

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.Run();

return;

void CurrentDomainProcessExit(object? sender, EventArgs e)
{
    orchestrator?.Dispose();
    orderService?.Dispose();
    AwaitAppStop().Wait();
    Environment.Exit(0);
}

// TODO: verify
async Task AwaitAppStop()
{
    //await app.StopAsync();
    //await app.DisposeAsync();
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}