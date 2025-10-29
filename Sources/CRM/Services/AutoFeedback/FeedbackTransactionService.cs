namespace CRM.Services.AutoFeedback;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using CRM.Constants;
using CRM.Criteria.AmendmentNotiFB;
using CRM.Criteria.ArchivingDA;
using CRM.Criteria.AutoFeedback;
using CRM.Criteria.ProblemsAccount;
using CRM.Criteria.TurnoverControl;
using CRM.Criteria.WinLossLimit;
using CRM.Models.AmendmentNotiFB;
using CRM.Models.ArchivingDA;
using CRM.Models.Common;
using CRM.Models.ProblemsAccount;
using CRM.Models.TurnoverControl;
using CRM.Models.WinLossLimit;
using CRM.Services.Common;

public interface IFeedbackTransactionService
{
    Task<ValueTuple<IEnumerable<WinLimitFeedBackInfo>, IEnumerable<BettypeInfo>, IEnumerable<SportInfo>>> ImportWinLimitFeedbackInfoAsync(
        IEnumerable<CustomerWinLimit> cusWinLimit,
        IEnumerable<GroupHeadline> headlines,
        string environment);

    Task<CRM.Models.Common.CommandResult> EditFeedbackWinLossInfoAsync(int LogID, string status, string description);

    Task<IEnumerable<TOControlFeedbackInfo>> ImportTOControlFeedbackInfoAsync(
        IEnumerable<GroupHeadline> headlines,
        IEnumerable<CustOutStanding> custOutStandings,
        IEnumerable<TONotification> custNotifications);

    Task<IEnumerable<ProblemsAccountAutoFeedbackInfo>> ImportProblemsAccountFeedbackInfoAsync(
        IEnumerable<GroupHeadline> headlines,
        IEnumerable<CustCategoryInfo> custCategories,
        string Environment);

    Task<CRM.Models.Common.CommandResult> EditTOControlFeedbackInfoAsync(string LogID, string status, string description);

    Task<CRM.Models.Common.CommandResult> EditProblemsAccountFeedbackInfoAsync(string LogID, string status, string description);

    Task<IEnumerable<AmendmentAutoFeedbackInfo>> GetAmendmentNotiAutoFeedbackInfo(int BatchSize);

    Task<CRM.Models.Common.CommandResult> InsertAmendmentNotiAutoFeedback(IEnumerable<CusReachAmendmentNotification> CustAmendments, string Environment);

    Task<CRM.Models.Common.CommandResult> EditAmendmentNotiAutoFeedback(int LogID, string status, string description);

    Task<IEnumerable<Message>> GetMessageContentInfoAsync(int messageId);

    Task<bool> GetArchivingDAStatus();

    Task<IEnumerable<ArchivingCustInfo>> GetArchivingCustInfo();

    Task<IEnumerable<ArchivingAutoFbInfo>> ArchivingAutoFeedbackInfoInsAsync(ArchivingDASetting setting, string environment);

    Task<CRM.Models.Common.CommandResult> ArchivingImportDirectAccountAsync(string archivedList, DateTime archivedDate);

    Task<CRM.Models.Common.CommandResult> EditArchivingDAAutoFeedback(int logFeedbackId, string status, string description);
}

