using CRM.BackgroundJob;
using CRM.BackgroundJob.DependencyInjection;
using Fanex.BotService.Client.AspNetCore;
using Fanex.Caching.Redis;
using Fanex.Data.SqlServer;
using Fanex.Logging.Sentry.RavenClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Web;
using ONEbook.UM.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();
builder.Host.UseNLog();

var services = builder.Services;
var configuration = builder.Configuration;
configuration.AddRemotingRef();

services.AddHealthChecks();
services.AddFanexSql(configuration, false, "FanexConnectionStrings");
services.AddFanexLogSentry(configuration);
services.AddOneBookUmForService(configuration);
services.AddRedisCache(configuration.GetSection("Fanex.Caching.Redis"));
services.AddConfiguration(configuration);
services.RegisterBotClient(configuration);
services.AddDependencyInjection();
services.AddWorkers();

var app = builder.Build();

app.UseMiddleware<ErrorHandler>();
app.MapHealthChecks("/");
app.MapGet("/", () => "OK");

app.MapGet("/sentry", async () =>
{
    var logService = app.Services.GetRequiredService<Fanex.Logging.ILogger>();
    await logService.ErrorAsync("Test Sentry");

    return "Test sentry!";
});

await app.RunAsync();