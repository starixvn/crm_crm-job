namespace CRM.BackgroundJob.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CRM.BackgroundJob.Core.Constants;
    using CRM.BackgroundJob.Core.Criteria.Announcement;
    using CRM.BackgroundJob.Core.Enumerables;
    using CRM.BackgroundJob.Core.Extensions.Xml;
    using CRM.BackgroundJob.Core.Models.BotServices.Announcement;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    public interface ISendAnnouncementService
    {
        Task ScanSendAndUpateAnnouncementNotification();
    }

    public class SendAnnouncementService : ISendAnnouncementService
    {
        private readonly IBaseService baseService;
        private readonly ICrmLogService logService;
        private readonly IChatBotSendMessageService chatBotSendMessageService;
        private readonly string BaseImageUrl;
        private const string formatImageUrl = "{0}{1}/{2}";
        private const byte BatchSize = 10;
        private const string EmptyContentFileNameAnn =
            "The notification is EMPTY because the language setting of this headline " +
            "was changed after confirming to send announcement.<br>" +
            "Please send a new announcement for this headline.";

        public SendAnnouncementService(IBaseService baseService,
            ICrmLogService logService,
            IConfiguration configuration,
            IChatBotSendMessageService chatBotSendMessageService)
        {
            this.baseService = baseService;
            this.logService = logService;
            this.chatBotSendMessageService = chatBotSendMessageService;
            BaseImageUrl = configuration.GetValue<string>("BaseImageUrl");
        }

        public async Task ScanSendAndUpateAnnouncementNotification()
        {
            var notifications = await GetAnnouncementNotificationAsync();
            var (emptyNotifications, actionableNotifications) = PartitionNotifications(notifications);

            await ProcessEmptyNotifications(emptyNotifications);
            await ProcessActionableNotifications(actionableNotifications);
        }

        protected internal async Task ProcessEmptyNotifications(IEnumerable<AnnouncementNotification> emptyNotifications)
        {
            if (emptyNotifications.Any())
            {
                await UpdateEmptyContentFileName(emptyNotifications);
            }
        }

        protected internal static (IEnumerable<AnnouncementNotification> empty, IEnumerable<AnnouncementNotification> actionable)
        PartitionNotifications(IEnumerable<AnnouncementNotification> notifications)
        {
            var empty = notifications.Where(n => string.IsNullOrWhiteSpace(n.Content) && string.IsNullOrWhiteSpace(n.FileName));
            var actionable = notifications.Except(empty).ToList();
            return (empty, actionable);
        }

        protected internal async Task ProcessActionableNotifications(IEnumerable<AnnouncementNotification> notifications)
        {
            if (notifications.Any())
            {
                await UpdateBotRemovedGroup(notifications.Where(n => n.ChatTypeId == ChatType.Group && n.BotId <= 0));
                await RetryImagesOnlyAsync(notifications.Where(n => n.BotId > 0 && n.IsOnlyRetryImage));
                await SendAnnouncementAsync(notifications.Where(n => n.BotId > 0 && !n.IsOnlyRetryImage));
            }
        }

        #region Send announcement

        protected internal async Task SendAnnouncementAsync(IEnumerable<AnnouncementNotification> notifications)
        {
            await Task.WhenAll(
                SendAnnouncementNotificationToBotServiceAsync(BotChannel.Letstalk, notifications),
                SendAnnouncementNotificationToBotServiceAsync(BotChannel.Telegram, notifications)
            );
        }

        protected internal async Task SendAnnouncementNotificationToBotServiceAsync(BotChannel botChannel, IEnumerable<AnnouncementNotification> notifications)
        {
            var filterNotifications = notifications.Where(x => x.ChannelId == botChannel).ToList();
            if (filterNotifications.Any())
            {
                var batches = DividedBatchToSendAnnouncement(filterNotifications);
                foreach (var batch in batches)
                {
                    try
                    {
                        var sentResults = await chatBotSendMessageService.SendMultipleFormattedMessageAndAttachmentAsync(botChannel, batch);
                        logService.LogInformation(JsonConvert.SerializeObject(sentResults));
                        var updatedResult = ConvertSendMessageResultToUpdateStatusResult(batch, sentResults);
                        await UpdateRecipientStatus(updatedResult);
                    }
                    catch (Exception ex)
                    {
                        var results = batch.Select(x => new AnnouncementNotificationUpdateResult
                        {
                            NotificationId = x.NotificationId,
                            ConversationGroupId = x.ConversationGroupId,
                            Status = SendNotificationStatuses.Retrying,
                            ErrMsg = SendNotificationErrorMessage.BotServiceErrorMsg,
                        });
                        await UpdateRecipientStatus(results);
                        await logService.ErrorAsync(ex);
                    }
                }
            }
        }

        protected internal static IEnumerable<AnnouncementNotificationUpdateResult> ConvertSendMessageResultToUpdateStatusResult(
            IEnumerable<AnnouncementNotification> notifications, IEnumerable<SendNotificationResult> sendResults
            )
        {
            var results = new List<AnnouncementNotificationUpdateResult>();
            foreach (var notification in notifications)
            {
                var matchedResult = sendResults.FirstOrDefault(x => x.ConversationId.Equals(notification.ConversationId) && x.AnnouncementId == notification.AnnouncementId);
                if (matchedResult is not null)
                {
                    var result = new AnnouncementNotificationUpdateResult
                    {
                        NotificationId = notification.NotificationId,
                        ConversationGroupId = notification.ConversationGroupId,
                        IsOnlyRetryImage = matchedResult.IsOnlyRetryImage,
                        ErrImages = GetErrorImagesResult(matchedResult.ErrImages)
                    };
                    MappingUpdateStatusErrorMessage(result, matchedResult);
                    results.Add(result);
                }
            }
            return results;
        }

        protected internal static string GetErrorImagesResult(IList<string> errorImages)
        {
            string result = string.Empty;
            if (errorImages is not null && errorImages.Count > 0)
            {
                var imgList = errorImages.Select(img => img.Split("/").LastOrDefault()).ToList();
                result = string.Join(",", imgList);
            }
            return result;
        }

        #endregion Send announcement

        #region Retry Images Only

        protected internal async Task RetryImagesOnlyAsync(IEnumerable<AnnouncementNotification> notifications)
        {
            await Task.WhenAll(
                RetryImageByChannelAsync(BotChannel.Letstalk, notifications),
                RetryImageByChannelAsync(BotChannel.Telegram, notifications)
            );
        }

        protected internal async Task RetryImageByChannelAsync(BotChannel botChannel, IEnumerable<AnnouncementNotification> notifications)
        {
            var filterNotifications = notifications.Where(x => x.ChannelId == botChannel).ToList();
            if (filterNotifications.Any())
            {
                var batches = DividedBatchToSendAnnouncement(filterNotifications);
                foreach (var batch in batches)
                {
                    try
                    {
                        var sentResults = await chatBotSendMessageService.SendImageOnlyAsync(botChannel, batch);
                        var updatedResults = ConvertSendMessageResultToUpdateStatusResult(batch, sentResults);
                        await HandleUpdateResultAnnouncement(updatedResults);
                    }
                    catch (Exception ex)
                    {
                        var results = batch.Select(x => new AnnouncementNotificationUpdateResult
                        {
                            NotificationId = x.NotificationId,
                            ConversationGroupId = x.ConversationGroupId,
                            Status = SendNotificationStatuses.Retrying,
                            ErrMsg = SendNotificationErrorMessage.BotServiceErrorMsg,
                            IsOnlyRetryImage = true,
                            ErrImages = x.ErrImages
                        });
                        await UpdateRecipientStatus(results);
                        await logService.ErrorAsync(ex);
                    }
                }
            }
        }

        protected internal async Task HandleUpdateResultAnnouncement(IEnumerable<AnnouncementNotificationUpdateResult> updatedResults)
        {
            if (!updatedResults.Any())
            {
                await logService.ErrorMessageAsync(JsonConvert.SerializeObject(updatedResults));
            }
            await UpdateRecipientStatus(updatedResults);
        }

        #endregion Retry Images Only

        #region Update status with content and fileName is empty

        protected internal async Task UpdateEmptyContentFileName(IEnumerable<AnnouncementNotification> notifications)
        {
            if (notifications.Any())
            {
                var updateStatusResult = new List<AnnouncementNotificationUpdateResult>();
                foreach (var item in notifications)
                {
                    updateStatusResult.Add(new AnnouncementNotificationUpdateResult
                    {
                        NotificationId = item.NotificationId,
                        ConversationGroupId = item.ConversationGroupId,
                        Status = "Error",
                        ErrMsg = EmptyContentFileNameAnn
                    });
                }
                await UpdateRecipientStatus(updateStatusResult);
            }
        }

        #endregion Update status with content and fileName is empty

        #region Update Bot Removed Groups

        protected internal async Task UpdateBotRemovedGroup(IEnumerable<AnnouncementNotification> botRemovedNotifications)
        {
            if (botRemovedNotifications.Any())
            {
                var updateStatusResult = new List<AnnouncementNotificationUpdateResult>();
                foreach (var item in botRemovedNotifications)
                {
                    updateStatusResult.Add(new AnnouncementNotificationUpdateResult
                    {
                        NotificationId = item.NotificationId,
                        ConversationGroupId = item.ConversationGroupId,
                        Status = "Error",
                        ErrMsg = "The Chatbot has been removed by users."
                    });
                }
                await UpdateRecipientStatus(updateStatusResult);
            }
        }

        #endregion Update Bot Removed Groups

        protected internal async Task UpdateRecipientStatus(IEnumerable<AnnouncementNotificationUpdateResult> sentResults)
        {
            var command = new UpdateAnnouncementNotificationStatusCommand
            {
                XmlData = sentResults.ToXML("root").ToString()
            };

            await baseService.ExecuteNonQueryAsync(command);
        }

        protected internal async Task<IEnumerable<AnnouncementNotification>> GetAnnouncementNotificationAsync()
        {
            var notifications = await baseService.GetDataAsync<AnnouncementNotification>(new GetAnnouncementNotificationToSendCriteria());
            foreach (var item in notifications)
            {
                if (!string.IsNullOrEmpty(item.FileName))
                {
                    var images = item.FileName.Split(",");
                    item.Images = images.Select(
                        img => string.Format(formatImageUrl, BaseImageUrl, item.AnnouncementId, img)
                        ).ToList();
                }

                if (!string.IsNullOrEmpty(item.ErrImages))
                {
                    var images = item.ErrImages.Split(",");
                    item.RetryImages = images.Select(
                        img => string.Format(formatImageUrl, BaseImageUrl, item.AnnouncementId, img)
                        ).ToList();
                }
            }

            return notifications;
        }

        protected internal static IEnumerable<IEnumerable<AnnouncementNotification>> DividedBatchToSendAnnouncement(IEnumerable<AnnouncementNotification> notifications)
        {
            var batches = new List<List<AnnouncementNotification>>();
            int totalBatch = (int)Math.Ceiling((double)notifications.Count() / BatchSize);
            for (int i = 0; i < totalBatch; i++)
            {
                batches.Add(notifications.Skip(i * BatchSize).Take(BatchSize).ToList());
            }
            return batches;
        }

        protected internal static void MappingUpdateStatusErrorMessage(AnnouncementNotificationUpdateResult result, SendNotificationResult matchedResult)
        {
            if (matchedResult.IsSuccessful)
            {
                result.Status = SendNotificationStatuses.Sent;
                result.ErrMsg = string.Empty;
            }

            if (!matchedResult.IsSuccessful && !matchedResult.IsBotBlocked && !matchedResult.IgnoredRetry)
            {
                result.Status = SendNotificationStatuses.Retrying;
                result.ErrMsg = matchedResult.ErrMsg;
            }

            if (!matchedResult.IsSuccessful && matchedResult.IgnoredRetry)
            {
                result.Status = SendNotificationStatuses.Error;
                result.ErrMsg = matchedResult.ErrMsg;
            }

            if (matchedResult.IsBotBlocked)
            {
                result.Status = SendNotificationStatuses.BotBlocked;
                result.ErrMsg = string.Empty;
            }
        }
    }
}