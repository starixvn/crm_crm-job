namespace CRM.UnitTests.Services.AutoChatBotMessage;

using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using CRM.Criteria.ChatBotRebateRetention;
using CRM.Models.ChatBotRebateRetention;
using CRM.Services.AutoChatBotMessage;
using CRM.Services.Common;
using NSubstitute;
using Xunit;

public class ChatBotRebateRetentionServiceTests
{
    private IBaseService baseService;

    [Fact]
    public async Task GetRebateRetentionInfoAsync_WhenCalled_ReturnExpectedMessages()
    {
        // Arrange
        var fixture = new Fixture();
        var criteria = new GetRebateRetentionInfoCriteria
        {
            LastLogSID = "1",
            BatchSize = 50,
            NewLogSID = "1"
        };
        var expected = fixture.Create<IEnumerable<ChatBotRebateRetentionInfo>>();
        var result = (expected, criteria.NewLogSID);
        var service = GetService();
        baseService.GetDataAsync<ChatBotRebateRetentionInfo>(Arg.Do<GetRebateRetentionInfoCriteria>(x => criteria = x)).Returns(expected);

        // Act
        var actual = await service.GetRebateRetentionInfoAsync("1", 50);

        // Assert
        Assert.Equal(result.expected, actual.Item1);
    }

    [Fact]
    public async Task ImportRebateRetentionCommand_WhenCalled_ReturnExpectedCommandResult()
    {
        // Arrange
        var fixture = new Fixture();
        var expected = fixture.Create<CRM.Models.Common.CommandResult>();
        var rrMessages = fixture.Create<IEnumerable<ChatBotRebateRetentionInfo>>();
        var service = GetService();
        baseService.ExecuteNonQueryAsync(Arg.Do<ImportRebateRetentionCommand>(x => _ = x)).Returns(expected);

        // Act
        var actual = await service.ImportRebateRetentionCommand(rrMessages, "1", "dev");

        // Assert
        Assert.Equal(expected, actual.Item1);
    }

    #region Helpers

    private ChatBotRebateRetentionService GetService()
    {
        baseService = Substitute.For<IBaseService>();
        return new ChatBotRebateRetentionService(baseService);
    }

    #endregion Helpers
}