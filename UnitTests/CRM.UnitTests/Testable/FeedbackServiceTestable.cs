namespace CRM.UnitTests.Testable;

using CRM.Criteria.AutoFeedback;
using CRM.Models.AbnormalBet;
using CRM.Models.Common;
using CRM.Services.AutoFeedback;
using CRM.Services.Common;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class FeedbackServiceTestable(
    IOptionsSnapshot<AppJobSettings> appSettings,
    IBaseService baseService,
    ISettingService settingService,
    IMessageService messageService,
    ICustomerService customerService,
    ICrmLogService logService,
    IFeedbackTicketVoidingService ticketVoidingService,
    ICommonFeedback commonFeedback)
    : FeedbackService(appSettings,
        baseService,
        settingService,
        messageService,
        customerService,
        logService,
        ticketVoidingService,
        commonFeedback)
{
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
        CreateMultipleFeedbackCommand criteria)
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

    public IList<SettingItem> Public_GetBetType(
        bool isMixParlay,
        IEnumerable<int> subProducts,
        IEnumerable<TransactionInfo> transactionInfos,
        IEnumerable<SettingItem> settings,
        IEnumerable<SystemSettingMapping> settingMappings)
    {
        return base.GetBetType(isMixParlay, subProducts, transactionInfos, settings, settingMappings);
    }

    public IList<SettingItem> Public_GetBetTypeParlay(
        IEnumerable<int> subProducts,
        IEnumerable<SettingItem> settings,
        IEnumerable<SystemSettingMapping> settingMappings)
    {
        return base.GetBetTypeParlay(subProducts, settings, settingMappings);
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

    public string Public_GetDescriptionForDepositSite(string messageId, string requestIds)
    {
        return base.GetDescriptionForDepositSite(messageId, requestIds);
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