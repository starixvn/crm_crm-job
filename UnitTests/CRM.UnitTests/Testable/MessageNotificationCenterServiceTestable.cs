namespace CRM.UnitTests.Testable
{
    using CRM.Models.Common;
    using CRM.Models.MessageNotificationCenter;
    using CRM.Services.AutoFeedback;
    using CRM.Services.Common;
    using CRM.Services.NotificationCenter;
    using Fanex.BotService.Client.Core.Services.Interfaces;
    using Microsoft.Extensions.Configuration;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class MessageNotificationCenterServiceTestable(
        ICrmLogService logService,
        IConfiguration configuration,
        ICommonFeedback commonFeedback,
        ICaptureSettlementDataService captureSettlementDataService,
        IGenerateResultAmendmentService generateResultAmendmentService,
        IPushNotificationResultAmendmentService pushNotificationResultAmendmentService,
        IGenerateLapNotificationService generateLapNotificationService,
        ISendNotificationCenterService sendNotificationCenterService,
        ISystemSettingService systemSettingService,
        IBotService botService,
        BotServiceSetting botSetting)
        : MessageNotificationCenterService(logService,
            configuration,
            commonFeedback,
            captureSettlementDataService,
            generateResultAmendmentService,
            pushNotificationResultAmendmentService,
            generateLapNotificationService,
            sendNotificationCenterService,
            systemSettingService,
            botService,
            botSetting)
    {
        public string Public_GetResourceRemotingRef(string type, string id, string languageCode)
        {
            return GetResourceRemotingRef(type, id, languageCode);
        }

        public async Task Public_ProcessNotificationItemAsync(IEnumerable<MessageBotInfo> messages)
        {
            await ProcessNotificationItemAsync(messages);
        }

        public async Task Public_ProcessMessagesAsync(List<ConversationConfig> conversationConfigs, string content, byte isTest)
        {
            await ProcessMessagesAsync(conversationConfigs, content, isTest);
        }

        public async Task Public_ProcessSendRealTestMessagesAsync(List<ConversationConfig> conversationConfigs, string content)
        {
            await ProcessSendRealTestMessagesAsync(conversationConfigs, content);
        }

        public async Task Public_ProcessSendOnlyTestMessagesAsync(List<ConversationConfig> conversationConfigs, string content)
        {
            await ProcessSendOnlyTestMessagesAsync(conversationConfigs, content);
        }
    }
}