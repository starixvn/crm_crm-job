namespace CRM.UnitTests.Services.SendNotification
{
    using AutoFixture;
    using CRM.Constants;
    using CRM.Criteria.Announcement;
    using CRM.Enum;
    using CRM.Models.BotServices.Announcement;
    using CRM.UnitTests.Testable;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using NSubstitute;
    using NSubstitute.ExceptionExtensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CRM.Services.Common;
    using Xunit;
    using CRM.Services.Bot;

    public class SendAnnouncementServiceTests
    {
        private IBaseService baseService;
        private ICrmLogService logService;
        private IConfiguration configuration;
        private IChatBotSendMessageService chatbotService;

        [Fact]
        public async Task ScanSendAndUpdateAnnouncementNotification_WhenCalled_ReturnExpected()
        {
            // Arrange
            var fixture = new Fixture();
            var noti = fixture.Create<List<AnnouncementNotification>>();
            noti[0].Content = null;
            noti[0].FileName = null;
            var empty = noti.Where(n => string.IsNullOrWhiteSpace(n.Content) && string.IsNullOrWhiteSpace(n.FileName));
            var actionable = noti.Except(empty).ToList();
            var service = GetService();
            baseService.GetDataAsync<AnnouncementNotification>(Arg.Any<GetAnnouncementNotificationToSendCriteria>()).Returns(noti);

            // Act
            await service.ScanSendAndUpdateAnnouncementNotification();

            // Assert
            await service.Received().Public_ProcessEmptyNotifications(empty);
            await service.Received().Public_ProcessActionableNotifications(actionable);
        }

        [Fact]
        public void PartitionNotifications_WhenCalled_ReturnExpected()
        {
            // Arrange
            var fixture = new Fixture();
            var notifications = fixture.Create<IEnumerable<AnnouncementNotification>>();
            var empty = notifications.Where(n => string.IsNullOrWhiteSpace(n.Content) && string.IsNullOrWhiteSpace(n.FileName));
            var actionable = notifications.Except(empty).ToList();
            GetService();

            // Act
            var actual = SendAnnouncementServiceTestable.Public_PartitionNotifications(notifications);

            // Assert
            Assert.Equal(empty, actual.empty);
            Assert.Equal(actionable, actual.actionable);
        }

        [Fact]
        public async Task ProcessEmptyNotifications_WhenCalled_ReturnExpected()
        {
            // Arrange
            var fixture = new Fixture();
            var emptyNotifications = fixture.Create<IEnumerable<AnnouncementNotification>>();
            var service = GetService();

            // Act
            await service.Public_ProcessEmptyNotifications(emptyNotifications);

            // Assert
            await service.Received().Public_UpdateEmptyContentFileName(emptyNotifications);
        }

        [Fact]
        public async Task ProcessActionableNotifications_WhenCalled_ReturnExpected()
        {
            // Arrange
            var fixture = new Fixture();
            var notifications = fixture.Create<IEnumerable<AnnouncementNotification>>();
            var updateBot = notifications.Where(n => n.ChatTypeId == ChatType.Group && n.BotId <= 0);
            var retryImages = notifications.Where(n => n.BotId > 0 && n.IsOnlyRetryImage);
            var sendAnnouncement = notifications.Where(n => n.BotId > 0 && !n.IsOnlyRetryImage);
            var service = GetService();

            // Act
            await service.Public_ProcessActionableNotifications(notifications);

            // Assert
            await service.Received().Public_UpdateBotRemovedGroup(updateBot);
            await service.Received().Public_RetryImagesOnlyAsync(retryImages);
            await service.Received().Public_SendAnnouncementAsync(sendAnnouncement);
        }

        [Fact]
        public async Task SendAnnouncementAsync_WhenCalled_ReturnExpected()
        {
            // Arrange
            var fixture = new Fixture();
            var notifications = fixture.Create<IEnumerable<AnnouncementNotification>>();
            var service = GetService();

            // Act
            await service.Public_SendAnnouncementAsync(notifications);

            // Assert
            await service.Received().Public_SendAnnouncementNotificationToBotServiceAsync(BotChannel.Letstalk, notifications);
            await service.Received().Public_SendAnnouncementNotificationToBotServiceAsync(BotChannel.Telegram, notifications);
        }

        [Fact]
        public async Task SendAnnouncementNotificationToBotServiceAsync_WhenCalled_ReturnExpected()
        {
            // Arrange
            var fixture = new Fixture();
            var notifications = new List<AnnouncementNotification>
            {
                new() { ChannelId = BotChannel.Letstalk, AnnouncementId = 1, ConversationId = "1", NotificationId = 1, ConversationGroupId = 1 }
            };
            var send = fixture.Create<List<SendNotificationResult>>();
            send[0].AnnouncementId = 1;
            send[0].ConversationId = "1";
            var results = new List<AnnouncementNotificationUpdateResult>();
            var result = new AnnouncementNotificationUpdateResult
            {
                NotificationId = notifications[0].NotificationId,
                ConversationGroupId = notifications[0].ConversationGroupId,
                IsOnlyRetryImage = send[0].IsOnlyRetryImage,
                ErrImages = SendAnnouncementServiceTestable.Public_GetErrorImagesResult(send[0].ErrImages)
            };
            results.Add(result);

            var service = GetService();
            chatbotService.SendMultipleFormattedMessageAndAttachmentAsync(BotChannel.Letstalk, Arg.Any<IEnumerable<AnnouncementNotification>>()).Returns(send);

            // Act
            await service.Public_SendAnnouncementNotificationToBotServiceAsync(BotChannel.Letstalk, notifications);

            // Assert
            await service.Received().Public_UpdateRecipientStatus(results);
        }

        [Fact]
        public async Task SendAnnouncementNotificationToBotServiceAsync_WhenCalled_ThrowException()
        {
            // Arrange
            var fixture = new Fixture();
            var notifications = fixture.Create<IEnumerable<AnnouncementNotification>>();
            var exception = fixture.Create<Exception>();
            var service = GetService();
            chatbotService.SendMultipleFormattedMessageAndAttachmentAsync(BotChannel.Letstalk, Arg.Any<IEnumerable<AnnouncementNotification>>()).Throws(exception);

            // Act
            await service.Public_SendAnnouncementNotificationToBotServiceAsync(BotChannel.Letstalk, notifications);

            // Assert
            await logService.Received().ErrorAsync(Arg.Any<Exception>());
        }

        [Fact]
        public void ConvertSendMessageResultToUpdateStatusResult_WhenCalled_ReturnExpected()
        {
            // Arrange
            var notifications = new List<AnnouncementNotification>
            {
                new (){ AnnouncementId = 1, ConversationId = "1"}
            };
            var sendResults = new List<SendNotificationResult>
            {
                new (){ AnnouncementId = 1, ConversationId = "1", IsOnlyRetryImage = true, ErrImages = ["err.pdf"] }
            };

            // Act
            var actual = SendAnnouncementServiceTestable.Public_ConvertSendMessageResultToUpdateStatusResult(notifications, sendResults);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal("err.pdf", actual.FirstOrDefault()?.ErrImages);
        }

        [Fact]
        public void GetErrorImagesResult_WhenCalled_ReturnExpected()
        {
            // Arrange
            var errorImages = new List<string>
            {
                "test.png/err.pdf"
            };

            // Act
            var actual = SendAnnouncementServiceTestable.Public_GetErrorImagesResult(errorImages);

            // Assert
            Assert.Equal("err.pdf", actual);
        }

        [Fact]
        public async Task RetryImagesOnlyAsync_WhenCalled_ReturnExpected()
        {
            // Arrange
            var fixture = new Fixture();
            var notifications = fixture.Create<IEnumerable<AnnouncementNotification>>();
            var service = GetService();

            // Act
            await service.Public_RetryImagesOnlyAsync(notifications);

            // Assert
            await service.Received().Public_RetryImageByChannelAsync(BotChannel.Letstalk, notifications);
            await service.Received().Public_RetryImageByChannelAsync(BotChannel.Telegram, notifications);
        }

        [Fact]
        public async Task RetryImageByChannelAsync_WhenCalled_ReturnExpected()
        {
            // Arrange
            var fixture = new Fixture();
            var notifications = new List<AnnouncementNotification>
            {
                new() { ChannelId = BotChannel.Letstalk, AnnouncementId = 1, ConversationId = "1", NotificationId = 1, ConversationGroupId = 1 }
            };
            var send = fixture.Create<List<SendNotificationResult>>();
            send[0].AnnouncementId = 1;
            send[0].ConversationId = "1";
            var results = new List<AnnouncementNotificationUpdateResult>();
            var result = new AnnouncementNotificationUpdateResult
            {
                NotificationId = notifications[0].NotificationId,
                ConversationGroupId = notifications[0].ConversationGroupId,
                IsOnlyRetryImage = send[0].IsOnlyRetryImage,
                ErrImages = SendAnnouncementServiceTestable.Public_GetErrorImagesResult(send[0].ErrImages)
            };
            results.Add(result);

            var service = GetService();
            chatbotService.SendImageOnlyAsync(BotChannel.Letstalk, Arg.Any<IEnumerable<AnnouncementNotification>>()).Returns(send);

            // Act
            await service.Public_RetryImageByChannelAsync(BotChannel.Letstalk, notifications);

            // Assert
            await service.Received().Public_HandleUpdateResultAnnouncement(results);
        }

        [Fact]
        public async Task RetryImageByChannelAsync_WhenCalled_ThrowException()
        {
            // Arrange
            var fixture = new Fixture();
            var notifications = fixture.Create<IEnumerable<AnnouncementNotification>>();
            var exception = fixture.Create<Exception>();
            var service = GetService();
            chatbotService.SendImageOnlyAsync(BotChannel.Letstalk, Arg.Any<IEnumerable<AnnouncementNotification>>()).Throws(exception);

            // Act
            await service.Public_RetryImageByChannelAsync(BotChannel.Letstalk, notifications);

            // Assert
            await logService.Received(1).ErrorAsync(Arg.Any<Exception>());
        }

        [Fact]
        public async Task HandleUpdateResultAnnouncement_WhenCalled_ReturnExpected()
        {
            // Arrange
            var fixture = new Fixture();
            var updatedResults = fixture.Create<IEnumerable<AnnouncementNotificationUpdateResult>>();
            var service = GetService();

            // Act
            await service.Public_HandleUpdateResultAnnouncement(updatedResults);

            // Assert
            await service.Received().Public_UpdateRecipientStatus(updatedResults);
        }

        [Fact]
        public async Task HandleUpdateResultAnnouncement_WhenCalled_ReturnErrorLog()
        {
            // Arrange
            var updatedResults = new List<AnnouncementNotificationUpdateResult>();
            var service = GetService();

            // Act
            await service.Public_HandleUpdateResultAnnouncement(updatedResults);

            // Assert
            await logService.ErrorMessageAsync(JsonConvert.SerializeObject(updatedResults));
        }

        [Fact]
        public async Task UpdateBotRemovedGroup_WhenCalled_ReturnExpected()
        {
            // Arrange
            var fixture = new Fixture();
            var botRemovedNotifications = fixture.Create<IEnumerable<AnnouncementNotification>>();
            var service = GetService();

            // Act
            await service.Public_UpdateBotRemovedGroup(botRemovedNotifications);
        }

        [Fact]
        public async Task UpdateEmptyContentFileName_WhenCalled_ReturnExpected()
        {
            // Arrange
            var fixture = new Fixture();
            var notifications = fixture.Create<IEnumerable<AnnouncementNotification>>();
            var service = GetService();

            // Act
            await service.Public_UpdateEmptyContentFileName(notifications);
        }

        [Fact]
        public async Task UpdateRecipientStatus_WhenCalled_ReturnExpected()
        {
            // Arrange
            var fixture = new Fixture();
            var sentResults = fixture.Create<IEnumerable<AnnouncementNotificationUpdateResult>>();
            var service = GetService();

            // Act
            await service.Public_UpdateRecipientStatus(sentResults);

            // Assert
            await baseService.Received().ExecuteNonQueryAsync(Arg.Any<UpdateAnnouncementNotificationStatusCommand>());
        }

        [Fact]
        public async Task GetAnnouncementNotificationAsync_WhenCalled_ReturnExpected()
        {
            // Arrange
            var notifications = new List<AnnouncementNotification>()
            {
                new() { AnnouncementId = 1, FileName = "test01, test01", ErrImages = "eTest01, eTest02" }
            };
            var service = GetService();
            baseService.GetDataAsync<AnnouncementNotification>(Arg.Any<GetAnnouncementNotificationToSendCriteria>()).Returns(notifications);

            // Act
            var actual = await service.Public_GetAnnouncementNotificationAsync();

            // Assert
            Assert.Equal("1/test01", actual.FirstOrDefault()?.Images[0]);
            Assert.Equal("1/eTest01", actual.FirstOrDefault()?.RetryImages[0]);
        }

        [Fact]
        public void MappingUpdateStatusErrorMessage_WhenCalled_Sent()
        {
            // Arrange
            var fixture = new Fixture();
            var matchedResult = new SendNotificationResult
            {
                IsSuccessful = true,
                IsBotBlocked = false,
                IgnoredRetry = true
            };
            var result = fixture.Create<AnnouncementNotificationUpdateResult>();

            // Act
            SendAnnouncementServiceTestable.Public_MappingUpdateStatusErrorMessage(result, matchedResult);

            // Assert
            Assert.Equal(SendNotificationStatuses.Sent, result.Status);
            Assert.Equal(string.Empty, result.ErrMsg);
        }

        [Fact]
        public void MappingUpdateStatusErrorMessage_WhenCalled_Retrying()
        {
            // Arrange
            var fixture = new Fixture();
            var matchedResult = new SendNotificationResult
            {
                IsSuccessful = false,
                IsBotBlocked = false,
                IgnoredRetry = false
            };
            var result = fixture.Create<AnnouncementNotificationUpdateResult>();

            // Act
            SendAnnouncementServiceTestable.Public_MappingUpdateStatusErrorMessage(result, matchedResult);

            // Assert
            Assert.Equal(SendNotificationStatuses.Retrying, result.Status);
            Assert.Equal(matchedResult.ErrMsg, result.ErrMsg);
        }

        [Fact]
        public void MappingUpdateStatusErrorMessage_WhenCalled_Error()
        {
            // Arrange
            var fixture = new Fixture();
            var matchedResult = new SendNotificationResult
            {
                IsSuccessful = false,
                IsBotBlocked = false,
                IgnoredRetry = true
            };
            var result = fixture.Create<AnnouncementNotificationUpdateResult>();

            // Act
            SendAnnouncementServiceTestable.Public_MappingUpdateStatusErrorMessage(result, matchedResult);

            // Assert
            Assert.Equal(SendNotificationStatuses.Error, result.Status);
            Assert.Equal(matchedResult.ErrMsg, result.ErrMsg);
        }

        [Fact]
        public void MappingUpdateStatusErrorMessage_WhenCalled_BotBlocked()
        {
            // Arrange
            var fixture = new Fixture();
            var matchedResult = new SendNotificationResult
            {
                IsSuccessful = true,
                IsBotBlocked = true,
                IgnoredRetry = true
            };
            var result = fixture.Create<AnnouncementNotificationUpdateResult>();

            // Act
            SendAnnouncementServiceTestable.Public_MappingUpdateStatusErrorMessage(result, matchedResult);

            // Assert
            Assert.Equal(SendNotificationStatuses.BotBlocked, result.Status);
            Assert.Equal(string.Empty, result.ErrMsg);
        }

        [Fact]
        public void DividedBatchToSendAnnouncement_WhenCalled_ReturnExpected()
        {
            // Arrange
            var batches = new List<List<AnnouncementNotification>>();
            var notifications = new List<AnnouncementNotification>
            {
                new() { AnnouncementId = 1, Content = "" }
            };
            batches.Add(notifications);

            // Act
            var actual = SendAnnouncementServiceTestable.Public_DividedBatchToSendAnnouncement(notifications);

            // Assert
            Assert.Equal(batches, actual);
        }

        #region Helpers

        private SendAnnouncementServiceTestable GetService()
        {
            baseService = Substitute.For<IBaseService>();
            logService = Substitute.For<ICrmLogService>();
            configuration = Substitute.For<IConfiguration>();
            chatbotService = Substitute.For<IChatBotSendMessageService>();
            return Substitute.For<SendAnnouncementServiceTestable>(baseService, logService, configuration, chatbotService);
        }

        #endregion Helpers
    }
}