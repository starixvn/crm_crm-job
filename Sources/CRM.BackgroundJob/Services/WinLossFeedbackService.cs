namespace CRM.BackgroundJob.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Constants;
using Criteria.AutoFeedback;
using CRM.BackgroundJob.Models;
using CRM.BackgroundJob.Models.WinLossLimit;
using CRM.Models;
using CRM.Services;
using Microsoft.Extensions.Options;

public interface IWinLossFeedbackService
{
    Task CreateFeedbackForWinLossLimitAsync(WinLossSystemSetting setting);
}

public class WinLossFeedbackService : IWinLossFeedbackService
{
    private readonly AppJobSettings appSettings;
    private readonly IBaseService baseService;
    private readonly ISettingService settingService;
    private readonly ISystemSettingService systemSettingService;
    private readonly ICustomerService customerService;
    private readonly ICrmLogService logService;
    private readonly IFeedbackTransactionService transactionService;
    private readonly ICommonFeedback commonFeedback;

    private readonly string templateXml = "FeedbackTemplate.xml";

#pragma warning disable S107 // Methods should not have too many parameters

    public WinLossFeedbackService(
        IOptions<AppJobSettings> appSettings,
        IBaseService baseService,
        ISettingService settingService,
        ISystemSettingService systemSettingService,
        ICustomerService customerService,
        ICrmLogService logService,
        IFeedbackTransactionService transactionService,
        ICommonFeedback commonFeedback
    )
#pragma warning restore S107 // Methods should not have too many parameters
    {
        this.appSettings = appSettings.Value;
        this.baseService = baseService;
        this.settingService = settingService;
        this.systemSettingService = systemSettingService;
        this.customerService = customerService;
        this.logService = logService;
        this.transactionService = transactionService;
        this.commonFeedback = commonFeedback;
    }

    public async Task CreateFeedbackForWinLossLimitAsync(WinLossSystemSetting setting)
    {
        var settingJobStatus = new List<SystemSettingItem>
        {
            new() { SettingName = SystemSettingName.JobStatus, Value = JobStatus.Running },
            new() { SettingName = SystemSettingName.LastRuntime, Value = DateTimeOffset.Now.DateTime.ToString("yyyy-MM-dd hh:mm:ss.fff tt") }
        };
        logService.LogInformation($"Start run WL serivce with last log ID: {setting.LastLogId}!");
        await systemSettingService.EditSystemSettingAsync(settingJobStatus, BackgroundJobConstants.ServiceName_WinLimitFB);
        var cusWinLimit = await customerService.GetListCustReachLimitAsync(setting);
        var custIds = cusWinLimit.Select(x => x.CustId).Distinct();
        var headlines = await customerService.GetGroupHeadlineAsync(custIds);
        var settingNames = new[] { SettingName.Product, SettingName.SubProduct, SettingName.Bettype };
        var settings = await settingService.GetAllSettingAsync();
        var settingMappings = await settingService.GetSystemSettingMappingAsync(settingNames);

        var winlossInfo = await transactionService.ImportWinLimitFeedbackInfoAsync(
            cusWinLimit,
            headlines
        );
        if (!winlossInfo.Item1.Any())
        {
            logService.LogInformation(
                $"Import CWL doesn't have any data with custId: {string.Join(",", headlines.Select(x => x.HeadlineId))} headlineName: {string.Join(",", headlines.Select(x => x.HeadlineName))}!");
            var lastLogID = cusWinLimit.Any() ? cusWinLimit.Max(x => x.LogID) : setting.LastLogId;
            await commonFeedback.EditFeedbackSystemSettingAsync(lastLogID, BackgroundJobConstants.ServiceName_WinLimitFB);
            return;
        }

        foreach (var winLimit in winlossInfo.Item1)
        {
            var isVipHeadline = winLimit.SiteLabel == "789y" && winLimit.IsVip;
            await CreateWinLossFeedbackCreditAsync(
                isVipHeadline,
                winLimit,
                winlossInfo.Item2,
                winlossInfo.Item3,
                settings,
                settingMappings);
        }
        var maxLastLogID = winlossInfo.Item1.Max(x => x.LogID);
        await commonFeedback.EditFeedbackSystemSettingAsync(maxLastLogID, BackgroundJobConstants.ServiceName_WinLimitFB);
    }

