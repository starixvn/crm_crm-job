namespace CRM.BackgroundJob.UnitTests.Testable
{
    using CRM.BackgroundJob.Criteria.AutoFeedback;
    using CRM.BackgroundJob.Models;
    using CRM.BackgroundJob.Models.ArchivingDA;
    using CRM.BackgroundJob.Services;
    using CRM.Services;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class ArchivingDAServiceTestable(
        IOptionsSnapshot<AppJobSettings> appSettings,
        IBaseService baseService,
        ISettingService settingService,
        ICrmLogService logService,
        IFeedbackTransactionService transactionService,
        ICommonFeedback commonFeedback)
        : ArchivingDAService(appSettings,
            baseService,
            settingService,
            logService,
            commonFeedback,
            transactionService)
    {
        public async Task Public_CreateFeedbackForArchivingDAAsync(
            ArchivingDASetting archivingDASetting, bool isUAT,
            DateTime LastRuntimeEnv, string LastRuntimeNameEnv,
            string JobStatusNameEnv, string Environment)
        {
            await CreateFeedbackForArchivingDAAsync(archivingDASetting, isUAT,
            LastRuntimeEnv, LastRuntimeNameEnv,
            JobStatusNameEnv, Environment);
        }

        public async Task Public_CheckExistAnyArchivingFb(
           IEnumerable<ArchivingAutoFbInfo> archivingAutoFbInfos,
           IEnumerable<SettingItem> settings,
           IEnumerable<SystemSettingMapping> settingMappings,
           string jobStatusNameEnv, string lastRuntimeNameEnv)
        {
            await base.CheckExistAnyArchivingFb(archivingAutoFbInfos, settings,
            settingMappings, jobStatusNameEnv, lastRuntimeNameEnv);
        }

        public async Task Public_CreateFeedbackForArchivingDACreditAsync(
           ArchivingAutoFbInfo archivingAutoFbInfo,
           IEnumerable<SettingItem> settings,
           IEnumerable<SystemSettingMapping> settingMappings)
        {
            await base.CreateFeedbackForArchivingDACreditAsync(archivingAutoFbInfo, settings, settingMappings);
        }

        public string Public_GetArchivingDADescription(ArchivingAutoFbInfo archivingAutoFbInfo)
        {
            return base.GetArchivingDADescription(archivingAutoFbInfo);
        }

        public string Public_GetArchivingDASolution(ArchivingAutoFbInfo archivingAutoFbInfo)
        {
            return base.GetArchivingDASolution(archivingAutoFbInfo);
        }

        public string Public_GetArchivingRelatedCustomerXml(ArchivingAutoFbInfo archivingAutoFbInfo)
        {
            return base.GetArchivingRelatedCustomerXml(archivingAutoFbInfo);
        }

        public async Task Public_CheckValidArchivingParamsAndInsertFeedbackAsync(
            ArchivingAutoFbInfo archivingAutoFbInfo, CreateMultipleFeedbackCommand criteria
        )
        {
            await base.CheckValidArchivingParamsAndInsertFeedbackAsync(archivingAutoFbInfo, criteria);
        }

        public string Public_GetProductXmlNA()
        {
            return base.GetProductXmlNA();
        }

        public Tuple<int, int, string> Public_GetCategorySubCategoryArchivingDA(IEnumerable<SettingItem> settings, string settingValueName)
        {
            return base.GetCategorySubCategoryArchivingDA(settings, settingValueName);
        }

        public async Task Public_UpdateArchivingStatus(bool isSyncArchivedAccount, string lastRuntimeNameEnv, string jobStatusNameEnv)
        {
            await base.UpdateArchivingStatus(isSyncArchivedAccount, lastRuntimeNameEnv, jobStatusNameEnv);
        }
    }
}