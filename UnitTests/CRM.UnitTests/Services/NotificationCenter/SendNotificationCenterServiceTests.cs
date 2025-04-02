namespace CRM.UnitTests.Services.NotificationCenter;

using AutoFixture;
using CRM.Criteria.SendNotificationCenter;
using CRM.Models.MessageNotificationCenter;
using CRM.Services.Common;
using CRM.Services.NotificationCenter;
using NSubstitute;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class SendNotificationCenterServiceTests
{
    private IBaseService baseService;

    [Fact]
    public async Task InsertNotificationAlertAsync_WhenCalled_ReturnExpected()
    {
        // Arrange
        var fixture = new Fixture();
        var expected = fixture.Create<CRM.Models.Common.CommandResult>();
        var service = GetService();
        baseService.ExecuteNonQueryAsync(Arg.Any<InsertNotificationAlertCommand>()).Returns(expected);

        // Act
        var actual = await service.InsertNotificationAlertAsync("dev");

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetNotificationAlertAsync_WhenCalled_ReturnExpected()
    {
        // Arrange
        var fixture = new Fixture();
        var expected = fixture.Create<IEnumerable<MessageBotInfo>>();
        var service = GetService();
        baseService.GetDataAsync<MessageBotInfo>(Arg.Any<GetNotificationAlertCriteria>()).Returns(expected);

        // Act
        var actual = await service.GetNotificationAlertAsync("dev");

        // Assert
        Assert.Equal(expected, actual);
    }

    #region Helpers

    private SendNotificationCenterService GetService()
    {
        baseService = Substitute.For<IBaseService>();
        return new SendNotificationCenterService(baseService);
    }

    #endregion Helpers
}