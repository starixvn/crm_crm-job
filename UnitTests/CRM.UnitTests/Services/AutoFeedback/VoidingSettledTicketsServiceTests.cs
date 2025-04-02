namespace CRM.UnitTests.Services.AutoFeedback;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using CRM.Criteria.ChatBot_ResultAmendment;
using CRM.Criteria.VoidingSettledTickets;
using CRM.Extensions.Xml;
using CRM.Models.AmendmentNotiFB;
using CRM.Models.ChatBotResulAmendment;
using CRM.Models.VoidingSettledTickets;
using CRM.Services.AutoFeedback;
using CRM.Services.Common;
using NSubstitute;
using Xunit;

public class VoidingSettledTicketsServiceTests
{
    private IBaseService baseService;
    private ICrmLogService logService;

    [Fact]
    public async Task GetMessageVoidSettledAsync_WhenCalled_ReturnExpectedMessages()
    {
        // Arrange
        var fixture = new Fixture();
        var expected = fixture.Create<IEnumerable<MessageVoidSettled>>();
        var service = GetVoidingSettledService();
        baseService.GetDataAsync<MessageVoidSettled>(Arg.Any<GetMessagesVoidingSettledCriteria>()).Returns(expected);

        // Act
        var actual = await service.GetMessageVoidSettledAsync();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetAutoVoidingMessageAsync_WhenCalled_ReturnExpectedVoidingMessage()
    {
        // Arrange
        var fixture = new Fixture();
        var voidingSettledTicketSetting = fixture.Create<AmendmentNotiFbSetting>();
        var expected = fixture.Create<IEnumerable<MessageVoidSettled>>();
        var service = GetVoidingSettledService();
        baseService.GetDataAsync<MessageVoidSettled>(Arg.Any<GetAutoVoidingMessageCriteria>()).Returns(expected);

        // Act
        var actual = await service.GetAutoVoidingMessageAsync(voidingSettledTicketSetting, "LIVE");

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetAutoVoidingTicketAsync_WhenCalled_ReturnExpectedTickets()
    {
        // Arrange
        var fixture = new Fixture();
        var voidingSettledTicketSetting = fixture.Create<AmendmentNotiFbSetting>();
        var voidSettles = fixture.Create<List<MessageVoidSettled>>();
        var expected = fixture.Create<ValueTuple<IEnumerable<VoidingTicketEventInfo>, IEnumerable<MsgTarget>>>();
        var service = GetVoidingSettledService();
        baseService.FetchMultipleAsync<VoidingTicketEventInfo, MsgTarget>(Arg.Any<GetAutoVoidingTicketCriteria>()).Returns(expected);

        // Act
        var actual = await service.GetAutoVoidingTicketAsync(voidingSettledTicketSetting, voidSettles, "LIVE");

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetFailVoidingMessageAsync_WhenCalled_ReturnExpectedFailMessages()
    {
        // Arrange
        var fixture = new Fixture();
        var expected = fixture.Create<IEnumerable<MessageVoidSettled>>();
        var service = GetVoidingSettledService();
        baseService.GetDataAsync<MessageVoidSettled>(Arg.Any<GetFailVoidingMessageCriteria>()).Returns(expected);

        // Act
        var actual = await service.GetFailVoidingMessageAsync();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetVoidingAutoFeedbackInfoAsync_WhenCalled_ReturnExpectedFeedbackInfo()
    {
        // Arrange
        var fixture = new Fixture();
        var expected = fixture.Create<ValueTuple<IEnumerable<VoidingAutoFeedbackInfo>, IEnumerable<AutoVoidingEventDetails>>>();
        var service = GetVoidingSettledService();
        baseService.FetchMultipleAsync<VoidingAutoFeedbackInfo, AutoVoidingEventDetails>(Arg.Any<GetVoidingAutoFeedbackInfoCriteria>()).Returns(expected);

        // Act
        var actual = await service.GetVoidingAutoFeedbackInfoAsync(10);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task VoidingMessageGetSettledTicketAsync_WhenCalled_ReturnExpectedMessages()
    {
        // Arrange
        var fixture = new Fixture();
        var expected = fixture.Create<IEnumerable<MessageVoidSettled>>();
        var messageVoidSettles = fixture.Create<IEnumerable<MessageVoidSettled>>();
        var service = GetVoidingSettledService();
        baseService.GetDataAsync<MessageVoidSettled>(Arg.Any<VoidingGetSettledTicketMessageCriteria>()).Returns(expected);

        // Act
        var actual = await service.VoidingMessageGetSettledTicketAsync(messageVoidSettles);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetVoidingMessageCheckAsync_WhenCalled_ReturnExpectedMessages()
    {
        // Arrange
        var fixture = new Fixture();
        var expected = fixture.Create<IEnumerable<MessageVoidSettled>>();
        var messageVoidSettles = fixture.Create<IEnumerable<MessageVoidSettled>>();
        var service = GetVoidingSettledService();
        baseService.GetDataAsync<MessageVoidSettled>(Arg.Any<GetChatbotVoidingMessageCheck>()).Returns(expected);

        // Act
        var actual = await service.GetVoidingMessageCheckAsync(messageVoidSettles, "LIVE");

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task ImportMessageVoidSettledAsync_WhenCalled_ReturnExpectedCommandResult()
    {
        // Arrange
        var fixture = new Fixture();
        var messages = fixture.Create<IEnumerable<MessageVoidSettled>>();
        var expected = fixture.Create<CRM.Models.Common.CommandResult>();
        var xml = messages.ToXML("root").ToString();
        var criteria = new ImportMessageVoidSettledCommand();
        var service = GetVoidingSettledService();
        baseService.ExecuteNonQueryAsync(Arg.Do<ImportMessageVoidSettledCommand>(x => criteria = x)).Returns(expected);

        // Act
        var actual = await service.ImportMessageVoidSettledAsync(messages);

        // Assert
        Assert.Equal(expected, actual);
        Assert.Equal(xml, criteria.Message);
    }

    [Fact]
    public async Task UpdateAutoVoidingMessageAsync_WhenCalled_ReturnExpectedCommandResult()
    {
        // Arrange
        var fixture = new Fixture();
        var messages = fixture.Create<IEnumerable<MsgTarget>>();
        var expected = fixture.Create<CRM.Models.Common.CommandResult>();
        var xml = messages.ToXML("ROOT").ToString();
        var criteria = new AutoVoidingMessageUpdCommand();
        var service = GetVoidingSettledService();
        baseService.ExecuteNonQueryAsync(Arg.Do<AutoVoidingMessageUpdCommand>(x => criteria = x)).Returns(expected);

        // Act
        var actual = await service.UpdateAutoVoidingMessageAsync(messages, "LIVE");

        // Assert
        Assert.Equal(expected, actual);
        Assert.Equal(xml, criteria.MessageIds);
    }

    [Fact]
    public async Task UpdateVoidingAutoFeedbackInfoBatchAsync_WhenCalled_ReturnExpectedCommandResult()
    {
        // Arrange
        var fixture = new Fixture();
        var expected = fixture.Create<CRM.Models.Common.CommandResult>();
        var service = GetVoidingSettledService();
        baseService.ExecuteNonQueryAsync(Arg.Do<VoidingAutoFeedbackInfoBatchUpdCommand>(x => _ = x)).Returns(expected);

        // Act
        var actual = await service.UpdateVoidingAutoFeedbackInfoBatchAsync(1, "status", "description", "LIVE");

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task ImportVoidingAutoFeedbackInfoAsync_WhenCalled_ReturnExpectedCommandResult()
    {
        // Arrange
        var fixture = new Fixture();
        var expected = fixture.Create<CRM.Models.Common.CommandResult>();
        var ticketEventInfos = fixture.Create<IEnumerable<VoidingTicketEventInfo>>();
        var criteria = new VoidingAutoFeedbackInfoInsCommand();
        var xml = ticketEventInfos.ToXML("ROOT").ToString();
        var service = GetVoidingSettledService();
        baseService.ExecuteNonQueryAsync(Arg.Do<VoidingAutoFeedbackInfoInsCommand>(x => criteria = x)).Returns(expected);

        // Act
        var actual = await service.ImportVoidingAutoFeedbackInfoAsync(ticketEventInfos, "LIVE");

        // Assert
        Assert.Equal(expected, actual);
        Assert.Equal(xml, criteria.Message);
    }

    #region Helpers

    private VoidingSettledTicketsService GetVoidingSettledService()
    {
        baseService = Substitute.For<IBaseService>();
        logService = Substitute.For<ICrmLogService>();
        return new VoidingSettledTicketsService(baseService, logService);
    }

    #endregion Helpers
}