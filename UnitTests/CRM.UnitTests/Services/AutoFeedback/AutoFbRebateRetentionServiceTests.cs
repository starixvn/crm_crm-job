namespace CRM.UnitTests.Services.AutoFeedback;

using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using CRM.Criteria.AutoFbRebateRetention;
using CRM.Criteria.ChatBotRebateRetention;
using CRM.Models.ChatBotRebateRetention;
using CRM.Services.AutoFeedback;
using CRM.Services.Common;
using NSubstitute;
using Xunit;

public class AutoFbRebateRetentionServiceTests
{
    private IBaseService baseService;

    [Fact]
    public async Task GetRebateRetentionInfoAsync_WhenCalled_ReturnExpected()
    {
        // Arrange
        var fixture = new Fixture();
        var rrInfos = fixture.Create<IEnumerable<ChatBotRebateRetentionInfo>>();
        var criteria = new GetRebateRetentionInfoCriteria { BatchSize = 50 };
        var expected = (rrInfos, criteria.NewLogSID);
        var service = GetService();
        baseService.GetDataAsync<ChatBotRebateRetentionInfo>(Arg.Do<GetRebateRetentionInfoCriteria>(x => criteria = x)).Returns(rrInfos);

        // Act
        var actual = await service.GetRebateRetentionInfoAsync("1", 50);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task ImportRebateRetentionCommand_WhenCalled_ReturnExpected()
    {
        // Arrange
        var fixture = new Fixture();
        var rrMessages = fixture.Create<IEnumerable<ChatBotRebateRetentionInfo>>();
        var commandResult = fixture.Create<CRM.Models.Common.CommandResult>();
        var service = GetService();
        baseService.ExecuteNonQueryAsync(Arg.Do<ImportAutoFbRebateRetentionCommand>(x => _ = x)).Returns(commandResult);

        // Act
        var actual = await service.ImportRebateRetentionCommand(rrMessages, "1", "dev");

        // Assert
        Assert.Equal(commandResult, actual.Item1);
    }

    #region Helpers

    private AutoFbRebateRetentionService GetService()
    {
        baseService = Substitute.For<IBaseService>();
        return new AutoFbRebateRetentionService(baseService);
    }

    #endregion Helpers
}