namespace CRM.BackgroundJob.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Constants;
using Criteria.AutoFeedback;
using CRM.BackgroundJob.Criteria.VoidingSettledTickets;
using CRM.BackgroundJob.Models.AmendmentNotiFB;
using CRM.BackgroundJob.Models.VoidingSettledTickets;
using CRM.Models;
using CRM.Services;
using Microsoft.Extensions.Options;
using Models;

public interface IVoidingSettledTicketsFeedbackService
{
    Task CreateFeedbackForVoidingSettledTickets(AmendmentNotiFbSetting voidingSettledTicketSetting, bool isUAT,
        DateTime lastRuntimeEnv, string lastRuntimeNameEnv,
        string jobStatusNameEnv, string environment);
}

public class VoidingSettledTicketsFeedbackService : IVoidingSettledTicketsFeedbackService
{
    private readonly AppJobSettings appSettings;
    private readonly IBaseService baseService;
    private readonly ISettingService settingService;
    private readonly ICrmLogService logService;
    private readonly ICommonFeedback commonFeedback;
    private readonly IVoidingSettledTicketsService voidingSettledTicketsService;
    private readonly IFeedbackTransactionService feedbackTransaction;

    private readonly string descTemplateXml = "VoidingSettledTicketDescriptionTemplate.xml";
    private readonly string solTemplateXml = "VoidingSettledTicketSolutionTemplate.xml";
    private readonly int[] MixParlayMatchIds = { 9, 29, 2799, 10449, 11099 };

    private readonly int EventType2 = 2;
    private readonly int EventType3 = 3;
    private readonly int EventType5 = 5;
    private readonly int SportType180 = 180;
    private readonly int SportType186 = 186;
    private readonly int SportType161 = 161;
    private const int DuplicateTicketIssueTypeId = 20;

#pragma warning disable S107 // Methods should not have too many parameters

    public VoidingSettledTicketsFeedbackService(
        IOptions<AppJobSettings> appSettings,
        IBaseService baseService,
        ISettingService settingService,
        ICrmLogService logService,
        ICommonFeedback commonFeedback,
        IVoidingSettledTicketsService voidingSettledTicketsService,
        IFeedbackTransactionService feedbackTransaction
    )
#pragma warning restore S107 // Methods should not have too many parameters
    {
        this.appSettings = appSettings.Value;
        this.baseService = baseService;
        this.settingService = settingService;
        this.logService = logService;
        this.commonFeedback = commonFeedback;
        this.voidingSettledTicketsService = voidingSettledTicketsService;
        this.feedbackTransaction = feedbackTransaction;
    }

