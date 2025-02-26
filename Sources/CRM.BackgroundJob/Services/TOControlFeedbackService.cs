using CRM.BackgroundJob.Models.TurnoverControl;

namespace CRM.BackgroundJob.Services;

using Constants;
using Criteria.AutoFeedback;
using CRM.BackgroundJob.Models.WinLossLimit;
using CRM.Services;
using Microsoft.Extensions.Options;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface ITOControlFeedbackService
{
    Task CreateFeedbackForTurnoverControlAsync(WinLossSystemSetting turnOverSetting);
}

public class TOControlFeedbackService : ITOControlFeedbackService
{
    private readonly AppJobSettings appSettings;
    private readonly IBaseService baseService;
    private readonly ISettingService settingService;
    private readonly ISystemSettingService systemSettingService;
    private readonly ICustomerService customerService;
    private readonly ICrmLogService logService;
    private readonly ICommonFeedback commonFeedback;
    private readonly IFeedbackTransactionService transactionService;

    private const string descTemplateXml = "TODesctiptionTemplate.xml";
    private const string solTemplateXml = "TOSolutionTemplate.xml";

#pragma warning disable S107 // Methods should not have too many parameters

    public TOControlFeedbackService(
        IOptions<AppJobSettings> appSettings,
        IBaseService baseService,
        ISettingService settingService,
        ISystemSettingService systemSettingService,
        ICustomerService customerService,
        ICrmLogService logService,
        ICommonFeedback commonFeedback,
        IFeedbackTransactionService transactionService
    )
#pragma warning restore S107 // Methods should not have too many parameters
    {
        this.appSettings = appSettings.Value;
        this.baseService = baseService;
        this.settingService = settingService;
        this.systemSettingService = systemSettingService;
        this.customerService = customerService;
        this.logService = logService;
        this.commonFeedback = commonFeedback;
        this.transactionService = transactionService;
    }

    public async Task CreateFeedbackForTurnoverControlAsync(WinLossSystemSetting turnOverSetting)
    {
        var settingJobStatus = new List<SystemSettingItem>
        {
            new() { SettingName = SystemSettingName.JobStatus, Value = JobStatus.Running },
            new() { SettingName = SystemSettingName.LastRuntime, Value = DateTimeOffset.Now.DateTime.ToString("yyyy-MM-dd hh:mm:ss.fff tt") }
        };

        await systemSettingService.EditSystemSettingAsync(settingJobStatus, BackgroundJobConstants.ServiceName_TOControlFB);
        var custNotifications = await customerService.GetListSMANotificationAsync(turnOverSetting);
        var custNotificationsDistinct = custNotifications.GroupBy(item => new { item.CustID, item.MatchID })
            .Select(g => g.First())
            .ToList();

        var custOutStandings = await customerService.GetListCustOutStandingAsync(custNotificationsDistinct);
        var custIds = custOutStandings.Select(x => x.CustID);
        var headlines = await customerService.GetGroupHeadlineAsync(custIds);
        var importTOFeedbacks = await transactionService.ImportTOControlFeedbackInfoAsync(
            headlines,
            custOutStandings,
            custNotifications);

        if (!importTOFeedbacks.Any())
        {
            logService.LogInformation(
                $"Import doesn't have any data with custId: {string.Join(",", headlines.Select(x => x.HeadlineId))} headlineName: {string.Join(",", headlines.Select(x => x.HeadlineName))}!");
            var maxLogID = custNotifications.Any() ? custNotifications.Max(x => x.LogID) : turnOverSetting.LastLogId;

            await commonFeedback.EditFeedbackSystemSettingAsync(maxLogID, BackgroundJobConstants.ServiceName_TOControlFB);
            return;
        }

        var settingNames = new[] { SettingName.Product, SettingName.SubProduct, SettingName.Bettype };
        var settings = await settingService.GetAllSettingAsync();
        var settingMappings = await settingService.GetSystemSettingMappingAsync(settingNames);

        foreach (var ToInfo in importTOFeedbacks)
        {
            await CreateTOControlFeedbackAsync(
                ToInfo,
                settings,
                settingMappings);
        }
        var maxLogIdTO = importTOFeedbacks.Max(x => x.LogID);
        await commonFeedback.EditFeedbackSystemSettingAsync(maxLogIdTO, BackgroundJobConstants.ServiceName_TOControlFB);
    }

