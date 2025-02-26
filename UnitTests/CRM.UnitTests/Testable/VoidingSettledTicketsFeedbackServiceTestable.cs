namespace CRM.UnitTests.Testable
{
    using CRM.Criteria.AutoFeedback;
    using CRM.Models.Common;
    using CRM.Models.VoidingSettledTickets;
    using CRM.Services.AutoFeedback;
    using CRM.Services.Common;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class VoidingSettledTicketsFeedbackServiceTestable(
        IOptionsSnapshot<AppJobSettings> appSettings,
        IBaseService baseService,
        ISettingService settingService,
        ICrmLogService logService,
        ICommonFeedback commonFeedback,
        IVoidingSettledTicketsService voidingSettledTicketsService,
        IFeedbackTransactionService feedbackTransaction)
        : VoidingSettledTicketsFeedbackService(appSettings,
            baseService,
            settingService,
            logService,
            commonFeedback,
            voidingSettledTicketsService,
            feedbackTransaction)
    {
        public async Task Public_CreateVoidingSettledTicketsAsync(
           VoidingAutoFeedbackInfo feedbackInfo,
           IEnumerable<AutoVoidingEventDetails> eventDetails,
           IEnumerable<Message> messages,
           IEnumerable<SettingItem> settings,
           IEnumerable<SystemSettingMapping> settingMappings,
           string environment)
            => await base.CreateVoidingSettledTicketsAsync(feedbackInfo, eventDetails, messages, settings, settingMappings, environment);

        public async Task Public_CheckValidVoidingSettledParamsAndInsertFeedbackAsync(
            VoidingAutoFeedbackInfo feedbackInfo,
            CreateMultipleFeedbackCommand criteria,
            string environment)
            => await base.CheckValidVoidingSettledParamsAndInsertFeedbackAsync(feedbackInfo, criteria, environment);

        public string Public_GetVoidingSettledTicketsProductXml(
            bool isMixParlay,
            IEnumerable<int> productIds,
            IEnumerable<short> sportTypeId,
            string betTypes,
            IEnumerable<SettingItem> settings,
            IEnumerable<SystemSettingMapping> settingMappings)
            => base.GetVoidingSettledTicketsProductXml(isMixParlay, productIds, sportTypeId, betTypes, settings, settingMappings);

        public string Public_GetProductSubXml(
           SettingItem product,
            IList<SettingItem> subProducts,
            IList<SystemSettingMapping> betTypesMapping)
            => base.GetProductSubXml(product, subProducts, betTypesMapping);

        public IList<SettingItem> Public_GetVoidingSettledTicketsProduct(
            IEnumerable<SettingItem> settings,
            IEnumerable<SystemSettingMapping> settingMappings,
            IEnumerable<int> productIds)
            => base.GetVoidingSettledTicketsProduct(settings, settingMappings, productIds);

        public IList<SettingItem> Public_GetVoidingSettledTicketsSubProduct(
            bool isMixParlay,
            IEnumerable<int> productIds,
            IEnumerable<short> sportTypeId,
            IEnumerable<SettingItem> settings,
            IEnumerable<SystemSettingMapping> settingMappings) =>
            base.GetVoidingSettledTicketsSubProduct(isMixParlay, productIds, sportTypeId, settings, settingMappings);

        public IList<SystemSettingMapping> Public_GetVoidingSettledTicketsBetType(
            bool isMixParlay,
            string[] listBetTypes,
            IEnumerable<SystemSettingMapping> settingMappings)
            => base.GetVoidingSettledTicketsBetType(isMixParlay, listBetTypes, settingMappings);

        public Tuple<int, int, string> Public_GetCategorySubCategoryVoidingSettledTicket(IEnumerable<SettingItem> settings, string settingValueName, int issueTypeId) => base.GetCategorySubCategoryVoidingSettledTicket(settings, settingValueName, issueTypeId);

        public string Public_GetRelatedCustomerXml(VoidingAutoFeedbackInfo feedbackInfo) => base.GetRelatedCustomerXml(feedbackInfo);

        public string Public_GetVoidingSettledTicketSolution(VoidingAutoFeedbackInfo feedbackInfo, IEnumerable<Message> messages) => base.GetVoidingSettledTicketSolution(feedbackInfo, messages);

        public string Public_GetVoidingSettledTicketDescription(VoidingAutoFeedbackInfo feedbackInfo,
           IEnumerable<AutoVoidingEventDetails> eventDetails)
            => base.GetVoidingSettledTicketDescription(feedbackInfo, eventDetails);

        public string Public_GetVoidingSettledTicketDescriptionByProduct(AutoVoidingEventDetails eventDetail, VoidingSettledTicketDescriptionTemplate ticketDescTemplate, string site) => base.GetVoidingSettledTicketDescriptionByProduct(eventDetail, ticketDescTemplate, site);

        public string Public_GetDescriptionSportBook(AutoVoidingEventDetails eventDetail, string productName,
          string kO_time, string leagueName, string homeName, string sportName, string awayName, string matchCode) =>
            base.GetDescriptionSportBook(eventDetail, productName, kO_time, leagueName, homeName, sportName, awayName, matchCode);
    }
}