    protected internal virtual async Task CreateWinLossFeedbackCreditAsync(
        bool isVipHeadline,
        WinLimitFeedBackInfo winLimit,
        IEnumerable<BettypeInfo> bettype,
        IEnumerable<SportInfo> sports,
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
        var category = commonFeedback.GetCategorySubCategoryCWL(settings, DefaultValue.CompanyWLLimit);
        var betTypeLists = winLimit.BettypeIDList.Split(",");
        var betTypeMappings = (from bet in bettype
            where betTypeLists.Contains(bet.Bettype)
            select bet).ToList();
        var criteria = new CreateMutipleFeedbackCommand
        {
            CreatorId = appSettings.CreatorId,
            UserId = appSettings.CreatorId,
            FeedbackDate = feedbackDate.Subtract(timeSpan),
            ResolutionDate = feedbackDate.AddMinutes(1).Subtract(timeSpan),
            IsCheckedDefaultComm = true,
            StationId = station != null ? station.SettingId.ToString() : string.Empty,
            StatusId = status != null ? status.SettingId : 0,
            ResponseId = response != null ? response.SettingId : 0,
            UserRole = BackgroundJobConstants.UserRole,
            CategoryXml = category.Item3,
            CategoryIds = category.Item1.ToString(),
            SubCategoryIds = category.Item2.ToString(),
            Subject = DefaultValue.Special_Notification,
            AutoField = string.Empty,
            AutoField1 = string.Empty,
            AutoField2 = string.Format(DefaultValue.DateTimeTool, feedbackDate.ToString("yyyyMMdd"), feedbackDate.ToString("hh:mmtt")),
            AutoField3 = $"{Environment.NewLine}System ID: ",
            LogID = winLimit.LogID,
            CustomerXml = string.Format(DefaultValue.CustomerXml, winLimit.HeadlineId, winLimit.HeadlineName),
            ProductXml = GetProductXml(winLimit, betTypeMappings, sports, settings, settingMappings),
            Solution = GetSolution(winLimit, isVipHeadline),
            RelatedCustomerXml = GetRelatedCustomerXml(winLimit),
            ListOrigCustIds = string.Join(",", winLimit.CustId),
            Description = GetDescription(winLimit, betTypeMappings)
        };

        logService.LogInformation($"end create win loss feedback custID: {winLimit.CustId}!");
        await CheckValidParamsAndInsertFeedbackAsync(winLimit, criteria);
    }

    protected internal virtual async Task CheckValidParamsAndInsertFeedbackAsync(
        WinLimitFeedBackInfo winLimit,
        CreateMutipleFeedbackCommand criteria)
    {
        var checkValidParamBeforeInsert = commonFeedback.CheckFeedbackValidParams(criteria);
        if (!checkValidParamBeforeInsert.Item1)
        {
            var messageInvalid = "Invalid Params: " + checkValidParamBeforeInsert.Item2;
            logService.LogInformation(
                $"Invalid Params. @Categorystr: {criteria.CategoryIds} @Category: {criteria.CategoryXml} @Product: {criteria.ProductXml} @StatusID: {criteria.StatusId} @SubCategorystr:{criteria.SubCategoryIds} @ResponseID:{criteria.ResponseId} @StationID:{criteria.StationId}");
            await transactionService.EditFeedbackWinLossInfoAsync(winLimit.LogID, CreateFeedbackJobStatus.Fail, messageInvalid);
            return;
        }
        try
        {
            var commandResult = await baseService.ExecuteNonQueryAsync(criteria);
            if (commandResult.IsSuccessful)
            {
                logService.LogInformation(
                    $"Create feedback successfully. HeadlineId: {winLimit.HeadlineId} logID:{winLimit.LogID}");
            }
            else
            {
                logService.LogInformation(
                    $"Create feedback failed. HeadlineId: {winLimit.HeadlineId} logID: {winLimit.LogID}");

                await transactionService.EditFeedbackWinLossInfoAsync(winLimit.LogID, CreateFeedbackJobStatus.Fail, commandResult.Message);
                await logService.ErrorAsync(commandResult, criteria);
            }
        }
        catch (Exception ex)
        {
            logService.LogInformation(
                $"Create feedback failed. HeadlineId: {winLimit.HeadlineId} logID:{winLimit.LogID}");
            await transactionService.EditFeedbackWinLossInfoAsync(winLimit.LogID, CreateFeedbackJobStatus.Fail, ex.Message);
            await logService.ErrorAsync(ex);
        }
    }

