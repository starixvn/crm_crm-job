namespace CRM.Services.AutoFeedback;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRM.Constants;
using CRM.Criteria.AutoFeedback;
using CRM.Models.AbnormalBet;
using CRM.Models.Common;
using CRM.Models.MixParlay;
using CRM.Services.Common;
using Microsoft.Extensions.Options;

public interface IFeedbackService
{
    Task CreateFeedbackForVoidingAbnormalBetAsync(AbnormalBetSystemSetting setting, bool isUAT,
        DateTime lastRuntimeEnv, string lastRuntimeNameEnv,
        string jobStatusNameEnv, string environment);
}

#pragma warning disable S107
public class FeedbackService(
    IOptions<AppJobSettings> appSettings,
    IBaseService baseService,
    ISettingService settingService,
    IMessageService messageService,
    ICustomerService customerService,
    ICrmLogService logService,
    IFeedbackTicketVoidingService ticketVoidingService,
    ICommonFeedback commonFeedback)
    : IFeedbackService
#pragma warning restore S107
{
    private readonly AppJobSettings appSettings = appSettings.Value;

    private const int abnormalAccount = 13;
    private const int mixParlayIssueTypeId = 11;
    private const int duplicatedTicketIssueTypeId = 20;
    private readonly int[] parlayBetTypeIds = [9, 29, 2799];
    private const string templateXml = "MixParlayTemplate.xml";
    private const string templateDuplicateXml = "FeedbackDuplicateTemplate.xml";

    public async Task CreateFeedbackForVoidingAbnormalBetAsync(AbnormalBetSystemSetting setting, bool isUAT,
        DateTime lastRuntimeEnv, string lastRuntimeNameEnv,
        string jobStatusNameEnv, string environment)
    {
        logService.LogInformation("CreateFeedbackForVoidingAbnormalBetAsync start!!!");

        await commonFeedback.EditSystemSettingAsyncByEnv(jobStatusNameEnv, lastRuntimeNameEnv, JobStatus.Running, BackgroundJobConstants.ServiceName_AutoFB);
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
            environment,
            lastRuntimeEnv);

        logService.LogInformation("after ImportVoidingMessageAsync!!!");

        var failMessages = await messageService.GetFailMessageAsync();
        var messagesImportCombineDistinct = messagesImport.Concat(failMessages).Distinct().ToList();
        if (!messagesImportCombineDistinct.Any())
        {
            logService.LogInformation("Abnormal Bet - import do not have any data");
            await commonFeedback.EditSystemSettingAsyncByEnv(jobStatusNameEnv, lastRuntimeNameEnv, JobStatus.Completed, BackgroundJobConstants.ServiceName_AutoFB);
            return;
        }
        IEnumerable<int> messageIds = messagesImportCombineDistinct.Where(x => x.MessageId != 0).Select(y => y.MessageId).Distinct().ToList();
        var messages = await messageService.GetMessageInfoAsync(messageIds);

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
        var matchInfoCombine = matchInfos.Concat(matchInfosDuplicate);

        if (!matchInfoCombine.Any())
        {
            logService.LogInformation("Abnormal Bet - import do not have any ticket");
            await messageService.EditStatusVoidingMessageAsync(messagesImportCombineDistinct, environment);
            await commonFeedback.EditSystemSettingAsyncByEnv(jobStatusNameEnv, lastRuntimeNameEnv, JobStatus.Completed, BackgroundJobConstants.ServiceName_AutoFB);
            return;
        }

        logService.LogInformation(
            $"Abnormal Bet - Create feedback for voidingHistoryCollectionId: {string.Join(",", matchInfo.Select(x => x.VoidingHistoryCollectionId))}"
        );

        var ticketDuplicate = matchInfo.Where(x => x.IssueTypeId == duplicatedTicketIssueTypeId);
        var ticketDuplicateDetail = duplicateTicketDetails.Where(x => ticketDuplicate.Any(y => x.VoidingHistoryCollectionId == y.VoidingHistoryCollectionId));

        await CreateFeedbackForCreditDepositSiteAsync(
            messages,
            matchInfoCombine,
            transactionTicketsCombine,
            messagesImportCombineDistinct,
            isUAT,
            setting,
            ticketDuplicateDetail
        );
    }

    protected internal virtual async Task CreateFeedbackForCreditDepositSiteAsync(
        IEnumerable<Message> messages,
        IEnumerable<MatchInfo> matchInfos,
        IEnumerable<TransactionInfo> transactionInfo,
        IEnumerable<VoidingMessageUserName> messagesImportCombineDistinct,
        bool isUAT,
        AbnormalBetSystemSetting setting,
        IEnumerable<DuplicateTicketDetail> duplicateTicketDetails
    )
    {
        var settingNames = new[] { SettingName.Product, SettingName.SubProduct, SettingName.Bettype };
        var settings = await settingService.GetAllSettingAsync();
        var settingMappings = await settingService.GetSystemSettingMappingAsync(settingNames);
        var siteSettingMappings = await settingService.GetSiteSettingMappingAsync();
        var licenseeInfo = (await customerService.GetCustomerInfoAsync(appSettings.LicenseeUsername, true)) ?? new CustomerInfo();
        var custIds = transactionInfo.Where(x => !x.IsDeposit).Select(x => x.CustId).Distinct();
        var headlines = await customerService.GetGroupHeadlineVipAsync(custIds);
        var transactionByHeadline = transactionInfo.Where(x => headlines.Any(y => y.CustId == x.CustId));

        var (newMatchInfos, newTransactionInfos) = await ticketVoidingService.ImportTicketVoidingInfoAsync(
            messagesImportCombineDistinct,
            matchInfos,
            transactionInfo,
            siteSettingMappings,
            headlines,
            licenseeInfo);

        foreach (var matchInfo in newMatchInfos)
        {
            var messageContents = messages.Where(x => matchInfo.MessageId.Contains(x.MessageId.ToString()));
            var messageByHeadline = messageContents.Where(x => transactionByHeadline.Any(y => y.MessageId == x.MessageId)).DistinctBy(x => x.MessageId);
            var credits = newTransactionInfos.Where(x => x.VoidingHistoryCollectionId == matchInfo.VoidingHistoryCollectionId && !x.IsDeposit);
            var deposits = newTransactionInfos.Where(x => x.VoidingHistoryCollectionId == matchInfo.VoidingHistoryCollectionId && x.IsDeposit);
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
        var maxVoidingHistoryCollectionId = CalculateMaxVoidingHistoryCollectionId(isUAT, setting, transactionInfo);

        await UpdateSystemSetting(isUAT, setting, maxVoidingHistoryCollectionId);
    }

    private static int CalculateMaxVoidingHistoryCollectionId(bool isUAT, AbnormalBetSystemSetting setting,
        IEnumerable<TransactionInfo> transactionInfo)
    {
        var voidingHistoryCollectionId = isUAT ? setting.MaxVoidingHistoryCollectionIdUAT : setting.MaxVoidingHistoryCollectionId;
        var maxVoidingHistoryCollectionId = transactionInfo.Any() ? transactionInfo.Max(x => x.VoidingHistoryCollectionId) : voidingHistoryCollectionId;
        return maxVoidingHistoryCollectionId;
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
                                                   && x.SettingId == BackgroundJobConstants.ExtensionNA);
        var category = GetCategorySubCategory(settings, matchInfo.IssueTypeId);

        var criteria = new CreateMultipleFeedbackCommand
        {
            CreatorId = appSettings.CreatorId,
            UserId = appSettings.CreatorId,
            UserRole = BackgroundJobConstants.UserRole,
            FeedbackDate = feedbackDate.Subtract(timeSpan),
            ResolutionDate = feedbackDate.AddMinutes(1).Subtract(timeSpan),
            IsCheckedDefaultComm = true,
            StationId = station != null ? station.SettingId.ToString() : string.Empty,
            StatusId = status?.SettingId ?? 0,
            ResponseId = response?.SettingId ?? 0,
            CategoryXml = category.Item3,
            CategoryIds = category.Item1.ToString(),
            SubCategoryIds = category.Item2.ToString(),
            Subject = "Special Notification",
            AutoField = string.Empty,
            AutoField1 = string.Empty,
            AutoField2 = $"Date Time & Tool: {feedbackDate:yyyyMMdd}/{feedbackDate:hh:mmtt}/",
            AutoField3 = $"{Environment.NewLine}System ID: ",
            VoidingHistoryCollectionId = matchInfo.VoidingHistoryCollectionId
        };
        logService.LogInformation("Abnormal Bet - End mapping common setting for VoidingHistoryCollectionId: " + matchInfo.VoidingHistoryCollectionId);

        var headlineIds = headlines.Select(x => x.HeadlineId).Distinct();
        foreach (var headlineId in headlineIds)
        {
            var headline = headlines.First(x => x.HeadlineId == headlineId);
            var relatedCustomerIds = headlines.Where(x => x.HeadlineId == headline.HeadlineId).Select(x => x.CustId);
            criteria.CustomerXml =
                $"<ROOT><Cust CustId='{headline.HeadlineId}' Username='{headline.HeadlineName}' /></ROOT>";

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
        CreateMultipleFeedbackCommand criteria)
#pragma warning restore S107 // Methods should not have too many parameters
    {
        var itemEachBatch = appSettings.NoOfRelatedCustomerPerFeedback;
        var totalBatch = (int)Math.Ceiling((double)relatedCustomerIds.Count() / itemEachBatch);
        var messageJoin = string.Join(",", messages.Select(x => x.MessageId));
        for (var i = 0; i < totalBatch; i++)
        {
            var batch = relatedCustomerIds.Skip(i * itemEachBatch).Take(itemEachBatch).ToList();

            logService.LogInformation(
                $"Abnormal Bet - Begin mapping setting for VoidingHistoryCollectionId: " +
                $"{matchInfo.VoidingHistoryCollectionId}\n\rHeadlineId: {headline.HeadlineId}\n\rRelatedCustIds: {string.Join(",", batch)}");

            var transactions = transactionInfos.Where(x => batch.Exists(y => y == x.CustId));
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

            logService.LogInformation(
                $"Abnormal Bet - End mapping setting for VoidingHistoryCollectionId: " +
                $"{matchInfo.VoidingHistoryCollectionId}\n\rHeadlineId: {headline.HeadlineId}" +
                $"\n\rRelatedCustIds: {string.Join(",", batch)}");
            IEnumerable<int> requestIds = transactions.Select(x => x.RequestId).Distinct().ToList();
            criteria.RequestIds = string.Join(",", requestIds);
            var description = isMixParlay ? GetDescriptionMixParlayForCreditSite(string.Join(",", messages.Select(x => x.MessageId)), criteria.RequestIds, matchInfo)
                : GetDescriptionForCreditSite(string.Join(",", messages.Select(x => x.MessageId)), criteria.RequestIds, matchInfo, transactions);
            criteria.Description = description;
            try
            {
                logService.LogInformation(
                    $"Abnormal Bet - Begin create feedback for VoidingHistoryCollectionId: {matchInfo.VoidingHistoryCollectionId}" +
                    $"\n\rHeadlineId: {headline.HeadlineId}\n\rRelatedCustIds: {string.Join(",", batch)}");
                var commandResult = await baseService.ExecuteNonQueryAsync(criteria);
                if (commandResult.IsSuccessful)
                {
                    logService.LogInformation(
                        $"Abnormal Bet - Create feedback successfully. VoidingHistoryCollectionId: {matchInfo.VoidingHistoryCollectionId}" +
                        $"\n\rHeadlineId: {headline.HeadlineId}\n\rRelatedCustIds: {string.Join(",", batch)}");
                }
                else
                {
                    logService.LogInformation(
                        $"Abnormal Bet - {commandResult.Message}. VoidingHistoryCollectionId: {matchInfo.VoidingHistoryCollectionId}\n\r" +
                        $"HeadlineId: {headline.HeadlineId}\n\rRelatedCustIds: {string.Join(",", batch)}");
                    await ticketVoidingService.EditFeedbackInfoAsync(messageJoin, requestIds, batch, CreateFeedbackJobStatus.Fail, commandResult.Message);
                    await logService.ErrorAsync(commandResult, criteria);
                }
            }
            catch (Exception ex)
            {
                logService.LogInformation(
                    $"Abnormal Bet - {ex.Message}. VoidingHistoryCollectionId: {matchInfo.VoidingHistoryCollectionId}\n\r" +
                    $"HeadlineId: {headline.HeadlineId}\n\rRelatedCustIds: {string.Join(",", batch)}");
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
                                                   && x.SettingId == BackgroundJobConstants.ExtensionNA);
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
            StatusId = status?.SettingId ?? 0,
            LanguageId = licenseeInfo.LanguageId,
            ResponseId = response?.SettingId ?? 0,
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
        var itemEachBatch = appSettings.NoOfRelatedCustomerPerFeedback;
        var totalBatch = (int)Math.Ceiling((double)licenseeManagerIds.Count / itemEachBatch);
        for (var i = 0; i < totalBatch; i++)
        {
            IList<int> batch = licenseeManagerIds.Skip(i * itemEachBatch).Take(itemEachBatch).ToList();
            IList<TransactionInfo> transactions = transactionInfos.Where(x => batch.Any(y => y == x.LicenseeManagerId)).ToList();
            var licenseeManagers = transactions.Where(x => x.LicenseeManagerId != 0).Select(x => x.LicenseeManager).Distinct().ToList();
            IList<int> custIds = transactions.Select(x => x.CustId).Distinct().ToList();
            var relatedIds = batch.ToList();
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
                GetDescriptionDuplicateForDepositSite(duplicateTickets.Where(duplicateTicket => duplicateTicket.IsDeposit), messages, joinMessageString, string.Join(",", requestIds)) :
                GetDescriptionForDepositSite(joinMessageString, criteria.RequestIds);
            try
            {
                logService.LogInformation(
                    $"Abnormal Bet - Begin create feedback for MessageId: {joinMessageString}\n\rHeadlineId: {criteria.CustId}" +
                    $"\n\rRelatedCustIds: {string.Join(",", batch)}\n\rCustIds: {string.Join(",", custIds)}");
                var commandResult = await baseService.ExecuteNonQueryAsync(criteria);
                if (commandResult.IsSuccessful)
                {
                    logService.LogInformation(
                        $"Abnormal Bet - Create feedback successfully. MessageId: {joinMessageString}\n\r" +
                        $"HeadlineId: {criteria.CustId}\n\rRelatedCustIds: {string.Join(",", batch)}\n\rCustIds: {string.Join(",", custIds)}");
                }
                else
                {
                    logService.LogInformation(
                        $"Abnormal Bet - {commandResult.Message}. MessageId: {joinMessageString}\n\r" +
                        $"HeadlineId: {criteria.CustId}\n\rRelatedCustIds: {string.Join(",", batch)}\n\rCustIds: {string.Join(",", custIds)}");
                    await ticketVoidingService.EditFeedbackInfoAsync(joinMessageString, requestIds, custIds, CreateFeedbackJobStatus.Fail, commandResult.Message);
                    await logService.ErrorAsync(commandResult, criteria);
                }
            }
            catch (Exception ex)
            {
                logService.LogInformation(
                    $"{ex.Message}. MessageId: {joinMessageString}\n\rHeadlineId: {criteria.CustId}\n\rRelatedCustIds: {string.Join(",", batch)}" +
                    $"\n\rCustIds: {string.Join(",", custIds)}");
                await ticketVoidingService.EditFeedbackInfoAsync(joinMessageString, requestIds, custIds, CreateFeedbackJobStatus.Fail, ex.Message);
                await logService.ErrorAsync(ex);
            }
        }
    }

    protected internal virtual void GetLicenseeManager(
        IEnumerable<TransactionInfo> transactionInfos,
        IEnumerable<SiteSettingMapping> siteSettingMappings)
    {
        foreach (var transaction in transactionInfos)
        {
            var licenseeManager = siteSettingMappings.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.SettingSource)
                                                                          && x.SettingSource.Equals(transaction.Site, StringComparison.OrdinalIgnoreCase));

            if (licenseeManager != null)
            {
                transaction.LicenseeManagerId = licenseeManager.LicenseeManagerId;
                transaction.LicenseeManager = licenseeManager.LicenseeManager;
            }
        }
    }

    protected internal virtual string GetDescriptionDuplicateForDepositSite(IEnumerable<DuplicateTicketDetail> duplicateTicketDetails,
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
                var remainTicket = ticket.Key.Split(',').Length > 1 ? string.Empty : ticket.Key;
                stringBuilderTicket.AppendFormat(templateDuplicate.EffectMemberTicketDesc, remainTicket, refNoVoided, BackgroundJobConstants.Break, BackgroundJobConstants.LineBreak);
            }
        }

        stringBuilder.AppendFormat(templateDuplicate.Description, messageIds, requestIds, noOfRemainedTickets,
            message.Content_en, message.Content_cs,
            stringBuilderTicket, BackgroundJobConstants.Break, BackgroundJobConstants.LineBreak);

        return stringBuilder.ToString();
    }

    protected internal virtual SettingItem GetCommunicationTool(IEnumerable<SettingItem> settings, int issueTypeId)
    {
        var settingValue = issueTypeId == duplicatedTicketIssueTypeId ? "Email" : "Skype";
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

        var products = GetProductByIds(settings, settingMappings, productIds);
        var productSettingIds = products.Select(x => x.SettingId);

        var subProducts = GetSubProducts(isMixParlay, productSettingIds, sportTypeIds, settings, settingMappings);

        IList<SystemSettingMapping> betTypesMapping = GetBetTypes(isMixParlay, listBetTypes.ToArray(), settingMappings);

        if (products.Any())
        {
            foreach (var product in products)
            {
                var productSubXml = GetProductSubXml(product, subProducts, betTypesMapping);
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

        var productMappings = settingMappings.Where(p => p.SettingName.Equals(SettingName.Product, StringComparison.OrdinalIgnoreCase)
                                                         && !string.IsNullOrWhiteSpace(p.SettingSource)
                                                         && productNames.Exists(productName => p.SettingSource.Equals(productName, StringComparison.OrdinalIgnoreCase))).ToList();

        var products = settings.Where(x => x.SettingName.Equals(SettingName.Product, StringComparison.OrdinalIgnoreCase)
                                           && productMappings.Exists(mapping => x.SettingId == mapping.SettingId)).ToList();

        logService.LogInformation("End mapping amendment product.");

        return products.Any()
            ? products.GroupBy(x => x.SettingId).Select(x => x.First()).ToList()
            : [new SettingItem { SettingId = BackgroundJobConstants.ProductNA }];
    }

    private IList<SettingItem> GetSubProducts(
        bool isMixParlay,
        IEnumerable<int> productSettingIds,
        IEnumerable<short> sportTypeIds,
        IEnumerable<SettingItem> settings,
        IEnumerable<SystemSettingMapping> settingMappings)
    {
        logService.LogInformation("Begin mapping sub-product.");
        var sportNames = new List<string>();
        foreach (var sportTypeId in sportTypeIds)
        {
            var sportName = commonFeedback.GetSportName(sportTypeId.ToString(), Language.en_US);
            sportNames.Add(sportName);
        }
        var mappings = settingMappings.Where(mapping => mapping.SettingName.Equals(SettingName.SubProduct, StringComparison.OrdinalIgnoreCase)
                                                        && !string.IsNullOrWhiteSpace(mapping.SettingSource)
                                                        && sportNames.Exists(sportName => mapping.SettingSource.Equals(sportName, StringComparison.OrdinalIgnoreCase))).ToList();

        var subProducts = isMixParlay ? settings.Where(x => x.SettingName.Equals(SettingName.SubProduct, StringComparison.OrdinalIgnoreCase)
                                                            && x.SettingValue.Equals("Multiple Sports", StringComparison.OrdinalIgnoreCase)).ToList() :
            settings.Where(sub => sub.SettingName.Equals(SettingName.SubProduct, StringComparison.OrdinalIgnoreCase)
                                  && sub.ParentIds.Any(parent => productSettingIds.Any(productName => productName == parent))
                                      && mappings.Exists(mapping => mapping.SettingId == sub.SettingId)).ToList();

        logService.LogInformation("End mapping sub-product.");

        return subProducts.Any()
            ? subProducts.GroupBy(x => x.SettingId).Select(x => x.First()).ToList()
            :
            [
                new()
                {
                    SettingId = BackgroundJobConstants.SubProductNA,
                    ParentIds = new List<int> { BackgroundJobConstants.ProductNA }
                }
            ];
    }

    private List<SystemSettingMapping> GetBetTypes(
        bool isMixParlay,
        string[] listBetTypes,
        IEnumerable<SystemSettingMapping> settingMappings)
    {
        logService.LogInformation("Begin mapping betType.");
        var betTypeMappings = new List<string>();

        foreach (var betType in listBetTypes)
        {
            var betTypeName = commonFeedback.GetBetTypeName(betType, Language.en_US);
            betTypeMappings.Add(betTypeName);
        }
        var betTypes = isMixParlay ? settingMappings.Where(mapping => mapping.SettingName.Equals(SettingName.Bettype, StringComparison.OrdinalIgnoreCase)
                                                                      && !string.IsNullOrWhiteSpace(mapping.SettingSource)
                                                                      && mapping.SettingSource.Equals("Mix Parlay", StringComparison.OrdinalIgnoreCase)).ToList() :
            settingMappings.Where(mapping => mapping.SettingName.Equals(SettingName.Bettype, StringComparison.OrdinalIgnoreCase)
                                             && !string.IsNullOrWhiteSpace(mapping.SettingSource)
                                             && betTypeMappings.Exists(name => name.Equals(mapping.SettingSource, StringComparison.OrdinalIgnoreCase))).ToList();

        logService.LogInformation("End mapping betType.");
        return betTypes.Any()
            ? betTypes.GroupBy(x => x.SettingId).Select(x => x.First()).ToList()
            : [new SystemSettingMapping { SettingId = BackgroundJobConstants.BettypeNA }];
    }

    private static string GetProductSubXml(
        SettingItem product,
        IList<SettingItem> subProducts,
        IList<SystemSettingMapping> betTypesMapping)
    {
        var stringBuilder = new StringBuilder();
        if (!subProducts.Any())
        {
            stringBuilder.AppendFormat(DefaultValue.ProductXml, product.SettingId, BackgroundJobConstants.SubProductNA, BackgroundJobConstants.BettypeNA);
        }
        else
        {
            foreach (var subProduct in subProducts)
            {
                if (betTypesMapping.Any())
                {
                    var betTypeXml = GetBetTypeXml(product, subProduct, betTypesMapping);
                    stringBuilder.Append(betTypeXml);
                }
                else
                {
                    stringBuilder.AppendFormat(DefaultValue.ProductXml, product.SettingId, subProduct.SettingId, BackgroundJobConstants.BettypeNA);
                }
            }
        }
        return stringBuilder.ToString();
    }

    private static string GetBetTypeXml(
        SettingItem product,
        SettingItem subProduct,
        IList<SystemSettingMapping> betTypesMapping)
    {
        var stringBuilder = new StringBuilder();
        foreach (var betType in betTypesMapping)
        {
            stringBuilder.AppendFormat(DefaultValue.ProductXml, product.SettingId, subProduct.SettingId, betType.SettingId);
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
        var sportsbook = GetSportsbookProduct(settings);
        var subProducts = GetSubProduct(isMixParlay, sportsbook.SettingId, matchInfo, settings, settingMappings);
        var betTypes = GetBetType(isMixParlay, subProducts.Select(x => x.SettingId), transactionInfos, settings, settingMappings);
        var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);

        foreach (var settingId in subProducts.Select(x => x.SettingId))
        {
            IList<SettingItem> tempBetTypes = betTypes.Where(x => x.ParentIds.Any(y => y == settingId)).ToList();
            if (tempBetTypes.Any())
            {
                foreach (var betType in tempBetTypes)
                {
                    stringBuilder.AppendFormat(XmlTemplate, sportsbook.SettingId, settingId, betType.SettingId);
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
                                                              && mappings.Any(mapping => mapping.SettingId == sub.SettingId)).ToList();

        var subProductMixParlay = settings.Where(x => x.SettingName.Equals(SettingName.SubProduct, StringComparison.OrdinalIgnoreCase)
                                                      && x.SettingValue.Equals("Multiple Sports", StringComparison.OrdinalIgnoreCase));

        var subProducts = isMixParlay ? subProductMixParlay : subProduct;

        return subProducts.Any()
            ? subProducts.GroupBy(x => x.SettingId).Select(x => x.First()).ToList()
            :
            [
                new SettingItem
                {
                    SettingId = BackgroundJobConstants.SubProductNA,
                    ParentIds = new List<int> { BackgroundJobConstants.ProductNA }
                }
            ];
    }

    protected internal virtual IList<SettingItem> GetBetType(
        bool isMixParlay,
        IEnumerable<int> subProducts,
        IEnumerable<TransactionInfo> transactionInfos,
        IEnumerable<SettingItem> settings,
        IEnumerable<SystemSettingMapping> settingMappings)
    {
        var transactionAbnormalAccount = transactionInfos.Where(x => x.Bettype != null);
        List<SettingItem> betTypes;
        if (isMixParlay)
        {
            betTypes = GetBetTypeParlay(subProducts, settings, settingMappings);
        }
        else
        {
            IList<SystemSettingMapping> mappings = settingMappings.Where(mapping => mapping.SettingName.Equals(SettingName.Bettype, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(mapping.SettingSource)
                && transactionAbnormalAccount.Any(tran => tran.Bettype.Equals(mapping.SettingSource, StringComparison.OrdinalIgnoreCase))).ToList();

            betTypes = settings.Where(betType => betType.SettingName.Equals(SettingName.Bettype, StringComparison.OrdinalIgnoreCase)
                                                 && subProducts.Any(sub => betType.ParentIds.Any(parent => parent == sub))
                                                 && mappings.Any(mapping => mapping.SettingId == betType.SettingId)).ToList();
        }

        return betTypes != null && betTypes.Any()
            ? betTypes.GroupBy(x => x.SettingId).Select(x => x.First()).ToList()
            :
            [
                new()
                {
                    SettingId = BackgroundJobConstants.BettypeNA,
                    ParentIds = new List<int> { BackgroundJobConstants.SubProductNA }
                }
            ];
    }

    protected internal virtual List<SettingItem> GetBetTypeParlay(
        IEnumerable<int> subProducts,
        IEnumerable<SettingItem> settings,
        IEnumerable<SystemSettingMapping> settingMappings)
    {
        IList<SystemSettingMapping> mappingsParlay = settingMappings.Where(mapping => mapping.SettingName.Equals(SettingName.Bettype, StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(mapping.SettingSource)
            && mapping.SettingSource.Equals("Mix Parlay", StringComparison.OrdinalIgnoreCase)).ToList();

        var betTypes = settings.Where(betType => betType.SettingName.Equals(SettingName.Bettype, StringComparison.OrdinalIgnoreCase)
                                                 && subProducts.Any(sub => betType.ParentIds.Any(parent => parent == sub))
                                                 && mappingsParlay.Any(mapping => mapping.SettingId == betType.SettingId)).ToList();

        return betTypes;
    }

    private static string GetSubCategorySettingValue(int issueTypeId)
    {
        var subCategory = "Abnormal Bets";
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

    protected internal virtual Tuple<int, int, string> GetCategorySubCategory(IEnumerable<SettingItem> settings, int issueTypeId)
    {
        var category = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.MainCategory, StringComparison.OrdinalIgnoreCase)
                                                    && x.SettingValue.Equals("Special Notification", StringComparison.OrdinalIgnoreCase));
        var subCategoryId = 0;

        if (category != null)
        {
            subCategoryId = CalculateSubCategoryId(settings, category, issueTypeId);
        }

        return new Tuple<int, int, string>(
            category?.SettingId ?? 0,
            subCategoryId,
            $"<ROOT><Category MainCategoryID='{category?.SettingId ?? 0}' SubCategoryID='{subCategoryId}' /></ROOT>");
    }

    private static int CalculateSubCategoryId(IEnumerable<SettingItem> settings, SettingItem category, int issueTypeId)
    {
        var subCategory = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.SubCategory, StringComparison.OrdinalIgnoreCase)
                                                       && x.ParentIds.Any(y => y == category.SettingId)
                                                       && x.SettingValue.Equals(GetSubCategorySettingValue(issueTypeId), StringComparison.OrdinalIgnoreCase));
        return subCategory?.SettingId ?? 0;
    }

    protected internal virtual string GetRelatedCustomerXml(int headlineId, IEnumerable<TransactionInfo> transactionInfos, int IssueTypeId)
    {
        const string XmlTemplate = "<Row CustId='{0}' RelatedCustId='{1}' Username='{2}' />";
        var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);

        var transactions = IssueTypeId == abnormalAccount ? transactionInfos.GroupBy(x => x.RelatedCustID).Select(x => x.First()) : transactionInfos.GroupBy(x => x.CustId).Select(x => x.First());
        foreach (var transaction in transactions)
        {
            var customerId = IssueTypeId == abnormalAccount ? transaction.RelatedCustID : transaction.CustId;
            var userName = IssueTypeId == abnormalAccount ? transaction.RelatedUserName : transaction.Username;
            stringBuilder.AppendFormat(XmlTemplate, headlineId, customerId, userName);
        }

        return stringBuilder.Append(BackgroundJobConstants.EndRootTag).ToString();
    }

    protected internal virtual string GetAutoField(
        DateTime feedbackDate,
        string communicationTool,
        IEnumerable<string> licenseeManagers)
    {
        var stringBuilder = new StringBuilder("Date Time & Tool: ");
        stringBuilder.Append($"{feedbackDate:yyyyMMdd}/{feedbackDate.ToString("hh:mmtt")}/{communicationTool}");
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append($"System ID: {string.Join(", ", licenseeManagers)}");

        return stringBuilder.ToString();
    }

    protected internal virtual string GetDescriptionForDepositSite(string messageId, string requestIds)
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
            stringBuilder.Append($"<br />Date: {matchInfo.EventDate:dd-MMM-yyyy}");
            stringBuilder.Append($"<br />Sport: {matchInfo.SportName}");
            stringBuilder.Append($"<br />Event Start Time: {matchInfo.KickOffTime}");
            stringBuilder.Append($"<br />League: {matchInfo.LeagueName}");
            stringBuilder.Append($"<br />Match: {matchInfo.HomeName} - vs - {matchInfo.AwayName}");
            return stringBuilder.ToString();
        }
        else
        {
            var transactions = transactionInfos.GroupBy(x => x.RelatedCustID).Select(x => x.First()).OrderBy(x => x.RelatedUserName);
            var affectedAccount = string.Join(", ", transactions.Select(x => x.RelatedUserName));
            var stringBuilderAbnormalAccount = new StringBuilder(DefaultValue.autoGenerated);
            stringBuilderAbnormalAccount.AppendFormat(DefaultValue.messageDescription, messageId, requestIds);
            stringBuilderAbnormalAccount.Append(
                $"<br />Event Date: {matchInfo.MinWinLostDate:dd-MMM-yyyy} - {matchInfo.MaxWinLostDate:dd-MMM-yyyy}");
            stringBuilderAbnormalAccount.Append($"<br />Affected accounts: {affectedAccount}");
            stringBuilderAbnormalAccount.Append($"<br />Decided By: {matchInfo.DecidedBy}");
            stringBuilderAbnormalAccount.Append($"<br />Requested by: {matchInfo.RequestBy}");
            stringBuilderAbnormalAccount.Append($"<br />Voided by: {matchInfo.VoidBy}");
            return stringBuilderAbnormalAccount.ToString();
        }
    }

    protected internal virtual string GetDescriptionMixParlayForCreditSite(string messageId, string requestIds, MatchInfo matchInfo)
    {
        var template = commonFeedback.GetFeedbackTemplates<MixParlayTemplate>(templateXml);
        var stringBuilder = new StringBuilder(DefaultValue.autoGenerated);
        stringBuilder.AppendFormat(DefaultValue.messageDescription, messageId, requestIds);
        stringBuilder.Append(BackgroundJobConstants.LineBreak);
        stringBuilder.AppendFormat(template.DescriptionCredit, matchInfo.DecidedBy, matchInfo.RequestBy, matchInfo.VoidBy, BackgroundJobConstants.Break);
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
                stringBuilder.Append($"<br />{transaction.Username} {transaction.TransId}");
            }
        }
        return stringBuilder.ToString();
    }

    protected internal virtual string GetSolutionMixParlayForCreditSite(Message message, IEnumerable<TransactionInfo> transactionInfos, bool isVip)
    {
        var stringBuilder = new StringBuilder();
        var stringBuilderAffected = new StringBuilder();
        const string templateAffected = "- {0} {1} {2}";
        var solutionTemplate = commonFeedback.GetFeedbackTemplates<MixParlayTemplate>(templateXml);
        foreach (var tran in transactionInfos)
        {
            stringBuilderAffected.AppendFormat(templateAffected, tran.Username, tran.TransId, BackgroundJobConstants.Break);
        }
        if (isVip)
        {
            stringBuilder.AppendFormat(solutionTemplate.SolutionCreditENYN, message.Content_en, stringBuilderAffected, BackgroundJobConstants.LineBreak, BackgroundJobConstants.Break);
        }
        else
        {
            stringBuilder.AppendFormat(solutionTemplate.SolutionCreditENSC, message.Content_en, message.Content_cs, stringBuilderAffected,
                BackgroundJobConstants.LineBreak, BackgroundJobConstants.Break
            );
        }
        return stringBuilder.ToString();
    }
}