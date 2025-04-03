namespace CRM.UnitTests.Services.Common;

using Fanex.Data.Repository;
using Fanex.Logging.Extensions;
using Newtonsoft.Json;
using NSubstitute;
using System;
using System.Threading.Tasks;
using CRM.Services.Common;
using Xunit;

public class CrmLogServiceTests
{
    private Fanex.Logging.ILogger logger;

    [Fact]
    public async Task ErrorAsync_WithException_WillCallsFanexLogError()
    {
        // Arrange
        var exception = new Exception("This is message");
        var service = GetService();

        // Act
        await service.ErrorAsync(exception);

        // Assert
        await logger.Received().ErrorAsync(Arg.Is<string>(x => x.Contains(exception.Message)));
    }

    [Fact]
    public async Task ErrorAsync_Command_WillCallsFanexLogErrorWithExpectedMessage()
    {
        // Arrange
        var command = new Models.Common.CommandResult { Code = 1, Message = "This is error message" };
        var criteria = Substitute.For<ICriteria>();
        var logMessage = new LogMessage($"{command.Code}: {command.Message}")
            .AddMessage(JsonConvert.SerializeObject(criteria, Formatting.Indented))
            .ToString();
        var service = GetService();

        // Act
        await service.ErrorAsync(command, criteria);

        // Assert
        await logger.Received().ErrorAsync(logMessage);
    }

    private CrmLogService GetService()
    {
        logger = Substitute.For<Fanex.Logging.ILogger>();
        return new CrmLogService(logger);
    }
}