namespace CRM.BackgroundJob.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Constants;
using Criteria.AutoFeedback;
using CRM.BackgroundJob.Models.AmendmentNotiFB;
using CRM.Services;
using Microsoft.Extensions.Options;
using Models;

public interface IAmendmentNotiFbService
{
    Task CreateFeedbackForAmendmentNotiFbAsync(AmendmentNotiFbSetting resultAmendmentSetting, bool isUAT,
        DateTime LastRuntimeEnv, string LastRuntimeNameEnv,
        string JobStatusNameEnv, string Environment);
}

#pragma warning disable S1541 // Methods and properties should not be too complex
public class AmendmentNotiFbService(
    IOptions<AppJobSettings> appSettings,
    IBaseService baseService,
    ISettingService settingService,
    ICustomerService customerService,
    ICrmLogService logService,
    ICommonFeedback commonFeedback,
    IFeedbackTransactionService transactionService)
    : IAmendmentNotiFbService
{
    private readonly AppJobSettings appSettings = appSettings.Value;

    private const string descTemplateXml = "AmendmentDescriptionTemplate.xml";
    private const string solTemplateXml = "AmendmentSolutionTemplate.xml";
    private const int SportType180 = 180;
    private const int SportType186 = 186;
    private const int SportType161 = 161;
    private const int EventType2 = 2;
    private const int EventType9 = 9;
    private const int EventType5 = 5;

    public async Task CreateFeedbackForAmendmentNotiFbAsync(AmendmentNotiFbSetting resultAmendmentSetting, bool isUAT,
        DateTime LastRuntimeEnv, string LastRuntimeNameEnv,
        string JobStatusNameEnv, string Environment)
    {
        await commonFeedback.EditSystemSettingAsyncByEnv(JobStatusNameEnv, LastRuntimeNameEnv, JobStatus.Running, BackgroundJobConstants.ServiceName_AmendmentNotiFB);

        var settingNames = new[] { SettingName.Product, SettingName.SubProduct, SettingName.Bettype };
        var settings = await settingService.GetAllSettingAsync();
        var settingMappings = await settingService.GetSystemSettingMappingAsync(settingNames);

        var custAmendmentNotifications = await customerService.GetListResultAmendmentNotificationAsync(resultAmendmentSetting);
        var isAnyNotCompeleteStatus = custAmendmentNotifications.Any(x => x.Status != "Completed");
        if (isAnyNotCompeleteStatus)
        {
            logService.LogInformation("Data has some status not completed");
            await commonFeedback.EditSystemSettingAsyncByEnv(JobStatusNameEnv, LastRuntimeNameEnv, JobStatus.Completed, BackgroundJobConstants.ServiceName_AmendmentNotiFB);
            return;
        }
        if (custAmendmentNotifications.Any())
        {
            try
            {
                var commandResultAmendment = await transactionService.InserAmendmentNotiAutoFeedback(custAmendmentNotifications, Environment);
                if (commandResultAmendment.IsSuccessful)
                {
                    logService.LogInformation("Import Amendment Notification Auto FeedBack Successfull!");
                    await ResultAmendmentUpdateSystemSetting(isUAT, resultAmendmentSetting, custAmendmentNotifications, JobStatusNameEnv);
                }
                else
                {
                    logService.LogInformation("Import Amendment Notification Auto FeedBack Fail!");
                }
            }
            catch (Exception ex)
            {
                await logService.ErrorAsync(ex);
            }
        }

        var amendmentAutoFeedbackInfors = await transactionService.GetAmendmentNotiAutoFeedbackInfor(resultAmendmentSetting.BatchSize);
        if (!amendmentAutoFeedbackInfors.Any())
        {
            logService.LogInformation(
                $"Import Amendment Auto Feedback Infor doesn't have any data with custId: {string.Join(",", custAmendmentNotifications.Select(x => x.CustId))} TicketId: {string.Join(",", custAmendmentNotifications.Select(x => x.TicketId))}!");
            await commonFeedback.EditSystemSettingAsyncByEnv(JobStatusNameEnv, LastRuntimeNameEnv, JobStatus.Completed, BackgroundJobConstants.ServiceName_AmendmentNotiFB);
            return;
        }

        foreach (var amendment in amendmentAutoFeedbackInfors)
        {
            var isEventTypeNotIn2And9 = amendment.EventType != EventType2 && amendment.EventType != EventType9;
            var isBelongProductId = amendment.ProductId == 1 || amendment.ProductId == 27 || amendment.ProductId == 88;
            var isNoResultTemplate = isEventTypeNotIn2And9 && isBelongProductId && amendment.MessageId != 0;
            await CreateResultAmendmentFeedbackCreditAsync(
                amendment,
                settings,
                settingMappings,
                isNoResultTemplate);
        }

        await commonFeedback.EditSystemSettingAsyncByEnv(JobStatusNameEnv, LastRuntimeNameEnv, JobStatus.Completed, BackgroundJobConstants.ServiceName_AmendmentNotiFB);
        logService.LogInformation("end service result amendment serivce!");
    }

    protected internal virtual async Task ResultAmendmentUpdateSystemSetting(
        bool isUAT,
        AmendmentNotiFbSetting resultAmendmentSetting,
        IEnumerable<CusReachAmendmentNotification> custAmendmentNotifications,
        string JobStatusNameEnv
    )
    {
        var lastSeqEnvName = isUAT ? "LastSeqUAT" : "LastSeq";
        var lastSubmitTimeEnvName = isUAT ? "LastSubmitDateUAT" : "LastSubmitDate";
        var maxLastSubmitTime = custAmendmentNotifications.Max(x => x.SubmitDate);
        var getSeqByMaxSubmitDates = custAmendmentNotifications.Where(x => x.SubmitDate == maxLastSubmitTime);
        var maxSeq = getSeqByMaxSubmitDates.Max(x => x.Seq);
        await commonFeedback.EditSystemSettingUpdateLastSeqAndLastSubmitDateAsync(
            JobStatusNameEnv,
            lastSeqEnvName,
            maxSeq,
            lastSubmitTimeEnvName,
            maxLastSubmitTime.ToString("yyyy-MM-dd H:mm:ss.fffffff"),
            BackgroundJobConstants.ServiceName_AmendmentNotiFB
        );
    }

    protected internal virtual async Task CreateResultAmendmentFeedbackCreditAsync(
        AmendmentAutoFeedbackInfor amendment,
        IEnumerable<SettingItem> settings,
        IEnumerable<SystemSettingMapping> settingMappings,
        bool isNoResultTemplate)
    {
        logService.LogInformation(
            $"Start create amendment feedback logId: {amendment.LogId} , headlineName: {amendment.HeadlineName} productId: {amendment.ProductId}!");
        var timeSpan = new TimeSpan(appSettings.TimeSpan, 0, 0);
        var feedbackDate = DateTimeOffset.Now.DateTime.Add(timeSpan);
        var status = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.Status, StringComparison.OrdinalIgnoreCase)
                                                  && x.SettingValue.Equals(DefaultValue.Closed, StringComparison.OrdinalIgnoreCase));
        var response = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.Response, StringComparison.OrdinalIgnoreCase)
                                                    && x.SettingValue.Equals(DefaultValue.Neutral, StringComparison.OrdinalIgnoreCase));
        var station = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.Extension, StringComparison.OrdinalIgnoreCase)
                                                   && x.SettingId == BackgroundJobConstants.ExtentionNA);
        var betTypeSplits = amendment.Listbettype.Split(",");
        var betTypeMappings = new List<string>();
        foreach (var betType in betTypeSplits.Distinct().ToList())
        {
            var betTypeName = commonFeedback.GetBetTypeName(betType, Language.en_US);
            betTypeMappings.Add(betTypeName);
        }
        var category = GetCategorySubCategoryResultAmendment(settings, DefaultValue.ResultAmended);

        var messageContent = await transactionService.GetMessageContentInfoAsync(amendment.MessageId);
        logService.LogInformation("Begin create result amendment criteria!!!");
        var criteria = new CreateMutipleFeedbackCommand
        {
            FeedbackDate = feedbackDate.Subtract(timeSpan),
            ResolutionDate = feedbackDate.AddMinutes(1).Subtract(timeSpan),
            CreatorId = appSettings.CreatorId,
            UserId = appSettings.CreatorId,
            IsCheckedDefaultComm = true,
            StationId = station != null ? station.SettingId.ToString() : string.Empty,
            StatusId = status != null ? status.SettingId : 0,
            CategoryXml = category.Item3,
            CategoryIds = category.Item1.ToString(),
            SubCategoryIds = category.Item2.ToString(),
            ResponseId = response != null ? response.SettingId : 0,
            UserRole = BackgroundJobConstants.UserRole,
            Subject = DefaultValue.Special_Notification,
            AutoField2 = string.Format(DefaultValue.DateTimeTool, feedbackDate.ToString("yyyyMMdd"), feedbackDate.ToString("hh:mmtt")),
            AutoField = string.Empty,
            AutoField1 = string.Empty,
            AutoField3 = $"{Environment.NewLine}System ID: ",
            CustomerXml = string.Format(DefaultValue.CustomerXml, amendment.HeadlineId, amendment.HeadlineName),
            ProductXml = GetAmendmentProductXml(amendment.Sporttype, amendment.ProductId, betTypeMappings, settings, settingMappings),
            Solution = GetAmenmendSolution(amendment, messageContent, isNoResultTemplate),
            RelatedCustomerXml = GetAmendmentRelatedCustomerXml(amendment),
            ListOrigCustIds = amendment.affectedId,
            AmendmentLogID = amendment.LogId,
            Description = GetAmendmentDescription(amendment, isNoResultTemplate)
        };
        await CheckValidAmendmentParamsAndInsertFeedbackAsync(amendment, criteria);
    }

    protected internal virtual async Task CheckValidAmendmentParamsAndInsertFeedbackAsync(
        AmendmentAutoFeedbackInfor amendment,
        CreateMutipleFeedbackCommand criteria)
    {
        var checkValidParamBeforeInsert = commonFeedback.CheckFeedbackValidParams(criteria);
        if (!checkValidParamBeforeInsert.Item1)
        {
            var messageInvalid = "Invalid Params: " + checkValidParamBeforeInsert.Item2;
            logService.LogInformation(
                $"Invalid Params. @Categorystr: {criteria.CategoryIds} @Category: {criteria.CategoryXml} @Product: {criteria.ProductXml} @StatusID: {criteria.StatusId} @SubCategorystr:{criteria.SubCategoryIds} @ResponseID:{criteria.ResponseId} @StationID:{criteria.StationId}");
            await transactionService.EditAmendmentNotiAutoFeedback(amendment.LogId, CreateFeedbackJobStatus.Fail, messageInvalid);
            return;
        }
        try
        {
            logService.LogInformation(
                $"Begin insert amendment feedback : HeadlineId {amendment.HeadlineId} HeadlineName:{amendment.HeadlineName} CustName: {amendment.affectedName} CustId: {amendment.affectedId}");

            var commandResult = await baseService.ExecuteNonQueryAsync(criteria);
            if (commandResult.IsSuccessful)
            {
                logService.LogInformation(
                    $"Create amendment feedback successfully HeadlineId {amendment.HeadlineId} HeadlineName:{amendment.HeadlineName}");
            }
            else
            {
                logService.LogInformation(
                    $"Create amendment feedback Faild HeadlineId {amendment.HeadlineId} HeadlineName:{amendment.HeadlineName}");

                await transactionService.EditAmendmentNotiAutoFeedback(amendment.LogId, CreateFeedbackJobStatus.Fail, commandResult.Message);
                await logService.ErrorAsync(commandResult, criteria);
            }
        }
        catch (Exception ex)
        {
            await transactionService.EditAmendmentNotiAutoFeedback(amendment.LogId, CreateFeedbackJobStatus.Fail, ex.Message);
            await logService.ErrorAsync(ex);
        }
    }

    protected internal virtual string GetAmendmentProductXml(
        int sportTypeId,
        int productId,
        List<string> betTypeMappings,
        IEnumerable<SettingItem> settings,
        IEnumerable<SystemSettingMapping> settingMappings)
    {
        var product = GetAmenmentProduct(settings, settingMappings, productId);
        var subProducts = GetAmendmentSubProduct(product.SettingId, sportTypeId, settings, settingMappings);
        var tempBetTypes = GetAmendmentBetType(betTypeMappings, settingMappings);
        var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);
        foreach (var settingId in subProducts.Select(x => x.SettingId))
        {
            if (tempBetTypes.Any())
            {
                foreach (var betType in tempBetTypes)
                {
                    stringBuilder.AppendFormat(DefaultValue.ProductXml, product.SettingId, settingId, betType.SettingId);
                }
            }
            else
            {
                stringBuilder.AppendFormat(DefaultValue.ProductXml, product.SettingId, settingId, BackgroundJobConstants.BettypeNA);
            }
        }
        return stringBuilder.Append(BackgroundJobConstants.EndRootTag).ToString();
    }

    protected internal virtual SettingItem GetAmenmentProduct(
        IEnumerable<SettingItem> settings,
        IEnumerable<SystemSettingMapping> settingMappings,
        int productId)
    {
        var productName = commonFeedback.GetProductName(productId.ToString(), Language.en_US);
        var productMapping = settingMappings.FirstOrDefault(p => p.SettingName.Equals(SettingName.Product, StringComparison.OrdinalIgnoreCase)
                                                                 && !string.IsNullOrWhiteSpace(p.SettingSource)
                                                                 && p.SettingSource.Equals(productName, StringComparison.OrdinalIgnoreCase));

        var product = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.Product, StringComparison.OrdinalIgnoreCase)
                                                   && productMapping != null
                                                   && x.SettingId == productMapping.SettingId);

        return product ?? new SettingItem { SettingId = BackgroundJobConstants.ProductNA };
    }

    protected internal virtual IList<SettingItem> GetAmendmentSubProduct(
        int productId,
        int sportTypeId,
        IEnumerable<SettingItem> settings,
        IEnumerable<SystemSettingMapping> settingMappings)
    {
        var sportName = commonFeedback.GetSportName(sportTypeId.ToString(), Language.en_US);
        var mappings = settingMappings.Where(mapping => mapping.SettingName.Equals(SettingName.SubProduct, StringComparison.OrdinalIgnoreCase)
                                                        && !string.IsNullOrWhiteSpace(mapping.SettingSource)
                                                        && mapping.SettingSource.Equals(sportName, StringComparison.OrdinalIgnoreCase)).ToList();
        var subProducts = settings.Where(sub => sub.SettingName.Equals(SettingName.SubProduct, StringComparison.OrdinalIgnoreCase)
                                                && sub.ParentIds.Any(parent => parent == productId)
                                                && mappings != null
                                                && mappings.Exists(mapping => mapping.SettingId == sub.SettingId)).ToList();

        return subProducts.Any()
            ? subProducts.GroupBy(x => x.SettingId).Select(x => x.First()).ToList()
            : new List<SettingItem>
            {
                new() { SettingId = BackgroundJobConstants.SubProductNA, ParentIds = new List<int> { BackgroundJobConstants.ProductNA } }
            };
    }

    protected internal virtual IList<SystemSettingMapping> GetAmendmentBetType(
        List<string> betTypeMappings,
        IEnumerable<SystemSettingMapping> settingMappings)
    {
        IList<SystemSettingMapping> betTypes = settingMappings.Where(mapping => mapping.SettingName.Equals(SettingName.Bettype, StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(mapping.SettingSource)
            && betTypeMappings.Exists(name => name.Equals(mapping.SettingSource, StringComparison.OrdinalIgnoreCase))).ToList();
        return betTypes != null && betTypes.Any()
            ? betTypes.GroupBy(x => x.SettingId).Select(x => x.First()).ToList()
            : new List<SystemSettingMapping>
            {
                new() { SettingId = BackgroundJobConstants.BettypeNA }
            };
    }

    protected internal virtual Tuple<int, int, string> GetCategorySubCategoryResultAmendment(IEnumerable<SettingItem> settings, string settingValueName)
    {
        var category = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.MainCategory, StringComparison.OrdinalIgnoreCase)
                                                    && x.SettingValue.Equals(settingValueName, StringComparison.OrdinalIgnoreCase));
        var subCategoryId = 0;

        if (category != null)
        {
            var subCategory = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.SubCategory, StringComparison.OrdinalIgnoreCase)
                                                           && x.SettingId == 4029);
            subCategoryId = subCategory != null ? subCategory.SettingId : 0;
        }

        return new Tuple<int, int, string>(
            category != null ? category.SettingId : 0, subCategoryId,
            string.Format(DefaultValue.CategoryXml,
                category != null ? category.SettingId : 0,
                subCategoryId));
    }

    protected internal virtual string GetAmendmentRelatedCustomerXml(AmendmentAutoFeedbackInfor amendment)
    {
        var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);

        if (string.IsNullOrEmpty(amendment.affectedId))
        {
            return stringBuilder.Append(BackgroundJobConstants.EndRootTag).ToString();
        }
        var relateCustIds = amendment.affectedId.Split(",");
        var userNames = amendment.affectedName.Split(",");
        const string XmlTemplate = DefaultValue.RelatedCustomerXml;
        for (var i = 0; i < relateCustIds.Length; i++)
        {
            stringBuilder.AppendFormat(XmlTemplate, amendment.HeadlineId, relateCustIds[i], userNames[i]);
        }
        return stringBuilder.Append(BackgroundJobConstants.EndRootTag).ToString();
    }

    protected internal virtual Func<string, string, string> GetTeamNameFunction(int eventType)
    {
        return eventType == EventType5
            ? commonFeedback.GetTeamName3rd
            : new Func<string, string, string>(commonFeedback.GetTeamName);
    }

