namespace CRM.BackgroundJob.UnitTests.Testable
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CRM.BackgroundJob.Core.Services;
    using CRM.BackgroundJob.Models.AbnormalBet;
    using CRM.BackgroundJob.Services;
    using Fanex.CRM.BackgroundJob.Criteria;
    using Fanex.CRM.BackgroundJob.Models;
    using Fanex.CRM.BackgroundJob.Services;
    using Microsoft.Extensions.Options;

    public class FeedbackServiceTestable : FeedbackService
    {
        public FeedbackServiceTestable(IOptionsSnapshot<AppJobSettings> appSettings,
            IBaseService baseService,
            ISettingService settingService,
            IMessageService messageService,
            ICustomerService customerService,
            ICrmLogService logService,
            IFeedbackTicketVoidingService ticketVoidingService,
            ICommonFeedback commonFeedback)
            : base(appSettings,
                baseService,
                settingService,
                messageService,
                customerService,
                logService,
                ticketVoidingService,
                commonFeedback)
        {
        }

        public async Task Public_CreateFeedbackForCreditDepositSiteAsync(
            IEnumerable<Message> messages,
            IEnumerable<MatchInfo> matchInfos,
            IEnumerable<TransactionInfo> transactionInfos,
            IEnumerable<VoidingMessageUserName> messagesImportCombineDistinct,
             bool isUAT,
            AbnormalBetSystemSetting setting,
            IEnumerable<DuplicateTicketDetail> duplicateTicketDetails)
        {
            await base.CreateFeedbackForCreditDepositSiteAsync(messages, matchInfos, transactionInfos, messagesImportCombineDistinct, isUAT, setting, duplicateTicketDetails);
        }

        public async Task Public_CreateFeedbackForCreditSiteAsync(
            IEnumerable<Message> messages,
            MatchInfo matchInfo,
            IEnumerable<TransactionInfo> transactionInfos,
            IEnumerable<GroupHeadline> headlines,
            IEnumerable<SettingItem> settings,
            IEnumerable<SystemSettingMapping> settingMappings)
        {
            await base.CreateFeedbackForCreditSiteAsync(messages, matchInfo, transactionInfos, headlines, settings, settingMappings);
        }

        public async Task Public_CreateFeedbackForCreditSiteByBatchAsync(
            GroupHeadline headline,
            IEnumerable<int> relatedCustomerIds,
            IEnumerable<Message> messages,
            MatchInfo matchInfo,
            IEnumerable<TransactionInfo> transactionInfos,
            IEnumerable<SettingItem> settings,
            IEnumerable<SystemSettingMapping> settingMappings,
            CreateMutipleFeedbackCommand criteria)
        {
            await base.CreateFeedbackForCreditSiteByBatchAsync(
                headline,
                relatedCustomerIds,
                messages,
                matchInfo,
                transactionInfos,
                settings,
                settingMappings,
                criteria);
        }

        public async Task Public_CreateFeedbackForDepositSiteAsync(
            CustomerInfo licenseeInfo,
            IEnumerable<Message> messages,
            MatchInfo matchInfo,
            IEnumerable<TransactionInfo> transactionInfos,
            IEnumerable<SettingItem> settings,
            IEnumerable<SystemSettingMapping> settingMappings,
            IEnumerable<DuplicateTicketDetail> duplicateTicketDetails)
        {
            await base.CreateFeedbackForDepositSiteAsync(licenseeInfo, messages, matchInfo, transactionInfos, settings, settingMappings, duplicateTicketDetails);
        }

        public async Task Public_CreateFeedbackForDepositByBatchAsync(
            IEnumerable<Message> messages,
            IEnumerable<TransactionInfo> transactionInfos,
            string communicationTool,
            CreateFeedbackCommand criteria,
            IEnumerable<DuplicateTicketDetail> duplicateTicketDetails)
        {
            await base.CreateFeedbackForDepositByBatchAsync(messages, transactionInfos, communicationTool, criteria, duplicateTicketDetails);
        }

        public async Task Public_UpdateSystemSetting(bool isUAT, AbnormalBetSystemSetting setting, int maxVoidingHistoryCollectionId)
        {
            await base.UpdateSystemSetting(isUAT, setting, maxVoidingHistoryCollectionId);
        }

        public void Public_GetLicenseeManager(
            IEnumerable<TransactionInfo> transactionInfos,
            IEnumerable<SiteSettingMapping> siteSettingMappings)
        {
            base.GetLicenseeManager(transactionInfos, siteSettingMappings);
        }

        public SettingItem Public_GetCommunicationTool(IEnumerable<SettingItem> settings)
        {
            return base.GetCommunicationTool(settings, 1);
        }

        public string Public_GetProductXml(
            bool isMixParlay,
            MatchInfo matchInfo,
            IEnumerable<TransactionInfo> transactionInfos,
            IEnumerable<SettingItem> settings,
            IEnumerable<SystemSettingMapping> settingMappings)
        {
            return base.GetProductXml(isMixParlay, matchInfo, transactionInfos, settings, settingMappings);
        }

        public SettingItem Public_GetSportsbookProduct(IEnumerable<SettingItem> settings)
        {
            return base.GetSportsbookProduct(settings);
        }

        public IList<SettingItem> Public_GetSubProduct(
            bool isMixParlay,
            int productId,
            MatchInfo matchInfo,
            IEnumerable<SettingItem> settings,
            IEnumerable<SystemSettingMapping> settingMappings)
        {
            return base.GetSubProduct(isMixParlay, productId, matchInfo, settings, settingMappings);
        }

        public IList<SettingItem> Public_GetBettype(
            bool isMixParlay,
            IEnumerable<int> subProducts,
            IEnumerable<TransactionInfo> transactionInfos,
            IEnumerable<SettingItem> settings,
            IEnumerable<SystemSettingMapping> settingMappings)
        {
            return base.GetBettype(isMixParlay, subProducts, transactionInfos, settings, settingMappings);
        }

        public IList<SettingItem> Public_GetBettypeParlay(
            IEnumerable<int> subProducts,
            IEnumerable<SettingItem> settings,
            IEnumerable<SystemSettingMapping> settingMappings)
        {
            return base.GetBettypeParlay(subProducts, settings, settingMappings);
        }

        public Tuple<int, int, string> Public_GetCategorySubCategory(IEnumerable<SettingItem> settings, int IssueTypeId)
        {
            return base.GetCategorySubCategory(settings, IssueTypeId);
        }

        public string Public_GetRelatedCustomerXml(int headlineId, IEnumerable<TransactionInfo> transactionInfos, int IssueTypeId)
        {
            return base.GetRelatedCustomerXml(headlineId, transactionInfos, IssueTypeId);
        }

        public string Public_GetAutoField(
            DateTime feedbackDate,
            string communicationTool,
            IEnumerable<string> licenseeManagers)
        {
            return base.GetAutoField(feedbackDate, communicationTool, licenseeManagers);
        }

        public string Public_GetDescriptionForDepositeSite(string messageId, string requestIds)
        {
            return base.GetDescriptionForDepositeSite(messageId, requestIds);
        }

        public string Public_GetDescriptionForCreditSite(string messageId, string requestIds, MatchInfo matchInfo, IEnumerable<TransactionInfo> transactionInfos)
        {
            return base.GetDescriptionForCreditSite(messageId, requestIds, matchInfo, transactionInfos);
        }

        public string Public_GetDescriptionMixParlayForCreditSite(string messageId, string requestIds, MatchInfo matchInfo)
        {
            return base.GetDescriptionMixParlayForCreditSite(messageId, requestIds, matchInfo);
        }

        public string Public_GetSolution(Message message)
        {
            return base.GetSolution(message);
        }

        public string Public_GetSolutionForCreditSite(Message message, IEnumerable<TransactionInfo> transactionInfos, int issueTypeId)
        {
            return base.GetSolutionForCreditSite(message, transactionInfos, issueTypeId);
        }

        public string Public_GetSolutionMixParlayForCreditSite(Message message, IEnumerable<TransactionInfo> transactionInfos, bool isVip)
        {
            return base.GetSolutionMixParlayForCreditSite(message, transactionInfos, isVip);
        }
    }
}