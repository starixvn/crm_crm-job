namespace CRM.BackgroundJob.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CRM.BackgroundJob.Core.Constants;
    using CRM.BackgroundJob.Core.Criteria;
    using CRM.BackgroundJob.Core.Enumerables;
    using CRM.BackgroundJob.Core.Extensions.Xml;
    using CRM.BackgroundJob.Core.Models.BotServices;
    using Microsoft.Extensions.Configuration;

    public interface ISendNotificationService
    {
        Task ScanAndSendMessageToBotEndpointAsync();
    }

    public class SendNotificationService : ISendNotificationService
    {
        private readonly int ItemEachBatch;
        private const int DefaultItemEachBatch = 10;

        private readonly IBaseService baseService;
        private readonly ICrmLogService logService;
        private readonly IChatBotSendMessageService chatbotService;

        public SendNotificationService(IBaseService baseService,
            ICrmLogService logService,
            IConfiguration configuration,
            IChatBotSendMessageService chatbotService)
        {
            this.baseService = baseService;
            this.logService = logService;
            this.chatbotService = chatbotService;
            ItemEachBatch = configuration.GetValue<int?>("SendNotificationItemEachBatch") ?? DefaultItemEachBatch;
        }

        public async Task ScanAndSendMessageToBotEndpointAsync()
        {
            var notifications = await GetNotificationToSend();

            if (notifications.Any())
            {
                await UpdateBotRemovedStatus(notifications.Where(noti => noti.BotId == 0 && noti.ChatType == ChatType.Group));

                await SendMessageToRecipients(notifications.Where(noti => noti.BotId > 0));
            }
        }

        private async Task SendMessageToRecipients(IEnumerable<NotificationMessage> notifications)
        {
            await Task.WhenAll(
                SendMessageByBotServiceAsync(BotChannel.Letstalk, notifications),
                SendMessageByBotServiceAsync(BotChannel.Telegram, notifications)
            );
        }

        private async Task SendMessageByBotServiceAsync(BotChannel botChannel, IEnumerable<NotificationMessage> notifications)
        {
            var filterNotifications = notifications.Where(x => x.ChannelId == botChannel).ToList();
            if (filterNotifications.Any())
            {
                await SendNotification(botChannel, filterNotifications);
            }
        }

        private async Task SendNotification(BotChannel botChannel, IEnumerable<NotificationMessage> notifications)
        {
            var batches = DivideBatchToSend(notifications);
            foreach (var batch in batches)
            {
                var results = new List<NotificationMessageUpdateResult>();
                try
                {
                    var sendResults = await chatbotService.SendMultiplePlaintextMessageAsync(botChannel, batch);
                    results = GetUpdateResultModel(sendResults).ToList();
                }
                catch (Exception ex)
                {
                    foreach (var noti in batch)
                    {
                        results.Add(new NotificationMessageUpdateResult
                        {
                            NotificationId = noti.NotificationId,
                            ConversationGroupId = noti.ConversationGroupId,
                            Status = SendNotificationStatuses.Retrying,
                            ErrMsg = string.Format(SendNotificationErrorMessage.BotServiceErrorMsg, botChannel.ToString().ToLowerInvariant())
                        });
                    }
                    await logService.ErrorAsync(ex);
                }
                finally
                {
                    await UpdateRecipientStatus(results);
                }
            }
        }

        private List<List<NotificationMessage>> DivideBatchToSend(IEnumerable<NotificationMessage> notifications)
        {
            var batches = new List<List<NotificationMessage>>();
            int totalBatch = (int)Math.Ceiling((double)notifications.Count() / ItemEachBatch);
            for (int i = 0; i < totalBatch; i++)
            {
                batches.Add(notifications.Skip(i * ItemEachBatch).Take(ItemEachBatch).ToList());
            }

            return batches;
        }

        private async Task<IEnumerable<NotificationMessage>> GetNotificationToSend()
        {
            var notifications = await baseService.GetDataAsync<NotificationMessage>(new GetNotificationToSendCriteria());

            return notifications;
        }

        private async Task UpdateBotRemovedStatus(IEnumerable<NotificationMessage> notifications)
        {
            if (notifications.Any())
            {
                var results = new List<NotificationMessageUpdateResult>();
                foreach (var notification in notifications)
                {
                    var result = new NotificationMessageUpdateResult
                    {
                        NotificationId = notification.NotificationId,
                        ConversationGroupId = notification.ConversationGroupId,
                        SentDate = DateTimeOffset.UtcNow.DateTime,
                        Status = SendNotificationStatuses.Error,
                        ErrMsg = SendNotificationErrorMessage.BotRemovedMsg
                    };
                    results.Add(result);
                }

                await UpdateRecipientStatus(results);
            }
        }

        private async Task UpdateRecipientStatus(IEnumerable<NotificationMessageUpdateResult> results)
        {
            var command = new UpdateNotificationStatusCommand
            {
                XmlData = results.ToXML("root").ToString()
            };

            await baseService.ExecuteNonQueryAsync(command);
        }

        private static IEnumerable<NotificationMessageUpdateResult> GetUpdateResultModel(IEnumerable<SendResult> sendResults)
        {
            var updateResults = new List<NotificationMessageUpdateResult>();
            if (sendResults != null && sendResults.Any())
            {
                foreach (var result in sendResults)
                {
                    var updateModel = new NotificationMessageUpdateResult
                    {
                        NotificationId = result.NotificationId,
                        ConversationGroupId = result.ConversationGroupId,
                        SentDate = result.SentTime,
                        Status = GetUpdateStatus(result),
                        ErrMsg = result.ErrorMessage
                    };
                    updateResults.Add(updateModel);
                }
            }

            return updateResults;
        }

        private static string GetUpdateStatus(SendResult result)
        {
            if (result.IsBlockedBot)
            {
                return SendNotificationStatuses.BotBlocked;
            }
            if (result.IsSuccessful && !result.IsBlockedBot)
            {
                return SendNotificationStatuses.Sent;
            }
            if (!result.IsSuccessful && !result.IsIgnoreRetry)
            {
                return SendNotificationStatuses.Retrying;
            }
            return SendNotificationStatuses.Error;
        }
    }
}