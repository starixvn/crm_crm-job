namespace CRM.BackgroundJob.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Constants;
using Criteria.AutoFeedback;
using CRM.BackgroundJob.Models.TurnoverControll;
using CRM.BackgroundJob.Models.WinLossLimit;
using CRM.Models;
using CRM.Services;
using Microsoft.Extensions.Options;
using Models;

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

    private readonly string descTemplateXml = "TODesctiptionTemplate.xml";
    private readonly string solTemplateXml = "TOSolutionTemplate.xml";

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
        var importTOFeedbacks = await transactionService.ImportTOControllFeedbackInfoAsync(
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

        foreach (var ToInfor in importTOFeedbacks)
        {
            await CreateTOControlFeedbackAsync(
                ToInfor,
                settings,
                settingMappings);
        }
        var maxLogIdTO = importTOFeedbacks.Max(x => x.LogID);
        await commonFeedback.EditFeedbackSystemSettingAsync(maxLogIdTO, BackgroundJobConstants.ServiceName_TOControlFB);
    }

    protected internal virtual async Task CreateTOControlFeedbackAsync(
        TOControlFeedbackInfor ToInfor,
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
                                                   && x.SettingId == BackgroundJobConstants.ExtentionNA);
        var category = commonFeedback.GetCategorySubCategoryCWL(settings, DefaultValue.MANotification);

        var sportTypeName = commonFeedback.GetSportName(ToInfor.SportType.ToString(), Language.en_US);
        var bettypeSplits = ToInfor.BettypeList.Split(",");
        var betTypeMappings = new List<string>();

        foreach (var betType in bettypeSplits.Distinct().ToList())
        {
            var betTypeName = commonFeedback.GetBettypeName(betType, Language.en_US);
            betTypeMappings.Add(betTypeName);
        }

        var sportsbook = GetTOSportsbookProduct(settings);
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
            CustomerXml = string.Format(DefaultValue.CustomerXml, ToInfor.HeadlineID, ToInfor.HeadlineName),
            ProductXml = GetTOProductXml(sportTypeName, sportsbook.SettingId, betTypeMappings, settings, settingMappings),
            Solution = GetTOSolution(ToInfor),
            RelatedCustomerXml = GetTORelatedCustomerXml(ToInfor),
            ListOrigCustIds = string.Join(",", ToInfor.CustID),
            TOControlLogID = ToInfor.LogID.ToString(),
            Description = GetTODescription(ToInfor)
        };
        await CheckValidTOParamsAndInsertFeedbackAsync(ToInfor, criteria);
    }

    protected internal virtual async Task CheckValidTOParamsAndInsertFeedbackAsync(
        TOControlFeedbackInfor ToInfor,
        CreateMutipleFeedbackCommand criteria)
    {
        var checkValidParamBeforeInsert = commonFeedback.CheckFeedbackValidParams(criteria);
        if (!checkValidParamBeforeInsert.Item1)
        {
            var messageInvalid = "Invalid Params: " + checkValidParamBeforeInsert.Item2;
            logService.LogInformation(
                $"Invalid Params. @Categorystr: {criteria.CategoryIds} @Category: {criteria.CategoryXml} @Product: {criteria.ProductXml} @StatusID: {criteria.StatusId} @SubCategorystr:{criteria.SubCategoryIds} @ResponseID:{criteria.ResponseId} @StationID:{criteria.StationId}");
            await transactionService.EditTOControlFeedbackInfoAsync(ToInfor.LogID.ToString(), CreateFeedbackJobStatus.Fail, messageInvalid);
            return;
        }
        try
        {
            var commandResult = await baseService.ExecuteNonQueryAsync(criteria);
            if (commandResult.IsSuccessful)
            {
                logService.LogInformation(
                    $"Create TO feedback successfully HeadlineId {ToInfor.HeadlineID} HeadlineName:{ToInfor.HeadlineName} ProfileName: {ToInfor.ProfileName} custName: {ToInfor.UserName}");
            }
            else
            {
                logService.LogInformation(
                    $"Create TO feedback Faild HeadlineId {ToInfor.HeadlineID} HeadlineName:{ToInfor.HeadlineName} ProfileName: {ToInfor.ProfileName} custName: {ToInfor.UserName}");

                await transactionService.EditTOControlFeedbackInfoAsync(ToInfor.LogID.ToString(), CreateFeedbackJobStatus.Fail, commandResult.Message);
                await logService.ErrorAsync(commandResult, criteria);
            }
        }
        catch (Exception ex)
        {
            await transactionService.EditTOControlFeedbackInfoAsync(ToInfor.LogID.ToString(), CreateFeedbackJobStatus.Fail, ex.Message);
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
        var tempBettypes = GetTOBettype(betTypeMappings, settingMappings);

        var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);
        foreach (var settingId in subProducts.Select(x => x.SettingId))
        {
            if (tempBettypes.Any())
            {
                foreach (var bettype in tempBettypes)
                {
                    stringBuilder.AppendFormat(DefaultValue.ProductXml, product.SettingId, settingId, bettype.SettingId);
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
                                                && mappings != null
                                                && mappings.Exists(mapping => mapping.SettingId == sub.SettingId)).ToList();

        return subProducts.Any()
            ? subProducts.GroupBy(x => x.SettingId).Select(x => x.First()).ToList()
            : new List<SettingItem>
            {
                new() { SettingId = BackgroundJobConstants.SubProductNA, ParentIds = new List<int> { BackgroundJobConstants.ProductNA } }
            };
    }

    protected internal virtual IList<SystemSettingMapping> GetTOBettype(
        List<string> betTypeMappings,
        IEnumerable<SystemSettingMapping> settingMappings)
    {
        IList<SystemSettingMapping> bettypes = settingMappings.Where(mapping => mapping.SettingName.Equals(SettingName.Bettype, StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(mapping.SettingSource)
            && betTypeMappings.Exists(name => name.Equals(mapping.SettingSource, StringComparison.OrdinalIgnoreCase))).ToList();

        return bettypes != null && bettypes.Any()
            ? bettypes.GroupBy(x => x.SettingId).Select(x => x.First()).ToList()
            : new List<SystemSettingMapping>
            {
                new() { SettingId = BackgroundJobConstants.BettypeNA }
            };
    }

    protected internal virtual string GetTORelatedCustomerXml(TOControlFeedbackInfor ToInfor)
    {
        const string XmlTemplate = DefaultValue.RelatedCustomerXml;
        var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);
        stringBuilder.AppendFormat(XmlTemplate, ToInfor.HeadlineID, ToInfor.RelatedCustID, ToInfor.UserName);
        return stringBuilder.Append(BackgroundJobConstants.EndRootTag).ToString();
    }

    protected internal virtual string GetTODescription(TOControlFeedbackInfor ToInfor)
    {
        var stringBuilder = new StringBuilder(DefaultValue.autoGenerated);
        stringBuilder.Append(BackgroundJobConstants.Break);
        var TODescTemplate = commonFeedback.GetFeedbackTemplates<TODescriptionTemplate>(descTemplateXml);
        const byte SuperID = 4;
        const byte MasterID = 3;

        var isMaster = ToInfor.RoleID == MasterID ? SMALevel.Master : SMALevel.Agent;
        var smaLevel = ToInfor.RoleID == SuperID ? SMALevel.Super : isMaster;

        var sportName = commonFeedback.GetSportName(ToInfor.SportType.ToString(), Language.en_US);
        var leagueName = commonFeedback.GetLeagueName(ToInfor.LeagueID.ToString(), Language.en_US);
        var homeName = commonFeedback.GetTeamName(ToInfor.HomeID.ToString(), Language.en_US);
        var awayName = commonFeedback.GetTeamName(ToInfor.AwayID.ToString(), Language.en_US);

        stringBuilder.AppendFormat(TODescTemplate.Desc,
            smaLevel,
            ToInfor.Level,
            sportName,
            leagueName,
            homeName,
            awayName,
            ToInfor.UserName,
            ToInfor.Currency,
            ToInfor.TotalTurnover.ToString(BackgroundJobConstants.ThousanPaseMoney),
            ToInfor.ProfileName,
            ToInfor.HouseTurnover.ToString(BackgroundJobConstants.ThousanPaseMoney),
            ToInfor.MaxTurnover.ToString(BackgroundJobConstants.ThousanPaseMoney),
            ToInfor.Usage.ToString(BackgroundJobConstants.ThousanPaseMoney),
            ToInfor.Outstanding.ToString(BackgroundJobConstants.ThousanPaseMoney),
            BackgroundJobConstants.Break
        );

        stringBuilder.Append(BackgroundJobConstants.Break);
        return stringBuilder.ToString();
    }

    protected internal virtual string GetTOSolution(TOControlFeedbackInfor ToInfor)
    {
        var isVipCust = ToInfor.SiteLabel == "789y" && ToInfor.IsVip;

        var TOSolTemplate = commonFeedback.GetFeedbackTemplates<TOSolutionTemplate>(solTemplateXml);
        var smaLevelEN = "";
        var smaLevelSC = "";
        const byte SuperID = 4;
        const byte MasterID = 3;
        if (ToInfor.RoleID == SuperID)
        {
            smaLevelEN = SMALevel.Super;
            smaLevelSC = SMALevel.SuperSC;
        }
        else
        {
            smaLevelEN = ToInfor.RoleID == MasterID ? SMALevel.Master : SMALevel.Agent;
            smaLevelSC = ToInfor.RoleID == MasterID ? SMALevel.MasterSC : SMALevel.AgentSC;
        }

        var stringBuilder = new StringBuilder();
        var solutionTemplateEN = GetTOSolutionTemplateEN(ToInfor, TOSolTemplate, smaLevelEN);
        var solutionTemplateSC = GetTOSolutionTemplateYNSC(ToInfor, isVipCust, smaLevelEN, smaLevelSC, TOSolTemplate);

        stringBuilder.Append(solutionTemplateEN);
        stringBuilder.Append(BackgroundJobConstants.Break);
        stringBuilder.Append(solutionTemplateSC);
        stringBuilder.Append(BackgroundJobConstants.Break);
        return stringBuilder.ToString();
    }

    protected internal virtual StringBuilder GetTOSolutionTemplateEN(
        TOControlFeedbackInfor ToInfor,
        TOSolutionTemplate TOSolTemplate,
        string smaLevelEN)
    {
        var stringBuilder = new StringBuilder();
        var outstanding = ToInfor.Outstanding.ToString(BackgroundJobConstants.ThousanPaseMoney);
        var sportNameEN = commonFeedback.GetSportName(ToInfor.SportType.ToString(), Language.en_US);
        var leagueNameEN = commonFeedback.GetLeagueName(ToInfor.LeagueID.ToString(), Language.en_US);
        var homeNameEN = commonFeedback.GetTeamName(ToInfor.HomeID.ToString(), Language.en_US);
        var awayNameEN = commonFeedback.GetTeamName(ToInfor.AwayID.ToString(), Language.en_US);

        stringBuilder.AppendFormat(
            TOSolTemplate.SolutionEN,
            smaLevelEN,
            ToInfor.UserName,
            outstanding,
            ToInfor.Currency,
            sportNameEN,
            leagueNameEN,
            homeNameEN,
            awayNameEN,
            BackgroundJobConstants.Break);
        return stringBuilder;
    }

    protected internal virtual StringBuilder GetTOSolutionTemplateYNSC(
        TOControlFeedbackInfor ToInfor,
        bool isVipCust,
        string smaLevelEN,
        string smaLevelSC,
        TOSolutionTemplate TOSolTemplate)
    {
        var stringBuilder = new StringBuilder();
        var outstanding = ToInfor.Outstanding.ToString(BackgroundJobConstants.ThousanPaseMoney);
        var smaLevel = "";
        var solutionTemplate = "";
        var sportName = "";
        var leagueName = "";
        var homeName = "";
        var awayName = "";

        if (isVipCust)
        {
            smaLevel = smaLevelEN;
            solutionTemplate = TOSolTemplate.SolutionYN;
            sportName = commonFeedback.GetSportName(ToInfor.SportType.ToString(), Language.vi_VN);
            leagueName = commonFeedback.GetLeagueName(ToInfor.LeagueID.ToString(), Language.vi_VN);
            homeName = commonFeedback.GetTeamName(ToInfor.HomeID.ToString(), Language.vi_VN);
            awayName = commonFeedback.GetTeamName(ToInfor.AwayID.ToString(), Language.vi_VN);
        }
        else
        {
            smaLevel = smaLevelSC;
            solutionTemplate = TOSolTemplate.SolutionSC;
            sportName = commonFeedback.GetSportName(ToInfor.SportType.ToString(), Language.zh_CN);
            leagueName = commonFeedback.GetLeagueName(ToInfor.LeagueID.ToString(), Language.zh_CN);
            homeName = commonFeedback.GetTeamName(ToInfor.HomeID.ToString(), Language.zh_CN);
            awayName = commonFeedback.GetTeamName(ToInfor.AwayID.ToString(), Language.zh_CN);
        }

        stringBuilder.AppendFormat(
            solutionTemplate,
            smaLevel,
            ToInfor.UserName,
            outstanding,
            ToInfor.Currency,
            sportName,
            leagueName,
            homeName,
            awayName,
            BackgroundJobConstants.Break);

        return stringBuilder;
    }
}