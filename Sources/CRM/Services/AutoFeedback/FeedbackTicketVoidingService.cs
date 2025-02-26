namespace CRM.Services.AutoFeedback;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using CRM.Constants;
using CRM.Criteria.AutoFeedback;
using CRM.Models.AbnormalBet;
using CRM.Models.Common;
using CRM.Services.Common;
using Microsoft.Extensions.Configuration;

public interface IFeedbackTicketVoidingService
{
    Task<ValueTuple<IEnumerable<MatchInfo>, IEnumerable<TransactionInfo>, IEnumerable<DuplicateTicketDetail>, string>> GetTicketVoidingInfoAsync(
        IEnumerable<VoidingMessageUserName> voidingMessageUserNames,
        AbnormalBetSystemSetting setting);

    Task<ValueTuple<IEnumerable<MatchInfo>, IEnumerable<TransactionInfo>>> ImportTicketVoidingInfoAsync(
        IEnumerable<VoidingMessageUserName> voidingMessageUserNames,
        IEnumerable<MatchInfo> matchInfo,
        IEnumerable<TransactionInfo> transactionInfo,
        IEnumerable<SiteSettingMapping> siteSettingMappings,
        IEnumerable<GroupHeadline> headlines,
        CustomerInfo licenseeInfo);

    Task<CRM.Models.Common.CommandResult> EditFeedbackInfoAsync(
        string messageIds,
        IEnumerable<int> requestIds,
        IEnumerable<int> custIds,
        string status,
        string description);

    Task<IEnumerable<TransactionInfo>> GetAbnormalTicketsByTransAsync(AbnormalBetSystemSetting setting, string checkTrans);
}

public class FeedbackTicketVoidingService(
    IBaseService baseService,
    IMessageService messageService,
    ICrmLogService logService,
    IConfiguration configuration)
    : IFeedbackTicketVoidingService
{
    private readonly string Environment = string.IsNullOrEmpty(configuration.GetValue<string>("SiteEnvironment")) ? DefaultEnvironment : configuration.GetValue<string>("SiteEnvironment");

    private const string DefaultEnvironment = BackgroundJobConstants.DefaultEnvironment;

    public async Task<ValueTuple<IEnumerable<MatchInfo>, IEnumerable<TransactionInfo>, IEnumerable<DuplicateTicketDetail>, string>> GetTicketVoidingInfoAsync(
        IEnumerable<VoidingMessageUserName> voidingMessageUserNames,
        AbnormalBetSystemSetting setting)
    {
        const string XmlTemplate = "<r VoidingHistoryCollectionId='{0}' MsgID='{1}' ReqID='{2}' UsernameList='{3}' CustIdList='{4}' />";
        var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);

        foreach (var voidingMessageUserName in voidingMessageUserNames)
        {
            stringBuilder.AppendFormat(XmlTemplate,
                voidingMessageUserName.VoidingHistoryCollectionId,
                voidingMessageUserName.MessageId,
                voidingMessageUserName.RequestId,
                voidingMessageUserName.UsernameList,
                voidingMessageUserName.CustIdList
            );
        }

        stringBuilder.Append(BackgroundJobConstants.EndRootTag);

        var criteria = new GetAbnormalTicketInfoCriteria
        {
            MessageIds = stringBuilder.ToString(),
            ConfigUAT = setting.ConfigUAT,
            ConfigPRO = setting.ConfigPRO,
            Environment = Environment
        };
        var (matchInfo, transactionInfo, duplicateTicketDetails) = await baseService.FetchMultipleAsync<MatchInfo, TransactionInfo, DuplicateTicketDetail>(criteria);

        var collectionId = matchInfo.Where(x => transactionInfo.All(y => x.VoidingHistoryCollectionId != y.VoidingHistoryCollectionId))
            .Select(x => x.VoidingHistoryCollectionId).ToList();
        if (collectionId.Any())
        {
            var collectionIds = voidingMessageUserNames.Where(x => collectionId.Exists(y => x.VoidingHistoryCollectionId == y));
            await messageService.EditStatusVoidingMessageAsync(collectionIds, Environment);
            logService.LogInformation($"Don't have ticket info: {string.Join(",", collectionIds)}");
        }

        return new ValueTuple<IEnumerable<MatchInfo>, IEnumerable<TransactionInfo>,
            IEnumerable<DuplicateTicketDetail>, string>(matchInfo, transactionInfo, duplicateTicketDetails, criteria.TranIDs);
    }

    public async Task<(IEnumerable<MatchInfo>, IEnumerable<TransactionInfo>)> ImportTicketVoidingInfoAsync(
        IEnumerable<VoidingMessageUserName> voidingMessageUserNames,
        IEnumerable<MatchInfo> matchInfo,
        IEnumerable<TransactionInfo> transactionInfo,
        IEnumerable<SiteSettingMapping> siteSettingMappings,
        IEnumerable<GroupHeadline> headlines,
        CustomerInfo licenseeInfo)
    {
        logService.LogInformation("Abnormal Bet - Begin import ticket info");
        var xml = GetXmlImportTicketFeedback(matchInfo, transactionInfo, siteSettingMappings, headlines, licenseeInfo);

        var criteria = new ImportFeedbackInfoCriteria
        {
            Xml = xml,
            Environment = Environment
        };
        var feedbackInfo = await baseService.GetDataAsync<AutoFeedbackInfo>(criteria);

        var voidingHistoryCollectionIds = matchInfo.Where(x => feedbackInfo.All(y => y.VoidingHistoryCollectionId != x.VoidingHistoryCollectionId))
            .Select(x => x.VoidingHistoryCollectionId).ToList();
        if (voidingHistoryCollectionIds.Any())
        {
            var messages = voidingMessageUserNames.Where(x => voidingHistoryCollectionIds.Exists(y => x.VoidingHistoryCollectionId == y));
            await messageService.EditStatusVoidingMessageAsync(messages, Environment);
            logService.LogInformation(
                $"Abnormal Bet - Don't have ticket info or created: {string.Join(",", voidingHistoryCollectionIds)}");
        }

        var resultMatchInfo = matchInfo.Where(x => feedbackInfo.Any(y => y.VoidingHistoryCollectionId == x.VoidingHistoryCollectionId) && x.VoidingHistoryCollectionId != 0);
        var resultTransactionInfo = transactionInfo.Where(x => feedbackInfo.Any(y => y.VoidingHistoryCollectionId == x.VoidingHistoryCollectionId
            && y.CustId == x.CustId) && x.VoidingHistoryCollectionId != 0);
        logService.LogInformation("Abnormal Bet - End import ticket info");
        return (resultMatchInfo, resultTransactionInfo);
    }

