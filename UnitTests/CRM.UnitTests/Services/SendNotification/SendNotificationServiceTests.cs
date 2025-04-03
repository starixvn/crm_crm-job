namespace CRM.UnitTests.Services.SendNotification;

using CRM.Criteria.Common;
using CRM.Criteria.Notification;
using CRM.Enum;
using CRM.Models.BotServices;
using CRM.Services.Bot;
using CRM.Services.Common;
using CRM.Services.SendNotification;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class SendNotificationServiceTests
{
    private readonly SendNotificationService service;
    private readonly IBaseService baseService;
    private readonly ICrmLogService logService;
    private readonly IChatBotSendMessageService chatbotService;

    public SendNotificationServiceTests()
    {
        var configuration = Substitute.For<IConfiguration>();
        baseService = Substitute.For<IBaseService>();
        logService = Substitute.For<ICrmLogService>();
        chatbotService = Substitute.For<IChatBotSendMessageService>();
        service = new SendNotificationService(baseService, logService, configuration, chatbotService);
    }

    [Fact]
    public async Task SendNotificationToLetstalk_WhenNoHaveAnyNotification_DoNothing()
    {
        baseService.GetDataAsync<NotificationMessage>(Arg.Any<GetNotificationToSendCriteria>()).Returns(new List<NotificationMessage>());

        await service.ScanAndSendMessageToBotEndpointAsync();

        await baseService.Received(1).GetDataAsync<NotificationMessage>(Arg.Any<GetNotificationToSendCriteria>());
        await logService.DidNotReceive().ErrorAsync(Arg.Any<Exception>());
        await chatbotService.DidNotReceive().SendMultiplePlaintextMessageAsync(Arg.Any<BotChannel>(), Arg.Any<IEnumerable<NotificationMessage>>());
    }

    [Fact]
    public async Task SendNotificationToLetstalk_WhenHaveOnlyBotRemoveNotification_WillUpdateStatusWithBotRemoveReason()
    {
        var notifications = new List<NotificationMessage>
        {
            new() {BotId = 0, ConversationGroupId = 1, NotificationId = 1, ChatTypeId = 1},
            new() {BotId = 0, ConversationGroupId = 2, NotificationId = 2, ChatTypeId = 2}
        };
        baseService.GetDataAsync<NotificationMessage>(Arg.Any<GetNotificationToSendCriteria>()).Returns(notifications);

        await service.ScanAndSendMessageToBotEndpointAsync();

        await baseService.Received(1).GetDataAsync<NotificationMessage>(Arg.Any<GetNotificationToSendCriteria>());
        await baseService.Received(1).ExecuteNonQueryAsync(Arg.Any<UpdateNotificationStatusCommand>());
        await logService.DidNotReceive().ErrorAsync(Arg.Any<Exception>());
    }

    [Fact]
    public async Task SendNotificationToLetstalk_WhenHaveNotification_AndSendFailedAllNotification_WillCallsExpectedNumberLetstalkService()
    {
        var notifications = FakeNotification();
        baseService.GetDataAsync<NotificationMessage>(Arg.Any<GetNotificationToSendCriteria>()).Returns(notifications);
        await service.ScanAndSendMessageToBotEndpointAsync();

        await baseService.Received(1).GetDataAsync<NotificationMessage>(Arg.Any<GetNotificationToSendCriteria>());
        await chatbotService.Received(1).SendMultiplePlaintextMessageAsync(Arg.Is(BotChannel.Letstalk), Arg.Any<IEnumerable<NotificationMessage>>());
        await chatbotService.Received(1).SendMultiplePlaintextMessageAsync(Arg.Is(BotChannel.Telegram), Arg.Any<IEnumerable<NotificationMessage>>());
        await baseService.Received(2).ExecuteNonQueryAsync(Arg.Any<UpdateNotificationStatusCommand>());
    }

    private static List<NotificationMessage> FakeNotification()
        => [new()
            {
                BotId = 100, ConversationGroupId = 1, NotificationId = 1, ChatTypeId = 1, ConversationId = "1",
                ChannelId = BotChannel.Letstalk
            },
            new()
            {
                BotId = 100, ConversationGroupId = 2, NotificationId = 2, ChatTypeId = 1, ConversationId = "2",
                ChannelId = BotChannel.Letstalk
            },
            new()
            {
                BotId = 100, ConversationGroupId = 3, NotificationId = 3, ChatTypeId = 2, ConversationId = "3",
                ChannelId = BotChannel.Telegram
            },
            new()
            {
                BotId = 100, ConversationGroupId = 4, NotificationId = 4, ChatTypeId = 2, ConversationId = "4",
                ChannelId = BotChannel.Telegram
            },
            new()
            {
                BotId = 100, ConversationGroupId = 5, NotificationId = 5, ChatTypeId = 2, ConversationId = "5",
                ChannelId = BotChannel.Telegram
            }];
}