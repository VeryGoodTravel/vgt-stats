using Microsoft.Extensions.Configuration;
using NLog;
using NLog.Extensions.Logging;

IConfigurationRoot? config = null;
try
{
    config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", false, true)
        .AddEnvironmentVariables()
        .Build();
}
catch (InvalidDataException e)
{
    Console.WriteLine(e);
    Environment.Exit(0);
}

try
{
    LogManager.Configuration = new NLogLoggingConfiguration(config.GetSection("NLog"));
}
catch (InvalidDataException e)
{
    Console.WriteLine(e);
    Environment.Exit(0);
}

LogManager.AutoShutdown = true;

var logger = LogManager.GetCurrentClassLogger();

logger.Info("Hello word");