namespace CRM.BackgroundJob.Services;

using Constants;
using Criteria.AutoFeedback;
using CRM.Extensions.Xml;
using CRM.Models;
using Fanex.RemotingRef;
using Microsoft.AspNetCore.Hosting;
using Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public interface ICommonFeedback
{
    string GetProductName(string productId, string languageCode);

    string GetSportName(string sportTypeId, string languageCode);

    string GetLeagueName(string leagueId, string languageCode);

    string GetLeagueName(string leagueId, string leagueGroupId, string languageCode);

    string GetLeagueNameBySiteName(string siteName, int leagueId, int leagueGroupId, string languageCode);

    string GetLeagueName3rd(string leagueId, string languageCode);

    string GetTeamName(string teamId, string languageCode);

    string GetTeamName3rd(string teamId, string languageCode);

    string GetBetTypeName(string betTypeId, string languageCode);

    string GetMatchCode(string matchCode, string languageCode);

    string GetMatchCodeFormat(string matchCode, string language);

    string GetMatchCodeVirtualSport(string matchCode, string languageCode);

    string GetMatchCodeVirtualSportFormat(string matchCode, string language);

    string GetCategoryName(string categoryId, string languageCode);

    string GetResourceCricketName(string resourceCricketId, string languageCode);

    string GetResultSettlementName(string resultSettlementId, string languageCode);

    string GetResourceByRefIdAndResourceId(string type, string refId, string languageCode);

    T GetFeedbackTemplates<T>(string fileXml);

    ValueTuple<bool, string> CheckFeedbackValidParams(CreateMultipleFeedbackCommand criteria);

    Tuple<int, int, string> GetCategorySubCategoryCWL(IEnumerable<SettingItem> settings, string settingValueName);

    Task<CommandResult> EditFeedbackSystemSettingAsync(long LastLogID, string ServiceName);

    Task<CommandResult> EditSystemSettingAsyncByEnv(string JobStatusNameEnv,
        string LastRuntimeNameEnv, string JobStatusValue, string serviceName);

    Task<CommandResult> AutoFbEditSystemSettingAsyncByEnv(string JobStatusNameEnv, string LastRuntimeNameEnv,
        string VoidingHistoryCollectionNameEnv, int MaxVoidingHistoryCollectionId);

    Task<CommandResult> EditSystemSettingAsyncByEnvFormatDate(string JobStatusNameEnv,
        string LastRuntimeNameEnv, string JobStatusValue, string serviceName);

    Task<CommandResult> EditSystemSettingUpdateLastLogIdAsyncByEnv(string JobStatusNameEnv, string LastRuntimeNameEnv,
        string lastLogIdName, int maxLastLogId, string serviceName);

    Task<CommandResult> EditSystemSettingUpdateLastSeqAndLastSubmitDateAsync(
        string JobStatusNameEnv,
        string lastSeqEnvName,
        int maxSeq,
        string lastSubmitTimeEnvName,
        string maxLastSubmitTime,
        string serviceName);

    Task<CommandResult> EditArchivingDAStatusSettingAsync(string serviceName, string isSyncArchivedAccount);

    Task<CommandResult> EditArchivingDAMaxLogIdSettingAsync(string serviceName, string maxLogFBId);
}

