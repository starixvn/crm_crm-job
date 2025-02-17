namespace Fanex.CRM.BackgroundJob.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fanex.CRM.BackgroundJob.Constants;
    using Fanex.CRM.BackgroundJob.Criteria;
    using Fanex.CRM.BackgroundJob.Models;
    using global::CRM.BackgroundJob.Core.Models;
    using global::CRM.BackgroundJob.Core.Services;
    using global::CRM.BackgroundJob.Models.AbnormalBet;
    using global::CRM.BackgroundJob.Models.MixParlay;
    using global::CRM.BackgroundJob.Services;
    using Microsoft.Extensions.Options;

    public interface IFeedbackService
    {
        Task CreateFeedbackForVoidingAbnormalBetAsync(AbnormalBetSystemSetting setting, bool isUAT,
            DateTime LastRuntimeEnv, string LastRuntimeNameEnv,
            string JobStatusNameEnv, string Environment);
    }

    public class FeedbackService : IFeedbackService
    {
        private readonly AppJobSettings appSettings;
        private readonly IBaseService baseService;
        private readonly ISettingService settingService;
        private readonly IMessageService messageService;
        private readonly ICustomerService customerService;
        private readonly ICrmLogService logService;
        private readonly IFeedbackTicketVoidingService ticketVoidingService;
        private readonly ICommonFeedback commonFeedback;

        private const int abnormalAccount = 13;
        private const int mixParlayIssueTypeId = 11;
        private const int duplicatedTicketIssueTypeId = 20;
        private readonly int[] parlayBetTypeIds = { 9, 29, 2799 };
        private const string templateXml = "MixParlayTemplate.xml";
        private const string templateDuplicateXml = "FeedbackDuplicateTemplate.xml";

#pragma warning disable S107 // Methods should not have too many parameters

        public FeedbackService(
            IOptions<AppJobSettings> appSettings,
            IBaseService baseService,
            ISettingService settingService,
            IMessageService messageService,
            ICustomerService customerService,
            ICrmLogService logService,
            IFeedbackTicketVoidingService ticketVoidingService,
            ICommonFeedback commonFeedback
            )

#pragma warning restore S107 // Methods should not have too many parameters
        {
            this.appSettings = appSettings.Value;
            this.baseService = baseService;
            this.settingService = settingService;
            this.messageService = messageService;
            this.customerService = customerService;
            this.logService = logService;
            this.ticketVoidingService = ticketVoidingService;
            this.commonFeedback = commonFeedback;
        }

#pragma warning disable S138

        public async Task CreateFeedbackForVoidingAbnormalBetAsync(AbnormalBetSystemSetting setting, bool isUAT,
                DateTime LastRuntimeEnv, string LastRuntimeNameEnv,
                string JobStatusNameEnv, string Environment)
#pragma warning restore S138
        {
            try
            {
                logService.LogInformation("CreateFeedbackForVoidingAbnormalBetAsync start!!!");

                await commonFeedback.EditSystemSettingAsyncByEnv(JobStatusNameEnv, LastRuntimeNameEnv, JobStatus.Running, BackgroundJobConstants.ServiceName_AutoFB);
                var MaxVoidingHistoryCollectionId = isUAT ? setting.MaxVoidingHistoryCollectionIdUAT : setting.MaxVoidingHistoryCollectionId;
                var voidingMessages = await messageService.GetVoidingMessageAsync(MaxVoidingHistoryCollectionId);

                var messageImportCheck = await messageService.ImportVoidingMessageHistoryCheckAsync(voidingMessages.Item2);
                var voidingMessageUserName = Enumerable.Empty<VoidingMessageUserName>();
                if (messageImportCheck.Any())
                {
                    voidingMessageUserName = await messageService.VoidingHisMessageUserNameList(messageImportCheck);
                }

                var messagesImport = await messageService.ImportVoidingMessageAsync(voidingMessageUserName,
                    voidingMessages.Item1,
                    setting.BatchSize,
                    Environment,
                    LastRuntimeEnv);

                logService.LogInformation("after ImportVoidingMessageAsync!!!");

                IEnumerable<VoidingMessageUserName> failMessages = await messageService.GetFailMessageAsync();
                var messagesImportCombineDistinct = messagesImport.Concat(failMessages).Distinct().ToList();
                if (!messagesImportCombineDistinct.Any())
                {
                    logService.LogInformation("Abnormal Bet - import do not have any data");
                    await commonFeedback.EditSystemSettingAsyncByEnv(JobStatusNameEnv, LastRuntimeNameEnv, JobStatus.Completed, BackgroundJobConstants.ServiceName_AutoFB);
                    return;
                }
                IEnumerable<int> messageIds = messagesImportCombineDistinct.Where(x => x.MessageId != 0).Select(y => y.MessageId).Distinct().ToList();
                IEnumerable<Message> messages = await messageService.GetMessageInfoAsync(messageIds);

                var (matchInfo, transInfo, duplicateTicketDetails, checkTrans) = await ticketVoidingService.GetTicketVoidingInfoAsync(messagesImportCombineDistinct, setting);

                var transactionTickets = Enumerable.Empty<TransactionInfo>();
                if (!string.IsNullOrEmpty(checkTrans))
                {
                    transactionTickets = await ticketVoidingService.GetAbnormalTicketsByTransAsync(setting, checkTrans);
                }
                var castTransactions = new List<TransactionInfo>();
                foreach (var ticket in duplicateTicketDetails)
                {
                    var castTransaction = new TransactionInfo
                    {
                        CustId = ticket.CustId,
                        MessageId = ticket.MessageId,
                        RequestId = ticket.RequestID,
                        Username = ticket.Username,
                        IsDeposit = ticket.IsDeposit,
                        Site = ticket.Site,
                        VoidingHistoryCollectionId = ticket.VoidingHistoryCollectionId,
                        ProductId = ticket.ProductId,
                        BetTypeId = ticket.BetTypeId,
                        SportTypeId = ticket.SportTypeId,
                        Bettype = ticket.Bettype
                    };
                    castTransactions.Add(castTransaction);
                }
                var transactionTicketsCombine = transactionTickets.Concat(transInfo).Concat(castTransactions);

                IEnumerable<MatchInfo> matchInfos = matchInfo.Where(x => transactionTicketsCombine.Any(y => x.VoidingHistoryCollectionId == y.VoidingHistoryCollectionId)).ToList();
                IEnumerable<MatchInfo> matchInfosDuplicate = matchInfo.Where(x => duplicateTicketDetails.Any(y => x.VoidingHistoryCollectionId == y.VoidingHistoryCollectionId)).ToList();
                IEnumerable<MatchInfo> matchInfosCombine = matchInfos.Concat(matchInfosDuplicate);

                if (!matchInfosCombine.Any())
                {
                    logService.LogInformation("Abnormal Bet - import do not have any ticket");
                    await messageService.EditStatusVoidingMessageAsync(messagesImportCombineDistinct, Environment);
                    await commonFeedback.EditSystemSettingAsyncByEnv(JobStatusNameEnv, LastRuntimeNameEnv, JobStatus.Completed, BackgroundJobConstants.ServiceName_AutoFB);
                    return;
                }

                logService.LogInformation(string.Format("Abnormal Bet - Create feedback for voidingHistoryCollectionId: {0}",
                    string.Join(",", matchInfo.Select(x => x.VoidingHistoryCollectionId)))
                );

                var ticketDuplicate = matchInfo.Where(x => x.IssueTypeId == duplicatedTicketIssueTypeId);
                var ticketDuplicateDetail = duplicateTicketDetails.Where(x => ticketDuplicate.Any(y => x.VoidingHistoryCollectionId == y.VoidingHistoryCollectionId));

                await CreateFeedbackForCreditDepositSiteAsync(
                    messages,
                    matchInfosCombine,
                    transactionTicketsCombine,
                    messagesImportCombineDistinct,
                    isUAT,
                    setting,
                    ticketDuplicateDetail
                );
            }
            catch (Exception ex)
            {
#pragma warning disable S3445
                throw ex;
#pragma warning restore S3445
            }
        }

#pragma warning disable S1541

        protected internal virtual async Task CreateFeedbackForCreditDepositSiteAsync(
            IEnumerable<Message> messages,
            IEnumerable<MatchInfo> matchInfos,
            IEnumerable<TransactionInfo> transactionInfos,
            IEnumerable<VoidingMessageUserName> messagesImportCombineDistinct,
            bool isUAT,
            AbnormalBetSystemSetting setting,
            IEnumerable<DuplicateTicketDetail> duplicateTicketDetails
        )
#pragma warning restore S1541
        {
            var settingNames = new[] { SettingName.Product, SettingName.SubProduct, SettingName.Bettype };
            IEnumerable<SettingItem> settings = await settingService.GetAllSettingAsync();
            IEnumerable<SystemSettingMapping> settingMappings = await settingService.GetSystemSettingMappingAsync(settingNames);
            IEnumerable<SiteSettingMapping> siteSettingMappings = await settingService.GetSiteSettingMappingAsync();
            CustomerInfo licenseeInfo = (await customerService.GetCustomerInfoAsync(appSettings.LicenseeUsername, true)) ?? new CustomerInfo();
            var custIds = transactionInfos.Where(x => !x.IsDeposit).Select(x => x.CustId).Distinct();
            IEnumerable<GroupHeadline> headlines = await customerService.GetGroupHeadlineVipAsync(custIds);
            var transactionByHeadline = transactionInfos.Where(x => headlines.Any(y => y.CustId == x.CustId));

            var (newMatchInfos, newTransactionInfos) = await ticketVoidingService.ImportTicketVoidingInfoAsync(
                                                                messagesImportCombineDistinct,
                                                                matchInfos,
                                                                transactionInfos,
                                                                siteSettingMappings,
                                                                headlines,
                                                                licenseeInfo);

            foreach (MatchInfo matchInfo in newMatchInfos)
            {
                IEnumerable<Message> messageContents = messages.Where(x => matchInfo.MessageId.Contains(x.MessageId.ToString()));
                IEnumerable<Message> messageByHeadline = messageContents.Where(x => transactionByHeadline.Any(y => y.MessageId == x.MessageId)).DistinctBy(x => x.MessageId);
                IEnumerable<TransactionInfo> credits = newTransactionInfos.Where(x => x.VoidingHistoryCollectionId == matchInfo.VoidingHistoryCollectionId && !x.IsDeposit);
                IEnumerable<TransactionInfo> deposits = newTransactionInfos.Where(x => x.VoidingHistoryCollectionId == matchInfo.VoidingHistoryCollectionId && x.IsDeposit);
                if (credits.Any() && matchInfo.IssueTypeId != duplicatedTicketIssueTypeId)
                {
                    await CreateFeedbackForCreditSiteAsync(
                        messageByHeadline,
                        matchInfo,
                        credits,
                        headlines.Where(x => credits.Any(y => x.CustId == y.CustId)),
                        settings,
                        settingMappings);
                }

                if (deposits.Any())
                {
                    GetLicenseeManager(deposits, siteSettingMappings);

                    var messageByLicenseeManager = messageContents.Where(x => deposits.Any(y => y.MessageId == x.MessageId));
                    var ticketDetailsByVoidingHistory = duplicateTicketDetails.Where(ticket => ticket.VoidingHistoryCollectionId == matchInfo.VoidingHistoryCollectionId && ticket.IsDeposit);
                    await CreateFeedbackForDepositSiteAsync(
                        licenseeInfo,
                        messageByLicenseeManager,
                        matchInfo,
                        deposits,
                        settings,
                        settingMappings,
                        ticketDetailsByVoidingHistory
                    );
                }
            }
            logService.LogInformation("Abnormal Bet - Update job and time");
            logService.LogInformation("end CreateFeedbackForCreditDepositSiteAsync!!!");
            var voidingHistoryCollectionId = isUAT ? setting.MaxVoidingHistoryCollectionIdUAT : setting.MaxVoidingHistoryCollectionId;
            var maxVoidingHistoryCollectionId = transactionInfos.Any() ? transactionInfos.Max(x => x.VoidingHistoryCollectionId) : voidingHistoryCollectionId;

            await UpdateSystemSetting(isUAT, setting, maxVoidingHistoryCollectionId);
        }

        protected internal virtual async Task UpdateSystemSetting(bool isUAT, AbnormalBetSystemSetting setting, int maxVoidingHistoryCollectionId)
        {
            var LastRuntimeNameEnv = isUAT ? SystemSettingName.LastRuntimeUAT : SystemSettingName.LastRuntime;
            var JobStatusNameEnv = isUAT ? SystemSettingName.JobStatusUAT : SystemSettingName.JobStatus;
            var VoidingHistoryCollectionNameEnv = isUAT ? SystemSettingName.MaxVoidingHistoryCollectionIdUAT : SystemSettingName.MaxVoidingHistoryCollectionId;
            await commonFeedback.AutoFbEditSystemSettingAsyncByEnv(JobStatusNameEnv, LastRuntimeNameEnv, VoidingHistoryCollectionNameEnv, maxVoidingHistoryCollectionId);
        }

        protected internal virtual async Task CreateFeedbackForCreditSiteAsync(
            IEnumerable<Message> messages,
            MatchInfo matchInfo,
            IEnumerable<TransactionInfo> transactionInfos,
            IEnumerable<GroupHeadline> headlines,
            IEnumerable<SettingItem> settings,
            IEnumerable<SystemSettingMapping> settingMappings)
        {
            logService.LogInformation("Abnormal Bet - Begin mapping common setting for VoidingHistoryCollectionId: " + matchInfo.VoidingHistoryCollectionId);
            var timeSpan = new TimeSpan(appSettings.TimeSpan, 0, 0);
            var feedbackDate = DateTimeOffset.Now.DateTime.Add(timeSpan);
            var status = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.Status, StringComparison.OrdinalIgnoreCase)
                && x.SettingValue.Equals("Closed", StringComparison.OrdinalIgnoreCase));
            var response = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.Response, StringComparison.OrdinalIgnoreCase)
                && x.SettingValue.Equals("Neutral", StringComparison.OrdinalIgnoreCase));
            var station = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.Extension, StringComparison.OrdinalIgnoreCase)
                && x.SettingId == BackgroundJobConstants.ExtentionNA);
            var category = GetCategorySubCategory(settings, matchInfo.IssueTypeId);

            var criteria = new CreateMutipleFeedbackCommand
            {
                CreatorId = appSettings.CreatorId,
                UserId = appSettings.CreatorId,
                UserRole = BackgroundJobConstants.UserRole,
                FeedbackDate = feedbackDate.Subtract(timeSpan),
                ResolutionDate = feedbackDate.AddMinutes(1).Subtract(timeSpan),
                IsCheckedDefaultComm = true,
                StationId = station != null ? station.SettingId.ToString() : string.Empty,
                StatusId = status != null ? status.SettingId : 0,
                ResponseId = response != null ? response.SettingId : 0,
                CategoryXml = category.Item3,
                CategoryIds = category.Item1.ToString(),
                SubCategoryIds = category.Item2.ToString(),
                Subject = "Special Notification",
                AutoField = string.Empty,
                AutoField1 = string.Empty,
                AutoField2 = string.Format("Date Time & Tool: {0}/{1}/", feedbackDate.ToString("yyyyMMdd"), feedbackDate.ToString("hh:mmtt")),
                AutoField3 = $"{Environment.NewLine}System ID: ",
                VoidingHistoryCollectionId = matchInfo.VoidingHistoryCollectionId
            };
            logService.LogInformation("Abnormal Bet - End mapping common setting for VoidingHistoryCollectionId: " + matchInfo.VoidingHistoryCollectionId);

            var headlineIds = headlines.Select(x => x.HeadlineId).Distinct();
            foreach (var headlineId in headlineIds)
            {
                GroupHeadline headline = headlines.First(x => x.HeadlineId == headlineId);
                IEnumerable<int> relatedCustomerIds = headlines.Where(x => x.HeadlineId == headline.HeadlineId).Select(x => x.CustId);
                criteria.CustomerXml = string.Format("<ROOT><Cust CustId='{0}' Username='{1}' /></ROOT>", headline.HeadlineId, headline.HeadlineName);

                await CreateFeedbackForCreditSiteByBatchAsync(
                    headline,
                    relatedCustomerIds,
                    messages,
                    matchInfo,
                    transactionInfos,
                    settings,
                    settingMappings,
                    criteria);
            }
        }

