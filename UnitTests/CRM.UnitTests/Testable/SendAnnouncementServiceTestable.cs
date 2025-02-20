namespace CRM.BackgroundJob.Core.UnitTests.Testable
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CRM.BackgroundJob.Core.Enumerables;
    using CRM.BackgroundJob.Core.Models.BotServices.Announcement;
    using CRM.BackgroundJob.Core.Services;
    using Microsoft.Extensions.Configuration;

    public class SendAnnouncementServiceTestable : SendAnnouncementService
    {
        public SendAnnouncementServiceTestable(
            IBaseService baseService,
            ICrmLogService logService,
            IConfiguration configuration,
            IChatBotSendMessageService chatbotService
        ) : base(baseService, logService, configuration, chatbotService) { }

        public static (IEnumerable<AnnouncementNotification> empty, IEnumerable<AnnouncementNotification> actionable) Public_PartitionNotifications(IEnumerable<AnnouncementNotification> notifications)
        {
            return PartitionNotifications(notifications);
        }

        public async Task Public_ProcessEmptyNotifications(IEnumerable<AnnouncementNotification> emptyNotifications)
        {
            await ProcessEmptyNotifications(emptyNotifications);
        }

        public async Task Public_ProcessActionableNotifications(IEnumerable<AnnouncementNotification> notifications)
        {
            await ProcessActionableNotifications(notifications);
        }

        public async Task Public_SendAnnouncementAsync(IEnumerable<AnnouncementNotification> notifications)
        {
            await SendAnnouncementAsync(notifications);
        }

        public async Task Public_SendAnnouncementNotificationToBotServiceAsync(BotChannel botChannel, IEnumerable<AnnouncementNotification> notifications)
        {
            await SendAnnouncementNotificationToBotServiceAsync(botChannel, notifications);
        }

        public static IEnumerable<AnnouncementNotificationUpdateResult> Public_ConvertSendMessageResultToUpdateStatusResult(
            IEnumerable<AnnouncementNotification> notifications, IEnumerable<SendNotificationResult> sendResults
        )
        {
            return ConvertSendMessageResultToUpdateStatusResult(notifications, sendResults);
        }

        public static string Public_GetErrorImagesResult(IList<string> errorImages)
        {
            return GetErrorImagesResult(errorImages);
        }

        public async Task Public_RetryImagesOnlyAsync(IEnumerable<AnnouncementNotification> notifications)
        {
            await RetryImagesOnlyAsync(notifications);
        }

        public async Task Public_RetryImageByChannelAsync(BotChannel botChannel, IEnumerable<AnnouncementNotification> notifications)
        {
            await RetryImageByChannelAsync(botChannel, notifications);
        }

        public async Task Public_HandleUpdateResultAnnouncement(IEnumerable<AnnouncementNotificationUpdateResult> updatedResults)
        {
            await HandleUpdateResultAnnouncement(updatedResults);
        }

        public async Task Public_UpdateBotRemovedGroup(IEnumerable<AnnouncementNotification> botRemovedNotifications)
        {
            await UpdateBotRemovedGroup(botRemovedNotifications);
        }

        public async Task Public_UpdateEmptyContentFileName(IEnumerable<AnnouncementNotification> notifications)
        {
            await UpdateEmptyContentFileName(notifications);
        }

        public async Task Public_UpdateRecipientStatus(IEnumerable<AnnouncementNotificationUpdateResult> sentResults)
        {
            await UpdateRecipientStatus(sentResults);
        }

        public async Task<IEnumerable<AnnouncementNotification>> Public_GetAnnouncementNotificationAsync()
        {
            return await GetAnnouncementNotificationAsync();
        }

        public static void Public_MappingUpdateStatusErrorMessage(AnnouncementNotificationUpdateResult result, SendNotificationResult matchedResult)
        {
            MappingUpdateStatusErrorMessage(result, matchedResult);
        }

        public static IEnumerable<IEnumerable<AnnouncementNotification>> Public_DividedBatchToSendAnnouncement(IEnumerable<AnnouncementNotification> notifications)
        {
            return DividedBatchToSendAnnouncement(notifications);
        }
    }
}
