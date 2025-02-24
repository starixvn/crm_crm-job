namespace CRM.BackgroundJob.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Constants;
using Criteria.AutoFeedback;
using CRM.BackgroundJob.Models.ProblemsAccount;
using CRM.Services;
using Microsoft.Extensions.Options;
using Models;

public interface IProblemsAccountFeedbackService
{
    Task CreateFeedbackForProblemsAccountAsync(ProblemsAccountSetting problemsAccountSetting, bool isUAT,
        DateTime LastRuntimeEnv, string LastRuntimeNameEnv, string JobStatusNameEnv, string Environment
    );
}

public class ProblemsAccountFeedbackService(
    IOptions<AppJobSettings> appSettings,
    IBaseService baseService,
    ISettingService settingService,
    ICustomerService customerService,
    ICrmLogService logService,
    ICommonFeedback commonFeedback,
    IFeedbackTransactionService transactionService)
    : IProblemsAccountFeedbackService
{
    private readonly AppJobSettings appSettings = appSettings.Value;

    private const string templateXml = "ProblemsSolutionTemplate.xml";
    private const int maxUserNameInOneFb = 250;

    public async Task CreateFeedbackForProblemsAccountAsync(ProblemsAccountSetting problemsAccountSetting, bool isUAT,
        DateTime LastRuntimeEnv, string LastRuntimeNameEnv,
        string JobStatusNameEnv, string Environment)
    {
        await commonFeedback.EditSystemSettingAsyncByEnv(JobStatusNameEnv, LastRuntimeNameEnv, JobStatus.Running, BackgroundJobConstants.ServiceName_ProblemsAccount);
        var custCategory = await customerService.GetCustCategoryInfor(problemsAccountSetting, isUAT);
        var custIds = custCategory.Select(x => x.CustID).Distinct();
        var headlines = await customerService.GetGroupHeadlineAsync(custIds);
        var problemsAccountInfor = await transactionService.ImportProblemsAccountFeedbackInfoAsync(headlines, custCategory, Environment);
        if (!problemsAccountInfor.Any())
        {
            logService.LogInformation(
                $"Import problems account doesn't have any data with custId: {string.Join(",", headlines.Select(x => x.HeadlineId))} headlineName: {string.Join(",", headlines.Select(x => x.HeadlineName))}!");
            await commonFeedback.EditSystemSettingAsyncByEnv(JobStatusNameEnv, LastRuntimeNameEnv, JobStatus.Completed, BackgroundJobConstants.ServiceName_ProblemsAccount);
            return;
        }
        var settingNames = new[] { SettingName.Product, SettingName.SubProduct, SettingName.Bettype };
        var settings = await settingService.GetAllSettingAsync();
        var settingMappings = await settingService.GetSystemSettingMappingAsync(settingNames);
        var groupProblemsAccountByHeadlineId = problemsAccountInfor.GroupBy(x => x.HeadlineID);
        var cateKey = "";
        foreach (IEnumerable<ProblemsAccountAutoFeedbackInfor> problemsAccounts in groupProblemsAccountByHeadlineId)
        {
            var groupCategoryProblemsAccount = problemsAccounts.GroupBy(x => x.CategoryID);
            foreach (var categoryProblemAccount in groupCategoryProblemsAccount)
            {
                var distinctUserName = categoryProblemAccount.GroupBy(x => x.UserName).Select(group => group.First()).ToList();
                if (distinctUserName.Count > maxUserNameInOneFb)
                {
                    cateKey = categoryProblemAccount.Key.ToString();
                    await CreateProblemsAccountFeedbackByBatchAsync(categoryProblemAccount, problemsAccounts, cateKey, settings, settingMappings);
                }
            }
            if (string.IsNullOrEmpty(cateKey))
            {
                await CreateProblemsAccountFeedbackCreditAsync(
                    problemsAccounts,
                    settings,
                    settingMappings
                );
            }
        }
        var lastLogIdName = isUAT ? "LastLogIdUAT" : "LastLogID";
        if (custCategory.Any())
        {
            var maxLastLogId = custCategory.Max(x => x.LogID);
            await commonFeedback.EditSystemSettingUpdateLastLogIdAsyncByEnv(JobStatusNameEnv, LastRuntimeNameEnv, lastLogIdName, maxLastLogId, BackgroundJobConstants.ServiceName_ProblemsAccount);
            return;
        }
        await commonFeedback.EditSystemSettingAsyncByEnv(JobStatusNameEnv, LastRuntimeNameEnv, JobStatus.Completed, BackgroundJobConstants.ServiceName_ProblemsAccount);
    }

    protected internal virtual async Task CreateProblemsAccountFeedbackByBatchAsync(
        IEnumerable<ProblemsAccountAutoFeedbackInfor> categoryProblemAccount,
        IEnumerable<ProblemsAccountAutoFeedbackInfor> problemsAccounts,
        string cateKey,
        IEnumerable<SettingItem> settings,
        IEnumerable<SystemSettingMapping> settingMappings
    )
    {
        var countProblemsAccount = categoryProblemAccount.ToList().Count;
        var batchCreateFbPerUSer = (int)Math.Ceiling((double)countProblemsAccount / maxUserNameInOneFb);
        var startRange = 0;
        for (var i = 0; i < batchCreateFbPerUSer; i++)
        {
            if (i == batchCreateFbPerUSer - 1)
            {
                var categoryProblemAccountRemain = categoryProblemAccount.Skip(maxUserNameInOneFb * (batchCreateFbPerUSer - 1));
                var problemsAccountsRemain = problemsAccounts.Where(x => x.CategoryID.ToString() != cateKey).ToList();
                var totalProblemAccountRemains = problemsAccountsRemain.Count >= maxUserNameInOneFb ? categoryProblemAccountRemain : problemsAccountsRemain.Concat(categoryProblemAccountRemain);
                await CreateProblemsAccountFeedbackCreditAsync(
                    totalProblemAccountRemains,
                    settings,
                    settingMappings
                );
            }
            else
            {
                await CreateProblemsAccountFeedbackCreditAsync(
                    categoryProblemAccount.ToList().GetRange(startRange, maxUserNameInOneFb),
                    settings,
                    settingMappings
                );
                startRange += maxUserNameInOneFb;
            }
        }
    }

    protected internal virtual async Task CreateProblemsAccountFeedbackCreditAsync(
        IEnumerable<ProblemsAccountAutoFeedbackInfor> problemsAccounts,
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
        var category = commonFeedback.GetCategorySubCategoryCWL(settings, DefaultValue.ProblemAccount);

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
            PALogID = string.Join(",", problemsAccounts.Select(x => x.LogID)),
            CustomerXml = GetCustomerXml(problemsAccounts),
            ProductXml = GetProductXml(settings),
            Solution = GetSolution(problemsAccounts),
            RelatedCustomerXml = GetRelatedCustomerXml(problemsAccounts),
            ListOrigCustIds = string.Join(",", problemsAccounts.Select(x => x.CustID)),
            Description = GetDescription()
        };
        await CheckValidParamsAndInsertFeedbackAsync(problemsAccounts, criteria);
    }

    protected internal virtual async Task CheckValidParamsAndInsertFeedbackAsync(
        IEnumerable<ProblemsAccountAutoFeedbackInfor> problemsAccounts,
        CreateMutipleFeedbackCommand criteria)
    {
        var checkValidParamBeforeInsert = commonFeedback.CheckFeedbackValidParams(criteria);
        if (!checkValidParamBeforeInsert.Item1)
        {
            var messageInvalid = "Invalid Params: " + checkValidParamBeforeInsert.Item2;
            await transactionService.EditProblemsAccountFeedbackInfoAsync(string.Join(",", problemsAccounts.Select(x => x.LogID)), CreateFeedbackJobStatus.Fail, messageInvalid);
            return;
        }
        try
        {
            var commandResult = await baseService.ExecuteNonQueryAsync(criteria);
            if (commandResult.IsSuccessful)
            {
                logService.LogInformation(
                    $"Insert feedback problems account successfully. HeadlineId: {string.Join(",", problemsAccounts.Select(x => x.HeadlineID))} logID:{string.Join(",", problemsAccounts.Select(x => x.LogID))}");
            }
            else
            {
                logService.LogInformation(
                    $"Insert feedback failed problems account. HeadlineId: {string.Join(",", problemsAccounts.Select(x => x.HeadlineID))} logID:{string.Join(",", problemsAccounts.Select(x => x.LogID))}");

                await transactionService.EditProblemsAccountFeedbackInfoAsync(string.Join(",", problemsAccounts.Select(x => x.LogID)), CreateFeedbackJobStatus.Fail, commandResult.Message);
                await logService.ErrorAsync(commandResult, criteria);
            }
        }
        catch (Exception ex)
        {
            await transactionService.EditProblemsAccountFeedbackInfoAsync(string.Join(",", problemsAccounts.Select(x => x.LogID)), CreateFeedbackJobStatus.Fail, ex.Message);
            await logService.ErrorAsync(ex);
        }
    }

    protected internal virtual string GetProductXml(
        IEnumerable<SettingItem> settings
    )
    {
        var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);
        var product = GetProblemsSportsbookProduct(settings);
        stringBuilder.AppendFormat(DefaultValue.ProductXml, product.SettingId, BackgroundJobConstants.SubProductNA, BackgroundJobConstants.BettypeNA);
        return stringBuilder.Append(BackgroundJobConstants.EndRootTag).ToString();
    }

    protected internal virtual string GetRelatedCustomerXml(IEnumerable<ProblemsAccountAutoFeedbackInfor> problemsAccounts)
    {
        const string XmlTemplate = DefaultValue.RelatedCustomerXml;
        var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);
        var distinctRelatedAndUserName = problemsAccounts.GroupBy(x => new { x.HeadlineID, x.RelatedCustID, x.UserName }).Select(group => group.First()).ToList();
        foreach (var problemsAccount in distinctRelatedAndUserName)
        {
            stringBuilder.AppendFormat(XmlTemplate, problemsAccount.HeadlineID, problemsAccount.RelatedCustID, problemsAccount.UserName);
        }
        return stringBuilder.Append(BackgroundJobConstants.EndRootTag).ToString();
    }

    protected internal virtual string GetCustomerXml(IEnumerable<ProblemsAccountAutoFeedbackInfor> problemsAccounts)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendFormat(DefaultValue.CustomerXml, problemsAccounts.FirstOrDefault()?.HeadlineID, problemsAccounts.FirstOrDefault()?.HeadlineName);
        return stringBuilder.ToString();
    }

    protected internal virtual SettingItem GetProblemsSportsbookProduct(IEnumerable<SettingItem> settings)
    {
        var product = settings.FirstOrDefault(x => x.SettingName.Equals(SettingName.Product, StringComparison.OrdinalIgnoreCase)
                                                   && x.SettingValue.Equals(DefaultValue.Sportsbook, StringComparison.OrdinalIgnoreCase));
        return product ?? new SettingItem { SettingId = BackgroundJobConstants.ProductNA };
    }

    protected internal virtual string GetSolution(IEnumerable<ProblemsAccountAutoFeedbackInfor> problemsAccounts)
    {
        var stringBuilder = new StringBuilder();
        var stringBuilderContent = new StringBuilder();
        const string templateContent = "- {0}/{1}: {2}.";
        var languageContent = problemsAccounts.First().IsVip ? Language.vi_VN : Language.zh_CN;
        var template = commonFeedback.GetFeedbackTemplates<SolutionTemplate>(templateXml);
        var templateYNSC = problemsAccounts.First().IsVip ? template.SolutionENYN : template.SolutionENSC;
        var groupCategoryProblemsAccount = problemsAccounts.GroupBy(x => x.CategoryID);
        foreach (var categoryProblemAccount in groupCategoryProblemsAccount)
        {
            var categoryNameEN = commonFeedback.GetCategogyName(categoryProblemAccount.Key.ToString(), Language.en_US);
            var categoryNameYNSC = commonFeedback.GetCategogyName(categoryProblemAccount.Key.ToString(), languageContent);
            var distinctUserName = categoryProblemAccount.GroupBy(x => x.UserName)
                .Select(group => group.First()).ToList();
            var listUserNames = string.Join(", ", distinctUserName.Select(x => x.UserName));
            stringBuilderContent.AppendFormat(templateContent, categoryNameEN, categoryNameYNSC, listUserNames);
            stringBuilderContent.Append(BackgroundJobConstants.Break);
        }
        stringBuilder.AppendFormat(templateYNSC, stringBuilderContent, BackgroundJobConstants.LineBreak);
        return stringBuilder.ToString();
    }

    protected internal virtual string GetDescription()
    {
        var stringBuilder = new StringBuilder(DefaultValue.autoGenerated);
        stringBuilder.Append(BackgroundJobConstants.Break);
        stringBuilder.Append("Announcement sent to Headline and informed our related department marked their member as problem account.");
        stringBuilder.Append(BackgroundJobConstants.LineBreak);
        return stringBuilder.ToString();
    }
}