#pragma warning disable S107 // Methods should not have too many parameters

        protected internal virtual async Task CreateFeedbackForCreditSiteByBatchAsync(
            GroupHeadline headline,
            IEnumerable<int> relatedCustomerIds,
            IEnumerable<Message> messages,
            MatchInfo matchInfo,
            IEnumerable<TransactionInfo> transactionInfos,
            IEnumerable<SettingItem> settings,
            IEnumerable<SystemSettingMapping> settingMappings,
            CreateMutipleFeedbackCommand criteria)
#pragma warning restore S107 // Methods should not have too many parameters
        {
            int itemEachBatch = appSettings.NoOfRelatedCustomerPerFeedback;
            int totalBatch = (int)Math.Ceiling((double)relatedCustomerIds.Count() / itemEachBatch);
            var messageJoin = string.Join(",", messages.Select(x => x.MessageId));
            for (int i = 0; i < totalBatch; i++)
            {
                List<int> batch = relatedCustomerIds.Skip(i * itemEachBatch).Take(itemEachBatch).ToList();

                logService.LogInformation(string.Format("Abnormal Bet - Begin mapping setting for VoidingHistoryCollectionId: {0}\n\rHeadlineId: {1}\n\rRelatedCustIds: {2}",
                    matchInfo.VoidingHistoryCollectionId,
                    headline.HeadlineId,
                    string.Join(",", batch)));

                IEnumerable<TransactionInfo> transactions = transactionInfos.Where(x => batch.Exists(y => y == x.CustId));
                var isVip = headline.IsVip && headline.SiteLabel == "789y";
                var isMixParlay = matchInfo.IssueTypeId == mixParlayIssueTypeId && matchInfo.FilterBy == 1;
                criteria.ProductXml = GetProductXml(isMixParlay, matchInfo, transactions, settings, settingMappings);
                var solution = isMixParlay ? GetSolutionMixParlayForCreditSite(messages.FirstOrDefault(), transactions, isVip) :
                                             GetSolutionForCreditSite(messages.FirstOrDefault(), transactions, matchInfo.IssueTypeId);
                criteria.Solution = solution;
                criteria.RelatedCustomerXml = GetRelatedCustomerXml(headline.HeadlineId, transactions, matchInfo.IssueTypeId);
                criteria.MessageId = messageJoin;
                criteria.VoidingHistoryCollectionId = matchInfo.VoidingHistoryCollectionId;
                criteria.ListOrigCustIds = string.Join(",", batch);

                logService.LogInformation(string.Format("Abnormal Bet - End mapping setting for VoidingHistoryCollectionId: {0}\n\rHeadlineId: {1}\n\rRelatedCustIds: {2}",
                    matchInfo.VoidingHistoryCollectionId,
                    headline.HeadlineId,
                    string.Join(",", batch)));
                IEnumerable<int> requestIds = transactions.Select(x => x.RequestId).Distinct().ToList();
                criteria.RequestIds = string.Join(",", requestIds);
                var description = isMixParlay ? GetDescriptionMixParlayForCreditSite(string.Join(",", messages.Select(x => x.MessageId)), criteria.RequestIds, matchInfo)
                                                : GetDescriptionForCreditSite(string.Join(",", messages.Select(x => x.MessageId)), criteria.RequestIds, matchInfo, transactions);
                criteria.Description = description;
                try
                {
                    logService.LogInformation(string.Format("Abnormal Bet - Begin create feedback for VoidingHistoryCollectionId: {0}\n\rHeadlineId: {1}\n\rRelatedCustIds: {2}",
                        matchInfo.VoidingHistoryCollectionId,
                        headline.HeadlineId,
                        string.Join(",", batch)));
                    CommandResult commandResult = await baseService.ExecuteNonQueryAsync(criteria);
                    if (commandResult.IsSuccessful)
                    {
                        logService.LogInformation(string.Format("Abnormal Bet - Create feedback successfully. VoidingHistoryCollectionId: {0}\n\rHeadlineId: {1}\n\rRelatedCustIds: {2}",
                            matchInfo.VoidingHistoryCollectionId,
                            headline.HeadlineId,
                            string.Join(",", batch)));
                    }
                    else
                    {
                        logService.LogInformation(string.Format("Abnormal Bet - {0}. VoidingHistoryCollectionId: {1}\n\rHeadlineId: {2}\n\rRelatedCustIds: {3}",
                            commandResult.Message,
                            matchInfo.VoidingHistoryCollectionId,
                            headline.HeadlineId,
                            string.Join(",", batch)));
                        await ticketVoidingService.EditFeedbackInfoAsync(messageJoin, requestIds, batch, CreateFeedbackJobStatus.Fail, commandResult.Message);
                        await logService.ErrorAsync(commandResult, criteria);
                    }
                }
                catch (Exception ex)
                {
                    logService.LogInformation(string.Format("Abnormal Bet - {0}. VoidingHistoryCollectionId: {1}\n\rHeadlineId: {2}\n\rRelatedCustIds: {3}",
                        ex.Message,
                        matchInfo.VoidingHistoryCollectionId,
                        headline.HeadlineId,
                        string.Join(",", batch)));
                    await ticketVoidingService.EditFeedbackInfoAsync(messageJoin, requestIds, batch, CreateFeedbackJobStatus.Fail, ex.Message);
                    await logService.ErrorAsync(ex);
                }
            }
        }