#pragma warning disable S138 // Methods should not have too many lines of code
    protected internal virtual string GetAmendmentDescription(AmendmentAutoFeedbackInfor amendment, bool isNoResultTemplate)
    {
        var stringBuilder = new StringBuilder(DefaultValue.autoGenerated);
        stringBuilder.Append(BackgroundJobConstants.Break);
        var getTeamNameFunc = GetTeamNameFunction(amendment.EventType);
        var amendmentDescTemplate = commonFeedback.GetFeedbackTemplates<AmendmentDescriptionTemplate>(descTemplateXml);
        var productName = commonFeedback.GetProductName(amendment.ProductId.ToString(), Language.en_US);
        var sportName = commonFeedback.GetSportName(amendment.Sporttype.ToString(), Language.en_US);
        var leagueName = commonFeedback.GetLeagueNameBySiteName(amendment.Site, amendment.LeagueId, amendment.LeagueGroupId, Language.en_US);
        var leagueName3rd = commonFeedback.GetLeagueName3rd(amendment.LeagueId.ToString(), Language.en_US);
        var homeName = getTeamNameFunc(amendment.HomeId.ToString(), Language.en_US);
        var awayName = getTeamNameFunc(amendment.Awayid.ToString(), Language.en_US);

        var EventdateGMT4Format = amendment.Eventdate.ToString(DefaultValue.FortmatddmmmyyyyDash);
        var timeSpan = new TimeSpan(appSettings.TimeSpan, 0, 0);
        var kO_time = amendment.KO_time.Add(timeSpan).ToString(DefaultValue.Formathhmmtt);
        var startTimeOutright = amendment.KO_time.Add(timeSpan).ToString(DefaultValue.Fortmatddmmmyyyyhhmmtt);
        var race_time = amendment.Race_time.Add(timeSpan).ToString(DefaultValue.Formathhmmtt);
        var matchCodeReplace = commonFeedback.GetMatchCodeFormat(amendment.MatchCode, Language.en_US);
        var matchCodeVirtualSport = commonFeedback.GetMatchCodeVirtualSportFormat(amendment.MatchCode, Language.en_US);

        var virtualSports = GetAmendmentDescriptionVirtualSports(
            amendment, amendmentDescTemplate, sportName,
            homeName, awayName, race_time, EventdateGMT4Format);
        var numberGame = GetAmendmentDescriptionNumberGame(
            amendment, amendmentDescTemplate, leagueName, sportName, EventdateGMT4Format);
        var outright = GetAmendmentDescriptionSportOutRight(
            amendment, amendmentDescTemplate, startTimeOutright, leagueName, homeName);
        if (isNoResultTemplate)
        {
            const string messageIdColor = "<span style=\"color: rgb(255, 0, 0);\">Msg ID: {0}{1}</span>";
            var leagueNameNoResult = amendment.EventType == EventType5 ? leagueName3rd : leagueName;
            stringBuilder.AppendFormat(messageIdColor, amendment.MessageId, BackgroundJobConstants.LineBreak);
            stringBuilder.AppendFormat(amendmentDescTemplate.NoResult, EventdateGMT4Format, sportName, kO_time, leagueNameNoResult, homeName, awayName, BackgroundJobConstants.Break);
        }
        else
        {
            switch (productName)
            {
                case "Sportsbook":
                    if (amendment.EventType == EventType2 || amendment.EventType == EventType9)
                    {
                        stringBuilder.Append(outright);
                        break;
                    }

                    stringBuilder.AppendFormat(
                        amendmentDescTemplate.Sportsbook,
                        EventdateGMT4Format,
                        sportName,
                        kO_time,
                        leagueName,
                        homeName,
                        awayName,
                        BackgroundJobConstants.Break
                    );
                    break;
                case "Virtual Sports 2":
                    stringBuilder.AppendFormat(
                        amendmentDescTemplate.VirtualSports2,
                        EventdateGMT4Format,
                        sportName,
                        kO_time,
                        matchCodeReplace,
                        homeName,
                        awayName,
                        leagueName,
                        BackgroundJobConstants.Break
                    );
                    break;

                case "Virtual Sports":
                    stringBuilder.Append(virtualSports);
                    break;

                case "Number Game":
                    stringBuilder.Append(numberGame);
                    break;

                case "Virtual Sports 5":
                    stringBuilder.AppendFormat(
                        amendmentDescTemplate.VirtualSports2,
                        EventdateGMT4Format,
                        sportName,
                        kO_time,
                        matchCodeVirtualSport,
                        homeName,
                        awayName,
                        leagueName3rd,
                        BackgroundJobConstants.Break
                    );
                    break;

                default:
                    stringBuilder.Append("");
                    break;
            }
        }
        stringBuilder.Append(BackgroundJobConstants.Break);
        return stringBuilder.ToString();
    }