public class CommonFeedback(
    IRemotingRefService remotingRefService,
    ISystemSettingService systemSetting,
    IWebHostEnvironment hostingEnvironment)
    : ICommonFeedback
{
    private const string path = "Format_Templates/AutoCreateFeedback";

    public string GetProductName(string productId, string languageCode)
    {
        var productName = remotingRefService.GetResourceDataByRefIdAndResourceId("Product", productId, languageCode, string.Empty);
        return productName;
    }

    public string GetSportName(string sportTypeId, string languageCode)
    {
        var sportName = remotingRefService.GetSportName(sportTypeId, languageCode, string.Empty);
        return sportName;
    }

    public string GetLeagueName(string leagueId, string languageCode)
    {
        var leagueName = remotingRefService.GetLeagueName(leagueId, languageCode, string.Empty);
        return leagueName;
    }

    public string GetLeagueName(string leagueId, string leagueGroupId, string languageCode)
    {
        var leagueName = remotingRefService.GetLeagueName(leagueId, leagueGroupId, languageCode, string.Empty);
        return leagueName;
    }

    public string GetLeagueName3rd(string leagueId, string languageCode)
    {
        var leagueName = remotingRefService.GetLeagueName3rd(leagueId, languageCode);
        return leagueName;
    }

    public string GetLeagueNameBySiteName(string siteName, int leagueId, int leagueGroupId, string languageCode)
    {
        return siteName == BackgroundJobConstants.SiteLabel777 ? GetLeagueName(leagueId.ToString(), leagueGroupId.ToString(), languageCode) : GetLeagueName(leagueId.ToString(), languageCode);
    }

    public string GetTeamName(string teamId, string languageCode)
    {
        var teamName = remotingRefService.GetTeamName(teamId, languageCode, string.Empty);
        return teamName;
    }

    public string GetTeamName3rd(string teamId, string languageCode)
    {
        var teamName3rd = remotingRefService.GetTeamName3rd(teamId, languageCode, string.Empty);
        return teamName3rd;
    }

    public string GetBetTypeName(string betTypeId, string languageCode)
    {
        var betTypeName = remotingRefService.GetBetTypeName(betTypeId, languageCode);
        return betTypeName;
    }

    public string GetMatchCode(string matchCode, string languageCode)
    {
        var betTypeName = remotingRefService.GetBetRadarMatchCodeFormat(matchCode, languageCode);
        return betTypeName;
    }

    public string GetMatchCodeVirtualSport(string matchCode, string languageCode)
    {
        var betTypeName = remotingRefService.GetVirtualSportMatchCodeFormat(matchCode, "VYVS_MatchCode", languageCode);
        return betTypeName;
    }

    public string GetCategoryName(string categoryId, string languageCode)
    {
        var categoryName = remotingRefService.GetResourceDataByRefIdAndResourceId("Category", categoryId, languageCode, string.Empty);
        return categoryName;
    }

    public string GetResourceCricketName(string resourceCricketId, string languageCode)
    {
        var resourceCricketName = remotingRefService.GetResourceDataByRefIdAndResourceId("ResourceCricket", resourceCricketId, languageCode, string.Empty);
        return resourceCricketName;
    }

    public string GetResultSettlementName(string resultSettlementId, string languageCode)
    {
        var resultSettlementName = remotingRefService.GetResourceDataByRefIdAndResourceId("ResultSettlement", resultSettlementId, languageCode, string.Empty);
        return resultSettlementName;
    }

    public string GetResourceByRefIdAndResourceId(string type, string refId, string languageCode)
    {
        return remotingRefService.GetResourceDataByRefIdAndResourceId(type, refId, languageCode, string.Empty);
    }

    public T GetFeedbackTemplates<T>(string fileXml)
    {
        var templatePath = Path.Combine(hostingEnvironment.ContentRootPath, path, fileXml);
        var template = XmlIO.DeserializeXMLFileToObject<T>(templatePath);
        return (T)Convert.ChangeType(template, typeof(T));
    }

    public Tuple<int, int, string> GetCategorySubCategoryCWL(IEnumerable<SettingItem> settings, string settingValueName)
    {
        var category = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.MainCategory, StringComparison.OrdinalIgnoreCase)
                                                    && x.SettingValue.Equals(DefaultValue.Special_Notification, StringComparison.OrdinalIgnoreCase));
        var subCategoryId = 0;

        if (category != null)
        {
            var subCategory = GetSettingItem(settings, category, settingValueName);
            subCategoryId = subCategory?.SettingId ?? 0;
        }

        return new Tuple<int, int, string>(
            category?.SettingId ?? 0, subCategoryId,
            string.Format(DefaultValue.CategoryXml,
                category?.SettingId ?? 0,
                subCategoryId));
    }

    private static SettingItem GetSettingItem(IEnumerable<SettingItem> settings, SettingItem category, string settingValueName)
        => settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.SubCategory, StringComparison.OrdinalIgnoreCase)
                                        && x.ParentIds.Any(y => y == category.SettingId)
                                        && x.SettingValue.Equals(settingValueName, StringComparison.OrdinalIgnoreCase));

    public ValueTuple<bool, string> CheckFeedbackValidParams(CreateMultipleFeedbackCommand criteria)
    {
        var stringBuilder = new StringBuilder();
        const string invalid = "{0} is null.";
        var invalidParamsCategoryStr = string.IsNullOrEmpty(criteria.CategoryIds) ? "CategoryIds" : "";
        var invalidParamsCategory = string.IsNullOrEmpty(criteria.CategoryXml) ? "CategoryXml" : "";
        var invalidParamsProduct = string.IsNullOrEmpty(criteria.ProductXml) ? "Products" : "";
        var invalidParamsSubCategory = string.IsNullOrEmpty(criteria.SubCategoryIds) ? "SubCategoryIds" : "";
        var invalidParamsResponse = criteria.ResponseId == 0 ? "ResponseId" : "";
        var invalidParamsStation = string.IsNullOrEmpty(criteria.StationId) ? "StationId" : "";
        var invalidParamsStatus = criteria.StatusId == 0 ? "StatusId" : "";
        var invalidParams = invalidParamsCategoryStr + invalidParamsCategory + invalidParamsProduct
                            + invalidParamsSubCategory + invalidParamsResponse + invalidParamsStation + invalidParamsStatus;
        return string.IsNullOrEmpty(invalidParams) ?
            new ValueTuple<bool, string>(true, "") :
            new ValueTuple<bool, string>(false, stringBuilder.AppendFormat(invalid, invalidParams).ToString());
    }

    public async Task<CommandResult> EditFeedbackSystemSettingAsync(long LastLogID, string ServiceName)
    {
        var settingJobStatus = new List<SystemSettingItem>
        {
            new() { SettingName = SystemSettingName.JobStatus, Value = JobStatus.Completed },
            new() { SettingName = SystemSettingName.LastRuntime, Value = DateTimeOffset.Now.DateTime.ToString(BackgroundJobConstants.FormatDatetimeString) },
            new() { SettingName = SystemSettingName.LastLogID, Value = LastLogID.ToString() }
        };
        return await systemSetting.EditSystemSettingAsync(settingJobStatus, ServiceName);
    }

    public async Task<CommandResult> EditSystemSettingAsyncByEnv(string JobStatusNameEnv,
        string LastRuntimeNameEnv, string JobStatusValue, string serviceName)
    {
        var settingJobStatus = new List<SystemSettingItem>
        {
            new() { SettingName = JobStatusNameEnv, Value = JobStatusValue},
            new() { SettingName = LastRuntimeNameEnv, Value = DateTimeOffset.Now.DateTime.ToString(BackgroundJobConstants.FormatDatetimeString) }
        };
        return await systemSetting.EditSystemSettingAsync(settingJobStatus, serviceName);
    }

    public async Task<CommandResult> AutoFbEditSystemSettingAsyncByEnv(string JobStatusNameEnv, string LastRuntimeNameEnv,
        string VoidingHistoryCollectionNameEnv, int MaxVoidingHistoryCollectionId)
    {
        var settingJobStatus = new List<SystemSettingItem>
        {
            new() { SettingName = JobStatusNameEnv, Value = JobStatus.Completed},
            new() { SettingName = VoidingHistoryCollectionNameEnv, Value = MaxVoidingHistoryCollectionId.ToString()},
            new() { SettingName = LastRuntimeNameEnv, Value = DateTimeOffset.Now.DateTime.ToString(BackgroundJobConstants.FormatDatetimeString) }
        };
        return await systemSetting.EditSystemSettingAsync(settingJobStatus);
    }

    public async Task<CommandResult> EditSystemSettingAsyncByEnvFormatDate(string JobStatusNameEnv,
        string LastRuntimeNameEnv, string JobStatusValue, string serviceName)
    {
        var settingJobStatus = new List<SystemSettingItem>
        {
            new() { SettingName = JobStatusNameEnv, Value = JobStatusValue},
            new() { SettingName = LastRuntimeNameEnv, Value = DateTimeOffset.Now.DateTime.ToString("yyyy-MM-dd hh:mm:ss") }
        };
        return await systemSetting.EditSystemSettingAsync(settingJobStatus, serviceName);
    }

    public async Task<CommandResult> EditSystemSettingUpdateLastLogIdAsyncByEnv(string JobStatusNameEnv, string LastRuntimeNameEnv,
        string lastLogIdName, int maxLastLogId, string serviceName)
    {
        var settingJobStatus = new List<SystemSettingItem>
        {
            new() { SettingName = JobStatusNameEnv, Value = JobStatus.Completed },
            new() { SettingName = LastRuntimeNameEnv, Value = DateTimeOffset.Now.DateTime.ToString(BackgroundJobConstants.FormatDatetimeString) },
            new() { SettingName = lastLogIdName, Value = maxLastLogId.ToString() }
        };
        return await systemSetting.EditSystemSettingAsync(settingJobStatus, serviceName);
    }

    public async Task<CommandResult> EditSystemSettingUpdateLastSeqAndLastSubmitDateAsync(
        string JobStatusNameEnv,
        string lastSeqEnvName,
        int maxSeq,
        string lastSubmitTimeEnvName,
        string maxLastSubmitTime,
        string serviceName)
    {
        var settingJobStatus = new List<SystemSettingItem>
        {
            new() { SettingName = JobStatusNameEnv, Value = JobStatus.Completed },
            new() { SettingName = lastSeqEnvName, Value =  maxSeq.ToString()},
            new() { SettingName = lastSubmitTimeEnvName, Value = maxLastSubmitTime}
        };
        return await systemSetting.EditSystemSettingAsync(settingJobStatus, serviceName);
    }

    public static string FormatMatchCode(string matchCode, string language, Func<string, string, string> getMatchCodeFunc)
    {
        var MatchCodeReplace = matchCode;
        if (!string.IsNullOrEmpty(matchCode))
        {
            const string pattern = "({[^}]*})";
            var regex = Regex.Matches(matchCode, pattern);
            var matchCodeRegexId = regex.Select(m => m.Groups[1].Value).ToList();
            foreach (var matchCodeId in matchCodeRegexId)
            {
                var matchCodeName = getMatchCodeFunc(matchCodeId, language);
                MatchCodeReplace = MatchCodeReplace.Replace(matchCodeId, matchCodeName);
            }
        }
        return MatchCodeReplace;
    }

    public string GetMatchCodeFormat(string matchCode, string language)
    {
        return FormatMatchCode(matchCode, language, GetMatchCode);
    }

    public string GetMatchCodeVirtualSportFormat(string matchCode, string language)
    {
        return FormatMatchCode(matchCode, language, GetMatchCodeVirtualSport);
    }

    public async Task<CommandResult> EditArchivingDAStatusSettingAsync(string serviceName, string isSyncArchivedAccount)
    {
        var settingJobStatus = new List<SystemSettingItem>
        {
            new() { SettingName = SystemSettingName.IsSyncArchivedAccount, Value =  isSyncArchivedAccount}
        };
        return await systemSetting.EditSystemSettingAsync(settingJobStatus, serviceName);
    }

    public async Task<CommandResult> EditArchivingDAMaxLogIdSettingAsync(string serviceName, string maxLogFBId)
    {
        var settingJobStatus = new List<SystemSettingItem>
        {
            new() { SettingName = SystemSettingName.MaxLogFBId, Value =  maxLogFBId}
        };
        return await systemSetting.EditSystemSettingAsync(settingJobStatus, serviceName);
    }
}