public class FeedbackTransactionService(IBaseService baseService) : IFeedbackTransactionService
{
    public async Task<ValueTuple<IEnumerable<WinLimitFeedBackInfo>, IEnumerable<BettypeInfo>, IEnumerable<SportInfo>>> ImportWinLimitFeedbackInfoAsync(
        IEnumerable<CustomerWinLimit> cusWinLimit,
        IEnumerable<GroupHeadline> headlines,
        string environment)
    {
        const string XmlTemplate = "<r logid='{0}' custid='{1}' hl='{2}' hlname='{3}' related='{4}' product='{5}' group='{6}' level='{7}' " +
                                   "limitsetting='{8}' winamount='{9}' winamountRM='{10}' time='{11}' bettype='{12}' casinotype='{13}' isnormalgame='{14}'/>";
        var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);
        foreach (var limitInfo in cusWinLimit)
        {
            double settingAmount;
            var TotalWinAmt = limitInfo.TotalWinAmt;
            var TotalWinAmt_RM = limitInfo.TotalWinAmt_RM;
            var betTypeIdList = limitInfo.BettypeIDList ?? limitInfo.BettypeIDList;
            if (string.IsNullOrEmpty(limitInfo.MaxWinLimitSetting))
            {
                settingAmount = 0;
            }
            else
            {
                var element = XElement.Parse(limitInfo.MaxWinLimitSetting);
                var setting = new MaxWinLimitSetting
                {
                    Level1 = (double)element.Attribute("Level1"),
                    Level2 = (double)element.Attribute("Level2"),
                    Level3 = (double)element.Attribute("Level3"),
                    Level101 = (double)element.Attribute("Level101"),
                    Level102 = (double)element.Attribute("Level102")
                };
                settingAmount = GetLimitSetting(setting, limitInfo.LevelID);
            }

            var headline = headlines.FirstOrDefault(x => x.CustId == limitInfo.CustId);
            var headlineId = headline?.HeadlineId ?? 0;
            var hlName = headline != null ? headline.HeadlineName : "";
            var customerId = limitInfo.CustId;
            var related = limitInfo.CustId;

            stringBuilder.AppendFormat(XmlTemplate,
                limitInfo.LogID,
                customerId,
                headlineId,
                hlName,
                related,
                limitInfo.ProductID,
                limitInfo.ProductGroup,
                limitInfo.Level,
                settingAmount,
                TotalWinAmt,
                TotalWinAmt_RM,
                limitInfo.ReachedTime,
                betTypeIdList,
                limitInfo.CasinoType,
                limitInfo.IsNormalGame ? 1 : 0);
        }
        stringBuilder.Append(BackgroundJobConstants.EndRootTag);
        var criteria = new ImportWinLossFeedbackInfoCriteria
        {
            xml = stringBuilder.ToString(),
            Environment = environment
        };
        var WinLossInfo = await baseService.FetchMultipleAsync<WinLimitFeedBackInfo, BettypeInfo, SportInfo>(criteria);
        return new ValueTuple<IEnumerable<WinLimitFeedBackInfo>, IEnumerable<BettypeInfo>, IEnumerable<SportInfo>>(WinLossInfo.Item1, WinLossInfo.Item2, WinLossInfo.Item3);
    }

    protected internal virtual double GetLimitSetting(MaxWinLimitSetting setting, int levelID)
    {
        double amount;
        const int Level1 = 1;
        const int Level2 = 2;
        const int Level3 = 3;
        const int Level101 = 101;
        const int Level102 = 102;

        switch (levelID)
        {
            case Level1:
                amount = setting.Level1;
                break;

            case Level2:
                amount = setting.Level2;
                break;

            case Level3:
                amount = setting.Level3;
                break;

            case Level101:
                amount = setting.Level101;
                break;

            case Level102:
                amount = setting.Level102;
                break;

            default:
                amount = 0;
                break;
        }
        return amount;
    }

    public async Task<CRM.Models.Common.CommandResult> EditFeedbackWinLossInfoAsync(
        int LogID,
        string status,
        string description)
    {
        var criteria = new EditFeedbackWinLossInfoCommand
        {
            LogID = LogID,
            Status = status,
            Description = description
        };
        return await baseService.ExecuteNonQueryAsync(criteria);
    }

    public async Task<IEnumerable<TOControlFeedbackInfo>> ImportTOControlFeedbackInfoAsync(
        IEnumerable<GroupHeadline> headlines,
        IEnumerable<CustOutStanding> custOutStandings,
        IEnumerable<TONotification> custNotifications)
    {
        const string XmlTemplate = "<r logid='{0}' custid='{1}' matchid='{2}' profileID='{3}' hl='{4}' hlname='{5}' related='{6}' profileName='{7}' maxTO='{8}' " +
                                   "houseTO='{9}' totalTO='{10}' outstanding='{11}' bettype='{12}' level='{13}'/>";
        var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);
        foreach (var TO in custNotifications)
        {
            var custOutST = custOutStandings.FirstOrDefault(x => x.CustID == TO.CustID && x.MatchID == TO.MatchID);
            var outstanding = custOutST?.Outstanding ?? 0;

            var headline = headlines.FirstOrDefault(x => x.CustId == TO.CustID);
            var headlineId = headline?.HeadlineId ?? 0;
            var hlname = headline != null ? headline.HeadlineName : "";

            var custId = TO.CustID;
            var related = TO.CustID;

            stringBuilder.AppendFormat(XmlTemplate,
                TO.LogID,
                custId,
                TO.MatchID,
                TO.ProfileID,
                headlineId,
                hlname,
                related,
                HttpUtility.HtmlEncode(TO.ProfileName),
                TO.MaxTurnOver,
                TO.HouseTurnover,
                TO.TotalTurnover,
                outstanding,
                TO.Bettype,
                TO.ReachLevel
            );
        }
        stringBuilder.Append(BackgroundJobConstants.EndRootTag);
        var criteria = new InsertTOControlAutoFeedbackInfoCommand
        {
            xml = stringBuilder.ToString()
        };
        return await baseService.GetDataAsync<TOControlFeedbackInfo>(criteria);
    }

    public async Task<IEnumerable<ProblemsAccountAutoFeedbackInfo>> ImportProblemsAccountFeedbackInfoAsync(
        IEnumerable<GroupHeadline> headlines,
        IEnumerable<CustCategoryInfo> custCategories,
        string Environment)
    {
        const string XmlTemplate = "<r logid='{0}' custid='{1}' hl='{2}' hlname='{3}' related='{4}' catid='{5}'/>";
        var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);
        foreach (var custCategory in custCategories)
        {
            var headline = headlines.FirstOrDefault(x => x.CustId == custCategory.CustID);
            var headlineId = headline?.HeadlineId ?? 0;
            var hlname = headline != null ? headline.HeadlineName : "";
            var custId = custCategory.CustID;
            var related = custCategory.CustID;

            stringBuilder.AppendFormat(XmlTemplate,
                custCategory.LogID,
                custId,
                headlineId,
                hlname,
                related,
                custCategory.CategoryID
            );
        }
        stringBuilder.Append(BackgroundJobConstants.EndRootTag);
        var criteria = new InsertProblemsAccountAutoFeedbackInfoCommand
        {
            xml = stringBuilder.ToString(),
            Enviroment = Environment
        };
        return await baseService.GetDataAsync<ProblemsAccountAutoFeedbackInfo>(criteria);
    }

    public async Task<CRM.Models.Common.CommandResult> EditTOControlFeedbackInfoAsync(
        string LogID,
        string status,
        string description)
    {
        var criteria = new EditTOControlAutoFeedbackInfoUpdCommand
        {
            LogID = LogID,
            Status = status,
            Description = description
        };
        return await baseService.ExecuteNonQueryAsync(criteria);
    }

    public async Task<CRM.Models.Common.CommandResult> EditProblemsAccountFeedbackInfoAsync(
        string LogID,
        string status,
        string description)
    {
        var criteria = new EditProblemsAccountAutoFeedbackInfoCommand
        {
            LogID = LogID,
            Status = status,
            Description = description
        };
        return await baseService.ExecuteNonQueryAsync(criteria);
    }

    public async Task<IEnumerable<AmendmentAutoFeedbackInfo>> GetAmendmentNotiAutoFeedbackInfo(int BatchSize)
    {
        var criteria = new GetAmendmentNotiAutoFeedbackInfoCriteria
        {
            BatchSize = BatchSize
        };
        return await baseService.GetDataAsync<AmendmentAutoFeedbackInfo>(criteria);
    }

    public async Task<CRM.Models.Common.CommandResult> InsertAmendmentNotiAutoFeedback(IEnumerable<CusReachAmendmentNotification> CustAmendments, string Environment)
    {
        const string XmlTemplate = "<r ticketid='{0}' custid='{1}' ticketeventid='{2}' bettype='{3}' Seq='{4}' SubmitDate='{5}'/>";
        var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);
        foreach (var cust in CustAmendments)
        {
            stringBuilder.AppendFormat(XmlTemplate,
                cust.TicketId,
                cust.CustId,
                cust.TicketEventId,
                cust.Bettype,
                cust.Seq,
                cust.SubmitDate.ToString("yyyy-MM-dd H:mm:ss.fffffff"));
        }
        stringBuilder.Append(BackgroundJobConstants.EndRootTag);

        var criteria = new InsertAmendmentNotificationCommand
        {
            Xml = stringBuilder.ToString(),
            Environment = Environment
        };
        return await baseService.ExecuteNonQueryAsync(criteria);
    }

    public async Task<CRM.Models.Common.CommandResult> EditAmendmentNotiAutoFeedback(
        int LogID,
        string status,
        string description)
    {
        var criteria = new EditAmendmentAutoFeedbackInfoUpdCommand
        {
            LogID = LogID,
            Status = status,
            Description = description
        };
        return await baseService.ExecuteNonQueryAsync(criteria);
    }

    public async Task<IEnumerable<Message>> GetMessageContentInfoAsync(int messageId)
    {
        var criteria = new GetMessageInfoCriteria
        {
            MessageIds = messageId.ToString()
        };

        var messages = (await baseService.GetDataAsync<Message>(criteria)).ToList();
        foreach (var message in messages)
        {
            message.Content_en = message.Content_en?.Trim().Replace("@@", string.Empty);
            message.Content_cs = message.Content_cs?.Trim().Replace("@@", string.Empty);
        }

        return messages;
    }

    public async Task<bool> GetArchivingDAStatus()
    {
        var criteria = new GetArchivingDAStatusCriteria();
        return await baseService.GetDataEntityAsync<bool>(criteria);
    }

    public async Task<IEnumerable<ArchivingCustInfo>> GetArchivingCustInfo()
    {
        var criteria = new GetListArchivingAccountCriteria();
        return await baseService.GetDataAsync<ArchivingCustInfo>(criteria);
    }

    public async Task<IEnumerable<ArchivingAutoFbInfo>> ArchivingAutoFeedbackInfoInsAsync(ArchivingDASetting setting, string environment)
    {
        var command = new ArchivingAutoFeedbackInfoInsCommand
        {
            BatchSize = setting.BatchSize,
            MaxLogFBId = setting.MaxLogFBId,
            BatchSizeAccount = setting.BatchSizeAccount,
            Environment = environment,
            ServiceID = setting.ServiceID
        };
        return await baseService.GetDataAsync<ArchivingAutoFbInfo>(command);
    }

    public async Task<CRM.Models.Common.CommandResult> ArchivingImportDirectAccountAsync(string archivedList, DateTime archivedDate)
    {
        var command = new ArchivingImportDirectAccountCommand
        {
            ArchivedDate = archivedDate,
            ArchivedList = archivedList
        };
        return await baseService.ExecuteNonQueryAsync(command);
    }

    public async Task<CRM.Models.Common.CommandResult> EditArchivingDAAutoFeedback(int logFeedbackId, string status, string description)
    {
        var criteria = new ArchivingAutoFeedbackUpdateCommand
        {
            LogFeedbackId = logFeedbackId,
            Status = status,
            Description = description
        };
        return await baseService.ExecuteNonQueryAsync(criteria);
    }
}