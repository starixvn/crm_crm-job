namespace CRM.UnitTests.Testable
{
    using CRM.Criteria.AutoFeedback;
    using CRM.Models.Common;
    using CRM.Models.WinLossLimit;
    using CRM.Services.AutoFeedback;
    using CRM.Services.Common;
    using Microsoft.Extensions.Options;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class WinLossFeedbackServiceTestable(
        IOptionsSnapshot<AppJobSettings> appSettings,
        IBaseService baseService,
        ISettingService settingService,
        ISystemSettingService systemSettingService,
        ICustomerService customerService,
        ICrmLogService logService,
        IFeedbackTransactionService transactionService,
        ICommonFeedback commonFeedback)
        : WinLossFeedbackService(appSettings,
            baseService,
            settingService,
            systemSettingService,
            customerService,
            logService,
            transactionService,
            commonFeedback)
    {
        public async Task Public_CreateWinLossFeedbackCreditAsync(
            bool isVipHeadline,
            WinLimitFeedBackInfo winLimit,
            IEnumerable<BettypeInfo> betType,
            IEnumerable<SportInfo> sports,
            IEnumerable<SettingItem> settings,
            IEnumerable<SystemSettingMapping> settingMappings)
        {
            await base.CreateWinLossFeedbackCreditAsync(isVipHeadline, winLimit, betType, sports, settings, settingMappings);
        }

        public async Task Public_CheckValidParamsAndInsertFeedbackAsync(
            WinLimitFeedBackInfo winLimit,
            CreateMultipleFeedbackCommand criteria)
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

        public IList<SettingItem> Public_GetBetType(
           IEnumerable<int> subProducts,
           List<BettypeInfo> betTypeMappings,
           IEnumerable<SystemSettingMapping> settingMappings,
           IEnumerable<SettingItem> settings)
        {
            return base.GetBetType(subProducts, betTypeMappings, settingMappings, settings);
        }

        public string Public_GetSolution(WinLimitFeedBackInfo winLimit, bool isVipHeadline)
        {
            return base.GetSolution(winLimit, isVipHeadline);
        }

        public string Public_GetDescription(WinLimitFeedBackInfo winLimit, List<BettypeInfo> betType)
        {
            return base.GetDescription(winLimit, betType);
        }

        public string Public_GetRelatedCustomerXml(WinLimitFeedBackInfo winLimit)
        {
            return base.GetRelatedCustomerXml(winLimit);
        }
    }
}