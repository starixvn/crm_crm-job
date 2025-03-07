namespace CRM.BackgroundJob;

using Fanex.Logging;
using Fanex.Logging.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

public class ErrorHandler(RequestDelegate next, ILogger logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var logMessage = new LogMessage(ex.Message)
                .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
                .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(ex))
                .ToString();
            await logger.ErrorAsync(logMessage);
        }
    }
}