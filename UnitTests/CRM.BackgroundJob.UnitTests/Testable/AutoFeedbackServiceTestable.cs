namespace CRM.BackgroundJob.UnitTests.Testable
{
    using CRM.BackgroundJob.Models;
    using CRM.BackgroundJob.Models.AbnormalBet;
    using CRM.BackgroundJob.Models.AmendmentNotiFB;
    using CRM.BackgroundJob.Models.ProblemsAccount;
    using CRM.BackgroundJob.Services;
    using CRM.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using System.Threading.Tasks;

    public class AutoFeedbackServiceTestable(
        IOptionsSnapshot<AppJobSettings> appSettings,
        IJobRunningChecker jobRunningChecker,
        ISystemSettingService systemSettingService,
        IWinLossFeedbackService winLossService,
        ICrmLogService logService,
        ITOControlFeedbackService turnoverService,
        IAmendmentNotiFbService amendmentNotiFbService,
        IProblemsAccountFeedbackService problemsAccountFeedbackService,
        IFeedbackService feedbackService,
        ISynchronizeLicenseeService licenseeService,
        IVoidingSettledTicketsFeedbackService voidingSettledTicketsService,
        IArchivingDAService archivingDaService,
        ISendNotificationService sendNotificationService,
        IRefundSettledTicketService refundSettledTicketService,
        IVoidingBetIDService voidingBetIdService,
        IAbnormalBetService abnormalBetService,
        IAutoFbMessageNotificationCenterService autoFbMessageNotificationCenterService,
        IAutoFbRebateRetentionService autoFbRebateRetentionService,
        IConfiguration configuration)
        : AutoFeedbackService(appSettings,
            jobRunningChecker,
            systemSettingService,
            winLossService,
            logService,
            turnoverService,
            amendmentNotiFbService,
            problemsAccountFeedbackService,
            feedbackService,
            licenseeService,
            voidingSettledTicketsService,
            archivingDaService,
            sendNotificationService,
            refundSettledTicketService,
            voidingBetIdService,
            abnormalBetService,
            autoFbMessageNotificationCenterService,
            autoFbRebateRetentionService,
            configuration)
    {
        public async Task Public_ProblemsAccountRunJobByEnvironmentAsync(bool isUAT, bool isRunningUAT,
            bool isRunningLive, ProblemsAccountSetting problemsAccountSetting, string Environment)
        {
            await base.ProblemsAccountRunJobByEnvironmentAsync(isUAT, isRunningUAT,
                isRunningLive, problemsAccountSetting, Environment);
        }

        public async Task Public_ResultAmendmentRunJobByEnvironmentAsync(bool isUAT, bool isRunningUAT,
            bool isRunningLive, AmendmentNotiFbSetting resultAmendmentSetting, string Environment)
        {
            await base.ResultAmendmentRunJobByEnvironmentAsync(isUAT, isRunningUAT,
                isRunningLive, resultAmendmentSetting, Environment);
        }

        public async Task Public_VoidingSettledTicketsRunJobByEnvironmentAsync(bool isUAT, bool isRunningUAT,
            bool isRunningLive, AmendmentNotiFbSetting resultAmendmentSetting, string Environment)
        {
            await base.VoidingSettledTicketsRunJobByEnvironmentAsync(isUAT, isRunningUAT,
                isRunningLive, resultAmendmentSetting, Environment);
        }

        public async Task Public_AbnormalBetRunJobByEnvironmentAsync(bool isUAT, bool isRunningUAT,
            bool isRunningLive, AbnormalBetSystemSetting setting, string Environment)
        {
            await base.AbnormalBetRunJobByEnvironmentAsync(isUAT, isRunningUAT,
            isRunningLive, setting, Environment);
        }
    }
}