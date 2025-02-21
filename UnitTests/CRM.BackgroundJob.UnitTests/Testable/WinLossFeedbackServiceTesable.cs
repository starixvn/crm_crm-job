namespace CRM.BackgroundJob.UnitTests.Testable
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CRM.BackgroundJob.Models.WinLossLimit;
    using CRM.BackgroundJob.Services;
    using CRM.Services;
    using CRM.BackgroundJob.Criteria;
    using CRM.BackgroundJob.Models;
    using CRM.BackgroundJob.Services;
    using Microsoft.Extensions.Options;
    using CRM.BackgroundJob.Criteria.AutoFeedback;

    public class WinLossFeedbackServiceTesable : WinLossFeedbackService
    {
        public WinLossFeedbackServiceTesable(IOptionsSnapshot<AppJobSettings> appSettings,
            IBaseService baseService,
            ISettingService settingService,
            ISystemSettingService systemSettingService,
            ICustomerService customerService,
            ICrmLogService logService,
            IFeedbackTransactionService transactionService,
            ICommonFeedback commonFeedback)
            : base(appSettings,
                baseService,
                settingService,
                systemSettingService,
                customerService,
                logService,
                transactionService,
                commonFeedback)
        {
        }

        public async Task Public_CreateWinLossFeedbackCreditAsync(
            bool isVipHeadline,
            WinLimitFeedBackInfo winLimit,
            IEnumerable<BettypeInfo> bettype,
            IEnumerable<SportInfo> sports,
            IEnumerable<SettingItem> settings,
            IEnumerable<SystemSettingMapping> settingMappings)
        {
            await base.CreateWinLossFeedbackCreditAsync(isVipHeadline, winLimit, bettype, sports, settings, settingMappings);
        }

        public async Task Public_CheckValidParamsAndInsertFeedbackAsync(
            WinLimitFeedBackInfo winLimit,
            CreateMutipleFeedbackCommand criteria)
        {
            await base.CheckValidParamsAndInsertFeedbackAsync(winLimit, criteria);
        }

        public string Public_GetProductXml(
           WinLimitFeedBackInfo winLimit,
           List<BettypeInfo> betTypeMappings,
           List<SportInfo> sports,
           IEnumerable<SettingItem> settings,
           IEnumerable<SystemSettingMapping> settingMappings)
        {
            return base.GetProductXml(winLimit, betTypeMappings, sports, settings, settingMappings);
        }

        public SettingItem Public_GetProduct(
           IEnumerable<SettingItem> settings,
           IEnumerable<SystemSettingMapping> settingMappings,
           string productName)
        {
            return base.GetProduct(settings, settingMappings, productName);
        }

        public List<SettingItem> Public_GetSubProduct(
           WinLimitFeedBackInfo winLimit,
           IEnumerable<SportInfo> sports,
           int productId,
           IEnumerable<SettingItem> settings,
           IEnumerable<SystemSettingMapping> settingMappings)
        {
            return base.GetSubProduct(winLimit, sports, productId, settings, settingMappings);
        }
       
        public IList<SettingItem> Public_GetBettype(
           IEnumerable<int> subProducts,
           List<BettypeInfo> betTypeMappings,
           IEnumerable<SystemSettingMapping> settingMappings,
           IEnumerable<SettingItem> settings)
        {
            return base.GetBettype(subProducts, betTypeMappings, settingMappings, settings);
        }

        public string Public_GetSolution(WinLimitFeedBackInfo winLimit, bool isVipHeadline)
        {
            return base.GetSolution(winLimit, isVipHeadline);
        }

        public string Public_GetDescription(WinLimitFeedBackInfo winlimit, List<BettypeInfo> bettype)
        {
            return base.GetDescription(winlimit, bettype);
        }

        public string Public_GetRelatedCustomerXml(WinLimitFeedBackInfo winLimit)
        {
            return base.GetRelatedCustomerXml(winLimit);
        }
    }
}