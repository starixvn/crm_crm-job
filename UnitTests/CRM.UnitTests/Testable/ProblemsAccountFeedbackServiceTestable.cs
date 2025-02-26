namespace CRM.UnitTests.Testable
{
    using CRM.Criteria.AutoFeedback;
    using CRM.Models.Common;
    using CRM.Models.ProblemsAccount;
    using CRM.Services.AutoFeedback;
    using CRM.Services.Common;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class ProblemsAccountFeedbackServiceTestable(
        IOptionsSnapshot<AppJobSettings> appSettings,
        IBaseService baseService,
        ISettingService settingService,
        ICustomerService customerService,
        ICrmLogService logService,
        IFeedbackTransactionService transactionService,
        ICommonFeedback commonFeedback)
        : ProblemsAccountFeedbackService(appSettings,
            baseService,
            settingService,
            customerService,
            logService,
            commonFeedback,
            transactionService)
    {
        public async Task Public_CreateFeedbackForProblemsAccountAsync(
            ProblemsAccountSetting problemsAccountSetting, bool isUAT,
            DateTime LastRuntimeEnv, string LastRuntimeNameEnv,
            string JobStatusNameEnv, string Environment)
        {
            await CreateFeedbackForProblemsAccountAsync(problemsAccountSetting, isUAT,
            LastRuntimeEnv, LastRuntimeNameEnv,
            JobStatusNameEnv, Environment);
        }

        public async Task Public_CreateProblemsAccountFeedbackCreditAsync(
           IEnumerable<ProblemsAccountAutoFeedbackInfo> problemsAccounts,
           IEnumerable<SettingItem> settings,
           IEnumerable<SystemSettingMapping> settingMappings)
        {
            await base.CreateProblemsAccountFeedbackCreditAsync(problemsAccounts, settings, settingMappings);
        }

        public async Task Public_CreateProblemsAccountFeedbackByBatchAsync(
           IEnumerable<ProblemsAccountAutoFeedbackInfo> categoryProblemAccount,
           IEnumerable<ProblemsAccountAutoFeedbackInfo> problemsAccounts,
           string cateKey,
           IEnumerable<SettingItem> settings,
           IEnumerable<SystemSettingMapping> settingMappings)
        {
            await base.CreateProblemsAccountFeedbackByBatchAsync(categoryProblemAccount, problemsAccounts, cateKey, settings, settingMappings);
        }

        public async Task Public_CheckValidParamsAndInsertFeedbackAsync(
            IEnumerable<ProblemsAccountAutoFeedbackInfo> problemsAccounts,
            CreateMultipleFeedbackCommand criteria)
        {
            await base.CheckValidParamsAndInsertFeedbackAsync(problemsAccounts, criteria);
        }

        public string Public_GetProductXml(IEnumerable<SettingItem> settings)
        {
            return base.GetProductXml(settings);
        }

        public string Public_GetRelatedCustomerXml(IEnumerable<ProblemsAccountAutoFeedbackInfo> problemsAccounts)
        {
            return base.GetRelatedCustomerXml(problemsAccounts);
        }

        public string Public_GetCustomerXml(IEnumerable<ProblemsAccountAutoFeedbackInfo> problemsAccounts)
        {
            return base.GetCustomerXml(problemsAccounts);
        }

        public SettingItem Public_GetProblemsSportsbookProduct(IEnumerable<SettingItem> settings)
        {
            return base.GetProblemsSportsbookProduct(settings);
        }

        public string Public_GetSolution(IEnumerable<ProblemsAccountAutoFeedbackInfo> problemsAccounts)
        {
            return base.GetSolution(problemsAccounts);
        }

        public string Public_GetDescription()
        {
            return base.GetDescription();
        }
    }
}