#pragma warning restore S138 // Methods should not have too many lines of code

    protected internal virtual StringBuilder GetAmendmentDescriptionVirtualSports(
        AmendmentAutoFeedbackInfor amendment,
        AmendmentDescriptionTemplate amendmentDescTemplate,
        string sportName,
        string homeName,
        string awayName,
        string Race_time,
        string EventdateGMT4
    )
    {
        var stringBuilder = new StringBuilder();
        if (amendment.Sporttype == SportType180 || amendment.Sporttype == SportType186)
        {
            stringBuilder.AppendFormat(
                amendmentDescTemplate.VirtualSports180186,
                EventdateGMT4,
                sportName,
                homeName,
                awayName,
                BackgroundJobConstants.Break
            );
        }
        else
        {
            stringBuilder.AppendFormat(
                amendmentDescTemplate.VirtualSports181To185,
                EventdateGMT4,
                sportName,
                Race_time,
                BackgroundJobConstants.Break
            );
        }

        return stringBuilder;
    }

    protected internal virtual StringBuilder GetAmendmentDescriptionNumberGame(
        AmendmentAutoFeedbackInfor amendment,
        AmendmentDescriptionTemplate amendmentDescTemplate,
        string leagueName,
        string sportName,
        string EventdateGMT4
    )
    {
        var game = amendment.Sporttype == SportType161 ? leagueName : sportName;
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendFormat(
            amendmentDescTemplate.NumberGame,
            EventdateGMT4,
            game,
            amendment.MatchCode,
            BackgroundJobConstants.Break
        );
        return stringBuilder;
    }

    protected internal virtual StringBuilder GetAmendmentDescriptionSportOutRight(
        AmendmentAutoFeedbackInfor amendment,
        AmendmentDescriptionTemplate amendmentDescTemplate,
        string startTimeOutright,
        string leagueName,
        string homeName
    )
    {
        var stringBuilder = new StringBuilder();
        if (amendment.EventType == EventType2)
        {
            stringBuilder.AppendFormat(
                amendmentDescTemplate.SportsbookOutrightEventType2,
                amendment.Leaguenametc,
                startTimeOutright,
                leagueName,
                homeName,
                BackgroundJobConstants.Break
            );
        }
        else
        {
            stringBuilder.AppendFormat(
                amendmentDescTemplate.SportsbookOutrightEventType9,
                amendment.Leaguenametc,
                startTimeOutright,
                leagueName,
                BackgroundJobConstants.Break
            );
        }
        return stringBuilder;
    }

    protected internal virtual string GetAmenmendSolution(AmendmentAutoFeedbackInfor amendment, IEnumerable<Message> messageContent, bool isNoResultTemplate)
    {
        var amendmentSolTemplate = commonFeedback.GetFeedbackTemplates<AmendmentSolutionTemplate>(solTemplateXml);
        var isVipCust = amendment.SiteLabel == "789y" && amendment.IsVip;
        var stringBuilder = new StringBuilder();
        if (isNoResultTemplate)
        {
            if (isVipCust)
            {
                stringBuilder.AppendFormat(amendmentSolTemplate.NoResultENYN, messageContent?.FirstOrDefault()?.Content_en,
                    amendment.affectedName, BackgroundJobConstants.Break, BackgroundJobConstants.LineBreak);
            }
            else
            {
                stringBuilder.AppendFormat(amendmentSolTemplate.NoResultENSC, messageContent?.FirstOrDefault()?.Content_en,
                    messageContent?.FirstOrDefault()?.Content_cs, amendment.affectedName, BackgroundJobConstants.Break, BackgroundJobConstants.LineBreak);
            }
        }
        else
        {
            var solutionTemplateEN = GetAmendmentSolutionTemplateEN(amendment, amendmentSolTemplate, messageContent);
            var solutionTemplateYNSC = GetAmendmentSolutionTemplateYNSC(amendment, amendmentSolTemplate, messageContent, isVipCust);
            stringBuilder.Append(solutionTemplateEN);
            stringBuilder.Append(solutionTemplateYNSC);
        }

        return stringBuilder.ToString();
    }

    protected internal virtual StringBuilder GetAmendmentSolutionTemplateEN(
        AmendmentAutoFeedbackInfor amendment,
        AmendmentSolutionTemplate amendmentSolTemplate,
        IEnumerable<Message> messageContent)
    {
        var stringBuilder = new StringBuilder();
        var getTeamNameFunc = GetTeamNameFunction(amendment.EventType);
        var productName = commonFeedback.GetProductName(amendment.ProductId.ToString(), Language.en_US);
        var sportName = commonFeedback.GetSportName(amendment.Sporttype.ToString(), Language.en_US);
        var leagueName = commonFeedback.GetLeagueNameBySiteName(amendment.Site, amendment.LeagueId, amendment.LeagueGroupId, Language.en_US);
        var leagueName3rd = commonFeedback.GetLeagueName3rd(amendment.LeagueId.ToString(), Language.en_US);
        var homeName = getTeamNameFunc(amendment.HomeId.ToString(), Language.en_US);
        var awayName = getTeamNameFunc(amendment.Awayid.ToString(), Language.en_US);
        var matchCode = commonFeedback.GetMatchCode(amendment.MatchCode, Language.en_US);
        var Eventdate = amendment.Eventdate.ToString(DefaultValue.Formatddmm);
        var matchCodeReplace = commonFeedback.GetMatchCodeFormat(amendment.MatchCode, Language.en_US);
        var matchCodeVirtualSport = commonFeedback.GetMatchCodeVirtualSportFormat(amendment.MatchCode, Language.en_US);
        var outRightMessageEn = messageContent?.FirstOrDefault()?.Content_en;
        var numberGameEnGame = amendment.Sporttype == SportType161 ? leagueName : sportName;
        switch (productName)
        {
            case "Sportsbook":
                if (amendment.EventType == EventType2 || amendment.EventType == EventType9)
                {
                    stringBuilder.AppendFormat(amendmentSolTemplate.OutrightEN,
                        outRightMessageEn,
                        BackgroundJobConstants.LineBreak);
                    break;
                }
                stringBuilder.AppendFormat(amendmentSolTemplate.SportsbookEN,
                    sportName,
                    homeName,
                    awayName,
                    leagueName,
                    Eventdate,
                    amendment.ActualResult,
                    amendment.WrongResult,
                    BackgroundJobConstants.Break,
                    BackgroundJobConstants.LineBreak);
                break;

            case "Virtual Sports 2":
                stringBuilder.AppendFormat(
                    amendmentSolTemplate.VirtualSports2EN,
                    homeName,
                    awayName,
                    leagueName,
                    matchCodeReplace,
                    Eventdate,
                    amendment.ActualResult,
                    amendment.WrongResult,
                    BackgroundJobConstants.Break,
                    BackgroundJobConstants.LineBreak);
                break;

            case "Virtual Sports":
                stringBuilder.AppendFormat(amendmentSolTemplate.VirtualSportsEN,
                    sportName,
                    BackgroundJobConstants.LineBreak);
                break;

            case "Number Game":
                stringBuilder.AppendFormat(amendmentSolTemplate.NumberGameEN,
                    numberGameEnGame,
                    matchCode,
                    Eventdate,
                    BackgroundJobConstants.LineBreak,
                    BackgroundJobConstants.Break);
                break;

            case "Virtual Sports 5":
                stringBuilder.AppendFormat(
                    amendmentSolTemplate.VirtualSports2EN,
                    homeName,
                    awayName,
                    leagueName3rd,
                    matchCodeVirtualSport,
                    Eventdate,
                    amendment.ActualResult,
                    amendment.WrongResult,
                    BackgroundJobConstants.Break,
                    BackgroundJobConstants.LineBreak);
                break;

            default:
                stringBuilder.Append("");
                break;
        }
        return stringBuilder;
    }

    protected internal virtual StringBuilder GetAmendmentSolutionTemplateYNSC(
        AmendmentAutoFeedbackInfor amendment,
        AmendmentSolutionTemplate amendmentSolTemplate,
        IEnumerable<Message> messageContent,
        bool isVipCust)
    {
        var stringBuilder = new StringBuilder();
        var getTeamNameFunc = GetTeamNameFunction(amendment.EventType);
        var productName = commonFeedback.GetProductName(amendment.ProductId.ToString(), Language.en_US);
        var Eventdate = amendment.Eventdate.ToString(DefaultValue.Formatddmm);
        var sportNameYN = commonFeedback.GetSportName(amendment.Sporttype.ToString(), Language.vi_VN);
        var leagueNameYN = commonFeedback.GetLeagueNameBySiteName(amendment.Site, amendment.LeagueId, amendment.LeagueGroupId, Language.vi_VN);
        var leagueName3rdYN = commonFeedback.GetLeagueName3rd(amendment.LeagueId.ToString(), Language.vi_VN);
        var homeNameYN = getTeamNameFunc(amendment.HomeId.ToString(), Language.vi_VN);
        var awayNameYN = getTeamNameFunc(amendment.Awayid.ToString(), Language.vi_VN);
        var matchCodeReplaceYN = commonFeedback.GetMatchCodeFormat(amendment.MatchCode, Language.vi_VN);
        var matchCodeVirtualSportYN = commonFeedback.GetMatchCodeVirtualSportFormat(amendment.MatchCode, Language.vi_VN);
        var sportNameSC = commonFeedback.GetSportName(amendment.Sporttype.ToString(), Language.zh_CN);
        var leagueNameSC = commonFeedback.GetLeagueNameBySiteName(amendment.Site, amendment.LeagueId, amendment.LeagueGroupId, Language.zh_CN);
        var leagueName3rdSC = commonFeedback.GetLeagueName3rd(amendment.LeagueId.ToString(), Language.zh_CN);
        var homeNameSC = getTeamNameFunc(amendment.HomeId.ToString(), Language.zh_CN);
        var awayNameSC = getTeamNameFunc(amendment.Awayid.ToString(), Language.zh_CN);
        var matchCodeReplaceSC = commonFeedback.GetMatchCodeFormat(amendment.MatchCode, Language.zh_CN);
        var matchCodeVirtualSportSC = commonFeedback.GetMatchCodeVirtualSportFormat(amendment.MatchCode, Language.zh_CN);
        var numberGameNameYn = amendment.Sporttype == SportType161 ? leagueNameYN : sportNameYN;
        var numberGameNameSc = amendment.Sporttype == SportType161 ? leagueNameSC : sportNameSC;
        var sportBook = GetAmendmentSolutionTemplateYnSportBook(
            amendment, amendmentSolTemplate, messageContent,
            sportNameYN, homeNameYN, awayNameYN, leagueNameYN);
        var virtualSports = GetAmendmentSolutionTemplateYnScVirtualSport(amendment.affectedName, amendmentSolTemplate,
            isVipCust, sportNameYN, sportNameSC);
        var numberGame = GetAmendmentSolutionTemplateYnScNumberGame(
            amendment.MatchCode, amendment.affectedName, amendmentSolTemplate, isVipCust,
            numberGameNameYn, Eventdate, numberGameNameSc);

        var sportBookOrOutright = GetAmendmentSolutionTemplateSportBookOrOutright(amendment, isVipCust, sportBook, messageContent, amendmentSolTemplate, Eventdate);
        switch (productName)
        {
            case "Sportsbook":
                stringBuilder.Append(sportBookOrOutright);
                break;

            case "Virtual Sports 2":
                if (isVipCust)
                {
                    stringBuilder.AppendFormat(amendmentSolTemplate.VirtualSports2YN, homeNameYN, awayNameYN, leagueNameYN,
                        matchCodeReplaceYN, Eventdate, amendment.ActualResult, amendment.WrongResult, amendment.affectedName, BackgroundJobConstants.Break, BackgroundJobConstants.LineBreak);
                }
                else
                {
                    stringBuilder.AppendFormat(amendmentSolTemplate.VirtualSports2SC, homeNameSC, awayNameSC, leagueNameSC, matchCodeReplaceSC,
                        Eventdate, amendment.ActualResult, amendment.WrongResult, amendment.affectedName, BackgroundJobConstants.Break, BackgroundJobConstants.LineBreak);
                }
                break;

            case "Virtual Sports":
                stringBuilder.Append(virtualSports);
                break;

            case "Number Game":
                stringBuilder.Append(numberGame);
                break;

            case "Virtual Sports 5":
                if (isVipCust)
                {
                    stringBuilder.AppendFormat(amendmentSolTemplate.VirtualSports2YN, homeNameYN, awayNameYN, leagueName3rdYN,
                        matchCodeVirtualSportYN, Eventdate, amendment.ActualResult, amendment.WrongResult, amendment.affectedName, BackgroundJobConstants.Break, BackgroundJobConstants.LineBreak);
                }
                else
                {
                    stringBuilder.AppendFormat(amendmentSolTemplate.VirtualSports2SC, homeNameSC, awayNameSC, leagueName3rdSC, matchCodeVirtualSportSC,
                        Eventdate, amendment.ActualResult, amendment.WrongResult, amendment.affectedName, BackgroundJobConstants.Break, BackgroundJobConstants.LineBreak);
                }
                break;

            default:
                stringBuilder.Append("");
                break;
        }
        return stringBuilder;
    }

    protected internal virtual StringBuilder GetAmendmentSolutionTemplateSportBookOrOutright(
        AmendmentAutoFeedbackInfor amendment,
        bool isVipCust,
        StringBuilder sportBook,
        IEnumerable<Message> messageContent,
        AmendmentSolutionTemplate amendmentSolTemplate,
        string Eventdate
    )
    {
        var stringBuilder = new StringBuilder();
        var getTeamNameFunc = GetTeamNameFunction(amendment.EventType);
        var content_cs = messageContent?.FirstOrDefault()?.Content_cs;
        var content_en = messageContent?.FirstOrDefault()?.Content_en;
        var sportNameSC = commonFeedback.GetSportName(amendment.Sporttype.ToString(), Language.zh_CN);
        var homeNameSC = getTeamNameFunc(amendment.HomeId.ToString(), Language.zh_CN);
        var awayNameSC = getTeamNameFunc(amendment.Awayid.ToString(), Language.zh_CN);
        var leagueNameSC = commonFeedback.GetLeagueNameBySiteName(amendment.Site, amendment.LeagueId, amendment.LeagueGroupId, Language.zh_CN);
        if (isVipCust)
        {
            stringBuilder.Append(sportBook);
        }
        else
        {
            if (amendment.EventType == EventType2 || amendment.EventType == EventType9)
            {
                var outRightMessageSc = string.IsNullOrEmpty(content_cs) ? content_en : content_cs;
                stringBuilder.AppendFormat(amendmentSolTemplate.OutrightSC,
                    outRightMessageSc,
                    amendment.affectedName,
                    BackgroundJobConstants.LineBreak);
            }
            else
            {
                stringBuilder.AppendFormat(amendmentSolTemplate.SportsbookSC, sportNameSC, homeNameSC, awayNameSC,
                    leagueNameSC, Eventdate, amendment.ActualResult, amendment.WrongResult,
                    amendment.affectedName, BackgroundJobConstants.Break, BackgroundJobConstants.LineBreak);
            }
        }
        return stringBuilder;
    }

    protected internal virtual StringBuilder GetAmendmentSolutionTemplateYnSportBook(
        AmendmentAutoFeedbackInfor amendment,
        AmendmentSolutionTemplate amendmentSolTemplate,
        IEnumerable<Message> messageContent,
        string sportNameYN,
        string homeNameYN,
        string awayNameYN,
        string leagueNameYN)
    {
        var Eventdate = amendment.Eventdate.ToString(DefaultValue.Formatddmm);
        var stringBuilder = new StringBuilder();

        if (amendment.EventType == EventType2 || amendment.EventType == EventType9)
        {
            var content_vn = messageContent?.FirstOrDefault()?.Content_vn;
            var content_en = messageContent?.FirstOrDefault()?.Content_en;
            var outRightMessageYn = string.IsNullOrEmpty(content_vn) ? content_en : content_vn;
            stringBuilder.AppendFormat(amendmentSolTemplate.OutrightYN,
                outRightMessageYn,
                amendment.affectedName,
                BackgroundJobConstants.LineBreak);
        }
        else
        {
            stringBuilder.AppendFormat(amendmentSolTemplate.SportsbookYN, sportNameYN, homeNameYN, awayNameYN,
                leagueNameYN, Eventdate, amendment.ActualResult, amendment.WrongResult, amendment.affectedName, BackgroundJobConstants.Break, BackgroundJobConstants.LineBreak);
        }
        return stringBuilder;
    }

    protected internal virtual StringBuilder GetAmendmentSolutionTemplateYnScVirtualSport(
        string affectedName,
        AmendmentSolutionTemplate amendmentSolTemplate,
        bool isVipCust,
        string sportNameYN,
        string sportNameSC)
    {
        var stringBuilder = new StringBuilder();
        if (isVipCust)
        {
            stringBuilder.AppendFormat(amendmentSolTemplate.VirtualSportsYN,
                sportNameYN,
                affectedName,
                BackgroundJobConstants.LineBreak
            );
        }
        else
        {
            stringBuilder.AppendFormat(amendmentSolTemplate.VirtualSportsSC,
                sportNameSC,
                affectedName,
                BackgroundJobConstants.LineBreak
            );
        }
        return stringBuilder;
    }

    protected internal virtual StringBuilder GetAmendmentSolutionTemplateYnScNumberGame(
        string MatchCode,
        string affectedName,
        AmendmentSolutionTemplate amendmentSolTemplate,
        bool isVipCust,
        string numberGameNameYn,
        string Eventdate,
        string numberGameNameSc)
    {
        var stringBuilder = new StringBuilder();
        if (isVipCust)
        {
            stringBuilder.AppendFormat(amendmentSolTemplate.NumberGameYN, numberGameNameYn, MatchCode,
                Eventdate, affectedName, BackgroundJobConstants.LineBreak, BackgroundJobConstants.Break);
        }
        else
        {
            stringBuilder.AppendFormat(amendmentSolTemplate.NumberGameSC, numberGameNameSc, MatchCode, Eventdate,
                affectedName, BackgroundJobConstants.LineBreak, BackgroundJobConstants.Break);
        }
        return stringBuilder;
    }
}
#pragma warning restore S1541 // Methods and properties should not be too complex