    public async Task CreateFeedbackForVoidingSettledTickets(AmendmentNotiFbSetting voidingSettledTicketSetting, bool isUAT,
        DateTime lastRuntimeEnv, string lastRuntimeNameEnv,
        string jobStatusNameEnv, string environment)
    {
        logService.LogInformation("start service voiding settled!!!");
        await commonFeedback.EditSystemSettingAsyncByEnvFormatDate(jobStatusNameEnv, lastRuntimeNameEnv, JobStatus.Running, BackgroundJobConstants.ServiceName_AutoVoidFB);

        var settingNames = new[] { SettingName.Product, SettingName.SubProduct, SettingName.Bettype };
        var settings = await settingService.GetAllSettingAsync();
        var settingMappings = await settingService.GetSystemSettingMappingAsync(settingNames);

        var messageVoidSettled = await voidingSettledTicketsService.GetMessageVoidSettledAsync();
        var voidingMessageCheck = await voidingSettledTicketsService.GetVoidingMessasgeCheckAsync(messageVoidSettled, environment);
        if (voidingMessageCheck.Any())
        {
            var settledTicket = await voidingSettledTicketsService.VoidingMessageGetSettledTicketAsync(voidingMessageCheck);
            var commandMessage = await voidingSettledTicketsService.ImportMessageVoidSettledAsync(settledTicket);
            if (!commandMessage.IsSuccessful)
            {
                logService.LogInformation("Import Messages Settled Fail!!!");
                return;
            }
        }
        var voidingMessages = await voidingSettledTicketsService.GetAutoVoidingMessageAsync(voidingSettledTicketSetting, environment);
        var failVoidingMessages = await voidingSettledTicketsService.GetFailVoidingMessageAsync();
        var voidingMessageDistinct = voidingMessages.Concat(failVoidingMessages).Distinct().ToList();
        logService.LogInformation("before GetAutoVoidingTicketAsync!!!");
        var (voidingTickets, msgTargets) = await voidingSettledTicketsService.GetAutoVoidingTicketAsync(voidingSettledTicketSetting, voidingMessageDistinct, environment);
        logService.LogInformation("end GetAutoVoidingTicketAsync!!!");

        logService.LogInformation(
            $"GetAutoVoidingTicketAsync Item1: {string.Join(",", voidingTickets?.Select(x => x.MessageId))}, {string.Join(",", voidingTickets?.Select(x => x.TransId))}"
        );
        if (msgTargets.Any())
        {
            await voidingSettledTicketsService.UpdateAutoVoidingMessageAsync(msgTargets, environment);
        }
        if (!voidingTickets.Any())
        {
            await commonFeedback.EditSystemSettingAsyncByEnvFormatDate(jobStatusNameEnv, lastRuntimeNameEnv, JobStatus.Completed, BackgroundJobConstants.ServiceName_AutoVoidFB);
            logService.LogInformation("Do not have any data set1, return voiding settled tickets!!!");
            return;
        }
        await voidingSettledTicketsService.ImportVoidingAutoFeedbackInfoAsync(voidingTickets, environment);
        var (feedbackInfo, eventVoiding) = await voidingSettledTicketsService.GetVoidingAutoFeedbackInfoAsync(voidingSettledTicketSetting.BatchSize);

        var messageIssueTypes = await baseService.GetDataAsync<MsgIssueType>(new GetAutoVoidingIssueTypeCriteria { VoidingMessages = feedbackInfo.Select(i => i.MessageId).ToList() });
        foreach (var fb in feedbackInfo)
        {
            var msg = messageIssueTypes.FirstOrDefault(i => i.MessageId == fb.MessageId);
            if (msg is not null)
            {
                fb.IssueTypeId = msg.IssueTypeId;
            }
        }

        var groupFeedbackByMsgId = feedbackInfo.GroupBy(x => new { x.HeadlineId, x.MessageId }).Select(y => y.ToList());

        foreach (var feedback in groupFeedbackByMsgId)
        {
            var feedbackInfoFirst = feedback[0];
            var eventDetails = eventVoiding.Where(x => x.LogVoidingID == feedbackInfoFirst.LogVoidingID);
            var messages = await feedbackTransaction.GetMessageContentInfoAsync(feedbackInfoFirst.MessageId);
            await CreateVoidingSettledTicketsAsync(feedbackInfoFirst, eventDetails, messages, settings, settingMappings, environment);
        }

        await commonFeedback.EditSystemSettingAsyncByEnvFormatDate(jobStatusNameEnv, lastRuntimeNameEnv, JobStatus.Completed, BackgroundJobConstants.ServiceName_AutoVoidFB);
        logService.LogInformation("end service voiding settled!!!");
    }

