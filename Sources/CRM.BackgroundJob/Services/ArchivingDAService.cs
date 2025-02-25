namespace CRM.BackgroundJob.Services;

using Constants;
using Criteria.AutoFeedback;
using CRM.BackgroundJob.Models.ArchivingDA;
using CRM.Services;
using Microsoft.Extensions.Options;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IArchivingDAService
{
    Task CreateFeedbackForArchivingDAAsync(ArchivingDASetting archivingDASetting, bool isUAT,
        DateTime LastRuntimeEnv, string LastRuntimeNameEnv, string JobStatusNameEnv, string Environment
    );
}

public class ArchivingDAService(
    IOptions<AppJobSettings> appSettings,
    IBaseService baseService,
    ISettingService settingService,
    ICrmLogService logService,
    ICommonFeedback commonFeedback,
    IFeedbackTransactionService feedbackTransactionService)
    : IArchivingDAService
{
    private readonly AppJobSettings appSettings = appSettings.Value;

    private const string templateXml = "ArchivingDATemplate.xml";

    public async Task CreateFeedbackForArchivingDAAsync(ArchivingDASetting archivingDASetting, bool isUAT,
        DateTime LastRuntimeEnv, string LastRuntimeNameEnv, string JobStatusNameEnv, string Environment
    )
    {
        await commonFeedback.EditSystemSettingAsyncByEnv(JobStatusNameEnv, LastRuntimeNameEnv, JobStatus.Running, BackgroundJobConstants.ServiceName_ArchivingDA);
        var settingNames = new[] { SettingName.Product, SettingName.SubProduct, SettingName.Bettype };
        var settings = await settingService.GetAllSettingAsync();
        var settingMappings = await settingService.GetSystemSettingMappingAsync(settingNames);
        var isStageOne = await feedbackTransactionService.GetArchivingDAStatus();

        if (isStageOne)
        {
            IEnumerable<ArchivingAutoFbInfo> archivingAutoFbInfos;
            if (archivingDASetting.IsSyncArchivedAccount)
            {
                archivingAutoFbInfos = await feedbackTransactionService.ArchivingAutoFeedbackInfoInsAsync(archivingDASetting, Environment);
            }
            else
            {
                var custInfo = await feedbackTransactionService.GetArchivingCustInfo();
                var archivedDate = custInfo.First().ArchivedDate;
                var custIdsJoin = string.Join(",", custInfo.Select(x => x.CustID));
                await feedbackTransactionService.ArchivingImportDirectAccountAsync(custIdsJoin, archivedDate);
                await commonFeedback.EditArchivingDAStatusSettingAsync(BackgroundJobConstants.ServiceName_ArchivingDA, "1");
                archivingAutoFbInfos = await feedbackTransactionService.ArchivingAutoFeedbackInfoInsAsync(archivingDASetting, Environment);
            }

            await CheckExistAnyArchivingFb(archivingAutoFbInfos, settings, settingMappings, JobStatusNameEnv, LastRuntimeNameEnv);
        }
        else
        {
            logService.LogInformation("stage is not one, update status!!!");
            await UpdateArchivingStatus(archivingDASetting.IsSyncArchivedAccount, LastRuntimeNameEnv, JobStatusNameEnv);
        }
        await commonFeedback.EditSystemSettingAsyncByEnv(JobStatusNameEnv, LastRuntimeNameEnv, JobStatus.Completed, BackgroundJobConstants.ServiceName_ArchivingDA);
    }

    protected internal virtual async Task CheckExistAnyArchivingFb(
        IEnumerable<ArchivingAutoFbInfo> archivingAutoFbInfos,
        IEnumerable<SettingItem> settings,
        IEnumerable<SystemSettingMapping> settingMappings,
        string jobStatusNameEnv, string lastRuntimeNameEnv)
    {
        if (archivingAutoFbInfos.Any())
        {
            foreach (var archivingAutoFbInfo in archivingAutoFbInfos)
            {
                await CreateFeedbackForArchivingDACreditAsync(archivingAutoFbInfo, settings, settingMappings);
            }
            var maxLogId = archivingAutoFbInfos.Max(x => x.LogFeedbackId);
            await commonFeedback.EditArchivingDAMaxLogIdSettingAsync(BackgroundJobConstants.ServiceName_ArchivingDA, maxLogId.ToString());
        }
    }

    protected internal virtual async Task CreateFeedbackForArchivingDACreditAsync(
        ArchivingAutoFbInfo archivingAutoFbInfo,
        IEnumerable<SettingItem> settings,
        IEnumerable<SystemSettingMapping> settingMappings)
    {
        logService.LogInformation(
            $"Start create archiving DA feedback logId: {archivingAutoFbInfo.LogFeedbackId} , headlineName: {archivingAutoFbInfo.HeadlineName}!");
        var timeSpan = new TimeSpan(appSettings.TimeSpan, 0, 0);
        var feedbackDate = DateTimeOffset.Now.DateTime.Add(timeSpan);
        var status = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.Status, StringComparison.OrdinalIgnoreCase)
                                                  && x.SettingValue.Equals(DefaultValue.Closed, StringComparison.OrdinalIgnoreCase));
        var response = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.Response, StringComparison.OrdinalIgnoreCase)
                                                    && x.SettingValue.Equals(DefaultValue.Neutral, StringComparison.OrdinalIgnoreCase));
        var station = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.Extension, StringComparison.OrdinalIgnoreCase)
                                                   && x.SettingId == BackgroundJobConstants.ExtensionNA);
        var category = GetCategorySubCategoryArchivingDA(settings, DefaultValue.Special_Notification);

        var criteria = new CreateMultipleFeedbackCommand
        {
            CreatorId = appSettings.CreatorId,
            UserId = appSettings.CreatorId,
            IsCheckedDefaultComm = true,
            FeedbackDate = feedbackDate.Subtract(timeSpan),
            ResolutionDate = feedbackDate.AddMinutes(1).Subtract(timeSpan),
            StationId = station != null ? station.SettingId.ToString() : string.Empty,
            StatusId = status?.SettingId ?? 0,
            SubCategoryIds = category.Item2.ToString(),
            ResponseId = response?.SettingId ?? 0,
            CategoryXml = category.Item3,
            CategoryIds = category.Item1.ToString(),
            UserRole = BackgroundJobConstants.UserRole,
            Subject = DefaultValue.Special_Notification,
            AutoField2 = string.Format(DefaultValue.DateTimeTool, feedbackDate.ToString("yyyyMMdd"), feedbackDate.ToString("hh:mmtt")),
            AutoField = string.Empty,
            AutoField1 = string.Empty,
            AutoField3 = $"{Environment.NewLine}System ID: ",
            CustomerXml = string.Format(DefaultValue.CustomerXml, archivingAutoFbInfo.HeadlineID, archivingAutoFbInfo.HeadlineName),
            ProductXml = GetProductXmlNA(),
            Solution = GetArchivingDASolution(archivingAutoFbInfo),
            RelatedCustomerXml = GetArchivingRelatedCustomerXml(archivingAutoFbInfo),
            ListOrigCustIds = archivingAutoFbInfo.AffectedId,
            Description = GetArchivingDADescription(archivingAutoFbInfo),
            LogFeedbackId = archivingAutoFbInfo.LogFeedbackId,
            ServiceID = archivingAutoFbInfo.ServiceID
        };
        await CheckValidArchivingParamsAndInsertFeedbackAsync(archivingAutoFbInfo, criteria);
    }

    protected internal virtual string GetArchivingDADescription(ArchivingAutoFbInfo archivingAutoFbInfo)
    {
        const int fromArchiveDate = 90;
        var stringBuilder = new StringBuilder(DefaultValue.autoGenerated);
        stringBuilder.Append(BackgroundJobConstants.Break);
        var template = commonFeedback.GetFeedbackTemplates<ArchivingTemplate>(templateXml);
        stringBuilder.AppendFormat(template.Description,
            archivingAutoFbInfo.ArchivedDate.ToString("MMM-yyyy"),
            archivingAutoFbInfo.ArchivedDate.AddDays(-fromArchiveDate).ToString("MM/dd/yyyy"),
            archivingAutoFbInfo.ArchivedDate.ToString("MM/dd/yyyy"),
            BackgroundJobConstants.LineBreak);

        return stringBuilder.ToString();
    }

    protected internal virtual string GetArchivingDASolution(ArchivingAutoFbInfo archivingAutoFbInfo)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(BackgroundJobConstants.Break);
        var isVipCust = archivingAutoFbInfo.SiteLabel == "789y" && archivingAutoFbInfo.IsVip;
        var template = commonFeedback.GetFeedbackTemplates<ArchivingTemplate>(templateXml);
        var templateYNSC = isVipCust ? template.SolutionENYN : template.SolutionENSC;
        stringBuilder.AppendFormat(templateYNSC, archivingAutoFbInfo.AffectedName, BackgroundJobConstants.LineBreak, BackgroundJobConstants.Break);
        return stringBuilder.ToString();
    }

    protected internal virtual string GetArchivingRelatedCustomerXml(ArchivingAutoFbInfo archivingAutoFbInfo)
    {
        var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);

        if (string.IsNullOrEmpty(archivingAutoFbInfo.AffectedId))
        {
            return stringBuilder.Append(BackgroundJobConstants.EndRootTag).ToString();
        }
        var relateCustIds = archivingAutoFbInfo.AffectedId.Split(",");
        var userNames = archivingAutoFbInfo.AffectedName.Split(",");
        const string XmlTemplate = DefaultValue.RelatedCustomerXml;
        for (var i = 0; i < relateCustIds.Length; i++)
        {
            stringBuilder.AppendFormat(XmlTemplate, archivingAutoFbInfo.HeadlineID, relateCustIds[i], userNames[i]);
        }
        return stringBuilder.Append(BackgroundJobConstants.EndRootTag).ToString();
    }

    protected internal virtual async Task CheckValidArchivingParamsAndInsertFeedbackAsync(
        ArchivingAutoFbInfo archivingAutoFbInfo,
        CreateMultipleFeedbackCommand criteria)
    {
        var checkValidParamBeforeInsert = commonFeedback.CheckFeedbackValidParams(criteria);
        if (!checkValidParamBeforeInsert.Item1)
        {
            var messageInvalid = "Invalid Params: " + checkValidParamBeforeInsert.Item2;
            logService.LogInformation(
                $"Invalid Params. @Categorystr: {criteria.CategoryIds} @Category: {criteria.CategoryXml} @Product: {criteria.ProductXml} " +
                $"@StatusID: {criteria.StatusId} @SubCategorystr:{criteria.SubCategoryIds} @ResponseID:{criteria.ResponseId} " +
                $"@StationID:{criteria.StationId}");
            await feedbackTransactionService.EditArchivingDAAutoFeedback(archivingAutoFbInfo.LogFeedbackId, CreateFeedbackJobStatus.Fail, messageInvalid);
            return;
        }
        try
        {
            logService.LogInformation(
                $"Begin insert archiving DA feedback : HeadlineId {archivingAutoFbInfo.HeadlineID} HeadlineName:{archivingAutoFbInfo.HeadlineName}" +
                $" CustName: {archivingAutoFbInfo.AffectedName} CustId: {archivingAutoFbInfo.AffectedId}");

            var commandResult = await baseService.ExecuteNonQueryAsync(criteria);
            if (commandResult.IsSuccessful)
            {
                logService.LogInformation(
                    $"Create  archiving DA feedback successfully HeadlineId {archivingAutoFbInfo.HeadlineID} HeadlineName:{archivingAutoFbInfo.HeadlineName}");
            }
            else
            {
                logService.LogInformation(
                    $"Create  archiving DA feedback Failed HeadlineId {archivingAutoFbInfo.HeadlineID} HeadlineName:{archivingAutoFbInfo.HeadlineName}");

                await feedbackTransactionService.EditArchivingDAAutoFeedback(archivingAutoFbInfo.LogFeedbackId, CreateFeedbackJobStatus.Fail, commandResult.Message);
                await logService.ErrorAsync(commandResult, criteria);
            }
        }
        catch (Exception ex)
        {
            await feedbackTransactionService.EditArchivingDAAutoFeedback(archivingAutoFbInfo.LogFeedbackId, CreateFeedbackJobStatus.Fail, ex.Message);
            await logService.ErrorAsync(ex);
        }
    }

    protected internal virtual string GetProductXmlNA()
    {
        var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);
        stringBuilder.AppendFormat(DefaultValue.ProductXml, BackgroundJobConstants.ProductNA, BackgroundJobConstants.SubProductNA, BackgroundJobConstants.BettypeNA);
        return stringBuilder.Append(BackgroundJobConstants.EndRootTag).ToString();
    }

    protected internal virtual Tuple<int, int, string> GetCategorySubCategoryArchivingDA(IEnumerable<SettingItem> settings, string settingValueName)
    {
        const string subCateName = "Archive Dormant Accounts";
        var category = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.MainCategory, StringComparison.OrdinalIgnoreCase)
                                                    && x.SettingValue.Equals(settingValueName, StringComparison.OrdinalIgnoreCase));
        var subCategoryId = 0;

        if (category != null)
        {
            var subCategory = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.SubCategory, StringComparison.OrdinalIgnoreCase)
                                                           && x.SettingValue.Equals(subCateName, StringComparison.OrdinalIgnoreCase));
            subCategoryId = subCategory?.SettingId ?? 0;
        }

        return new Tuple<int, int, string>(
            category?.SettingId ?? 0, subCategoryId,
            string.Format(DefaultValue.CategoryXml,
                category?.SettingId ?? 0,
                subCategoryId));
    }

    protected internal virtual async Task UpdateArchivingStatus(bool isSyncArchivedAccount, string lastRuntimeNameEnv, string jobStatusNameEnv)
    {
        if (isSyncArchivedAccount)
        {
            await commonFeedback.EditArchivingDAStatusSettingAsync(BackgroundJobConstants.ServiceName_ArchivingDA, "0");
        }
        else
        {
            await commonFeedback.EditSystemSettingAsyncByEnv(jobStatusNameEnv, lastRuntimeNameEnv, JobStatus.Completed, BackgroundJobConstants.ServiceName_ArchivingDA);
        }
    }
}