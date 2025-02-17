namespace CRM.BackgroundJob.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using System.Xml.Linq;
    using CRM.BackgroundJob.Core.Models;
    using CRM.BackgroundJob.Core.Services;
    using CRM.BackgroundJob.Criteria.AmendmentNotiFB;
    using CRM.BackgroundJob.Criteria.ArchivingDA;
    using CRM.BackgroundJob.Criteria.ProblemsAccount;
    using CRM.BackgroundJob.Criteria.TurnoverControll;
    using CRM.BackgroundJob.Criteria.WinLossLimit;
    using CRM.BackgroundJob.Models.AmendmentNotiFB;
    using CRM.BackgroundJob.Models.ArchivingDA;
    using CRM.BackgroundJob.Models.ProblemsAccount;
    using CRM.BackgroundJob.Models.TurnoverControll;
    using CRM.BackgroundJob.Models.WinLossLimit;
    using Fanex.CRM.BackgroundJob.Constants;
    using Fanex.CRM.BackgroundJob.Criteria;
    using Fanex.CRM.BackgroundJob.Models;

    public interface IFeedbackTransactionService
    {
        Task<ValueTuple<IEnumerable<WinLimitFeedBackInfo>, IEnumerable<BettypeInfo>, IEnumerable<SportInfo>>> ImportWinLimitFeedbackInfoAsync(
               IEnumerable<CustomerWinLimit> cusWinLimit,
               IEnumerable<GroupHeadline> headlines);

        Task<CommandResult> EditFeedbackWinLossInfoAsync(int LogID, string status, string description);

        Task<IEnumerable<TOControlFeedbackInfor>> ImportTOControllFeedbackInfoAsync(
             IEnumerable<GroupHeadline> headlines,
             IEnumerable<CustOutStanding> custOutStandings,
             IEnumerable<TONotification> custNotifications);

        Task<IEnumerable<ProblemsAccountAutoFeedbackInfor>> ImportProblemsAccountFeedbackInfoAsync(
             IEnumerable<GroupHeadline> headlines,
             IEnumerable<CustCategoryInfor> custCatogorys,
             string Environment);

        Task<CommandResult> EditTOControlFeedbackInfoAsync(string LogID, string status, string description);

        Task<CommandResult> EditProblemsAccountFeedbackInfoAsync(string LogID, string status, string description);

        Task<IEnumerable<AmendmentAutoFeedbackInfor>> GetAmendmentNotiAutoFeedbackInfor(int BatchSize);

        Task<CommandResult> InserAmendmentNotiAutoFeedback(IEnumerable<CusReachAmendmentNotification> CustAmendments, string Environment);

        Task<CommandResult> EditAmendmentNotiAutoFeedback(int LogID, string status, string description);

        Task<IEnumerable<Message>> GetMessageContentInfoAsync(int messageId);

        Task<bool> GetArchivingDAStatus();

        Task<IEnumerable<ArchivingCustInfo>> GetArchivingCustInfo();

        Task<IEnumerable<ArchivingAutoFbInfo>> ArchivingAutoFeedbackInfoInsAsync(ArchivingDASetting setting, string environment);

        Task<CommandResult> ArchivingImportDirectAccountAsync(string achivedList, DateTime archivedDate);

        Task<CommandResult> EditArchivingDAAutoFeedback(int logFeedbackId, string status, string description);
    }

    public class FeedbackTransactionService : IFeedbackTransactionService
    {
        private readonly IBaseService baseService;

        public FeedbackTransactionService(
            IBaseService baseService)
        {
            this.baseService = baseService;
        }

        public async Task<ValueTuple<IEnumerable<WinLimitFeedBackInfo>, IEnumerable<BettypeInfo>, IEnumerable<SportInfo>>> ImportWinLimitFeedbackInfoAsync(
           IEnumerable<CustomerWinLimit> cusWinLimit,
           IEnumerable<GroupHeadline> headlines)
        {
            const string XmlTemplate = "<r logid='{0}' custid='{1}' hl='{2}' hlname='{3}' related='{4}' product='{5}' group='{6}' level='{7}' " +
                                       "limitsetting='{8}' winamount='{9}' winamountRM='{10}' time='{11}' bettype='{12}'/>";
            var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);
            foreach (var limitInfo in cusWinLimit)
            {
                double settingAmount = 0;
                var hlname = "";
                var headlineId = 0;
                double TotalWinAmt = limitInfo.TotalWinAmt;
                double TotalWinAmt_RM = limitInfo.TotalWinAmt_RM;
                var BettypeIDList = limitInfo.BettypeIDList ?? limitInfo.BettypeIDList;
                if (limitInfo.MaxWinLimitSetting == null || limitInfo.MaxWinLimitSetting == string.Empty)
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

                GroupHeadline headline = headlines.FirstOrDefault(x => x.CustId == limitInfo.CustId);
                headlineId = headline != null ? headline.HeadlineId : 0;
                hlname = headline != null ? headline.HeadlineName : "";
                var custId = limitInfo.CustId;
                var related = limitInfo.CustId;

                stringBuilder.AppendFormat(XmlTemplate,
                    limitInfo.LogID,
                    custId,
                    headlineId,
                    hlname,
                    related,
                    limitInfo.ProductID,
                    limitInfo.ProductGroup,
                    limitInfo.Level,
                    settingAmount,
                    TotalWinAmt,
                    TotalWinAmt_RM,
                    limitInfo.ReachedTime,
                    BettypeIDList);
            }
            stringBuilder.Append(BackgroundJobConstants.EndRootTag);
            var criteria = new ImportWinLossFeedbackInfoCriteria
            {
                xml = stringBuilder.ToString()
            };
            var WinLossInfo = await baseService.FetchMultipleAsync<WinLimitFeedBackInfo, BettypeInfo, SportInfo>(criteria);
            return new ValueTuple<IEnumerable<WinLimitFeedBackInfo>, IEnumerable<BettypeInfo>, IEnumerable<SportInfo>>(WinLossInfo.Item1, WinLossInfo.Item2, WinLossInfo.Item3);
        }

        protected internal virtual double GetLimitSetting(MaxWinLimitSetting setting, int levelID)
        {
            double amount = 0;
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

        public async Task<CommandResult> EditFeedbackWinLossInfoAsync(
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

        public async Task<IEnumerable<TOControlFeedbackInfor>> ImportTOControllFeedbackInfoAsync(
             IEnumerable<GroupHeadline> headlines,
             IEnumerable<CustOutStanding> custOutStandings,
             IEnumerable<TONotification> custNotifications)
        {
            const string XmlTemplate = "<r logid='{0}' custid='{1}' matchid='{2}' profileID='{3}' hl='{4}' hlname='{5}' related='{6}' profileName='{7}' maxTO='{8}' " +
                                       "houseTO='{9}' totalTO='{10}' outstanding='{11}' bettype='{12}' level='{13}'/>";
            var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);
            foreach (TONotification TO in custNotifications)
            {
                var hlname = "";
                var headlineId = 0;
                double outstanding = 0;

                CustOutStanding custOutST = custOutStandings.FirstOrDefault(x => x.CustID == TO.CustID && x.MatchID == TO.MatchID);
                outstanding = custOutST != null ? custOutST.Outstanding : 0;

                GroupHeadline headline = headlines.FirstOrDefault(x => x.CustId == TO.CustID);
                headlineId = headline != null ? headline.HeadlineId : 0;
                hlname = headline != null ? headline.HeadlineName : "";

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
            return await baseService.GetDataAsync<TOControlFeedbackInfor>(criteria);
        }

        public async Task<IEnumerable<ProblemsAccountAutoFeedbackInfor>> ImportProblemsAccountFeedbackInfoAsync(
             IEnumerable<GroupHeadline> headlines,
             IEnumerable<CustCategoryInfor> custCatogorys,
             string Environment)
        {
            const string XmlTemplate = "<r logid='{0}' custid='{1}' hl='{2}' hlname='{3}' related='{4}' catid='{5}'/>";
            var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);
            foreach (CustCategoryInfor custCategory in custCatogorys)
            {
                var hlname = "";
                var headlineId = 0;
                GroupHeadline headline = headlines.FirstOrDefault(x => x.CustId == custCategory.CustID);
                headlineId = headline != null ? headline.HeadlineId : 0;
                hlname = headline != null ? headline.HeadlineName : "";
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
            var criteria = new InsertProblemsAccountAutoFeedbackInforCommand
            {
                xml = stringBuilder.ToString(),
                Enviroment = Environment
            };
            return await baseService.GetDataAsync<ProblemsAccountAutoFeedbackInfor>(criteria);
        }

        public async Task<CommandResult> EditTOControlFeedbackInfoAsync(
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

        public async Task<CommandResult> EditProblemsAccountFeedbackInfoAsync(
          string LogID,
          string status,
          string description)
        {
            var criteria = new EditProblemsAccountAutoFeedbackInforCommand
            {
                LogID = LogID,
                Status = status,
                Description = description
            };
            return await baseService.ExecuteNonQueryAsync(criteria);
        }

        public async Task<IEnumerable<AmendmentAutoFeedbackInfor>> GetAmendmentNotiAutoFeedbackInfor(int BatchSize)
        {
            var criteria = new GetAmendmentNotiAutoFeedbackInforCriteria
            {
                BatchSize = BatchSize
            };
            return await baseService.GetDataAsync<AmendmentAutoFeedbackInfor>(criteria);
        }

        public async Task<CommandResult> InserAmendmentNotiAutoFeedback(IEnumerable<CusReachAmendmentNotification> CustAmendments, string Environment)
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

        public async Task<CommandResult> EditAmendmentNotiAutoFeedback(
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
            foreach (Message message in messages)
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

        public async Task<CommandResult> ArchivingImportDirectAccountAsync(string achivedList, DateTime archivedDate)
        {
            var command = new ArchivingImportDirectAccountCommand
            {
                ArchivedDate = archivedDate,
                ArchivedList = achivedList
            };
            return await baseService.ExecuteNonQueryAsync(command);
        }

        public async Task<CommandResult> EditArchivingDAAutoFeedback(int logFeedbackId, string status, string description)
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
}