using CRM.BackgroundJob;
using CRM.BackgroundJob.Extensions;
using Fanex.BotService.Client.AspNetCore;
using Fanex.Caching.Redis;
using Fanex.Data.SqlServer;
using Fanex.Logging;
using Fanex.Logging.Sentry;
using Fanex.RemotingRef;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog.Web;
using ONEbook.UM.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);

ConfigureWebHost(builder.WebHost);
ConfigureServices(builder.Services, builder.Configuration);
ConfigureLogging(builder.Configuration);

var app = builder.Build();

ConfigureMiddleware(app);

app.Run();

void ConfigureWebHost(IWebHostBuilder webHostBuilder)
{
    webHostBuilder
        .UseUrls("http://*:5000")
        .UseNLog();
}

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddControllers();
    services.AddFanexSql(configuration);
    services.AddOneBookUmForService(configuration);

    services.AddDataProtection()
        .SetApplicationName(configuration["Fanex.Caching.Redis:ApplicationName"])
        .PersistKeysToStackExchangeRedis(ConnectionMultiplexer.Connect(configuration["Fanex.Caching.Redis:ConnectionString"]),
            "CRM.BackgroundJob:DataProtection:Keys");

    services.AddRedisCache(configuration.GetSection("Fanex.Caching.Redis"));
    services.AddConfiguration(configuration);
    services.RegisterBotClient(configuration);
    services.AddDependencyInjection();
    services.AddWorkers();
}

void ConfigureLogging(IConfiguration configuration)
{
    LogManager
        .SetDefaultLogCategory(configuration["Fanex.Logging:LogCategory"])
        .Use(new List<ILogEngine>
        {
            new SentryLogging(new SentryEngineOptions
            {
                Dsn = configuration["Fanex.Logging:SentryEngineOptions:Dsn"]
            })
        });

    RefDataTypeContainer
        .ConnectTo(new Uri(configuration["RemotingRefService:Fanex.RemotingRef.Service.Url"]))
        .FromApplication("CRM.BackgroundJob");
}

void ConfigureMiddleware(WebApplication app)
{
    app.UseMiddleware<ErrorHandler>();
    app.UseHsts();
    app.UseStaticFiles();
    app.UseRouting();
    app.MapControllerRoute(
        "default",
        "{controller=Home}/{action=Index}/{id?}");
}