    protected internal virtual async Task CreateTOControlFeedbackAsync(
        TOControlFeedbackInfo ToInfo,
        IEnumerable<SettingItem> settings,
        IEnumerable<SystemSettingMapping> settingMappings)
    {
        var timeSpan = new TimeSpan(appSettings.TimeSpan, 0, 0);
        var feedbackDate = DateTimeOffset.Now.DateTime.Add(timeSpan);
        var status = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.Status, StringComparison.OrdinalIgnoreCase)
                                                  && x.SettingValue.Equals(DefaultValue.Closed, StringComparison.OrdinalIgnoreCase));
        var response = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.Response, StringComparison.OrdinalIgnoreCase)
                                                    && x.SettingValue.Equals(DefaultValue.Neutral, StringComparison.OrdinalIgnoreCase));
        var station = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.Extension, StringComparison.OrdinalIgnoreCase)
                                                   && x.SettingId == BackgroundJobConstants.ExtensionNA);
        var category = commonFeedback.GetCategorySubCategoryCWL(settings, DefaultValue.MANotification);

        var sportTypeName = commonFeedback.GetSportName(ToInfo.SportType.ToString(), Language.en_US);
        var betTypeSplits = ToInfo.BettypeList.Split(",");
        var betTypeMappings = new List<string>();

        foreach (var betType in betTypeSplits.Distinct().ToList())
        {
            var betTypeName = commonFeedback.GetBetTypeName(betType, Language.en_US);
            betTypeMappings.Add(betTypeName);
        }

        var sportsbook = GetTOSportsbookProduct(settings);
        var criteria = new CreateMultipleFeedbackCommand
        {
            FeedbackDate = feedbackDate.Subtract(timeSpan),
            ResolutionDate = feedbackDate.AddMinutes(1).Subtract(timeSpan),
            CreatorId = appSettings.CreatorId,
            UserId = appSettings.CreatorId,
            IsCheckedDefaultComm = true,
            StationId = station != null ? station.SettingId.ToString() : string.Empty,
            StatusId = status?.SettingId ?? 0,
            CategoryXml = category.Item3,
            CategoryIds = category.Item1.ToString(),
            SubCategoryIds = category.Item2.ToString(),
            ResponseId = response?.SettingId ?? 0,
            UserRole = BackgroundJobConstants.UserRole,
            Subject = DefaultValue.Special_Notification,
            AutoField2 = string.Format(DefaultValue.DateTimeTool, feedbackDate.ToString("yyyyMMdd"), feedbackDate.ToString("hh:mmtt")),
            AutoField = string.Empty,
            AutoField1 = string.Empty,
            AutoField3 = $"{Environment.NewLine}System ID: ",
            CustomerXml = string.Format(DefaultValue.CustomerXml, ToInfo.HeadlineID, ToInfo.HeadlineName),
            ProductXml = GetTOProductXml(sportTypeName, sportsbook.SettingId, betTypeMappings, settings, settingMappings),
            Solution = GetTOSolution(ToInfo),
            RelatedCustomerXml = GetTORelatedCustomerXml(ToInfo),
            ListOrigCustIds = string.Join(",", ToInfo.CustID),
            TOControlLogID = ToInfo.LogID.ToString(),
            Description = GetTODescription(ToInfo)
        };
        await CheckValidTOParamsAndInsertFeedbackAsync(ToInfo, criteria);
    }

    protected internal virtual async Task CheckValidTOParamsAndInsertFeedbackAsync(
        TOControlFeedbackInfo ToInfo,
        CreateMultipleFeedbackCommand criteria)
    {
        var checkValidParamBeforeInsert = commonFeedback.CheckFeedbackValidParams(criteria);
        if (!checkValidParamBeforeInsert.Item1)
        {
            var messageInvalid = "Invalid Params: " + checkValidParamBeforeInsert.Item2;
            logService.LogInformation(
                $"Invalid Params. @Categorystr: {criteria.CategoryIds} @Category: {criteria.CategoryXml} @Product: {criteria.ProductXml}" +
                $" @StatusID: {criteria.StatusId} @SubCategorystr:{criteria.SubCategoryIds} " +
                $"@ResponseID:{criteria.ResponseId} @StationID:{criteria.StationId}");
            await transactionService.EditTOControlFeedbackInfoAsync(ToInfo.LogID.ToString(), CreateFeedbackJobStatus.Fail, messageInvalid);
            return;
        }
        try
        {
            var commandResult = await baseService.ExecuteNonQueryAsync(criteria);
            if (commandResult.IsSuccessful)
            {
                logService.LogInformation(
                    $"Create TO feedback successfully HeadlineId {ToInfo.HeadlineID} HeadlineName:{ToInfo.HeadlineName} ProfileName: {ToInfo.ProfileName} custName: {ToInfo.UserName}");
            }
            else
            {
                logService.LogInformation(
                    $"Create TO feedback Failed HeadlineId {ToInfo.HeadlineID} HeadlineName:{ToInfo.HeadlineName} ProfileName: {ToInfo.ProfileName} custName: {ToInfo.UserName}");

                await transactionService.EditTOControlFeedbackInfoAsync(ToInfo.LogID.ToString(), CreateFeedbackJobStatus.Fail, commandResult.Message);
                await logService.ErrorAsync(commandResult, criteria);
            }
        }
        catch (Exception ex)
        {
            await transactionService.EditTOControlFeedbackInfoAsync(ToInfo.LogID.ToString(), CreateFeedbackJobStatus.Fail, ex.Message);
            await logService.ErrorAsync(ex);
        }
    }

    protected internal virtual string GetTOProductXml(
        string sportName,
        int productId,
        List<string> betTypeMappings,
        IEnumerable<SettingItem> settings,
        IEnumerable<SystemSettingMapping> settingMappings)
    {
        var subProducts = GetTOSubProduct(productId, sportName, settings, settingMappings);
        var product = GetTOSportsbookProduct(settings);
        var tempBetTypes = GetTOBetType(betTypeMappings, settingMappings);

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

    protected internal virtual SettingItem GetTOSportsbookProduct(IEnumerable<SettingItem> settings)
    {
        var product = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.Product, StringComparison.OrdinalIgnoreCase)
                                                   && x.SettingValue.Equals(DefaultValue.Sportsbook, StringComparison.OrdinalIgnoreCase));

        return product ?? new SettingItem { SettingId = BackgroundJobConstants.ProductNA };
    }

    protected internal virtual IList<SettingItem> GetTOSubProduct(
        int productId,
        string sportName,
        IEnumerable<SettingItem> settings,
        IEnumerable<SystemSettingMapping> settingMappings)
    {
        var mappings = settingMappings.Where(mapping => mapping.SettingName.Equals(SettingName.SubProduct, StringComparison.OrdinalIgnoreCase)
                                                        && !string.IsNullOrWhiteSpace(mapping.SettingSource)
                                                        && mapping.SettingSource.Equals(sportName, StringComparison.OrdinalIgnoreCase)).ToList();
        var subProducts = settings.Where(sub => sub.SettingName.Equals(SettingName.SubProduct, StringComparison.OrdinalIgnoreCase)
                                                && sub.ParentIds.Any(parent => parent == productId)
                                                && mappings.Exists(mapping => mapping.SettingId == sub.SettingId)).ToList();

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

    protected internal virtual IList<SystemSettingMapping> GetTOBetType(
        List<string> betTypeMappings,
        IEnumerable<SystemSettingMapping> settingMappings)
    {
        IList<SystemSettingMapping> betTypes = settingMappings.Where(mapping => mapping.SettingName.Equals(SettingName.Bettype, StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(mapping.SettingSource)
            && betTypeMappings.Exists(name => name.Equals(mapping.SettingSource, StringComparison.OrdinalIgnoreCase))).ToList();

        return betTypes.Any()
            ? betTypes.GroupBy(x => x.SettingId).Select(x => x.First()).ToList()
            : [new() { SettingId = BackgroundJobConstants.BettypeNA }];
    }

    protected internal virtual string GetTORelatedCustomerXml(TOControlFeedbackInfo toInfo)
    {
        const string XmlTemplate = DefaultValue.RelatedCustomerXml;
        var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);
        stringBuilder.AppendFormat(XmlTemplate, toInfo.HeadlineID, toInfo.RelatedCustID, toInfo.UserName);
        return stringBuilder.Append(BackgroundJobConstants.EndRootTag).ToString();
    }

    protected internal virtual string GetTODescription(TOControlFeedbackInfo toInfo)
    {
        var stringBuilder = new StringBuilder(DefaultValue.autoGenerated);
        stringBuilder.Append(BackgroundJobConstants.Break);
        var TODescTemplate = commonFeedback.GetFeedbackTemplates<TODescriptionTemplate>(descTemplateXml);
        const byte SuperID = 4;
        const byte MasterID = 3;

        var isMaster = toInfo.RoleID == MasterID ? SMALevel.Master : SMALevel.Agent;
        var smaLevel = toInfo.RoleID == SuperID ? SMALevel.Super : isMaster;

        var sportName = commonFeedback.GetSportName(toInfo.SportType.ToString(), Language.en_US);
        var leagueName = commonFeedback.GetLeagueName(toInfo.LeagueID.ToString(), Language.en_US);
        var homeName = commonFeedback.GetTeamName(toInfo.HomeID.ToString(), Language.en_US);
        var awayName = commonFeedback.GetTeamName(toInfo.AwayID.ToString(), Language.en_US);

        stringBuilder.AppendFormat(TODescTemplate.Desc,
            smaLevel,
            toInfo.Level,
            sportName,
            leagueName,
            homeName,
            awayName,
            toInfo.UserName,
            toInfo.Currency,
            toInfo.TotalTurnover.ToString(BackgroundJobConstants.ThousandParseMoney),
            toInfo.ProfileName,
            toInfo.HouseTurnover.ToString(BackgroundJobConstants.ThousandParseMoney),
            toInfo.MaxTurnover.ToString(BackgroundJobConstants.ThousandParseMoney),
            toInfo.Usage.ToString(BackgroundJobConstants.ThousandParseMoney),
            toInfo.Outstanding.ToString(BackgroundJobConstants.ThousandParseMoney),
            BackgroundJobConstants.Break
        );

        stringBuilder.Append(BackgroundJobConstants.Break);
        return stringBuilder.ToString();
    }

    protected internal virtual string GetTOSolution(TOControlFeedbackInfo toInfo)
    {
        var isVipCust = toInfo.SiteLabel == "789y" && toInfo.IsVip;

        var TOSolTemplate = commonFeedback.GetFeedbackTemplates<TOSolutionTemplate>(solTemplateXml);
        string smaLevelEN;
        string smaLevelSC;
        const byte SuperID = 4;
        const byte MasterID = 3;
        if (toInfo.RoleID == SuperID)
        {
            smaLevelEN = SMALevel.Super;
            smaLevelSC = SMALevel.SuperSC;
        }
        else
        {
            smaLevelEN = toInfo.RoleID == MasterID ? SMALevel.Master : SMALevel.Agent;
            smaLevelSC = toInfo.RoleID == MasterID ? SMALevel.MasterSC : SMALevel.AgentSC;
        }

        var stringBuilder = new StringBuilder();
        var solutionTemplateEN = GetTOSolutionTemplateEN(toInfo, TOSolTemplate, smaLevelEN);
        var solutionTemplateSC = GetTOSolutionTemplateYNSC(toInfo, isVipCust, smaLevelEN, smaLevelSC, TOSolTemplate);

        stringBuilder.Append(solutionTemplateEN);
        stringBuilder.Append(BackgroundJobConstants.Break);
        stringBuilder.Append(solutionTemplateSC);
        stringBuilder.Append(BackgroundJobConstants.Break);
        return stringBuilder.ToString();
    }

    protected internal virtual StringBuilder GetTOSolutionTemplateEN(
        TOControlFeedbackInfo toInfo,
        TOSolutionTemplate TOSolTemplate,
        string smaLevelEN)
    {
        var stringBuilder = new StringBuilder();
        var outstanding = toInfo.Outstanding.ToString(BackgroundJobConstants.ThousandParseMoney);
        var sportNameEN = commonFeedback.GetSportName(toInfo.SportType.ToString(), Language.en_US);
        var leagueNameEN = commonFeedback.GetLeagueName(toInfo.LeagueID.ToString(), Language.en_US);
        var homeNameEN = commonFeedback.GetTeamName(toInfo.HomeID.ToString(), Language.en_US);
        var awayNameEN = commonFeedback.GetTeamName(toInfo.AwayID.ToString(), Language.en_US);

        stringBuilder.AppendFormat(
            TOSolTemplate.SolutionEN,
            smaLevelEN,
            toInfo.UserName,
            outstanding,
            toInfo.Currency,
            sportNameEN,
            leagueNameEN,
            homeNameEN,
            awayNameEN,
            BackgroundJobConstants.Break);
        return stringBuilder;
    }

    protected internal virtual StringBuilder GetTOSolutionTemplateYNSC(
        TOControlFeedbackInfo toInfo,
        bool isVipCust,
        string smaLevelEN,
        string smaLevelSC,
        TOSolutionTemplate turnoverSolTemplate)
    {
        var stringBuilder = new StringBuilder();
        var outstanding = toInfo.Outstanding.ToString(BackgroundJobConstants.ThousandParseMoney);
        string smaLevel;
        string solutionTemplate;
        string sportName;
        string leagueName;
        string homeName;
        string awayName;

        if (isVipCust)
        {
            smaLevel = smaLevelEN;
            solutionTemplate = turnoverSolTemplate.SolutionYN;
            sportName = commonFeedback.GetSportName(toInfo.SportType.ToString(), Language.vi_VN);
            leagueName = commonFeedback.GetLeagueName(toInfo.LeagueID.ToString(), Language.vi_VN);
            homeName = commonFeedback.GetTeamName(toInfo.HomeID.ToString(), Language.vi_VN);
            awayName = commonFeedback.GetTeamName(toInfo.AwayID.ToString(), Language.vi_VN);
        }
        else
        {
            smaLevel = smaLevelSC;
            solutionTemplate = turnoverSolTemplate.SolutionSC;
            sportName = commonFeedback.GetSportName(toInfo.SportType.ToString(), Language.zh_CN);
            leagueName = commonFeedback.GetLeagueName(toInfo.LeagueID.ToString(), Language.zh_CN);
            homeName = commonFeedback.GetTeamName(toInfo.HomeID.ToString(), Language.zh_CN);
            awayName = commonFeedback.GetTeamName(toInfo.AwayID.ToString(), Language.zh_CN);
        }

        stringBuilder.AppendFormat(
            solutionTemplate,
            smaLevel,
            toInfo.UserName,
            outstanding,
            toInfo.Currency,
            sportName,
            leagueName,
            homeName,
            awayName,
            BackgroundJobConstants.Break);

        return stringBuilder;
    }
}