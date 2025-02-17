namespace CRM.BackgroundJob.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using CRM.BackgroundJob.Core.Enumerables;
    using CRM.BackgroundJob.Core.Models.BotServices;
    using CRM.BackgroundJob.Core.Models.BotServices.Announcement;
    using Fanex.Caching;

    public interface IChatBotSendMessageService
    {
        Task<IEnumerable<SendResult>> SendMultiplePlaintextMessageAsync(BotChannel botChannel, IEnumerable<NotificationMessage> notifications);

        Task<IEnumerable<SendNotificationResult>> SendMultipleFormattedMessageAndAttachmentAsync(BotChannel botChannel, IEnumerable<AnnouncementNotification> notifications);

        Task<IEnumerable<SendNotificationResult>> SendImageOnlyAsync(BotChannel botChannel, IEnumerable<AnnouncementNotification> notifications);

        Task<SendResult> SendSinglePlaintexMessageAsync(BotChannel botChannel, NotificationMessage notification);
    }

    public class ChatBotSendMessageService : BotServiceHttpClientBase, IChatBotSendMessageService
    {
        public ChatBotSendMessageService(BotServiceSettings botSettings, IHttpClientFactory httpClientFactory, ICacheService cacheService)
            : base(botSettings, httpClientFactory, cacheService)
        {
        }

        public async Task<IEnumerable<SendNotificationResult>> SendMultipleFormattedMessageAndAttachmentAsync(BotChannel botChannel, IEnumerable<AnnouncementNotification> notifications)
        {
            var apiConfig = GetApiConfig(botChannel);
            var results = await PostAsync<IEnumerable<SendNotificationResult>>(apiConfig.SendMultipleAnnouncementPath, notifications);
            return results;
        }

        public async Task<IEnumerable<SendResult>> SendMultiplePlaintextMessageAsync(BotChannel botChannel, IEnumerable<NotificationMessage> notifications)
        {
            var apiConfig = GetApiConfig(botChannel);
            var results = await PostAsync<IEnumerable<SendResult>>(apiConfig.SendMultipleMessagePath, notifications);
            return results;
        }

        public async Task<IEnumerable<SendNotificationResult>> SendImageOnlyAsync(BotChannel botChannel, IEnumerable<AnnouncementNotification> notifications)
        {
            var apiConfig = GetApiConfig(botChannel);
            var results = await PostAsync<IEnumerable<SendNotificationResult>>(apiConfig.SendOnlyImagesMultiplePath, notifications);
            return results;
        }

        public async Task<SendResult> SendSinglePlaintexMessageAsync(BotChannel botChannel, NotificationMessage notification)
        {
            var apiConfig = GetApiConfig(botChannel);

            var results = await PostAsync<SendResult>(apiConfig.SendSingleMessagePath, notification);
            return results;
        }

        private ApiConfig GetApiConfig(BotChannel botChannel) =>
            botSettings.ApiConfigs.Find(x => x.Channel == botChannel) ?? throw new ArgumentException($"Missing config for bot service type {botChannel}");
    }
}