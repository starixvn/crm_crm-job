namespace CRM.BackgroundJob.UnitTests.Testable
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using CRM.Services;
    using CRM.BackgroundJob.Models.AmendmentNotiFB;
    using CRM.BackgroundJob.Services;
    using CRM.BackgroundJob.Criteria;
    using CRM.BackgroundJob.Models;
    using CRM.BackgroundJob.Services;
    using Microsoft.Extensions.Options;
    using CRM.BackgroundJob.Criteria.AutoFeedback;

    public class ResultAmendmentServiceTestable : AmendmentNotiFbService
    {
        public ResultAmendmentServiceTestable(IOptionsSnapshot<AppJobSettings> appSettings,
            IBaseService baseService,
            ISettingService settingService,
            ICustomerService customerService,
            ICrmLogService logService,
            ICommonFeedback commonFeedback,
            IFeedbackTransactionService transactionService)
        : base(appSettings,
                baseService,
                settingService,
                customerService,
                logService,
                commonFeedback,
                transactionService
              )
        {
        }

        public async Task Public_ResultAmendmentUpdateSystemSetting(
            bool isUAT,
            AmendmentNotiFbSetting resultAmendmentSetting,
            IEnumerable<CusReachAmendmentNotification> custAmendmentNotifications,
            string JobStatusNameEnv
            )
        {
            await base.ResultAmendmentUpdateSystemSetting(isUAT, resultAmendmentSetting, custAmendmentNotifications, JobStatusNameEnv);
        }

        public async Task Public_CreateResultAmendmentFeedbackCreditAsync(
           AmendmentAutoFeedbackInfor amendment,
            IEnumerable<SettingItem> settings,
            IEnumerable<SystemSettingMapping> settingMappings,
            bool isNoResultTemplate)
        {
            await base.CreateResultAmendmentFeedbackCreditAsync(amendment, settings, settingMappings, isNoResultTemplate);
        }

        public async Task Public_CheckValidAmendmentParamsAndInsertFeedbackAsync(
           AmendmentAutoFeedbackInfor amendment,
            CreateMutipleFeedbackCommand criteria)
        {
            await base.CheckValidAmendmentParamsAndInsertFeedbackAsync(amendment, criteria);
        }

        public string Public_GetAmendmentProductXml(
            int sportTypeId,
            int productId,
            List<string> betTypeMappings,
            IEnumerable<SettingItem> settings,
            IEnumerable<SystemSettingMapping> settingMappings)
        {
            return base.GetAmendmentProductXml(sportTypeId, productId, betTypeMappings, settings, settingMappings);
        }

        public SettingItem Public_GetAmenmentProduct(
            IEnumerable<SettingItem> settings,
            IEnumerable<SystemSettingMapping> settingMappings,
            int productId)
        {
            return base.GetAmenmentProduct(settings, settingMappings, productId);
        }

        public IList<SettingItem> Public_GetAmendmentSubProduct(
            int productId,
            int sportTypeId,
            IEnumerable<SettingItem> settings,
            IEnumerable<SystemSettingMapping> settingMappings)
        {
            return base.GetAmendmentSubProduct(productId, sportTypeId, settings, settingMappings);
        }

        public IList<SystemSettingMapping> Public_GetAmendmentBetType(
            List<string> betTypeMappings,
            IEnumerable<SystemSettingMapping> settingMappings)
        {
            return base.GetAmendmentBetType(betTypeMappings, settingMappings);
        }

        public Tuple<int, int, string> Public_GetCategorySubCategoryResultAmendment(
            IEnumerable<SettingItem> settings,
            string settingValueName)
        {
            return base.GetCategorySubCategoryResultAmendment(settings, settingValueName);
        }

        public string Public_GetAmendmentRelatedCustomerXml(AmendmentAutoFeedbackInfor amendment)
        {
            return base.GetAmendmentRelatedCustomerXml(amendment);
        }

        public string Public_GetAmendmentDescription(AmendmentAutoFeedbackInfor amendment, bool isNoResultTemplate)
        {
            return base.GetAmendmentDescription(amendment, isNoResultTemplate);
        }

        public StringBuilder Public_GetAmendmentDescriptionVirtualSports(
            AmendmentAutoFeedbackInfor amendment,
            AmendmentDescriptionTemplate amendmentDescTemplate,
            string sportName,
            string homeName,
            string awayName,
            string Race_time,
            string EventdateGMT4
            )
        {
            return base.GetAmendmentDescriptionVirtualSports(amendment,
                amendmentDescTemplate,
                sportName,
                homeName,
                awayName,
                Race_time,
                EventdateGMT4);
        }

        public StringBuilder Public_GetAmendmentDescriptionNumberGame(
            AmendmentAutoFeedbackInfor amendment,
            AmendmentDescriptionTemplate amendmentDescTemplate,
            string leagueName,
            string sportName,
            string EventdateGMT4
            )
        {
            return base.GetAmendmentDescriptionNumberGame(
             amendment,
             amendmentDescTemplate,
             leagueName,
             sportName,
             EventdateGMT4);
        }

        public StringBuilder Public_GetAmendmentDescriptionSportOutRight(
            AmendmentAutoFeedbackInfor amendment,
            AmendmentDescriptionTemplate amendmentDescTemplate,
            string kO_time,
            string leagueName,
            string homeName
            )
        {
            return base.GetAmendmentDescriptionSportOutRight(
             amendment,
             amendmentDescTemplate,
             kO_time,
             leagueName,
             homeName);
        }

        public string Public_GetAmenmendSolution(AmendmentAutoFeedbackInfor amendment,
            IEnumerable<Message> messageContent, bool isNoResultTemplate)
        {
            return base.GetAmenmendSolution(amendment, messageContent, isNoResultTemplate);
        }

        public StringBuilder Public_GetAmendmentSolutionTemplateEN(
           AmendmentAutoFeedbackInfor amendment,
           AmendmentSolutionTemplate amendmentSolTemplate,
           IEnumerable<Message> messageContent)
        {
            return base.GetAmendmentSolutionTemplateEN(
            amendment,
            amendmentSolTemplate,
            messageContent);
        }

        public StringBuilder Public_GetAmendmentSolutionTemplateYNSC(
            AmendmentAutoFeedbackInfor amendment,
            AmendmentSolutionTemplate amendmentSolTemplate,
            IEnumerable<Message> messageContent,
            bool isVipCust)
        {
            return base.GetAmendmentSolutionTemplateYNSC(
            amendment,
            amendmentSolTemplate,
            messageContent,
            isVipCust);
        }

        public StringBuilder Public_GetAmendmentSolutionTemplateYnSportBook(
            AmendmentAutoFeedbackInfor amendment,
            AmendmentSolutionTemplate amendmentSolTemplate,
            IEnumerable<Message> messageContent,
            string sportNameYN,
            string homeNameYN,
            string awayNameYN,
            string leagueNameYN)
        {
            return base.GetAmendmentSolutionTemplateYnSportBook(
            amendment,
            amendmentSolTemplate,
            messageContent,
            sportNameYN,
            homeNameYN,
            awayNameYN,
            leagueNameYN);
        }

        public StringBuilder Public_GetAmendmentSolutionTemplateYnScVirtualSport(
            string affectedName,
            AmendmentSolutionTemplate amendmentSolTemplate,
            bool isVipCust,
            string sportNameYN,
            string sportNameSC)
        {
            return base.GetAmendmentSolutionTemplateYnScVirtualSport(
            affectedName,
            amendmentSolTemplate,
            isVipCust,
            sportNameYN,
            sportNameSC);
        }

        public StringBuilder Public_GetAmendmentSolutionTemplateYnScNumberGame(
            string MatchCode,
            string affectedName,
            AmendmentSolutionTemplate amendmentSolTemplate,
            bool isVipCust,
            string leagueNameYN,
            string Eventdate,
            string leagueNameSC)
        {
            return base.GetAmendmentSolutionTemplateYnScNumberGame(
            MatchCode,
            affectedName,
            amendmentSolTemplate,
            isVipCust,
            leagueNameYN,
            Eventdate,
            leagueNameSC);
        }
    }
}