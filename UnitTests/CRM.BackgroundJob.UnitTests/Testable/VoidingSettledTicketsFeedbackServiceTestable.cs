namespace CRM.BackgroundJob.UnitTests.Testable
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CRM.Services;
    using CRM.BackgroundJob.Models.VoidingSettledTickets;
    using CRM.BackgroundJob.Services;
    using CRM.BackgroundJob.Criteria;
    using CRM.BackgroundJob.Models;
    using CRM.BackgroundJob.Services;
    using Microsoft.Extensions.Options;
    using CRM.BackgroundJob.Criteria.AutoFeedback;

    public class VoidingSettledTicketsFeedbackServiceTestable : VoidingSettledTicketsFeedbackService
    {
        public VoidingSettledTicketsFeedbackServiceTestable(
            IOptionsSnapshot<AppJobSettings> appSettings,
            IBaseService baseService,
            ISettingService settingService,
            ICrmLogService logService,
            ICommonFeedback commonFeedback,
            IVoidingSettledTicketsService voidingSettledTicketsService,
            IFeedbackTransactionService feedbackTransaction)
        : base(appSettings,
                baseService,
                settingService,
                logService,
                commonFeedback,
                voidingSettledTicketsService,
                feedbackTransaction
              )
        {
        }

        public async Task Public_CreateVoidingSettledTicketsAsync(
           VoidingAutoFeedbackInfo feedbackInfo,
           IEnumerable<AutoVoidingEventDetails> eventDetails,
           IEnumerable<Message> messages,
           IEnumerable<SettingItem> settings,
           IEnumerable<SystemSettingMapping> settingMappings,
           string environment
            )
        {
            await base.CreateVoidingSettledTicketsAsync(feedbackInfo, eventDetails, messages, settings, settingMappings, environment);
        }

        public async Task Public_CheckValidVoidingSettledParamsAndInsertFeedbackAsync(
            VoidingAutoFeedbackInfo feedbackInfo,
            CreateMutipleFeedbackCommand criteria,
            string environment)
        {
            await base.CheckValidVoidingSettledParamsAndInsertFeedbackAsync(feedbackInfo, criteria, environment);
        }

        public string Public_GetVoidingSettledTicketsProductXml(
            bool isMixParlay,
            IEnumerable<int> productIds,
            IEnumerable<short> sportTypeId,
            string bettypes,
            IEnumerable<SettingItem> settings,
            IEnumerable<SystemSettingMapping> settingMappings)
        {
            return base.GetVoidingSettledTicketsProductXml(isMixParlay, productIds, sportTypeId, bettypes, settings, settingMappings);
        }

        public string Public_GetProductSubXml(
           SettingItem product,
            IList<SettingItem> subProducts,
            IList<SystemSettingMapping> bettypesMapping)
        {
            return base.GetProductSubXml(product, subProducts, bettypesMapping);
        }

        public IList<SettingItem> Public_GetVoidingSettledTicketsProduct(
            IEnumerable<SettingItem> settings,
            IEnumerable<SystemSettingMapping> settingMappings,
            IEnumerable<int> productIds)
        {
            return base.GetVoidingSettledTicketsProduct(settings, settingMappings, productIds);
        }

        public IList<SettingItem> Public_GetVoidingSettledTicketsSubProduct(
            bool isMixParlay,
            IEnumerable<int> productIds,
            IEnumerable<short> sportTypeId,
            IEnumerable<SettingItem> settings,
            IEnumerable<SystemSettingMapping> settingMappings)
        {
            return base.GetVoidingSettledTicketsSubProduct(isMixParlay, productIds, sportTypeId, settings, settingMappings);
        }

        public IList<SystemSettingMapping> Public_GetVoidingSettledTicketsBettype(
            bool isMixParlay,
            string[] listBetTypes,
            IEnumerable<SystemSettingMapping> settingMappings)
        {
            return base.GetVoidingSettledTicketsBettype(isMixParlay, listBetTypes, settingMappings);
        }

        public Tuple<int, int, string> Public_GetCategorySubCategoryVoidingSettledTicket(IEnumerable<SettingItem> settings, string settingValueName, int issueTypeId)
        {
            return base.GetCategorySubCategoryVoidingSettledTicket(settings, settingValueName, issueTypeId);
        }

        public string Public_GetRelatedCustomerXml(VoidingAutoFeedbackInfo feedbackInfo)
        {
            return base.GetRelatedCustomerXml(feedbackInfo);
        }

        public string Public_GetVoidingSettledTicketSolution(VoidingAutoFeedbackInfo feedbackInfo, IEnumerable<Message> messages)
        {
            return base.GetVoidingSettledTicketSolution(feedbackInfo, messages);
        }

        public string Public_GetVoidingSettledTicketDescription(VoidingAutoFeedbackInfo feedbackInfo,
           IEnumerable<AutoVoidingEventDetails> eventDetails)
        {
            return base.GetVoidingSettledTicketDescription(feedbackInfo, eventDetails);
        }

        public string Public_GetVoidingSettledTicketDescriptionByProduct(AutoVoidingEventDetails eventDetail, VoidingSettledTicketDescriptionTemplate ticketDescTemplate, string site)
        {
            return base.GetVoidingSettledTicketDescriptionByProduct(eventDetail, ticketDescTemplate, site);
        }

        public string Public_GetDescriptionSportBook(AutoVoidingEventDetails eventDetail, string productName,
          string kO_time, string leagueName, string homeName, string sportName, string awayName, string macthCode)
        {
            return base.GetDescriptionSportBook(eventDetail, productName, kO_time, leagueName, homeName, sportName, awayName, macthCode);
        }
    }
}