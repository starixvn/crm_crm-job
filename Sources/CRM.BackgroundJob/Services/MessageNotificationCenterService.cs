namespace CRM.BackgroundJob.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Constants;
using CRM.BackgroundJob.Criteria.PushResultAmendment;
using CRM.BackgroundJob.Models;
using CRM.BackgroundJob.Models.MessageNotificationCenter;
using CRM.Services;
using Fanex.BotService.Client.Core.Services.Interfaces;
using Microsoft.Extensions.Configuration;

public interface IMessageNotificationCenterService
{
    Task CaptureSettlementDataAsync();

    Task GenerateResultAmendmentAsync();

    Task PushNotificationResultAmendmentAsync();

    Task GenerateLapNotificationAsync();

    Task SendNotificationCenterAsync();
}

#pragma warning disable S134
#pragma warning disable S107
public class MessageNotificationCenterService(
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
    : IMessageNotificationCenterService
{
    private readonly string Environment = string.IsNullOrEmpty(configuration.GetValue<string>("SiteEnvironment")) ? DefaultEnvironment : configuration.GetValue<string>("SiteEnvironment");

    private const string DefaultEnvironment = BackgroundJobConstants.DefaultEnvironment;

    public async Task CaptureSettlementDataAsync()
    {
        try
        {
            var serviceFlag = await systemSettingService.GetSystemSettingNotificationAsync(Environment);
            if (!serviceFlag)
            {
                logService.LogInformation("Service Capture Settlement is shutdown.");
                return;
            }

            var initSiteDbConfigs = await captureSettlementDataService.GetInitialSiteDbConfigsAsync(Environment);
            if (initSiteDbConfigs.Any())
            {
                foreach (var item in initSiteDbConfigs)
                {
                    var sessionResultHistorys = await captureSettlementDataService.GetSessionByDateAsync(item);
                    var sessionResultHistory = sessionResultHistorys.FirstOrDefault();

                    if (sessionResultHistory != null)
                    {
                        await captureSettlementDataService.InsertDataLoadAsync(sessionResultHistory.Environment, sessionResultHistory.JSSettlResultHist);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await logService.ErrorAsync(ex);
        }
    }

    protected string GetResourceRemotingRef(string type, string id, string languageCode)
    {
        return type switch
        {
            "LeagueID" => commonFeedback.GetLeagueName(id, languageCode),
            "SportType" => commonFeedback.GetSportName(id, languageCode),
            "TeamID" => commonFeedback.GetTeamName(id, languageCode),
            "ResourceCricket" => commonFeedback.GetResourceByRefIdAndResourceId("ResourceCricket", id, languageCode),
            "ResultSettlement" => commonFeedback.GetResourceByRefIdAndResourceId("ResultSettlement", id, languageCode),
            "ResourceBasketball" => commonFeedback.GetResourceByRefIdAndResourceId("ResourceBasketball", id, languageCode),
            "ResourceESports" => commonFeedback.GetResourceByRefIdAndResourceId("ResourceESports", id, languageCode),
            _ => string.Empty
        };
    }

    public async Task GenerateResultAmendmentAsync()
    {
        try
        {
            var serviceFlag = await systemSettingService.GetSystemSettingNotificationAsync(Environment);
            if (!serviceFlag)
            {
                logService.LogInformation("Service Generate ResultAmendment is shutdown.");
                return;
            }

            var settlementGen = await generateResultAmendmentService.GetPrepareGenNotificationAsync(Environment);
            if (settlementGen.Any())
            {
                foreach (var item in settlementGen)
                {
                    item.RefValueEN = GetResourceRemotingRef(item.RefId1, item.RefId2, Language.en_US);
                    item.RefValueCH = GetResourceRemotingRef(item.RefId1, item.RefId2, Language.zh_TW);
                    item.RefValueCS = GetResourceRemotingRef(item.RefId1, item.RefId2, Language.zh_CN);
                    item.RefValueZHCN = GetResourceRemotingRef(item.RefId1, item.RefId2, Language.zh_CHS);
                }
                await generateResultAmendmentService.InsertGenResultAmendmentAsync(Environment, settlementGen);
            }
        }
        catch (Exception ex)
        {
            await logService.ErrorAsync(ex);
        }
    }

    public async Task PushNotificationResultAmendmentAsync()
    {
        try
        {
            var serviceFlag = await systemSettingService.GetSystemSettingNotificationAsync(Environment);
            if (!serviceFlag)
            {
                logService.LogInformation("Service Push Notification ResultAmendment is shutdown.");
                return;
            }

            var mncNotifications = await pushNotificationResultAmendmentService.GetPrepareNotificationAsync(Environment);
            if (mncNotifications.Any())
            {
                foreach (var item in mncNotifications)
                {
                    var command = new GetMessageResultAmendmentCommand
                    {
                        NotificationID = item.NotificationID,
                        JSNotification = item.JSNotification
                    };
                    await pushNotificationResultAmendmentService.GetMessageResultAmendmentAsync(command);
                    var (isTest, approveMode) = await pushNotificationResultAmendmentService
                        .EditPushNotificationResultAmendmentAsync(item.NotificationID, command.MessageID, Environment, "AfterInsMsg");
                    var adminMessage = await pushNotificationResultAmendmentService.EditPushMessageResultAmendmentAsync(command.MessageID, "AfterUpdNoti", isTest, approveMode);
                    if (adminMessage != null)
                    {
                        await pushNotificationResultAmendmentService.InsertAdminMessageSiteAsync(adminMessage);
                    }
                }
                await pushNotificationResultAmendmentService.InsertCompleteResultAmendmentAsync(Environment);
            }
        }
        catch (Exception ex)
        {
            await logService.ErrorAsync(ex);
        }
    }

    public async Task GenerateLapNotificationAsync()
    {
        try
        {
            var serviceFlag = await systemSettingService.GetSystemSettingNotificationAsync(Environment);
            if (!serviceFlag)
            {
                logService.LogInformation("Service Generate Lap Notification is shutdown.");
                return;
            }

            var settlementGenLap = await generateLapNotificationService.GetPrepareGenLapNotificationAsync(Environment);
            if (settlementGenLap.Any())
            {
                await generateLapNotificationService.InsertGenLapResultAmendmentAsync(settlementGenLap, Environment);
            }
        }
        catch (Exception ex)
        {
            await logService.ErrorAsync(ex);
        }
    }

    public async Task SendNotificationCenterAsync()
    {
        try
        {
            var serviceFlag = await systemSettingService.GetSystemSettingNotificationAsync(Environment);
            if (!serviceFlag)
            {
                logService.LogInformation("Service Send Notification Center is shutdown.");
                return;
            }

            await sendNotificationCenterService.InsertNotificationAlertAsync(Environment);
            var messages = await sendNotificationCenterService.GetNotificationAlertAsync(Environment);

            if (messages.Any())
            {
                await ProcessNotificationItemAsync(messages);
            }
        }
        catch (Exception ex)
        {
            await logService.ErrorAsync(ex);
        }
    }

    protected async Task ProcessNotificationItemAsync(IEnumerable<MessageBotInfo> messages)
    {
        foreach (var item in messages)
        {
            if (!string.IsNullOrWhiteSpace(item.Content))
            {
                var conversationConfigs = botSetting.ConversationConfigs
                    .FindAll(config => config.DisplayName == item.ChatGroupID).ToList();

                await ProcessMessagesAsync(conversationConfigs, item.Content, item.IsTest);
            }
        }
    }

    protected async Task ProcessMessagesAsync(List<ConversationConfig> conversationConfigs, string content, byte isTest)
    {
        if (conversationConfigs.Any())
        {
            if (isTest == 0)
            {
                await ProcessSendRealTestMessagesAsync(conversationConfigs, content);
            }
            else if (isTest == 1)
            {
                await ProcessSendOnlyTestMessagesAsync(conversationConfigs, content);
            }
            else
            {
                throw new InvalidOperationException($"Unexpected value of isTest: {isTest}");
            }
        }
    }

    protected async Task ProcessSendRealTestMessagesAsync(List<ConversationConfig> conversationConfigs, string content)
    {
        foreach (var conversationConfig in conversationConfigs)
        {
            if (conversationConfig.DisplayName == BackgroundJobConstants.SYS_IT)
            {
                await logService.ErrorMessageAsync(content);
            }
            else
            {
                await botService.SendSkypeAsync(conversationConfig.ConversationId, content);
            }
        }
    }

    protected async Task ProcessSendOnlyTestMessagesAsync(List<ConversationConfig> conversationConfigs, string content)
    {
        foreach (var conversationConfig in conversationConfigs)
        {
            if (conversationConfig.IsOnlySendTest)
            {
                await botService.SendSkypeAsync(conversationConfig.ConversationId, content);
            }
        }
    }
}
#pragma warning restore S134
#pragma warning restore S107