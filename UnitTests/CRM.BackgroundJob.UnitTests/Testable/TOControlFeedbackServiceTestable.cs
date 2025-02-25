namespace CRM.BackgroundJob.UnitTests.Testable
{
    using CRM.BackgroundJob.Criteria.AutoFeedback;
    using CRM.BackgroundJob.Models;
    using CRM.BackgroundJob.Models.TurnoverControl;
    using CRM.BackgroundJob.Services;
    using CRM.Services;
    using Microsoft.Extensions.Options;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public class TOControlFeedbackServiceTestable(
        IOptionsSnapshot<AppJobSettings> appSettings,
        IBaseService baseService,
        ISettingService settingService,
        ISystemSettingService systemSettingService,
        ICustomerService customerService,
        ICrmLogService logService,
        IFeedbackTransactionService transactionService,
        ICommonFeedback commonFeedback)
        : TOControlFeedbackService(appSettings,
            baseService,
            settingService,
            systemSettingService,
            customerService,
            logService,
            commonFeedback,
            transactionService)
    {
        public async Task Public_CreateTOControlFeedbackAsync(TOControlFeedbackInfo ToInfo, IEnumerable<SettingItem> settings, IEnumerable<SystemSettingMapping> settingMappings) => await base.CreateTOControlFeedbackAsync(ToInfo, settings, settingMappings);

        public async Task Public_CheckValidTOParamsAndInsertFeedbackAsync(TOControlFeedbackInfo ToInfo, CreateMultipleFeedbackCommand criteria)
            => await base.CheckValidTOParamsAndInsertFeedbackAsync(ToInfo, criteria);

        public string Public_GetTOProductXml(string sportName, int productId, List<string> betTypeMappings, IEnumerable<SettingItem> settings, IEnumerable<SystemSettingMapping> settingMappings)
            => base.GetTOProductXml(sportName, productId, betTypeMappings, settings, settingMappings);

        public SettingItem Public_GetTOSportsbookProduct(IEnumerable<SettingItem> settings) => base.GetTOSportsbookProduct(settings);

        public IList<SettingItem> Public_GetTOSubProduct(int productId, string sportName, IEnumerable<SettingItem> settings, IEnumerable<SystemSettingMapping> settingMappings) => base.GetTOSubProduct(productId, sportName, settings, settingMappings);

        public IList<SystemSettingMapping> Public_GetTOBetType(List<string> betTypeMappings, IEnumerable<SystemSettingMapping> settingMappings)
            => base.GetTOBetType(betTypeMappings, settingMappings);

        public string Public_GetTORelatedCustomerXml(TOControlFeedbackInfo toInfo) => base.GetTORelatedCustomerXml(toInfo);

        public string Public_GetTODescription(TOControlFeedbackInfo toInfo) => base.GetTODescription(toInfo);

        public string Public_GetTOSolution(TOControlFeedbackInfo toInfo) => base.GetTOSolution(toInfo);

        public StringBuilder Public_GetTOSolutionTemplateEN(TOControlFeedbackInfo toInfo, TOSolutionTemplate TOSolTemplate, string smaLevelEN)
            => base.GetTOSolutionTemplateEN(toInfo, TOSolTemplate, smaLevelEN);

        public StringBuilder Public_GetTOSolutionTemplateYNSC(TOControlFeedbackInfo toInfo, bool isVipCust, string smaLevelEN, string smaLevelSC, TOSolutionTemplate TOSolTemplate) => base.GetTOSolutionTemplateYNSC(toInfo, isVipCust, smaLevelEN, smaLevelSC, TOSolTemplate);
    }
}