    protected internal virtual async Task CreateVoidingSettledTicketsAsync(
        VoidingAutoFeedbackInfo feedbackInfo,
        IEnumerable<AutoVoidingEventDetails> eventDetails,
        IEnumerable<Message> messages,
        IEnumerable<SettingItem> settings,
        IEnumerable<SystemSettingMapping> settingMappings,
        string environment)
    {
        logService.LogInformation("begin CreateVoidingSettledTicketsAsync!!!");
        var timeSpan = new TimeSpan(appSettings.TimeSpan, 0, 0);
        var feedbackDate = DateTimeOffset.Now.DateTime.Add(timeSpan);
        var response = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.Response, StringComparison.OrdinalIgnoreCase)
                                                    && x.SettingValue.Equals(DefaultValue.Neutral, StringComparison.OrdinalIgnoreCase));
        var station = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.Extension, StringComparison.OrdinalIgnoreCase)
                                                   && x.SettingId == BackgroundJobConstants.ExtentionNA);
        var status = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.Status, StringComparison.OrdinalIgnoreCase)
                                                  && x.SettingValue.Equals(DefaultValue.Closed, StringComparison.OrdinalIgnoreCase));

        var category = GetCategorySubCategoryVoidingSettledTicket(settings, DefaultValue.ResultAmended, feedbackInfo.IssueTypeId);
        var sportTypeIds = eventDetails.Select(x => x.Sporttype);
        var productIds = eventDetails.Select(x => x.ProductId).Distinct();
        var isMixParlay = eventDetails.All(e => MixParlayMatchIds.Contains(e.MatchId));
        var criteria = new CreateMutipleFeedbackCommand
        {
            IsCheckedDefaultComm = true,
            StationId = station != null ? station.SettingId.ToString() : string.Empty,
            StatusId = status != null ? status.SettingId : 0,
            FeedbackDate = feedbackDate.Subtract(timeSpan),
            ResolutionDate = feedbackDate.AddMinutes(1).Subtract(timeSpan),
            CreatorId = appSettings.CreatorId,
            UserId = appSettings.CreatorId,
            CategoryXml = category.Item3,
            CategoryIds = category.Item1.ToString(),
            SubCategoryIds = category.Item2.ToString(),
            ResponseId = response != null ? response.SettingId : 0,
            AutoField2 = string.Format(DefaultValue.DateTimeTool, feedbackDate.ToString("yyyyMMdd"), feedbackDate.ToString("hh:mmtt")),
            AutoField = string.Empty,
            AutoField1 = string.Empty,
            UserRole = BackgroundJobConstants.UserRole,
            Subject = feedbackInfo.IssueTypeId == DuplicateTicketIssueTypeId ? DefaultValue.Special_Notification : DefaultValue.ResultAmended,
            AutoField3 = $"{Environment.NewLine}System ID: ",
            CustomerXml = string.Format(DefaultValue.CustomerXml, feedbackInfo.HeadlineId, feedbackInfo.HeadlineName),
            ProductXml = GetVoidingSettledTicketsProductXml(isMixParlay, productIds, sportTypeIds, feedbackInfo.Listbettype, settings, settingMappings),
            Solution = GetVoidingSettledTicketSolution(feedbackInfo, messages),
            RelatedCustomerXml = GetRelatedCustomerXml(feedbackInfo),
            MessageId = feedbackInfo.MessageId.ToString(),
            ListOrigCustIds = feedbackInfo.AffectedId,
            VoidingSettledID = feedbackInfo.LogVoidingID,
            Description = GetVoidingSettledTicketDescription(feedbackInfo, eventDetails)
        };

        logService.LogInformation("begin CheckValidVoidingSettledParamsAndInsertFeedbackAsync!!!");
        await CheckValidVoidingSettledParamsAndInsertFeedbackAsync(feedbackInfo, criteria, environment);
        logService.LogInformation("end CreateVoidingSettledTicketsAsync!!!");
    }

    protected internal virtual async Task CheckValidVoidingSettledParamsAndInsertFeedbackAsync(
        VoidingAutoFeedbackInfo feedbackInfo,
        CreateMutipleFeedbackCommand criteria,
        string environment)
    {
        var checkValidParamBeforeInsert = commonFeedback.CheckFeedbackValidParams(criteria);
        if (!checkValidParamBeforeInsert.Item1)
        {
            var messageInvalid = "Invalid Params: " + checkValidParamBeforeInsert.Item2;
            logService.LogInformation(
                $"Invalid Params. @Categorystr: {criteria.CategoryIds} @Category: {criteria.CategoryXml} @Product: {criteria.ProductXml} @StatusID: {criteria.StatusId} @SubCategorystr:{criteria.SubCategoryIds} @ResponseID:{criteria.ResponseId} @StationID:{criteria.StationId}");
            await voidingSettledTicketsService.UpdateVoidingAutoFeedbackInfoBatchAsync(feedbackInfo.LogVoidingID, CreateFeedbackJobStatus.Fail, messageInvalid, environment);
            return;
        }
        try
        {
            logService.LogInformation("begin insert voiding settled tickets!!!");

            var commandResult = await baseService.ExecuteNonQueryAsync(criteria);
            if (commandResult.IsSuccessful)
            {
                logService.LogInformation(
                    $"Create voiding settled tickets feedback successfully, HeadlineId {feedbackInfo.HeadlineId} HeadlineName:{feedbackInfo.HeadlineName}");
            }
            else
            {
                logService.LogInformation(
                    $"Create voiding settled tickets feedback fail, HeadlineId {feedbackInfo.HeadlineId} HeadlineName:{feedbackInfo.HeadlineName}");

                await voidingSettledTicketsService.UpdateVoidingAutoFeedbackInfoBatchAsync(feedbackInfo.LogVoidingID, CreateFeedbackJobStatus.Fail, commandResult.Message, environment);

                await logService.ErrorAsync(commandResult, criteria);
            }

            logService.LogInformation("end insert voiding settled tickets!!!");
        }
        catch (Exception ex)
        {
            await voidingSettledTicketsService.UpdateVoidingAutoFeedbackInfoBatchAsync(feedbackInfo.LogVoidingID, CreateFeedbackJobStatus.Fail, ex.Message, environment);
            await logService.ErrorAsync(ex);
        }
    }

    protected internal virtual string GetVoidingSettledTicketsProductXml(
        bool isMixParlay,
        IEnumerable<int> productIds,
        IEnumerable<short> sportTypeIds,
        string betTypes,
        IEnumerable<SettingItem> settings,
        IEnumerable<SystemSettingMapping> settingMappings)
    {
        var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);
        var listBetTypes = betTypes.Split(",");
        var products = GetVoidingSettledTicketsProduct(settings, settingMappings, productIds);
        var productSettingIds = products.Select(x => x.SettingId);
        var subProducts = GetVoidingSettledTicketsSubProduct(isMixParlay, productSettingIds, sportTypeIds, settings, settingMappings);
        IList<SystemSettingMapping> betTypesMapping = GetVoidingSettledTicketsBetType(isMixParlay, listBetTypes, settingMappings);
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

    protected internal virtual string GetProductSubXml(
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

    protected internal virtual string GetBetTypeXml(
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

    protected internal virtual IList<SettingItem> GetVoidingSettledTicketsProduct(
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
                                           && productMappings != null
                                           && productMappings.Exists(mapping => x.SettingId == mapping.SettingId)).ToList();

        logService.LogInformation("End mapping amenment product.");

        return products.Any()
            ? products.GroupBy(x => x.SettingId).Select(x => x.First()).ToList()
            : new List<SettingItem> { new() { SettingId = BackgroundJobConstants.ProductNA } };
    }

    protected internal virtual IList<SettingItem> GetVoidingSettledTicketsSubProduct(
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
        var mappings = settingMappings.Where(mapping => mapping.SettingName.Equals(SettingName.SubProduct, StringComparison.OrdinalIgnoreCase)
                                                        && !string.IsNullOrWhiteSpace(mapping.SettingSource)
                                                        && sportNames.Exists(sportName => mapping.SettingSource.Equals(sportName, StringComparison.OrdinalIgnoreCase))).ToList();

        var subProducts = isMixParlay ? settings.Where(x => x.SettingName.Equals(SettingName.SubProduct, StringComparison.OrdinalIgnoreCase)
                                                            && x.SettingValue.Equals("Multiple Sports", StringComparison.OrdinalIgnoreCase)).ToList() :
            settings.Where(sub => sub.SettingName.Equals(SettingName.SubProduct, StringComparison.OrdinalIgnoreCase)
                                  && sub.ParentIds.Any(parent => productSettingIds.Any(productName => productName == parent))
                                  && mappings != null && mappings.Exists(mapping => mapping.SettingId == sub.SettingId)).ToList();

        logService.LogInformation("End mapping subproduct.");

        return subProducts.Any()
            ? subProducts.GroupBy(x => x.SettingId).Select(x => x.First()).ToList()
            : new List<SettingItem>
            {
                new() { SettingId = BackgroundJobConstants.SubProductNA, ParentIds = new List<int> { BackgroundJobConstants.ProductNA } }
            };
    }

    protected internal virtual List<SystemSettingMapping> GetVoidingSettledTicketsBetType(
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
        return betTypes != null && betTypes.Any()
            ? betTypes.GroupBy(x => x.SettingId).Select(x => x.First()).ToList()
            : new List<SystemSettingMapping>
            {
                new() { SettingId = BackgroundJobConstants.BettypeNA }
            };
    }

    protected internal virtual Tuple<int, int, string> GetCategorySubCategoryVoidingSettledTicket(IEnumerable<SettingItem> settings, string settingValueName, int issueTypeId)
    {
        var settingName = issueTypeId == DuplicateTicketIssueTypeId ? "Special Notification" : settingValueName;
        var category = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.MainCategory, StringComparison.OrdinalIgnoreCase)
                                                    && x.SettingValue.Equals(settingName, StringComparison.OrdinalIgnoreCase));
        var subCategoryId = 0;

        if (category != null)
        {
            var subCategories = settings.Where(x => x.SettingName.Equals(SettingName.SubCategory, StringComparison.OrdinalIgnoreCase));
            var subCategory = issueTypeId == DuplicateTicketIssueTypeId ?
                subCategories.FirstOrDefault(x => x.ParentIds.Any(y => y == category.SettingId) && x.SettingValue.Equals("Duplicate Tickets", StringComparison.OrdinalIgnoreCase)) :
                subCategories.FirstOrDefault(x => x.SettingId == 4029);
            subCategoryId = subCategory != null ? subCategory.SettingId : 0;
        }

        return new Tuple<int, int, string>(
            category != null ? category.SettingId : 0, subCategoryId,
            string.Format(DefaultValue.CategoryXml,
                category != null ? category.SettingId : 0,
                subCategoryId));
    }

    protected internal virtual string GetRelatedCustomerXml(VoidingAutoFeedbackInfo feedbackInfo)
    {
        var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);

        if (string.IsNullOrEmpty(feedbackInfo.AffectedId))
        {
            return stringBuilder.Append(BackgroundJobConstants.EndRootTag).ToString();
        }
        var relateCustIds = feedbackInfo.AffectedId.Split(",");
        var userNames = feedbackInfo.AffectedName.Split(",");
        const string XmlTemplate = DefaultValue.RelatedCustomerXml;
        for (var i = 0; i < relateCustIds.Length; i++)
        {
            stringBuilder.AppendFormat(XmlTemplate, feedbackInfo.HeadlineId, relateCustIds[i], userNames[i]);
        }
        return stringBuilder.Append(BackgroundJobConstants.EndRootTag).ToString();
    }

    protected internal virtual string GetVoidingSettledTicketSolution(VoidingAutoFeedbackInfo feedbackInfo, IEnumerable<Message> messages)
    {
        var ticketSolTemplate = commonFeedback.GetFeedbackTemplates<VoidingSettledTicketSolutionTemplate>(solTemplateXml);
        var stringBuilder = new StringBuilder();
        var content_en = messages?.FirstOrDefault()?.Content_en;
        var content_cs = messages?.FirstOrDefault()?.Content_cs;
        logService.LogInformation(
            $"GetVoidingSettledTicketSolution, Site: {feedbackInfo.Site} IsVip:{feedbackInfo.IsVip}");
        if (feedbackInfo.SiteLabel == "789y" && feedbackInfo.IsVip)
        {
            stringBuilder.AppendFormat(ticketSolTemplate.SolutionENYN,
                content_en, feedbackInfo.AffectedName, BackgroundJobConstants.Break, BackgroundJobConstants.LineBreak
            );
        }
        else
        {
            stringBuilder.AppendFormat(ticketSolTemplate.SolutionENSC,
                content_en, content_cs, feedbackInfo.AffectedName, BackgroundJobConstants.LineBreak, BackgroundJobConstants.Break
            );
        }

        return stringBuilder.ToString();
    }

    protected internal virtual string GetVoidingSettledTicketDescription(VoidingAutoFeedbackInfo feedbackInfo,
        IEnumerable<AutoVoidingEventDetails> eventDetails)
    {
        const string messageTemplate = "<span style=\"color: rgb(255, 0, 0);\">Msg ID: {0} {1}</span>";
        const string settledTicketsText = "<span style =\"font-weight:bold;\">Following events have voided settled tickets: </span>";
        const string mainParlayticketsText = "<span style =\"font-weight:bold;\">Following Members have voided settled Main Parlay tickets: </span>";
        var stringBuilder = new StringBuilder(DefaultValue.autoGenerated);
        var stringBuilderMainParlay = new StringBuilder();
        var stringBuilderTicket = new StringBuilder();
        stringBuilder.Append(BackgroundJobConstants.Break);
        stringBuilder.AppendFormat(messageTemplate, feedbackInfo.MessageId, BackgroundJobConstants.Break);
        var ticketDescTemplate = commonFeedback.GetFeedbackTemplates<VoidingSettledTicketDescriptionTemplate>(descTemplateXml);
        var countAppendTicket = 0;
        var countAppendMainParlay = 0;
        foreach (var eventDetail in eventDetails)
        {
            var isMainParlay = MixParlayMatchIds.Contains(eventDetail.MatchId);
            if (!isMainParlay)
            {
                if (countAppendTicket == 0)
                {
                    stringBuilderTicket.Append(settledTicketsText);
                }
                stringBuilderTicket.Append(BackgroundJobConstants.Break);
                stringBuilderTicket.Append(GetVoidingSettledTicketDescriptionByProduct(eventDetail, ticketDescTemplate, feedbackInfo.Site));
                countAppendTicket++;
            }
            else
            {
                if (countAppendMainParlay == 0)
                {
                    stringBuilderMainParlay.Append(BackgroundJobConstants.Break);
                    stringBuilderMainParlay.Append(mainParlayticketsText);
                }
                stringBuilderMainParlay.Append(BackgroundJobConstants.Break);
                const string templateMainParlaytickets = "- {0}";
                stringBuilderMainParlay.AppendFormat(templateMainParlaytickets, eventDetail.MatchCode);
                countAppendMainParlay++;
            }
        }
        stringBuilder.Append(stringBuilderTicket);
        stringBuilder.Append(stringBuilderMainParlay);

        return stringBuilder.ToString();
    }