    protected internal virtual string GetProductXml(
        WinLimitFeedBackInfo winLimit,
        List<BettypeInfo> betTypeMappings,
        IEnumerable<SportInfo> sports,
        IEnumerable<SettingItem> settings,
        IEnumerable<SystemSettingMapping> settingMappings)
    {
        const string XmlTemplate = DefaultValue.ProductXml;
        var product = GetProduct(settings, settingMappings, winLimit.ProductName);
        var subProducts = GetSubProduct(winLimit, sports, product.SettingId, settings, settingMappings);
        var bettypes = GetBettype(subProducts.Select(x => x.SettingId), betTypeMappings, settingMappings, settings);
        var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);

        foreach (var settingId in subProducts.Select(x => x.SettingId))
        {
            IList<SettingItem> tempBettypes = bettypes.Where(x => x.ParentIds.Any(y => y == settingId)).ToList();
            if (tempBettypes.Any())
            {
                foreach (var bettype in tempBettypes)
                {
                    stringBuilder.AppendFormat(XmlTemplate, product.SettingId, settingId, bettype.SettingId);
                }
            }
            else
            {
                stringBuilder.AppendFormat(XmlTemplate, product.SettingId, settingId, BackgroundJobConstants.BettypeNA);
            }
        }
        return stringBuilder.Append(BackgroundJobConstants.EndRootTag).ToString();
    }

    protected internal virtual SettingItem GetProduct(
        IEnumerable<SettingItem> settings,
        IEnumerable<SystemSettingMapping> settingMappings,
        string productName)
    {
        var productMapping = settingMappings.FirstOrDefault(p => p.SettingName.Equals(SettingName.Product, StringComparison.OrdinalIgnoreCase)
                                                                 && !string.IsNullOrWhiteSpace(p.SettingSource)
                                                                 && p.SettingSource.Equals(productName, StringComparison.OrdinalIgnoreCase));

        var product = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.Product, StringComparison.OrdinalIgnoreCase)
                                                   && productMapping != null
                                                   && x.SettingId == productMapping.SettingId);

        return product ?? new SettingItem { SettingId = BackgroundJobConstants.ProductNA };
    }

    protected internal virtual List<SettingItem> GetSubProduct(
        WinLimitFeedBackInfo winLimit,
        IEnumerable<SportInfo> sports,
        int productId,
        IEnumerable<SettingItem> settings,
        IEnumerable<SystemSettingMapping> settingMappings)
    {
        var sportsMapping = sports.Where(x => x.LogID == winLimit.LogID);

        var subProductsMapping = settingMappings.Where(mapping => mapping.SettingName.Equals(SettingName.SubProduct, StringComparison.OrdinalIgnoreCase)
                                                                  && !string.IsNullOrWhiteSpace(mapping.SettingSource)
                                                                  && sportsMapping.Any(sport => mapping.SettingSource.Equals(sport.SportName, StringComparison.OrdinalIgnoreCase))).ToList();

        IList<SettingItem> subProducts = settings.Where(sub => sub.SettingName.Equals(SettingName.SubProduct, StringComparison.OrdinalIgnoreCase)
                                                               && sub.ParentIds.Any(parent => parent == productId)
                                                               && subProductsMapping != null
                                                               && subProductsMapping.Exists(mapping => mapping.SettingId == sub.SettingId)).ToList();

        return subProducts.Any()
            ? subProducts.GroupBy(x => x.SettingId).Select(x => x.First()).ToList()
            : new List<SettingItem>
            {
                new() { SettingId = BackgroundJobConstants.SubProductNA, ParentIds = new List<int> { BackgroundJobConstants.ProductNA } }
            };
    }

    protected internal virtual IList<SettingItem> GetBettype(
        IEnumerable<int> subProducts,
        List<BettypeInfo> betTypeMappings,
        IEnumerable<SystemSettingMapping> settingMappings,
        IEnumerable<SettingItem> settings)
    {
        IList<SystemSettingMapping> bettypeMapping = settingMappings.Where(mapping => mapping.SettingName.Equals(SettingName.Bettype, StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(mapping.SettingSource)
            && betTypeMappings.Exists(betInfo => betInfo.Name.Equals(mapping.SettingSource, StringComparison.OrdinalIgnoreCase))).ToList();

        IList<SettingItem> bettypes = settings.Where(bettype => bettype.SettingName.Equals(SettingName.Bettype, StringComparison.OrdinalIgnoreCase)
                                                                && subProducts.Any(sub => bettype.ParentIds.Any(parent => parent == sub))
                                                                && bettypeMapping != null
                                                                && bettypeMapping.Any(mapping => mapping.SettingId == bettype.SettingId)).ToList();

        return bettypes != null && bettypes.Any()
            ? bettypes.GroupBy(x => x.SettingId).Select(x => x.First()).ToList()
            : new List<SettingItem>
            {
                new() { SettingId = BackgroundJobConstants.BettypeNA, ParentIds = new List<int> { BackgroundJobConstants.SubProductNA } }
            };
    }

#pragma warning disable S1541 // Methods and properties should not be too complex

    protected internal virtual string GetSolution(WinLimitFeedBackInfo winLimit, bool isVipHeadline)
#pragma warning restore S1541 // Methods and properties should not be too complex
    {
        var productEN = GetTransformedProductName(winLimit, Language.en_US);
        var productSC = GetTransformedProductName(winLimit, Language.zh_CN);
        var productYN = GetTransformedProductName(winLimit, Language.vi_VN);

        var productFormat = "";
        var templateYellow = "";
        var templateRed = "";
        var templateBlack = "";
        var templateGrey = "";
        var templateRng = "";
        var templateJackpotPrize = "";

        var totalWinAmt = Math.Truncate(winLimit.TotalWinAmt).ToString(BackgroundJobConstants.ThousanPase);
        var templates = commonFeedback.GetFeedbackTemplates<FeedbackTemplate>(templateXml);
        var formatTemplates = templates.Templates;
        if (isVipHeadline)
        {
            templateRng = formatTemplates.RNGTemplateENYN;
            templateYellow = formatTemplates.YellowTemplateENYN;
            templateRed = formatTemplates.RedTemplateENYN;
            templateBlack = formatTemplates.BlackTemplateYN;
            templateGrey = formatTemplates.GreyTemplateENYN;
            templateJackpotPrize = formatTemplates.JackpotPrizeTemplateENYN;
            productFormat = productYN;
        }
        else
        {
            templateRng = formatTemplates.RNGTemplateENSC;
            templateYellow = formatTemplates.YellowTemplateENSC;
            templateRed = formatTemplates.RedTemplateENSC;
            templateBlack = formatTemplates.BlackTemplateSC;
            templateGrey = formatTemplates.GreyTemplateENSC;
            templateJackpotPrize = formatTemplates.JackpotPrizeTemplateENSC;
            productFormat = productSC;
        }
        var stringBuilder = new StringBuilder();
        if (winLimit.ProductName == "RNG Casino" && (winLimit.ProductGroup == "RNG Casino - Minor Jackpot" || winLimit.ProductGroup == "RNG Casino - Mega Jackpot"))
        {
            stringBuilder.AppendFormat(templateRng, winLimit.UserName, totalWinAmt, winLimit.Currency, BackgroundJobConstants.LineBreak);
        }
        else if (winLimit.ProductName == "PP" && winLimit.ProductGroup == "PP - Jackpot Prize")
        {
            stringBuilder.AppendFormat(templateJackpotPrize, winLimit.UserName, totalWinAmt, winLimit.Currency, BackgroundJobConstants.LineBreak);
        }
        else
        {
            switch (winLimit.Level)
            {
                case "Orange":
                case "Yellow":
                    stringBuilder.AppendFormat(templateYellow, winLimit.UserName, totalWinAmt, winLimit.Currency, productEN, productFormat, BackgroundJobConstants.LineBreak);
                    break;

                case "Grey":
                    stringBuilder.AppendFormat(templateGrey, winLimit.UserName, totalWinAmt, winLimit.Currency, productEN, productFormat, BackgroundJobConstants.LineBreak);
                    break;

                case "Red":
                    stringBuilder.AppendFormat(templateRed, winLimit.UserName, totalWinAmt, winLimit.Currency, productEN, productFormat, BackgroundJobConstants.LineBreak);
                    break;

                case "Black":
                    stringBuilder.AppendFormat(templateBlack, winLimit.UserName, totalWinAmt, winLimit.Currency, productEN, productFormat, BackgroundJobConstants.LineBreak);
                    break;

                default:
                    stringBuilder.Append("");
                    break;
            }
        }
        return stringBuilder.ToString();
    }

    protected internal virtual string GetDescription(WinLimitFeedBackInfo winlimit, List<BettypeInfo> bettype)
    {
        const int numGames = 3;
        var numGamesDisplay = bettype.Count > 3 ? bettype.Take(numGames).ToList() : bettype;
        var numGamesFormat = numGamesDisplay.Any() ? string.Join(", ", numGamesDisplay.Select(x => x.Name)) : "";
        var MaxWinLimitSetting = Math.Truncate(Convert.ToDouble(winlimit.MaxWinLimitSetting)).ToString(BackgroundJobConstants.ThousanPase);
        var TotalWinAmt_RM = Math.Truncate(winlimit.TotalWinAmt_RM).ToString(BackgroundJobConstants.ThousanPase);
        var TotalWinAmt = Math.Truncate(winlimit.TotalWinAmt).ToString(BackgroundJobConstants.ThousanPase);
        var productName = TransfromSABAxDToTaraPlaysxD(winlimit.Site, winlimit.ProductName);

        var stringBuilder = new StringBuilder(DefaultValue.autoGenerated);
        stringBuilder.Append("<br />Informed Headline Win/Loss Alert for below member:");
        stringBuilder.AppendFormat("<br />Product: {0}", productName);
        stringBuilder.Append("<br />Game Type: ");
        stringBuilder.AppendFormat(bettype.Count > numGames ? "{0} ..." : "{0}", numGamesFormat);
        stringBuilder.AppendFormat("<br /> UserName: {0}, Site: {1} ({2})", winlimit.UserName, winlimit.Site, winlimit.SiteLabel);
        stringBuilder.AppendFormat("<br /> Winning/Losing Limit Amount: {0} (RM), Winning/Losing Amount in RM: {1} (RM), Winloss Amount in player currency: {2} ({3})"
            , MaxWinLimitSetting, TotalWinAmt_RM, TotalWinAmt, winlimit.Currency);
        stringBuilder.AppendFormat("<br />Reached Limit Time: {0}, Reached Limit Level: {1}", winlimit.ReachedTime, winlimit.Level);
        stringBuilder.Append(BackgroundJobConstants.LineBreak);

        return stringBuilder.ToString();
    }

    protected internal virtual string GetRelatedCustomerXml(WinLimitFeedBackInfo winLimit)
    {
        const string XmlTemplate = DefaultValue.RelatedCustomerXml;
        var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);
        stringBuilder.AppendFormat(XmlTemplate, winLimit.HeadlineId, winLimit.RelatedCustId, winLimit.UserName);

        return stringBuilder.Append(BackgroundJobConstants.EndRootTag).ToString();
    }

    protected internal virtual string GetTransformedProductName(WinLimitFeedBackInfo winLimit, string language)
    {
        var productName = commonFeedback.GetProductName(winLimit.ProductID.ToString(), language);
        return TransfromSABAxDToTaraPlaysxD(winLimit.Site, productName);
    }

    protected internal virtual string TransfromSABAxDToTaraPlaysxD(string site, string productName)
    {
        const string sourceProduct = "SABA xD";
        const string transformedProduct = "TaraPlays xD";

        if (site == BackgroundJobConstants.SiteLabel777 && productName == sourceProduct)
        {
            return transformedProduct;
        }
        return productName;
    }
}