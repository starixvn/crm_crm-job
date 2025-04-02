namespace CRM.UnitTests.Services.AutoFeedback;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using CRM.Criteria.RefundSettledTicket;
using CRM.Criteria.VoidingBetID;
using CRM.Models.ChatBotResulAmendment;
using CRM.Models.RefundSettledTicket;
using CRM.Services.AutoFeedback;
using CRM.Services.Common;
using NSubstitute;
using Xunit;

public class RefundSettledTicketServiceTests
{
    private IBaseService baseService;

    [Fact]
    public async Task GetMessageRefundSettledTicketAsync_WhenCalled_ReturnExpected()
    {
        // Arrange
        var fixture = new Fixture();
        var expected = fixture.Create<ValueTuple<IEnumerable<MessageRefundSettledTicket>, IEnumerable<MessageVoidSettled>>>();
        var service = GetService();
        baseService.FetchMultipleAsync<MessageRefundSettledTicket, MessageVoidSettled>(Arg.Any<GetMessageRefundSettledTicketCriteria>()).Returns(expected);

        // Act
        var actual = await service.GetMessageRefundSettledTicketAsync(1, 2);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task ImportRefundSettledTicketAsync_WhenCalled_ReturnExpected()
    {
        // Arrange
        var fixture = new Fixture();
        var messageContent = fixture.Create<IEnumerable<MessageRefundSettledTicket>>();
        var message = fixture.Create<IEnumerable<MessageVoidSettled>>();
        var commandResult = fixture.Create<CRM.Models.Common.CommandResult>();
        var command = fixture.Create<ImportVoidBetIDCommand>();
        var expected = new ValueTuple<CRM.Models.Common.CommandResult, ImportVoidBetIDCommand>(commandResult, command);
        var service = GetService();
        baseService.ExecuteNonQueryAsync(Arg.Any<ImportRefundSettledTicketCommand>()).Returns(commandResult);

        // Act
        var actual = await service.ImportRefundSettledTicketAsync(messageContent, message, "UAT");

        // Assert
        Assert.Equal(expected.Item1, actual.Item1);
    }

    #region Helpers

    private RefundSettledTicketService GetService()
    {
        baseService = Substitute.For<IBaseService>();
        return new RefundSettledTicketService(baseService);
    }

    #endregion Helpers
}