#pragma warning disable S1541
    protected internal virtual string GetVoidingSettledTicketDescriptionByProduct(AutoVoidingEventDetails eventDetail,
        VoidingSettledTicketDescriptionTemplate ticketDescTemplate,
        string site)
    {
        var stringBuilder = new StringBuilder();
        const string line = "-------------------------------------------------------- {0}";
        var sportName = commonFeedback.GetSportName(eventDetail.Sporttype.ToString(), Language.en_US);
        var leagueName = commonFeedback.GetLeagueNameBySiteName(site, eventDetail.LeagueId, eventDetail.LeagueGroupId, Language.en_US);
        var getTeamNameFunc = eventDetail.EventType == EventType5
            ? new Func<string, string, string>(commonFeedback.GetTeamName3rd)
            : new Func<string, string, string>(commonFeedback.GetTeamName);
        var getMatchCodeFunc = eventDetail.EventType == EventType5
            ? new Func<string, string, string>(commonFeedback.GetMatchCodeVirtualSport)
            : new Func<string, string, string>(commonFeedback.GetMatchCode);
        var homeName = getTeamNameFunc(eventDetail.HomeId.ToString(), Language.en_US);
        var awayName = getTeamNameFunc(eventDetail.AwayId.ToString(), Language.en_US);

        var EventdateGMT4Format = eventDetail.Eventdate.ToString(DefaultValue.FortmatddmmmyyyyDash);
        var timeSpan = new TimeSpan(appSettings.TimeSpan, 0, 0);
        var kO_time = eventDetail.KO_time.Add(timeSpan).ToString(DefaultValue.Formathhmmtt);
        var race_time = eventDetail.Race_time.Add(timeSpan).ToString(DefaultValue.Formathhmmtt);
        var matchCode = getMatchCodeFunc(eventDetail.MatchCode, Language.en_US);
        var numberGameEnGame = eventDetail.Sporttype == SportType161 ? leagueName : sportName;
        var productName = commonFeedback.GetProductName(eventDetail.ProductId.ToString(), Language.en_US);
        stringBuilder.Append(GetDescriptionSportBook(eventDetail, productName, kO_time, leagueName, homeName, sportName, awayName, matchCode));
        if (productName == "Virtual Sports" && (eventDetail.Sporttype == SportType180 || eventDetail.Sporttype == SportType186))
        {
            stringBuilder.AppendFormat(line, BackgroundJobConstants.Break);
            stringBuilder.AppendFormat(ticketDescTemplate.VirtualSports, EventdateGMT4Format, sportName, homeName, awayName, BackgroundJobConstants.Break);
        }

        if (productName == "Virtual Sports" && (eventDetail.Sporttype > SportType180 && eventDetail.Sporttype < SportType186))
        {
            stringBuilder.AppendFormat(line, BackgroundJobConstants.Break);
            stringBuilder.AppendFormat(ticketDescTemplate.VirtualSports181185, EventdateGMT4Format, sportName, race_time, BackgroundJobConstants.Break);
        }

        if (productName == "Number Game")
        {
            stringBuilder.AppendFormat(line, BackgroundJobConstants.Break);
            stringBuilder.AppendFormat(ticketDescTemplate.NumberGames, EventdateGMT4Format, numberGameEnGame, matchCode, BackgroundJobConstants.Break);
        }

        return stringBuilder.ToString();
    }