#pragma warning disable S1541

        protected internal virtual async Task CreateFeedbackForDepositSiteAsync(
            CustomerInfo licenseeInfo,
            IEnumerable<Message> messages,
            MatchInfo matchInfo,
            IEnumerable<TransactionInfo> transactionInfos,
            IEnumerable<SettingItem> settings,
            IEnumerable<SystemSettingMapping> settingMappings,
            IEnumerable<DuplicateTicketDetail> duplicateTicketDetails)
#pragma warning restore S1541
        {
            logService.LogInformation("Abnormal Bet - Begin mapping setting for VoidingHistoryCollectionId: " + matchInfo.VoidingHistoryCollectionId);
            var timeSpan = new TimeSpan(appSettings.TimeSpan, 0, 0);
            var feedbackDate = DateTimeOffset.Now.DateTime.Add(timeSpan);
            var communicationTool = GetCommunicationTool(settings, matchInfo.IssueTypeId);
            var status = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.Status, StringComparison.OrdinalIgnoreCase)
                && x.SettingValue.Equals("Closed", StringComparison.OrdinalIgnoreCase));
            var response = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.Response, StringComparison.OrdinalIgnoreCase)
                && x.SettingValue.Equals("Neutral", StringComparison.OrdinalIgnoreCase));
            var station = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.Extension, StringComparison.OrdinalIgnoreCase)
                && x.SettingId == BackgroundJobConstants.ExtentionNA);
            var category = GetCategorySubCategory(settings, matchInfo.IssueTypeId);
            var isMixParlay = matchInfo.IssueTypeId == mixParlayIssueTypeId && matchInfo.FilterBy == 1;

            var criteria = new CreateFeedbackCommand
            {
                CreatorId = appSettings.CreatorId,
                UserId = appSettings.CreatorId,
                FeedbackDate = feedbackDate.Subtract(timeSpan),
                ResolutionDate = feedbackDate.AddMinutes(1).Subtract(timeSpan),
                CustId = licenseeInfo.CustId,
                Username = licenseeInfo.Username,
                StationId = station != null ? station.SettingId.ToString() : string.Empty,
                CommunicationId = communicationTool.SettingId.ToString(),
                CategoryXml = category != null ? category.Item3 : string.Empty,
                ProductXml = matchInfo.IssueTypeId == duplicatedTicketIssueTypeId ? GetDuplicatedTicketProductXml(transactionInfos, settings, settingMappings)
                    : GetProductXml(isMixParlay, matchInfo, transactionInfos, settings, settingMappings),
                StatusId = status != null ? status.SettingId : 0,
                LanguageId = licenseeInfo.LanguageId,
                ResponseId = response != null ? response.SettingId : 0,
                RoleId = licenseeInfo.RoleId,
                CountryId = licenseeInfo.CountryId,
                Subject = "Special Notification",
                Solution = duplicateTicketDetails.Any() ? "Sent done" : GetSolution(messages.FirstOrDefault()),
                LicenseeType = licenseeInfo.LicenseeType,
                TitleId = licenseeInfo.TitleId,
                SiteId = licenseeInfo.SiteId,
                VoidingHistoryCollectionId = matchInfo.VoidingHistoryCollectionId
            };
            logService.LogInformation("Abnormal Bet - End mapping setting for VoidingHistoryCollectionId: " + matchInfo.VoidingHistoryCollectionId);

            await CreateFeedbackForDepositByBatchAsync(
                messages,
                transactionInfos,
                communicationTool.SettingValue,
                criteria,
                duplicateTicketDetails);
        }

        protected internal virtual async Task CreateFeedbackForDepositByBatchAsync(
            IEnumerable<Message> messages,
            IEnumerable<TransactionInfo> transactionInfos,
            string communicationTool,
            CreateFeedbackCommand criteria,
            IEnumerable<DuplicateTicketDetail> duplicateTicketDetails)
        {
            if (duplicateTicketDetails.Any())
            {
                messages = messages.Where(m => duplicateTicketDetails.Any(t => t.MessageId == m.MessageId && t.IsDeposit));
            }
            var joinMessageString = string.Join(",", messages.Select(x => x.MessageId));

            IList<int> licenseeManagerIds = transactionInfos.Select(x => x.LicenseeManagerId).Distinct().ToList();
            int itemEachBatch = appSettings.NoOfRelatedCustomerPerFeedback;
            int totalBatch = (int)Math.Ceiling((double)licenseeManagerIds.Count() / itemEachBatch);
            for (int i = 0; i < totalBatch; i++)
            {
                IList<int> batch = licenseeManagerIds.Skip(i * itemEachBatch).Take(itemEachBatch).ToList();
                IList<TransactionInfo> transactions = transactionInfos.Where(x => batch.Any(y => y == x.LicenseeManagerId)).ToList();
                List<string> licenseeManagers = transactions.Where(x => x.LicenseeManagerId != 0).Select(x => x.LicenseeManager).Distinct().ToList();
                IList<int> custIds = transactions.Select(x => x.CustId).Distinct().ToList();
                List<int> relatedIds = batch.ToList();
                if (duplicateTicketDetails.Any())
                {
                    relatedIds.AddRange(transactionInfos.Select(transaction => transaction.CustId).Distinct());
                    licenseeManagers.AddRange(transactionInfos.Select(transaction => transaction.Username).Distinct());
                    licenseeManagers.Sort();
                }
                criteria.RelatedCustId = string.Join(", ", relatedIds.Where(x => x != 0).Select(x => x));
                criteria.AutoField = GetAutoField(DateTimeOffset.Now.DateTime, communicationTool, licenseeManagers);
                criteria.MessageId = joinMessageString;
                criteria.ListOrigCustIds = string.Join(",", custIds);
                IEnumerable<int> requestIds = transactions.Select(x => x.RequestId).Distinct().ToList();
                criteria.RequestIds = string.Join(",", requestIds);
                var duplicateTickets = duplicateTicketDetails.Where(x => transactionInfos.Any(y => y.VoidingHistoryCollectionId == x.VoidingHistoryCollectionId) && x.IsDeposit);

                criteria.Description = duplicateTickets.Any() ?
                    GetDescriptionDuplicateForDepositeSite(duplicateTickets.Where(i => i.IsDeposit), messages, joinMessageString, string.Join(",", requestIds)) :
                    GetDescriptionForDepositeSite(joinMessageString, criteria.RequestIds);
                try
                {
                    logService.LogInformation(string.Format("Abnormal Bet - Begin create feedback for MessageId: {0}\n\rHeadlineId: {1}\n\rRelatedCustIds: {2}\n\rCustIds: {3}",
                            joinMessageString,
                            criteria.CustId,
                            string.Join(",", batch),
                            string.Join(",", custIds)));
                    CommandResult commandResult = await baseService.ExecuteNonQueryAsync(criteria);
                    if (commandResult.IsSuccessful)
                    {
                        logService.LogInformation(string.Format("Abnormal Bet - {0}. MessageId: {1}\n\rHeadlineId: {2}\n\rRelatedCustIds: {3}\n\rCustIds: {4}",
                            "Create feedback successfully",
                            joinMessageString,
                            criteria.CustId,
                            string.Join(",", batch),
                            string.Join(",", custIds)));
                    }
                    else
                    {
                        logService.LogInformation(string.Format("Abnormal Bet - {0}. MessageId: {1}\n\rHeadlineId: {2}\n\rRelatedCustIds: {3}\n\rCustIds: {4}",
                            commandResult.Message,
                            joinMessageString,
                            criteria.CustId,
                            string.Join(",", batch),
                            string.Join(",", custIds)));
                        await ticketVoidingService.EditFeedbackInfoAsync(joinMessageString, requestIds, custIds, CreateFeedbackJobStatus.Fail, commandResult.Message);
                        await logService.ErrorAsync(commandResult, criteria);
                    }
                }
                catch (Exception ex)
                {
                    logService.LogInformation(string.Format("{0}. MessageId: {1}\n\rHeadlineId: {2}\n\rRelatedCustIds: {3}\n\rCustIds: {4}",
                        ex.Message,
                        joinMessageString,
                        criteria.CustId,
                        string.Join(",", batch),
                        string.Join(",", custIds)));
                    await ticketVoidingService.EditFeedbackInfoAsync(joinMessageString, requestIds, custIds, CreateFeedbackJobStatus.Fail, ex.Message);
                    await logService.ErrorAsync(ex);
                }
            }
        }

        protected internal virtual void GetLicenseeManager(
            IEnumerable<TransactionInfo> transactionInfos,
            IEnumerable<SiteSettingMapping> siteSettingMappings)
        {
            foreach (TransactionInfo transaction in transactionInfos)
            {
                SiteSettingMapping licenseeManager = siteSettingMappings.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.SettingSource)
                        && x.SettingSource.Equals(transaction.Site, StringComparison.OrdinalIgnoreCase));

                if (licenseeManager != null)
                {
                    transaction.LicenseeManagerId = licenseeManager.LicenseeManagerId;
                    transaction.LicenseeManager = licenseeManager.LicenseeManager;
                }
            }
        }

        protected internal virtual string GetDescriptionDuplicateForDepositeSite(IEnumerable<DuplicateTicketDetail> duplicateTicketDetails,
            IEnumerable<Message> messages, string messageIds, string requestIds)
        {
            var templateDuplicate = commonFeedback.GetFeedbackTemplates<DuplicateTemplateFeedback>(templateDuplicateXml);
            var stringBuilder = new StringBuilder(DefaultValue.autoGenerated);
            stringBuilder.Append(BackgroundJobConstants.Break);

            const string messageTemplate = "<span style=\"color: rgb(255, 0, 0);\">Msg ID: {0}, Request No.: {1}</span>";
            stringBuilder.Append(string.Format(messageTemplate, messageIds, requestIds));
            stringBuilder.Append(BackgroundJobConstants.Break);

            stringBuilder.Append(BackgroundJobConstants.Break);
            var stringBuilderTicket = new StringBuilder();
            var message = messages.First();

            var origTransIds = string.Join(",", duplicateTicketDetails.Select(x => x.OrgTransIds));
            var noOfRemainedTickets = origTransIds.Split(",").Distinct().Count();

            var duplicateGroupByUsernames = duplicateTicketDetails.GroupBy(x => x.Username);
            foreach (var duplicateTicket in duplicateGroupByUsernames.ToList())
            {
                var duplicateGroupByTransId = duplicateTicket.GroupBy(x => x.OrgTransIds);
                var duplicateGroupByTransIdList = duplicateGroupByTransId.ToList();
                stringBuilderTicket.Append("System ID: " + duplicateTicket.Key + BackgroundJobConstants.Break);
                foreach (var ticket in duplicateGroupByTransIdList)
                {
                    var refNoVoided = string.Join(", ", ticket.Select(x => x.VoidedTransId).Distinct());
                    string remainTicket = ticket.Key.Split(',').Count() > 1 ? string.Empty : ticket.Key;
                    stringBuilderTicket.AppendFormat(templateDuplicate.EffectMemberTicketDesc, remainTicket, refNoVoided, BackgroundJobConstants.Break, BackgroundJobConstants.LineBreak);
                }
            }

            stringBuilder.AppendFormat(templateDuplicate.Description, messageIds, requestIds, noOfRemainedTickets,
                   message.Content_en, message.Content_cs,
                   stringBuilderTicket.ToString(), BackgroundJobConstants.Break, BackgroundJobConstants.LineBreak);

            return stringBuilder.ToString();
        }

        protected internal virtual SettingItem GetCommunicationTool(IEnumerable<SettingItem> settings, int issueTypeId)
        {
            string settingValue = issueTypeId == duplicatedTicketIssueTypeId ? "Email" : "Skype";
            var communication = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.CommunicationTool, StringComparison.OrdinalIgnoreCase)
                && x.SettingValue.Equals(settingValue, StringComparison.OrdinalIgnoreCase));

            return communication ?? new SettingItem { SettingId = BackgroundJobConstants.CommunicationNA };
        }

        private string GetDuplicatedTicketProductXml(IEnumerable<TransactionInfo> transactions, IEnumerable<SettingItem> settings, IEnumerable<SystemSettingMapping> settingMappings)
        {
            var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);
            var listBetTypes = transactions.Select(t => t.BetTypeId.ToString());
            var productIds = transactions.Select(t => t.ProductId);
            var sportTypeIds = transactions.Select(t => t.SportTypeId);
            var isMixParlay = transactions.All(x => parlayBetTypeIds.Contains(x.BetTypeId));

            IList<SettingItem> products = GetProductByIds(settings, settingMappings, productIds);
            var productSettingIds = products.Select(x => x.SettingId);

            IList<SettingItem> subProducts = GetSubProducts(isMixParlay, productSettingIds, sportTypeIds, settings, settingMappings);

            IList<SystemSettingMapping> bettypesMapping = GetBettypes(isMixParlay, listBetTypes.ToArray(), settingMappings);

            if (products.Any())
            {
                foreach (SettingItem product in products)
                {
                    var productSubXml = GetProductSubXml(product, subProducts, bettypesMapping);
                    stringBuilder.Append(productSubXml);
                }
            }
            else
            {
                stringBuilder.AppendFormat(DefaultValue.ProductXml, BackgroundJobConstants.ProductNA, BackgroundJobConstants.SubProductNA, BackgroundJobConstants.BettypeNA);
            }
            logService.LogInformation("end mapping product, sub-product, bet type voiding settled tickets.");
            return stringBuilder.Append(BackgroundJobConstants.EndRootTag).ToString();
        }

        private IList<SettingItem> GetProductByIds(
            IEnumerable<SettingItem> settings,
            IEnumerable<SystemSettingMapping> settingMappings,
            IEnumerable<int> productIds)
        {
            logService.LogInformation("Begin mapping product with productId");
            var productNames = new List<string>();
            foreach (var productId in productIds)
            {
                var productName = commonFeedback.GetProductName(productId.ToString(), Language.en_US);
                productNames.Add(productName);
            }

            List<SystemSettingMapping> productMappings = settingMappings.Where(p => p.SettingName.Equals(SettingName.Product, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(p.SettingSource)
                && productNames.Exists(productName => p.SettingSource.Equals(productName, StringComparison.OrdinalIgnoreCase))).ToList();

            var products = settings.Where(x => x.SettingName.Equals(SettingName.Product, StringComparison.OrdinalIgnoreCase)
                && productMappings != null
                && productMappings.Exists(mapping => x.SettingId == mapping.SettingId)).ToList();

            logService.LogInformation("End mapping amenment product.");

            return products.Any()
                ? products.GroupBy(x => x.SettingId).Select(x => x.First()).ToList()
                : new List<SettingItem> { new SettingItem { SettingId = BackgroundJobConstants.ProductNA } };
        }

        private IList<SettingItem> GetSubProducts(
            bool isMixParlay,
            IEnumerable<int> productSettingIds,
            IEnumerable<short> sportTypeIds,
            IEnumerable<SettingItem> settings,
            IEnumerable<SystemSettingMapping> settingMappings)
        {
            logService.LogInformation("Begin mapping subproduct.");
            var sportNames = new List<string>();
            foreach (var sportTypeId in sportTypeIds)
            {
                var sportName = commonFeedback.GetSportName(sportTypeId.ToString(), Language.en_US);
                sportNames.Add(sportName);
            }
            List<SystemSettingMapping> mappings = settingMappings.Where(mapping => mapping.SettingName.Equals(SettingName.SubProduct, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(mapping.SettingSource)
                && sportNames.Exists(sportName => mapping.SettingSource.Equals(sportName, StringComparison.OrdinalIgnoreCase))).ToList();

            List<SettingItem> subProducts = isMixParlay ? settings.Where(x => x.SettingName.Equals(SettingName.SubProduct, StringComparison.OrdinalIgnoreCase)
                && x.SettingValue.Equals("Multiple Sports", StringComparison.OrdinalIgnoreCase)).ToList() :
                settings.Where(sub => sub.SettingName.Equals(SettingName.SubProduct, StringComparison.OrdinalIgnoreCase)
                && sub.ParentIds.Any(parent => productSettingIds.Any(productName => productName == parent))
                && mappings != null && mappings.Exists(mapping => mapping.SettingId == sub.SettingId)).ToList();

            logService.LogInformation("End mapping subproduct.");

            return subProducts.Any()
                ? subProducts.GroupBy(x => x.SettingId).Select(x => x.First()).ToList()
                : new List<SettingItem>
                {
                    new SettingItem { SettingId = BackgroundJobConstants.SubProductNA, ParentIds = new List<int> { BackgroundJobConstants.ProductNA } }
                };
        }

        private List<SystemSettingMapping> GetBettypes(
            bool isMixParlay,
            string[] listBetTypes,
            IEnumerable<SystemSettingMapping> settingMappings)
        {
            logService.LogInformation("Begin mapping betType.");
            var betTypeMappings = new List<string>();

            foreach (var betType in listBetTypes)
            {
                var betTypeName = commonFeedback.GetBettypeName(betType, Language.en_US);
                betTypeMappings.Add(betTypeName);
            }
            var bettypes = isMixParlay ? settingMappings.Where(mapping => mapping.SettingName.Equals(SettingName.Bettype, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(mapping.SettingSource)
                && mapping.SettingSource.Equals("Mix Parlay", StringComparison.OrdinalIgnoreCase)).ToList() :
                settingMappings.Where(mapping => mapping.SettingName.Equals(SettingName.Bettype, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(mapping.SettingSource)
                && betTypeMappings.Exists(name => name.Equals(mapping.SettingSource, StringComparison.OrdinalIgnoreCase))).ToList();

            logService.LogInformation("End mapping betType.");
            return bettypes != null && bettypes.Any()
                ? bettypes.GroupBy(x => x.SettingId).Select(x => x.First()).ToList()
                : new List<SystemSettingMapping>
                {
                    new SystemSettingMapping { SettingId = BackgroundJobConstants.BettypeNA }
                };
        }

        private static string GetProductSubXml(
            SettingItem product,
            IList<SettingItem> subProducts,
            IList<SystemSettingMapping> bettypesMapping)
        {
            var stringBuilder = new StringBuilder();
            if (!subProducts.Any())
            {
                stringBuilder.AppendFormat(DefaultValue.ProductXml, product.SettingId, BackgroundJobConstants.SubProductNA, BackgroundJobConstants.BettypeNA);
            }
            else
            {
                foreach (SettingItem subProduct in subProducts)
                {
                    if (bettypesMapping.Any())
                    {
                        var bettypeXml = GetBettypeXml(product, subProduct, bettypesMapping);
                        stringBuilder.Append(bettypeXml);
                    }
                    else
                    {
                        stringBuilder.AppendFormat(DefaultValue.ProductXml, product.SettingId, subProduct.SettingId, BackgroundJobConstants.BettypeNA);
                    }
                }
            }
            return stringBuilder.ToString();
        }

        private static string GetBettypeXml(
           SettingItem product,
           SettingItem subProduct,
           IList<SystemSettingMapping> bettypesMapping)
        {
            var stringBuilder = new StringBuilder();
            foreach (SystemSettingMapping bettype in bettypesMapping)
            {
                stringBuilder.AppendFormat(DefaultValue.ProductXml, product.SettingId, subProduct.SettingId, bettype.SettingId);
            }
            return stringBuilder.ToString();
        }

        protected internal virtual string GetProductXml(
            bool isMixParlay,
            MatchInfo matchInfo,
            IEnumerable<TransactionInfo> transactionInfos,
            IEnumerable<SettingItem> settings,
            IEnumerable<SystemSettingMapping> settingMappings)
        {
            logService.LogInformation("Abnormal Bet - Begin mapping product, sub-product, bet type.");
            const string XmlTemplate = "<Product ProductID='{0}' SubProductID='{1}' BetTypeID='{2}' />";
            SettingItem sportsbook = GetSportsbookProduct(settings);
            var subProducts = GetSubProduct(isMixParlay, sportsbook.SettingId, matchInfo, settings, settingMappings);
            var bettypes = GetBettype(isMixParlay, subProducts.Select(x => x.SettingId), transactionInfos, settings, settingMappings);
            var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);

            foreach (var settingId in subProducts.Select(x => x.SettingId))
            {
                IList<SettingItem> tempBettypes = bettypes.Where(x => x.ParentIds.Any(y => y == settingId)).ToList();
                if (tempBettypes.Any())
                {
                    foreach (SettingItem bettype in tempBettypes)
                    {
                        stringBuilder.AppendFormat(XmlTemplate, sportsbook.SettingId, settingId, bettype.SettingId);
                    }
                }
                else
                {
                    stringBuilder.AppendFormat(XmlTemplate, sportsbook.SettingId, settingId, BackgroundJobConstants.BettypeNA);
                }
            }

            logService.LogInformation("Abnormal Bet - End mapping product, sub-product, bet type.");
            return stringBuilder.Append(BackgroundJobConstants.EndRootTag).ToString();
        }

        protected internal virtual SettingItem GetSportsbookProduct(IEnumerable<SettingItem> settings)
        {
            var product = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.Product, StringComparison.OrdinalIgnoreCase)
                && x.SettingValue.Equals("Sportsbook", StringComparison.OrdinalIgnoreCase));

            return product ?? new SettingItem { SettingId = BackgroundJobConstants.ProductNA };
        }

        protected internal virtual IList<SettingItem> GetSubProduct(
            bool isMixParlay,
            int productId,
            MatchInfo matchInfo,
            IEnumerable<SettingItem> settings,
            IEnumerable<SystemSettingMapping> settingMappings)
        {
            var sport = matchInfo.SportName;
            IList<SystemSettingMapping> mappings = settingMappings.Where(mapping => mapping.SettingName.Equals(SettingName.SubProduct, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(mapping.SettingSource)
                && mapping.SettingSource.Equals(sport, StringComparison.OrdinalIgnoreCase)).ToList();
            IList<SettingItem> subProduct = settings.Where(sub => sub.SettingName.Equals(SettingName.SubProduct, StringComparison.OrdinalIgnoreCase)
                && sub.ParentIds.Any(parent => parent == productId)
                && mappings != null
                && mappings.Any(mapping => mapping.SettingId == sub.SettingId)).ToList();

            var subProductMixparlay = settings.Where(x => x.SettingName.Equals(SettingName.SubProduct, StringComparison.OrdinalIgnoreCase)
                && x.SettingValue.Equals("Multiple Sports", StringComparison.OrdinalIgnoreCase));

            var subProducts = isMixParlay ? subProductMixparlay : subProduct;

            return subProducts != null && subProducts.Any()
                ? subProducts.GroupBy(x => x.SettingId).Select(x => x.First()).ToList()
                : new List<SettingItem>
                {
                    new SettingItem { SettingId = BackgroundJobConstants.SubProductNA, ParentIds = new List<int> { BackgroundJobConstants.ProductNA } }
                };
        }

        protected internal virtual IList<SettingItem> GetBettype(
            bool isMixParlay,
            IEnumerable<int> subProducts,
            IEnumerable<TransactionInfo> transactionInfos,
            IEnumerable<SettingItem> settings,
            IEnumerable<SystemSettingMapping> settingMappings)
        {
            var transactionAbnormalAccount = transactionInfos.Where(x => x.Bettype != null);
            List<SettingItem> bettypes = null;
            if (isMixParlay)
            {
                bettypes = GetBettypeParlay(subProducts, settings, settingMappings);
            }
            else
            {
                IList<SystemSettingMapping> mappings = settingMappings.Where(mapping => mapping.SettingName.Equals(SettingName.Bettype, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(mapping.SettingSource)
                && transactionAbnormalAccount.Any(tran => tran.Bettype.Equals(mapping.SettingSource, StringComparison.OrdinalIgnoreCase))).ToList();

                bettypes = settings.Where(bettype => bettype.SettingName.Equals(SettingName.Bettype, StringComparison.OrdinalIgnoreCase)
                && subProducts.Any(sub => bettype.ParentIds.Any(parent => parent == sub))
                && mappings != null
                && mappings.Any(mapping => mapping.SettingId == bettype.SettingId)).ToList();
            }

            return bettypes != null && bettypes.Any()
                ? bettypes.GroupBy(x => x.SettingId).Select(x => x.First()).ToList()
                : new List<SettingItem>
                {
                    new SettingItem { SettingId = BackgroundJobConstants.BettypeNA, ParentIds = new List<int> { BackgroundJobConstants.SubProductNA } }
                };
        }

        protected internal virtual List<SettingItem> GetBettypeParlay(
            IEnumerable<int> subProducts,
            IEnumerable<SettingItem> settings,
            IEnumerable<SystemSettingMapping> settingMappings)
        {
            IList<SystemSettingMapping> mappingsParlay = settingMappings.Where(mapping => mapping.SettingName.Equals(SettingName.Bettype, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(mapping.SettingSource)
                && mapping.SettingSource.Equals("Mix Parlay", StringComparison.OrdinalIgnoreCase)).ToList();

            var bettypes = settings.Where(bettype => bettype.SettingName.Equals(SettingName.Bettype, StringComparison.OrdinalIgnoreCase)
            && subProducts.Any(sub => bettype.ParentIds.Any(parent => parent == sub))
            && mappingsParlay != null
            && mappingsParlay.Any(mapping => mapping.SettingId == bettype.SettingId)).ToList();

            return bettypes;
        }

        private static string GetSubCategorySettingValue(int issueTypeId)
        {
            string subCategory = "Abnormal Bets";
            if (issueTypeId == abnormalAccount)
            {
                subCategory = "Abnormal Account";
            }

            if (issueTypeId == duplicatedTicketIssueTypeId)
            {
                subCategory = "Duplicate Tickets";
            }
            return subCategory;
        }

        protected internal virtual Tuple<int, int, string> GetCategorySubCategory(IEnumerable<SettingItem> settings, int IssueTypeId)
        {
            var category = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.MainCategory, StringComparison.OrdinalIgnoreCase)
                && x.SettingValue.Equals("Special Notification", StringComparison.OrdinalIgnoreCase));
            var subCategoryId = 0;

            if (category != null)
            {
                var subCategory = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.SubCategory, StringComparison.OrdinalIgnoreCase)
                    && x.ParentIds.Any(y => y == category.SettingId)
                    && x.SettingValue.Equals(GetSubCategorySettingValue(IssueTypeId), StringComparison.OrdinalIgnoreCase));
                subCategoryId = subCategory != null ? subCategory.SettingId : 0;
            }

            return new Tuple<int, int, string>(
                category != null ? category.SettingId : 0,
                subCategoryId,
                string.Format("<ROOT><Category MainCategoryID='{0}' SubCategoryID='{1}' /></ROOT>",
                    category != null ? category.SettingId : 0,
                    subCategoryId));
        }

        protected internal virtual string GetRelatedCustomerXml(int headlineId, IEnumerable<TransactionInfo> transactionInfos, int IssueTypeId)
        {
            const string XmlTemplate = "<Row CustId='{0}' RelatedCustId='{1}' Username='{2}' />";
            var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);

            var transactions = IssueTypeId == abnormalAccount ? transactionInfos.GroupBy(x => x.RelatedCustID).Select(x => x.First()) : transactionInfos.GroupBy(x => x.CustId).Select(x => x.First());
            foreach (var transaction in transactions)
            {
                var custId = IssueTypeId == abnormalAccount ? transaction.RelatedCustID : transaction.CustId;
                var userName = IssueTypeId == abnormalAccount ? transaction.RelatedUserName : transaction.Username;
                stringBuilder.AppendFormat(XmlTemplate, headlineId, custId, userName);
            }

            return stringBuilder.Append(BackgroundJobConstants.EndRootTag).ToString();
        }

        protected internal virtual string GetAutoField(
            DateTime feedbackDate,
            string communicationTool,
            IEnumerable<string> licenseeManagers)
        {
            var stringBuilder = new StringBuilder("Date Time & Tool: ");
            stringBuilder.AppendFormat("{0}/{1}/{2}", feedbackDate.ToString("yyyyMMdd"), feedbackDate.ToString("hh:mmtt"), communicationTool);
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.AppendFormat("System ID: {0}", string.Join(", ", licenseeManagers));

            return stringBuilder.ToString();
        }

        protected internal virtual string GetDescriptionForDepositeSite(string messageId, string requestIds)
        {
            var stringBuilder = new StringBuilder(DefaultValue.autoGenerated);
            stringBuilder.AppendFormat(DefaultValue.messageDescription, messageId, requestIds);
            stringBuilder.Append("<br />Announcement sent to affected Licensees and informed our related department found tickets of abnormal accounts.");
            stringBuilder.Append("<br /><br />Please refer to the system email subject \"Voided Bets Report\" for those affected customers &amp; tickets for reference.");
            return stringBuilder.ToString();
        }

        protected internal virtual string GetDescriptionForCreditSite(string messageId, string requestIds, MatchInfo matchInfo, IEnumerable<TransactionInfo> transactionInfos)
        {
            if (matchInfo.IssueTypeId != abnormalAccount)
            {
                var stringBuilder = new StringBuilder(DefaultValue.autoGenerated);
                stringBuilder.AppendFormat(DefaultValue.messageDescription, messageId, requestIds);
                stringBuilder.AppendFormat("<br />Date: {0}", matchInfo.EventDate.ToString("dd-MMM-yyyy"));
                stringBuilder.AppendFormat("<br />Sport: {0}", matchInfo.SportName);
                stringBuilder.AppendFormat("<br />Event Start Time: {0}", matchInfo.KickOffTime);
                stringBuilder.AppendFormat("<br />League: {0}", matchInfo.LeagueName);
                stringBuilder.AppendFormat("<br />Match: {0} - vs - {1}", matchInfo.HomeName, matchInfo.AwayName);
                return stringBuilder.ToString();
            }
            else
            {
                var transactions = transactionInfos.GroupBy(x => x.RelatedCustID).Select(x => x.First()).OrderBy(x => x.RelatedUserName);
                var affectedAccount = string.Join(", ", transactions.Select(x => x.RelatedUserName));
                var stringBuilderAbnormalAccount = new StringBuilder(DefaultValue.autoGenerated);
                stringBuilderAbnormalAccount.AppendFormat(DefaultValue.messageDescription, messageId, requestIds);
                stringBuilderAbnormalAccount.AppendFormat("<br />Event Date: {0} - {1}", matchInfo.MinWinlostdate.ToString("dd-MMM-yyyy"), matchInfo.MaxWinlostdate.ToString("dd-MMM-yyyy"));
                stringBuilderAbnormalAccount.AppendFormat("<br />Affected accounts: {0}", affectedAccount);
                stringBuilderAbnormalAccount.AppendFormat("<br />Decided By: {0}", matchInfo.DecidedBy);
                stringBuilderAbnormalAccount.AppendFormat("<br />Requested by: {0}", matchInfo.Requestby);
                stringBuilderAbnormalAccount.AppendFormat("<br />Voided by: {0}", matchInfo.VoidBy);
                return stringBuilderAbnormalAccount.ToString();
            }
        }

        protected internal virtual string GetDescriptionMixParlayForCreditSite(string messageId, string requestIds, MatchInfo matchInfo)
        {
            var template = commonFeedback.GetFeedbackTemplates<MixParlayTemplate>(templateXml);
            var stringBuilder = new StringBuilder(DefaultValue.autoGenerated);
            stringBuilder.AppendFormat(DefaultValue.messageDescription, messageId, requestIds);
            stringBuilder.Append(BackgroundJobConstants.LineBreak);
            stringBuilder.AppendFormat(template.DescriptionCredit, matchInfo.DecidedBy, matchInfo.Requestby, matchInfo.VoidBy, BackgroundJobConstants.Break);
            return stringBuilder.ToString();
        }

        protected internal virtual string GetSolution(Message message)
        {
            var stringBuilder = new StringBuilder(message.Content_en);
            stringBuilder.Append("<br /><br />").Append(message.Content_cs);
            return stringBuilder.ToString();
        }

        protected internal virtual string GetSolutionForCreditSite(Message message, IEnumerable<TransactionInfo> transactionInfos, int issueTypeId)
        {
            var stringBuilder = new StringBuilder(message.Content_en);
            stringBuilder.Append("<br /><br />").Append(message.Content_cs);
            stringBuilder.Append("<br /><br />Kindly refer to Agency site \"Cancelled Bets\" for Voided user.");
            if (issueTypeId != abnormalAccount)
            {
                foreach (var transaction in transactionInfos)
                {
                    stringBuilder.AppendFormat("<br />{0} {1}", transaction.Username, transaction.TransId);
                }
            }
            return stringBuilder.ToString();
        }

        protected internal virtual string GetSolutionMixParlayForCreditSite(Message message, IEnumerable<TransactionInfo> transactionInfos, bool isVip)
        {
            var stringBuilder = new StringBuilder();
            var stringBuilderAffected = new StringBuilder();
            const string templateAffected = "- {0} {1} {2}";
            var solutionTempate = commonFeedback.GetFeedbackTemplates<MixParlayTemplate>(templateXml);
            foreach (var tran in transactionInfos)
            {
                stringBuilderAffected.AppendFormat(templateAffected, tran.Username, tran.TransId, BackgroundJobConstants.Break);
            }
            if (isVip)
            {
                stringBuilder.AppendFormat(solutionTempate.SolutionCreditENYN, message.Content_en, stringBuilderAffected, BackgroundJobConstants.LineBreak, BackgroundJobConstants.Break);
            }
            else
            {
                stringBuilder.AppendFormat(solutionTempate.SolutionCreditENSC, message.Content_en, message.Content_cs, stringBuilderAffected,
                    BackgroundJobConstants.LineBreak, BackgroundJobConstants.Break
                );
            }
            return stringBuilder.ToString();
        }
    }
}