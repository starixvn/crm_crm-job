namespace CRM.BackgroundJob.UnitTests.Testable
{
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using CRM.BackgroundJob.Core.Services;
    using CRM.BackgroundJob.Models.TurnoverControll;
    using CRM.BackgroundJob.Services;
    using Fanex.CRM.BackgroundJob.Criteria;
    using Fanex.CRM.BackgroundJob.Models;
    using Fanex.CRM.BackgroundJob.Services;
    using Microsoft.Extensions.Options;

    public class TOControlFeedbackServiceTestable : TOControlFeedbackService
    {
        public TOControlFeedbackServiceTestable(IOptionsSnapshot<AppJobSettings> appSettings,
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
                commonFeedback,
                transactionService
              )
        {
        }

        public async Task Public_CreateTOControlFeedbackAsync(
            TOControlFeedbackInfor ToInfor,
            IEnumerable<SettingItem> settings,
            IEnumerable<SystemSettingMapping> settingMappings)
        {
            await base.CreateTOControlFeedbackAsync(ToInfor, settings, settingMappings);
        }

        public async Task Public_CheckValidTOParamsAndInsertFeedbackAsync(
            TOControlFeedbackInfor ToInfor,
            CreateMutipleFeedbackCommand criteria)
        {
            await base.CheckValidTOParamsAndInsertFeedbackAsync(ToInfor, criteria);
        }

        public string Public_GetTOProductXml(
            string sportName,
            int productId,
            List<string> betTypeMappings,
            IEnumerable<SettingItem> settings,
            IEnumerable<SystemSettingMapping> settingMappings)
        {
            return base.GetTOProductXml(sportName, productId, betTypeMappings, settings, settingMappings);
        }

        public SettingItem Public_GetTOSportsbookProduct(IEnumerable<SettingItem> settings)
        {
            return base.GetTOSportsbookProduct(settings);
        }

        public IList<SettingItem> Public_GetTOSubProduct(
            int productId,
            string sportName,
            IEnumerable<SettingItem> settings,
            IEnumerable<SystemSettingMapping> settingMappings)
        {
            return base.GetTOSubProduct(productId, sportName, settings, settingMappings);
        }

        public IList<SystemSettingMapping> Public_GetTOBettype(
           List<string> betTypeMappings,
           IEnumerable<SystemSettingMapping> settingMappings)
        {
            return base.GetTOBettype(betTypeMappings, settingMappings);
        }

        public string Public_GetTORelatedCustomerXml(TOControlFeedbackInfor ToInfor)
        {
            return base.GetTORelatedCustomerXml(ToInfor);
        }

        public string Public_GetTODescription(TOControlFeedbackInfor ToInfor)
        {
            return base.GetTODescription(ToInfor);
        }

        public string Public_GetTOSolution(TOControlFeedbackInfor ToInfor)
        {
            return base.GetTOSolution(ToInfor);
        }

        public StringBuilder Public_GetTOSolutionTemplateEN(
           TOControlFeedbackInfor ToInfor,
           TOSolutionTemplate TOSolTemplate,
           string smaLevelEN)
        {
            return base.GetTOSolutionTemplateEN(ToInfor, TOSolTemplate, smaLevelEN);
        }

        public StringBuilder Public_GetTOSolutionTemplateYNSC(
            TOControlFeedbackInfor ToInfor,
            bool isVipCust,
            string smaLevelEN,
            string smaLevelSC,
            TOSolutionTemplate TOSolTemplate)
        {
            return base.GetTOSolutionTemplateYNSC(ToInfor, isVipCust, smaLevelEN, smaLevelSC, TOSolTemplate);
        }
    }
}