#pragma warning restore S1541

#pragma warning disable S1541, S1067, S121, S107

    protected internal virtual string GetDescriptionSportBook(AutoVoidingEventDetails eventDetail, string productName,
        string kO_time, string leagueName, string homeName, string sportName, string awayName, string matchCode)
#pragma warning restore S1541, S1067, S121, S107
    {
        var stringBuilder = new StringBuilder();
        const string line = "-------------------------------------------------------- {0}";
        var ticketDescTemplate = commonFeedback.GetFeedbackTemplates<VoidingSettledTicketDescriptionTemplate>(descTemplateXml);
        var EventdateGMT4Format = eventDetail.Eventdate.ToString(DefaultValue.FortmatddmmmyyyyDash);

        if (productName == "Sportsbook" && eventDetail.EventType == EventType2)
        {
            stringBuilder.AppendFormat(line, BackgroundJobConstants.Break);
            stringBuilder.AppendFormat(ticketDescTemplate.Outright, eventDetail.Leaguenametc, kO_time, leagueName, homeName, BackgroundJobConstants.Break);
        }

#pragma warning disable S1067
#pragma warning disable S121
        if ((productName != "Sportsbook" || eventDetail.EventType == EventType2) &&
            productName != "Virtual Sports 2" && productName != "Virtual Sports 3" &&
            productName != "Virtual Sports 4" && productName != "Virtual Sports 5") return stringBuilder.ToString();
#pragma warning restore S1067
#pragma warning restore S121

        stringBuilder.AppendFormat(line, BackgroundJobConstants.Break);
        var league = GetLeagueName(productName, leagueName, sportName, eventDetail);

        if (productName.Equals("Virtual Sports 2", StringComparison.OrdinalIgnoreCase) && eventDetail.EventType == EventType3)
        {
            stringBuilder.AppendFormat(ticketDescTemplate.SportsbookVirtualSports2Racing, EventdateGMT4Format, sportName, kO_time, league, eventDetail.VendorMatchId, BackgroundJobConstants.Break);
            return stringBuilder.ToString();
        }
            
        stringBuilder.AppendFormat(ticketDescTemplate.SportsbookVirtualSports2, EventdateGMT4Format, sportName, kO_time, league, homeName, awayName, matchCode, BackgroundJobConstants.Break);
            
        return stringBuilder.ToString();
    }

    protected string GetLeagueName(string productName, string leagueName, string sportName, AutoVoidingEventDetails eventDetail)
    {
        if (productName.Equals("Virtual Sports 2", StringComparison.OrdinalIgnoreCase) ||
            productName.Equals("Virtual Sports 3", StringComparison.OrdinalIgnoreCase) ||
            productName.Equals("Virtual Sports 4", StringComparison.OrdinalIgnoreCase))
        {
            return sportName;
        }
        else if (productName.Equals("Virtual Sports 5", StringComparison.OrdinalIgnoreCase))
        {
            return commonFeedback.GetLeagueName3rd(eventDetail.LeagueId.ToString(), Language.en_US);
        }
        else
        {
            return leagueName;
        }
    }
}