#pragma warning disable S3776
#pragma warning disable S1541

    protected internal virtual string GetXmlImportTicketFeedback(
        IEnumerable<MatchInfo> matchInfos,
        IEnumerable<TransactionInfo> transactionInfos,
        IEnumerable<SiteSettingMapping> siteSettingMappings,
        IEnumerable<GroupHeadline> headlines,
        CustomerInfo licenseeInfo)
#pragma warning restore S3776
#pragma warning restore S1541
    {
        const string XmlTemplate = "<r msgid='{0}' reqid='{1}' custid='{2}' hl='{3}' related='{4}' event='{5}' IssueTypeId='{6}' />";
        const int duplicateType = 20;
        var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);
        var transactionInfosDistinct = transactionInfos.Select(x => new
        {
            x.CustId,
            x.MessageId,
            x.RequestId,
            x.Username,
            x.IsDeposit,
            x.Site
        }).Distinct();
        foreach (var transaction in transactionInfosDistinct)
        {
            var matchInfo = matchInfos.FirstOrDefault(x => x.MessageId.Contains(transaction.MessageId.ToString()));
            int headlineId;
            int relatedCustomerId;

            if (transaction.IsDeposit)
            {
                var licenseeManager = GetLicenseeManager(siteSettingMappings, transaction.Site);
                headlineId = licenseeInfo.CustId;
                relatedCustomerId = licenseeManager?.LicenseeManagerId ?? 0;
            }
            else
            {
                var headline = headlines.FirstOrDefault(x => x.CustId == transaction.CustId);
                headlineId = headline?.HeadlineId ?? 0;
                relatedCustomerId = transaction.CustId;
            }
            if (transaction.IsDeposit || (matchInfo?.IssueTypeId != duplicateType))
            {
                var transactionInfo = new TransactionInfo
                {
                    CustId = transaction.CustId,
                    RequestId = transaction.RequestId,
                    MessageId = transaction.MessageId
                };

                GetXmlTemplate(stringBuilder, XmlTemplate, transactionInfo, matchInfo, headlineId,
                    transaction.IsDeposit && matchInfo?.IssueTypeId == duplicateType ? transaction.CustId : relatedCustomerId);
            }
        }

        stringBuilder.Append(BackgroundJobConstants.EndRootTag);
        return stringBuilder.ToString();
    }

    public async Task<CRM.Models.Common.CommandResult> EditFeedbackInfoAsync(
        string messageIds,
        IEnumerable<int> requestIds,
        IEnumerable<int> custIds,
        string status,
        string description)
    {
        var criteria = new EditFeedbackInfoCommand
        {
            MessageId = messageIds,
            RequestIds = string.Join(",", requestIds),
            CustIds = string.Join(",", custIds),
            Status = status,
            Description = description,
            Environment = Environment
        };
        return await baseService.ExecuteNonQueryAsync(criteria);
    }

    private static SiteSettingMapping GetLicenseeManager(IEnumerable<SiteSettingMapping> siteSettingMappings, string site)
    {
        return siteSettingMappings.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.SettingSource)
                                                       && x.SettingSource.Equals(site, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IEnumerable<TransactionInfo>> GetAbnormalTicketsByTransAsync(AbnormalBetSystemSetting setting, string checkTrans)
    {
        var criteria = new GetAbnormalTicketsCheckCriteria
        {
            ConfigPRO = setting.ConfigPRO,
            ConfigUAT = setting.ConfigUAT,
            Environment = Environment,
            TranIDs = checkTrans
        };
        return await baseService.GetDataFromSiteDbAsync<TransactionInfo>(criteria);
    }

    private static void GetXmlTemplate(StringBuilder stringBuilder, string xmlTemplate,
        TransactionInfo transaction, MatchInfo matchInfo, int headlineId, int relatedCustId)
    {
        stringBuilder.AppendFormat(xmlTemplate,
            transaction.MessageId,
            transaction.RequestId,
            transaction.CustId,
            headlineId,
            relatedCustId,
            $"{HttpUtility.HtmlEncode(matchInfo?.HomeName)} -vs- {HttpUtility.HtmlEncode(matchInfo?.AwayName)}",
            matchInfo?.IssueTypeId);
    }
}