namespace Fanex.CRM.BackgroundJob.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.CRM.BackgroundJob.Constants;
    using Fanex.CRM.BackgroundJob.Models;
    using global::CRM.BackgroundJob.Core.Services;
    using global::CRM.BackgroundJob.Models.AbnormalBet;
    using global::CRM.BackgroundJob.Models.ChatBotCTO;
    using global::CRM.BackgroundJob.Models.ChatBotCWL;
    using global::CRM.BackgroundJob.Models.ChatBotProblemsAccount;
    using global::CRM.BackgroundJob.Models.ChatBotResulAmendment;
    using global::CRM.BackgroundJob.Services;
    using Microsoft.Extensions.Configuration;

    public interface IChatBotJobService
    {
        Task PullMessageAndSendNotificationAsync();

        Task PullMessageCWLAndSendNotificationToHeadlineAsync();

        Task PullMessageCTOAndSendNotificationToHeadlineAsync();

        Task PullMessageProblemsAccountAndSendNotificationToHeadlineAsync();

        Task PullMessageResultAmendmentAndSendNotificationToHeadlineAsync();

        Task PullMessagArchivingDAAndSendNotificationToHeadlineAsync();

        Task PullMessageRebateRetentionAsync();
    }

    public class ChatBotJobService : IChatBotJobService
    {
        private readonly ICrmLogService logService;
        private readonly ISystemSettingService systemSettingService;
        private readonly IChatBotTicketVoidingService ticketVoidingService;
        private readonly ISendNotificationService sendNotificationService;
        private readonly IChatBotCWLService chatBotCWLService;
        private readonly IChatBotCTOService chatBotCTOService;
        private readonly IChatBotProblemAccountService chatBotProblemAccountService;
        private readonly IChatBotResultAmendmentService chatBotResultAmendmentService;
        private readonly IChatBotArchivingDAService chatBotArchivingDAService;
        private readonly IChatBotRebateRetentionService chatBotRebateRetentionService;
        private readonly IBaseService baseService;
        private readonly string Environment;

        private const string DefaultEnvironment = BackgroundJobConstants.DefaultEnvironment;

#pragma warning disable S107 // Methods should not have too many parameters

        public ChatBotJobService(
            ICrmLogService logService,
            ISystemSettingService systemSettingService,
            IChatBotTicketVoidingService ticketVoidingService,
            ISendNotificationService sendNotificationService,
            IChatBotCWLService chatBotCWLService,
            IConfiguration configuration,
            IChatBotCTOService chatBotCTOService,
            IChatBotProblemAccountService chatBotProblemAccountService,
            IChatBotResultAmendmentService chatBotResultAmendmentService,
            IChatBotArchivingDAService chatBotArchivingDAService,
            IChatBotRebateRetentionService chatBotRebateRetentionService,
            IBaseService baseService
            )
#pragma warning restore S107 // Methods should not have too many parameters
        {
            this.logService = logService;
            this.systemSettingService = systemSettingService;
            this.ticketVoidingService = ticketVoidingService;
            this.sendNotificationService = sendNotificationService;
            this.chatBotCWLService = chatBotCWLService;
            this.chatBotCTOService = chatBotCTOService;
            this.chatBotProblemAccountService = chatBotProblemAccountService;
            this.chatBotResultAmendmentService = chatBotResultAmendmentService;
            this.chatBotArchivingDAService = chatBotArchivingDAService;
            this.chatBotRebateRetentionService = chatBotRebateRetentionService;
            this.baseService = baseService;
            Environment = string.IsNullOrEmpty(configuration.GetValue<string>("SiteEnvironment")) ? DefaultEnvironment : configuration.GetValue<string>("SiteEnvironment");
        }

        public async Task PullMessageAndSendNotificationAsync()
        {
            try
            {
                AbnormalBetSystemSetting setting = await systemSettingService.GetChatBotSystemSettingAsync(BackgroundJobConstants.ServiceName_ChatBot, Environment);
                if (!setting.ServiceFlag)
                {
                    logService.LogInformation("ChatBot - AbnormalBets Service is shutdown.");
                    return;
                }

                var voidingCollectInfo = await ticketVoidingService.GetVoidingMessageAsync(setting.LastVoidingHistoryCollectionId, setting.BatchSize);
                var voidingBetIdInfo = await ticketVoidingService.GetVoidingBetIdMessageAsync(setting.LastMsgTargetId, setting.BatchSize);
                var ticketInfos = await ticketVoidingService.GetTicketVoidingInfoAsync(voidingCollectInfo, voidingBetIdInfo, Environment, setting.IncludeUUSCurrency);
                IEnumerable<Message> messages = await ticketVoidingService.GetMessageInfoAsync(ticketInfos.Item2);

                var commandResultImportTicket = await ticketVoidingService.ImportTicketVoidingAsync(
                    messages,
                    ticketInfos.Item1,
                    ticketInfos.Item3,
                    ticketInfos.Item4,
                    Environment);

                if (commandResultImportTicket.Item1.IsSuccessful)
                {
                    logService.LogInformation("Import ChatBot - AbnormalBets Successful!!!");
                    await sendNotificationService.ScanAndSendMessageToBotEndpointAsync();
                }
                else
                {
                    await logService.ErrorAsync(commandResultImportTicket.Item1, commandResultImportTicket.Item2);
                    logService.LogInformation(string.Format("Import ChatBot - AbnormalBets Fail: {0}", commandResultImportTicket.Item1.Message));
                }
            }
            catch (Exception ex)
            {
                await logService.ErrorAsync(ex);
            }
        }

        public async Task PullMessageCWLAndSendNotificationToHeadlineAsync()
        {
            try
            {
                AbnormalBetSystemSetting setting = await systemSettingService.GetChatBotSystemSettingAsync(BackgroundJobConstants.ServiceName_ChatBot_CWL, Environment);
                if (!setting.ServiceFlag)
                {
                    logService.LogInformation("Chat Bot CWL Service is shutdown.");
                    return;
                }
                IEnumerable<ChatBotCWLInfo> cwlInfors = await chatBotCWLService.GetChatbotCWLLogInfoAsync(setting.LastLogId, setting.BatchSize);
                var result = await chatBotCWLService.ImportCWLLogInfoAsync(cwlInfors, Environment);
                if (result.Item1.IsSuccessful)
                {
                    await sendNotificationService.ScanAndSendMessageToBotEndpointAsync();
                }
                else
                {
                    logService.LogInformation(result.Item1.Message);
                    await logService.ErrorAsync(result.Item1, result.Item2);
                }
            }
            catch (Exception ex)
            {
                await logService.ErrorAsync(ex);
            }
        }

        public async Task PullMessageCTOAndSendNotificationToHeadlineAsync()
        {
            try
            {
                AbnormalBetSystemSetting settingCTO = await systemSettingService.GetChatBotSystemSettingAsync(BackgroundJobConstants.ServiceName_ChatBot_TOControl, Environment);
                if (!settingCTO.ServiceFlag)
                {
                    logService.LogInformation("Chat Bot CTO Service is shutdown.");
                    return;
                }
                IEnumerable<TOReachInfor> tOReachInfors = await chatBotCTOService.GetChatbotCTOSettingInfoAsync(settingCTO.LastLogId, settingCTO.BatchSize);
                IEnumerable<TOSMAInfor> tOSMAInfors = await chatBotCTOService.GetChatbotCTOSMAInfoAsync(tOReachInfors);
                var commandResult = await chatBotCTOService.ImportTOSMAInfoAsync(tOSMAInfors, Environment);
                if (commandResult.Item1.IsSuccessful)
                {
                    await sendNotificationService.ScanAndSendMessageToBotEndpointAsync();
                }
                else
                {
                    logService.LogInformation(commandResult.Item1.Message);
                    await logService.ErrorAsync(commandResult.Item1, commandResult.Item2);
                }
            }
            catch (Exception ex)
            {
                await logService.ErrorAsync(ex);
            }
        }

        public async Task PullMessageProblemsAccountAndSendNotificationToHeadlineAsync()
        {
            try
            {
                AbnormalBetSystemSetting settingProblemsAccount = await systemSettingService.GetChatBotSystemSettingAsync(BackgroundJobConstants.ServiceName_ChatBot_ProblemAccounts, Environment);
                if (!settingProblemsAccount.ServiceFlag)
                {
                    logService.LogInformation("Chat Bot Problems Account Service is shutdown.");
                    return;
                }
                IEnumerable<ChatBotProblemsAccountInfor> problemsAcountInfors = await chatBotProblemAccountService.GetChatbotProblemAccountInfoAsync(
                    settingProblemsAccount.LastLogId, settingProblemsAccount.BatchSize);
                var commandResult = await chatBotProblemAccountService.ImportProblemsAccountInfoAsync(problemsAcountInfors, Environment);
                if (commandResult.Item1.IsSuccessful)
                {
                    await sendNotificationService.ScanAndSendMessageToBotEndpointAsync();
                }
                else
                {
                    logService.LogInformation(commandResult.Item1.Message);
                    await logService.ErrorAsync(commandResult.Item1, commandResult.Item2);
                }
            }
            catch (Exception ex)
            {
                await logService.ErrorAsync(ex);
            }
        }

        public async Task PullMessageResultAmendmentAndSendNotificationToHeadlineAsync()
        {
            try
            {
                AbnormalBetSystemSetting settingResultAmendment = await systemSettingService.GetChatBotSystemSettingAsync(BackgroundJobConstants.ServiceName_ChatBot_ResultAmendment, Environment);
                if (!settingResultAmendment.ServiceFlag)
                {
                    logService.LogInformation("Chat Bot Result Amendment Service is shutdown.");
                    return;
                }
                var maxMsgTargetId = settingResultAmendment.MaxMsgTargetId;
                var batchSizeGetMsg = settingResultAmendment.BatchSizeGetMsg;
                var (voiddingMessageInfo, voidingMesssageSettled) = await chatBotResultAmendmentService.GetChatBotMessageVoidSettledAsync(maxMsgTargetId, batchSizeGetMsg);

                IEnumerable<MessageVoidSettled> settledTickets = new List<MessageVoidSettled>();
                if (voidingMesssageSettled.Any())
                {
                    var siteDbConfigs = await baseService.GetSiteDbConfigAsync();

                    foreach (var item in siteDbConfigs)
                    {
                        var settledTicket = await chatBotResultAmendmentService.VoidingMessageGetSettledTicketAsync(voidingMesssageSettled, item.SiteDBGroupID);
                        settledTickets = settledTickets.Concat(settledTicket);
                    }
                }
                var settledTicketInfo = settledTickets.Concat(voiddingMessageInfo);
                var voidingMessageInfo = await chatBotResultAmendmentService.GetChatbotMessageVoidSettledInfoAsync(settledTicketInfo);
                var resultAmendmentInfo = await chatBotResultAmendmentService.GetChatBotResultAmendmentInfoAsync(settingResultAmendment);
                var messagesCombineDistinct = resultAmendmentInfo.Item1.Item3.Concat(voidingMessageInfo.Item1).Distinct().ToList();
                var messages = Enumerable.Empty<MessageContent>();
                if (messagesCombineDistinct.Any())
                {
                    messages = await chatBotResultAmendmentService.GetChatBotMessageInfoAsync(messagesCombineDistinct);
                }

                var commandResult = await chatBotResultAmendmentService.ImportChatBotAmendmentResultAsync(
                    resultAmendmentInfo.Item1.Item1, resultAmendmentInfo.Item1.Item2, messages,
                    voidingMessageInfo.Item2, voidingMessageInfo.Item3, Environment, resultAmendmentInfo.Item2
                    );

                if (commandResult.Item1.IsSuccessful)
                {
                    await sendNotificationService.ScanAndSendMessageToBotEndpointAsync();
                }
                else
                {
                    logService.LogInformation(commandResult.Item1.Message);
                    await logService.ErrorAsync(commandResult.Item1, commandResult.Item2);
                }
            }
            catch (Exception ex)
            {
                await logService.ErrorAsync(ex);
            }
        }

        public async Task PullMessagArchivingDAAndSendNotificationToHeadlineAsync()
        {
            try
            {
                AbnormalBetSystemSetting archivingDASetting = await systemSettingService.GetChatBotSystemSettingAsync(BackgroundJobConstants.ServiceName_ChatBot_ArchivedAccounts, Environment);
                if (!archivingDASetting.ServiceFlag)
                {
                    logService.LogInformation("Chat Bot Archiving Dormant Accounts Service is shutdown.");
                    return;
                }
                var archivingDAInfors = await chatBotArchivingDAService.GetChatbotArchivingDASettingInfoAsync(archivingDASetting.LastLogId, archivingDASetting.BatchSize);
                var (commandResult, command) = await chatBotArchivingDAService.ImportArchivedAccountsAsync(archivingDAInfors, Environment);
                if (commandResult.IsSuccessful)
                {
                    await sendNotificationService.ScanAndSendMessageToBotEndpointAsync();
                }
                else
                {
                    logService.LogInformation(commandResult.Message);
                    await logService.ErrorAsync(commandResult, command);
                }
            }
            catch (Exception ex)
            {
                await logService.ErrorAsync(ex);
            }
        }

        public async Task PullMessageRebateRetentionAsync()
        {
            try
            {
                AbnormalBetSystemSetting rrSetting = await systemSettingService.GetChatBotSystemSettingAsync(BackgroundJobConstants.ServiceName_ChatBot_RebateRetention, Environment);
                if (!rrSetting.ServiceFlag)
                {
                    logService.LogInformation("Chat Bot Rebate Retention Service is shutdown.");
                    return;
                }
                var (rrMessages, newLogSIDs) = await chatBotRebateRetentionService.GetRebateRetentionInfoAsync(rrSetting.LastLogSId);
                var (commandResult, command) = await chatBotRebateRetentionService.ImportRebateRetentionCommand(rrMessages, newLogSIDs, Environment);
                if (commandResult.IsSuccessful)
                {
                    await sendNotificationService.ScanAndSendMessageToBotEndpointAsync();
                }
                else
                {
                    logService.LogInformation(commandResult.Message);
                    await logService.ErrorAsync(commandResult, command);
                }
            }
            catch (Exception ex)
            {

                await logService.ErrorAsync(ex);
            }
        }
    }
}