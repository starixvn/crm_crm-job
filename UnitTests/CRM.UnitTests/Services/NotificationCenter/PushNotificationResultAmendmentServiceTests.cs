namespace CRM.UnitTests.Services.NotificationCenter;

using AutoFixture;
using CRM.Criteria.PushResultAmendment;
using CRM.Models.MessageNotificationCenter;
using CRM.Services.Common;
using CRM.Services.NotificationCenter;
using NSubstitute;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class PushNotificationResultAmendmentServiceTests
{
    private IBaseService baseService;

    [Fact]
    public async Task GetPrepareNotificationAsync_WhenCalled_ReturnExpected()
    {
        // Arrange
        var fixture = new Fixture();
        var expected = fixture.Create<IEnumerable<MncMessageTarget>>();
        var service = GetService();
        baseService.GetDataAsync<MncMessageTarget>(Arg.Any<GetPrepareNotiResultAmendmentCriteria>()).Returns(expected);

        // Act
        var actual = await service.GetPrepareNotificationAsync("dev");

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetMessageResultAmendmentAsync_WhenCalled_ReturnExpected()
    {
        // Arrange
        var fixture = new Fixture();
        var command = fixture.Create<GetMessageResultAmendmentCommand>();
        var service = GetService();

        // Act
        await service.GetMessageResultAmendmentAsync(command);

        // Assert
        await baseService.Received(1).ExecuteNonQueryAsync(command);
    }

    [Fact]
    public async Task EditPushNotificationResultAmendmentAsync_WhenCalled_ReturnExpected()
    {
        // Arrange
        var fixture = new Fixture();
        var expected = fixture.Create<CRM.Models.Common.CommandResult>();
        var criteria = fixture.Create<EditNotiResultAmendmentCommand>();
        var service = GetService();
        baseService.ExecuteNonQueryAsync(Arg.Do<EditNotiResultAmendmentCommand>(x => criteria = x)).Returns(expected);

        // Act
        var actual = await service.EditPushNotificationResultAmendmentAsync(1, 2, "dev", "updType");

        // Assert
        Assert.Equal(criteria.IsTest, actual.isTest);
        Assert.Equal(criteria.ApproveMode, actual.approveMode);
    }

    [Fact]
    public async Task EditPushMessageResultAmendmentAsync_WhenCalled_ReturnExpected()
    {
        // Arrange
        var fixture = new Fixture();
        var expected = fixture.Create<AdminMessageSite>();
        var service = GetService();
        baseService.GetDataEntityAsync<AdminMessageSite>(Arg.Any<EditMessageResultAmendmentCriteria>()).Returns(expected);

        // Act
        var actual = await service.EditPushMessageResultAmendmentAsync(1, "updType", true, 1);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task InsertCompleteResultAmendmentAsync_WhenCalled_ReturnExpected()
    {
        // Arrange
        var service = GetService();

        // Act
        await service.InsertCompleteResultAmendmentAsync("dev");

        // Assert
        await baseService.Received(1).ExecuteNonQueryAsync(Arg.Any<InsertCompleteResultAmendmentCommand>());
    }

    [Fact]
    public async Task InsertAdminMessageSiteAsync_WhenCalled_ReturnExpected()
    {
        // Arrange
        var fixture = new Fixture();
        var adminMessage = fixture.Create<AdminMessageSite>();
        var service = GetService();

        // Act
        await service.InsertAdminMessageSiteAsync(adminMessage);

        // Assert
        await baseService.Received(1).ExecuteNonQueryAsync(Arg.Any<InsertInfoAdminSiteCommand>());
    }

    #region Helpers

    private PushNotificationResultAmendmentService GetService()
    {
        baseService = Substitute.For<IBaseService>();
        return new PushNotificationResultAmendmentService(baseService);
    }

    #endregion Helpers
}