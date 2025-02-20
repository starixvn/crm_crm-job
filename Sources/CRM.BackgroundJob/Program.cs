using CRM.BackgroundJob;
using Fanex.BotService.Client.AspNetCore;
using Fanex.Caching.Redis;
using Fanex.Data.SqlServer;
using Fanex.Logging.Sentry;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NLog.Web;
using ONEbook.UM.DependencyInjection;
using CRM.BackgroundJob.DependencyInjection;
using NLog;

var builder = WebApplication.CreateBuilder(args);
NLog.LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();
builder.Host.UseNLog();

var services = builder.Services;
var configuration = builder.Configuration;

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

await app.RunAsync();
