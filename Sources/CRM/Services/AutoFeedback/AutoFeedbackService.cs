namespace CRM.Services.AutoFeedback;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Constants;
using CRM.Models.AbnormalBet;
using CRM.Models.AmendmentNotiFB;
using CRM.Models.ArchivingDA;
using CRM.Models.Common;
using CRM.Models.ProblemsAccount;
using CRM.Services.Common;
using CRM.Services.SendNotification;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

public interface IAutoFeedbackService
{
    Task AbnormalBetExecuteJobAsync();

    Task WinLossExecuteJobAsync();

    Task TurnoverControlExecuteJobAsync();

    Task ProblemsAccountExecuteJobAsync();

    Task AmendmentNotiFbExecuteJobAsync();

    Task SynchronizeNewLicenseeOICExecuteJobAsync();

    Task VoidingSettledTicketsExecuteJobAsync();

    Task ArchivingDAExecuteJobAsync();

    Task RefundSettledTicketJobAsync();

    Task VoidingBetIDExecuteJobAsync();

    Task AbnormalBetsExecuteJobAsync();

    Task MessageNotificationCenterExecuteJobAsync();

    Task RebateRetentionExecuteJobAsync();
}

public class AutoFeedbackService : IAutoFeedbackService
{
    private readonly AppJobSettings appSettings;
    private readonly IJobRunningChecker jobRunningChecker;
    private readonly ISystemSettingService systemSettingService;
    private readonly IWinLossFeedbackService winLossService;
    private readonly ITOControlFeedbackService turnoverService;
    private readonly IAmendmentNotiFbService amendmentNotiFbService;
    private readonly ICrmLogService logService;
    private readonly IProblemsAccountFeedbackService problemsAccountFeedbackService;
    private readonly IFeedbackService feedbackService;
    private readonly ISynchronizeLicenseeService licenseeService;
    private readonly IVoidingSettledTicketsFeedbackService voidingSettledTicketsService;
    private readonly IArchivingDAService archivingDAService;
    private readonly ISendNotificationService sendNotificationService;
    private readonly IRefundSettledTicketService refundSettledTicketService;
    private readonly IVoidingBetIDService voidingBetIDService;
    private readonly IAbnormalBetService abnormalBetService;
    private readonly IAutoFbMessageNotificationCenterService autoFbMessageNotificationService;
    private readonly IAutoFbRebateRetentionService autoFbRebateRetentionService;
    private readonly string Environment;

    private const string DefaultEnvironment = BackgroundJobConstants.DefaultEnvironment;
    private const string dateTimeParse = "yyyy-MM-dd hh:mm:ss.fff tt";

#pragma warning disable S107 // Methods should not have too many parameters

    public AutoFeedbackService(
        IOptions<AppJobSettings> appSettings,
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
        IArchivingDAService archivingDAService,
        ISendNotificationService sendNotificationService,
        IRefundSettledTicketService refundSettledTicketService,
        IVoidingBetIDService voidingBetIDService,
        IAbnormalBetService abnormalBetService,
        IAutoFbMessageNotificationCenterService autoFbMessageNotificationService,
        IAutoFbRebateRetentionService autoFbRebateRetentionService,
        IConfiguration configuration)
#pragma warning restore S107 // Methods should not have too many parameters
    {
        this.appSettings = appSettings.Value;
        this.jobRunningChecker = jobRunningChecker;
        this.systemSettingService = systemSettingService;
        this.winLossService = winLossService;
        this.logService = logService;
        this.turnoverService = turnoverService;
        this.amendmentNotiFbService = amendmentNotiFbService;
        this.problemsAccountFeedbackService = problemsAccountFeedbackService;
        this.feedbackService = feedbackService;
        this.licenseeService = licenseeService;
        this.voidingSettledTicketsService = voidingSettledTicketsService;
        this.archivingDAService = archivingDAService;
        this.sendNotificationService = sendNotificationService;
        this.refundSettledTicketService = refundSettledTicketService;
        this.voidingBetIDService = voidingBetIDService;
        this.abnormalBetService = abnormalBetService;
        this.autoFbMessageNotificationService = autoFbMessageNotificationService;
        this.autoFbRebateRetentionService = autoFbRebateRetentionService;
        Environment = string.IsNullOrEmpty(configuration.GetValue<string>("SiteEnvironment")) ? DefaultEnvironment : configuration.GetValue<string>("SiteEnvironment");
    }

    public async Task AbnormalBetExecuteJobAsync()
    {
        try
        {
            logService.LogInformation("Abnormal Bet - Service is Start.");
            var setting = await systemSettingService.GetAbnormalBetSettingAsync(BackgroundJobConstants.ServiceName_AutoFB);
            if (!setting.ServiceFlag)
            {
                logService.LogInformation("Abnormal Bet - Service is shutdown.");
                return;
            }

            var isRunningLive = setting.JobStatus.Equals(JobStatus.Running, StringComparison.OrdinalIgnoreCase);
            var isRunningUAT = setting.JobStatusUAT.Equals(JobStatus.Running, StringComparison.OrdinalIgnoreCase);

            if (Environment == DefaultEnvironment)
            {
                await AbnormalBetRunJobByEnvironmentAsync(false, isRunningUAT, isRunningLive, setting, Environment);
            }
            else
            {
                await AbnormalBetRunJobByEnvironmentAsync(true, isRunningUAT, isRunningLive, setting, Environment);
            }
        }
        catch (Exception ex)
        {
            await jobRunningChecker.ClearRunning(JobName.CreateFeedbackForVoidingAbnormalBet);
            await logService.ErrorAsync(ex);
        }
    }

    protected internal virtual async Task AbnormalBetRunJobByEnvironmentAsync(bool isUAT, bool isRunningUAT,
        bool isRunningLive, AbnormalBetSystemSetting setting, string environment)
    {
        logService.LogInformation("AbnormalBetRunJobByEnvironmentAsync");
        var LastRuntimeEnv = isUAT ? setting.LastRuntimeUAT : setting.LastRuntime;
        var LastRuntimeNameEnv = isUAT ? SystemSettingName.LastRuntimeUAT : SystemSettingName.LastRuntime;
        var JobStatusNameEnv = isUAT ? SystemSettingName.JobStatusUAT : SystemSettingName.JobStatus;
        var isRunning = isUAT ? isRunningUAT : isRunningLive;

        if (!isRunning)
        {
            await Task.WhenAll(
                jobRunningChecker.MarkRunning(JobName.CreateFeedbackForVoidingAbnormalBet),
                feedbackService.CreateFeedbackForVoidingAbnormalBetAsync(setting, isUAT, LastRuntimeEnv, LastRuntimeNameEnv,
                    JobStatusNameEnv, environment)
            );
            await jobRunningChecker.ClearRunning(JobName.CreateFeedbackForVoidingAbnormalBet);
        }
        else
        {
            isRunning = await jobRunningChecker.IsRunning(JobName.CreateFeedbackForVoidingAbnormalBet);
            if (!isRunning && LastRuntimeEnv.AddMinutes(appSettings.RetryRuntime) < DateTimeOffset.Now.DateTime)
            {
                var settingJobStatus = new List<SystemSettingItem>
                {
                    new() { SettingName = JobStatusNameEnv, Value = JobStatus.Completed },
                    new() { SettingName = LastRuntimeNameEnv, Value = DateTimeOffset.Now.DateTime.ToString("yyyy-MM-dd hh:mm:ss.fff tt") }
                };
                await systemSettingService.EditSystemSettingAsync(settingJobStatus);
            }
        }
    }

    public async Task WinLossExecuteJobAsync()
    {
        try
        {
            var setting = await systemSettingService.GetWinLossSettingAsync();
            if (!setting.ServiceFlag)
            {
                logService.LogInformation("Service Company Winloss is shutdown.");
                return;
            }

            var isRunning = setting.JobStatus.Equals(JobStatus.Running, StringComparison.OrdinalIgnoreCase);

            if (!isRunning)
            {
                await Task.WhenAll(
                    jobRunningChecker.MarkRunning(JobName.CreateFeedbackForWinLossLimit),
                    winLossService.CreateFeedbackForWinLossLimitAsync(setting)
                );
                await jobRunningChecker.ClearRunning(JobName.CreateFeedbackForWinLossLimit);
            }
            else
            {
                isRunning = await jobRunningChecker.IsRunning(JobName.CreateFeedbackForWinLossLimit);
                if (!isRunning && setting.LastRuntime.AddMinutes(appSettings.RetryRuntime) < DateTimeOffset.Now.DateTime)
                {
                    logService.LogInformation("Rerun Company Winloss service.");
                    var settingJobStatus = new List<SystemSettingItem>
                    {
                        new() { SettingName = SystemSettingName.JobStatus, Value = JobStatus.Completed },
                        new() { SettingName = SystemSettingName.LastRuntime, Value = DateTimeOffset.Now.DateTime.ToString(dateTimeParse) }
                    };
                    await systemSettingService.EditSystemSettingAsync(settingJobStatus, BackgroundJobConstants.ServiceName_WinLimitFB);
                }
            }
        }
        catch (Exception ex)
        {
            await jobRunningChecker.ClearRunning(JobName.CreateFeedbackForWinLossLimit);
            await logService.ErrorAsync(ex);
        }
    }

    public async Task TurnoverControlExecuteJobAsync()
    {
        try
        {
            var turnOverSetting = await systemSettingService.GetTurnoverSettingAsync();
            if (!turnOverSetting.ServiceFlag)
            {
                logService.LogInformation("Service Turnover Control is shutdown.");
                return;
            }

            var isRunning = turnOverSetting.JobStatus.Equals(JobStatus.Running, StringComparison.OrdinalIgnoreCase);

            if (!isRunning)
            {
                await Task.WhenAll(
                    jobRunningChecker.MarkRunning(JobName.CreateFeedbackForTurnoverControl),
                    turnoverService.CreateFeedbackForTurnoverControlAsync(turnOverSetting)
                );
                await jobRunningChecker.ClearRunning(JobName.CreateFeedbackForTurnoverControl);
            }
            else
            {
                isRunning = await jobRunningChecker.IsRunning(JobName.CreateFeedbackForTurnoverControl);
                if (!isRunning && turnOverSetting.LastRuntime.AddMinutes(appSettings.RetryRuntime) < DateTimeOffset.Now.DateTime)
                {
                    logService.LogInformation("Rerun Turnover Control service.");
                    var settingJobStatus = new List<SystemSettingItem>
                    {
                        new() { SettingName = SystemSettingName.JobStatus, Value = JobStatus.Completed },
                        new() { SettingName = SystemSettingName.LastRuntime, Value = DateTimeOffset.Now.DateTime.ToString(dateTimeParse)}
                    };
                    await systemSettingService.EditSystemSettingAsync(settingJobStatus, BackgroundJobConstants.ServiceName_TOControlFB);
                }
            }
        }
        catch (Exception ex)
        {
            await jobRunningChecker.ClearRunning(JobName.CreateFeedbackForTurnoverControl);
            await logService.ErrorAsync(ex);
        }
    }

    public async Task AmendmentNotiFbExecuteJobAsync()
    {
        try
        {
            var resultAmendmentSetting = await systemSettingService.GetResultAmendmentSettingAsync();
            if (!resultAmendmentSetting.ServiceFlag)
            {
                logService.LogInformation("Service amendment notiFB is shutdown.");
                return;
            }

            var isRunningLive = resultAmendmentSetting.JobStatus.Equals(JobStatus.Running, StringComparison.OrdinalIgnoreCase);
            var isRunningUAT = resultAmendmentSetting.JobStatusUAT.Equals(JobStatus.Running, StringComparison.OrdinalIgnoreCase);

            if (Environment == DefaultEnvironment)
            {
                await ResultAmendmentRunJobByEnvironmentAsync(false, isRunningUAT, isRunningLive, resultAmendmentSetting, Environment);
            }
            else
            {
                await ResultAmendmentRunJobByEnvironmentAsync(true, isRunningUAT, isRunningLive, resultAmendmentSetting, Environment);
            }
        }
        catch (Exception ex)
        {
            await jobRunningChecker.ClearRunning(JobName.CreateFeedbackAmendmentNotiFB);
            await logService.ErrorAsync(ex);
        }
    }

    protected internal virtual async Task ResultAmendmentRunJobByEnvironmentAsync(bool isUAT, bool isRunningUAT,
        bool isRunningLive, AmendmentNotiFbSetting resultAmendmentSetting, string environment)
    {
        var LastRuntimeEnv = isUAT ? resultAmendmentSetting.LastRuntimeUAT : resultAmendmentSetting.LastRuntime;
        var LastRuntimeNameEnv = isUAT ? SystemSettingName.LastRuntimeUAT : SystemSettingName.LastRuntime;
        var JobStatusNameEnv = isUAT ? SystemSettingName.JobStatusUAT : SystemSettingName.JobStatus;
        var isRunning = isUAT ? isRunningUAT : isRunningLive;

        if (!isRunning)
        {
            await Task.WhenAll(
                jobRunningChecker.MarkRunning(JobName.CreateFeedbackAmendmentNotiFB),
                amendmentNotiFbService.CreateFeedbackForAmendmentNotiFbAsync(resultAmendmentSetting, isUAT, LastRuntimeEnv, LastRuntimeNameEnv,
                    JobStatusNameEnv, environment)
            );
            await jobRunningChecker.ClearRunning(JobName.CreateFeedbackAmendmentNotiFB);
        }
        else
        {
            logService.LogInformation("Service amendment notiFB is running.");
            isRunning = await jobRunningChecker.IsRunning(JobName.CreateFeedbackAmendmentNotiFB);
            if (!isRunning && LastRuntimeEnv.AddMinutes(appSettings.RetryRuntime) < DateTimeOffset.Now.DateTime)
            {
                logService.LogInformation("Update  status and return service result amendment.");
                var settingJobStatus = new List<SystemSettingItem>
                {
                    new() { SettingName = JobStatusNameEnv, Value = JobStatus.Completed },
                    new() { SettingName = LastRuntimeNameEnv, Value = DateTimeOffset.Now.DateTime.ToString(dateTimeParse) }
                };
                await systemSettingService.EditSystemSettingAsync(settingJobStatus, BackgroundJobConstants.ServiceName_AmendmentNotiFB);
            }
        }
    }

    public async Task ProblemsAccountExecuteJobAsync()
    {
        try
        {
            var problemsAccountSetting = await systemSettingService.GetProblemsAccountSettingAsync();
            if (!problemsAccountSetting.ServiceFlag)
            {
                logService.LogInformation("Service Problems Account is shutdown.");
                return;
            }

            var isRunningLive = problemsAccountSetting.JobStatus.Equals(JobStatus.Running, StringComparison.OrdinalIgnoreCase);
            var isRunningUAT = problemsAccountSetting.JobStatusUAT.Equals(JobStatus.Running, StringComparison.OrdinalIgnoreCase);

            if (Environment == DefaultEnvironment)
            {
                await ProblemsAccountRunJobByEnvironmentAsync(false, isRunningUAT, isRunningLive, problemsAccountSetting, Environment);
            }
            else
            {
                await ProblemsAccountRunJobByEnvironmentAsync(true, isRunningUAT, isRunningLive, problemsAccountSetting, Environment);
            }
        }
        catch (Exception ex)
        {
            await jobRunningChecker.ClearRunning(JobName.CreateFeedbackForProblemsAccount);
            await logService.ErrorAsync(ex);
        }
    }

    protected internal virtual async Task ProblemsAccountRunJobByEnvironmentAsync(bool isUAT, bool isRunningUAT,
        bool isRunningLive, ProblemsAccountSetting problemsAccountSetting, string environment)
    {
        var LastRuntimeEnv = isUAT ? problemsAccountSetting.LastRuntimeUAT : problemsAccountSetting.LastRuntime;
        var LastRuntimeNameEnv = isUAT ? SystemSettingName.LastRuntimeUAT : SystemSettingName.LastRuntime;
        var JobStatusNameEnv = isUAT ? SystemSettingName.JobStatusUAT : SystemSettingName.JobStatus;
        var isRunning = isUAT ? isRunningUAT : isRunningLive;

        if (!isRunning)
        {
            await Task.WhenAll(
                jobRunningChecker.MarkRunning(JobName.CreateFeedbackForProblemsAccount),
                problemsAccountFeedbackService.CreateFeedbackForProblemsAccountAsync(
                    problemsAccountSetting, isUAT, LastRuntimeEnv, LastRuntimeNameEnv,
                    JobStatusNameEnv, environment
                )
            );
            await jobRunningChecker.ClearRunning(JobName.CreateFeedbackForProblemsAccount);
        }
        else
        {
            logService.LogInformation("Problems account service is running.");
            isRunning = await jobRunningChecker.IsRunning(JobName.CreateFeedbackForProblemsAccount);
            if (!isRunning && LastRuntimeEnv.AddMinutes(appSettings.RetryRuntime) < DateTimeOffset.Now.DateTime)
            {
                logService.LogInformation("Update  status and return service problems account.");
                var settingJobStatus = new List<SystemSettingItem>
                {
                    new() { SettingName = JobStatusNameEnv, Value = JobStatus.Completed },
                    new() { SettingName = LastRuntimeNameEnv, Value = DateTimeOffset.Now.DateTime.ToString(dateTimeParse) }
                };
                await systemSettingService.EditSystemSettingAsync(settingJobStatus, BackgroundJobConstants.ServiceName_ProblemsAccount);
            }
        }
    }

    public async Task SynchronizeNewLicenseeOICExecuteJobAsync()
    {
        try
        {
            var licenseeInfo = await licenseeService.GetOICLicenseeInfo();
            var commandResult = await licenseeService.ImportLicenseeStatus(
                licenseeInfo,
                Environment);
            if (!commandResult.IsSuccessful)
            {
                logService.LogInformation(commandResult.Message);
            }
        }
        catch (Exception ex)
        {
            await logService.ErrorAsync(ex);
        }
    }

    public async Task VoidingSettledTicketsExecuteJobAsync()
    {
        try
        {
            var voidingSettledTicketSetting = await systemSettingService.GetVoidingSettledTicketsSettingAsync();
            if (!voidingSettledTicketSetting.ServiceFlag)
            {
                logService.LogInformation("Service Voiding Settled Tickets is shutdown.");
                return;
            }

            var isRunningLive = voidingSettledTicketSetting.JobStatus.Equals(JobStatus.Running, StringComparison.OrdinalIgnoreCase);
            var isRunningUAT = voidingSettledTicketSetting.JobStatusUAT.Equals(JobStatus.Running, StringComparison.OrdinalIgnoreCase);

            if (Environment == DefaultEnvironment)
            {
                await VoidingSettledTicketsRunJobByEnvironmentAsync(false, isRunningUAT, isRunningLive, voidingSettledTicketSetting, Environment);
            }
            else
            {
                await VoidingSettledTicketsRunJobByEnvironmentAsync(true, isRunningUAT, isRunningLive, voidingSettledTicketSetting, Environment);
            }
        }
        catch (Exception ex)
        {
            await jobRunningChecker.ClearRunning(JobName.CreateFeedbackForVoidingSettledTickets);
            await logService.ErrorAsync(ex);
        }
    }

    protected internal virtual async Task VoidingSettledTicketsRunJobByEnvironmentAsync(bool isUAT, bool isRunningUAT,
        bool isRunningLive, AmendmentNotiFbSetting voidingSettledTicketSetting, string environment)
    {
        var LastRuntimeEnv = isUAT ? voidingSettledTicketSetting.LastRuntimeUAT : voidingSettledTicketSetting.LastRuntime;
        var LastRuntimeNameEnv = isUAT ? SystemSettingName.LastRuntimeUAT : SystemSettingName.LastRuntime;
        var JobStatusNameEnv = isUAT ? SystemSettingName.JobStatusUAT : SystemSettingName.JobStatus;
        var isRunning = isUAT ? isRunningUAT : isRunningLive;

        if (!isRunning)
        {
            await Task.WhenAll(
                jobRunningChecker.MarkRunning(JobName.CreateFeedbackForVoidingSettledTickets),
                voidingSettledTicketsService.CreateFeedbackForVoidingSettledTickets(voidingSettledTicketSetting, isUAT, LastRuntimeEnv, LastRuntimeNameEnv,
                    JobStatusNameEnv, environment)
            );
            await jobRunningChecker.ClearRunning(JobName.CreateFeedbackForVoidingSettledTickets);
        }
        else
        {
            logService.LogInformation("Voiding settled tickets service is running.");
            isRunning = await jobRunningChecker.IsRunning(JobName.CreateFeedbackForVoidingSettledTickets);
            if (!isRunning && LastRuntimeEnv.AddMinutes(appSettings.RetryRuntime) < DateTimeOffset.Now.DateTime)
            {
                logService.LogInformation("Update  status and return service voiding settled tickets.");
                var settingJobStatus = new List<SystemSettingItem>
                {
                    new() { SettingName = JobStatusNameEnv, Value = JobStatus.Completed },
                    new() { SettingName = LastRuntimeNameEnv, Value = DateTimeOffset.Now.DateTime.ToString("yyyy-MM-dd hh:mm:ss") }
                };
                await systemSettingService.EditSystemSettingAsync(settingJobStatus, BackgroundJobConstants.ServiceName_AutoVoidFB);
            }
        }
    }

    public async Task ArchivingDAExecuteJobAsync()
    {
        try
        {
            var setting = await systemSettingService.GetArchivingDASettingAsync();
            if (!setting.ServiceFlag)
            {
                logService.LogInformation("Service Archiving DA is shutdown.");
                return;
            }

            var isRunningLive = setting.JobStatus.Equals(JobStatus.Running, StringComparison.OrdinalIgnoreCase);
            var isRunningUAT = setting.JobStatusUAT.Equals(JobStatus.Running, StringComparison.OrdinalIgnoreCase);

            if (Environment == DefaultEnvironment)
            {
                await ArchivingDARunJobByEnvironmentAsync(false, isRunningUAT, isRunningLive, setting, Environment);
            }
            else
            {
                await ArchivingDARunJobByEnvironmentAsync(true, isRunningUAT, isRunningLive, setting, Environment);
            }
        }
        catch (Exception ex)
        {
            await jobRunningChecker.ClearRunning(JobName.CreateFeedbackForArchivingDA);
            await logService.ErrorAsync(ex);
        }
    }

    protected internal virtual async Task ArchivingDARunJobByEnvironmentAsync(bool isUAT, bool isRunningUAT,
        bool isRunningLive, ArchivingDASetting setting, string environment)
    {
        var LastRuntimeEnv = isUAT ? setting.LastRuntimeUAT : setting.LastRuntime;
        var LastRuntimeNameEnv = isUAT ? SystemSettingName.LastRuntimeUAT : SystemSettingName.LastRuntime;
        var JobStatusNameEnv = isUAT ? SystemSettingName.JobStatusUAT : SystemSettingName.JobStatus;
        var isRunning = isUAT ? isRunningUAT : isRunningLive;

        if (!isRunning)
        {
            await Task.WhenAll(
                jobRunningChecker.MarkRunning(JobName.CreateFeedbackForArchivingDA),
                archivingDAService.CreateFeedbackForArchivingDAAsync(
                    setting, isUAT, LastRuntimeEnv, LastRuntimeNameEnv,
                    JobStatusNameEnv, environment
                )
            );
            await jobRunningChecker.ClearRunning(JobName.CreateFeedbackForArchivingDA);
        }
        else
        {
            logService.LogInformation("Archiving DA service is running.");
            isRunning = await jobRunningChecker.IsRunning(JobName.CreateFeedbackForArchivingDA);
            if (!isRunning && LastRuntimeEnv.AddMinutes(appSettings.RetryRuntime) < DateTimeOffset.Now.DateTime)
            {
                logService.LogInformation("Update  status and return service archiving DA.");
                var settingJobStatus = new List<SystemSettingItem>
                {
                    new() { SettingName = JobStatusNameEnv, Value = JobStatus.Completed },
                    new() { SettingName = LastRuntimeNameEnv, Value = DateTimeOffset.Now.DateTime.ToString(dateTimeParse) }
                };
                await systemSettingService.EditSystemSettingAsync(settingJobStatus, BackgroundJobConstants.ServiceName_ArchivingDA);
            }
        }
    }

    public async Task RefundSettledTicketJobAsync()
    {
        try
        {
            var refundSettledSetting = await systemSettingService.GetSystemSettingAsync(BackgroundJobConstants.ServiceName_RefundSettledTickets, Environment);
            if (!refundSettledSetting.ServiceFlag)
            {
                logService.LogInformation("Service Refund Settled Tickets is shutdown.");
                return;
            }
            var (messageContent, message) = await refundSettledTicketService.GetMessageRefundSettledTicketAsync(refundSettledSetting.LastMsgTargetId, refundSettledSetting.BatchSize);
            var (commandResult, command) = await refundSettledTicketService.ImportRefundSettledTicketAsync(messageContent, message, Environment);
            if (commandResult.IsSuccessful)
            {
                await sendNotificationService.ScanAndSendMessageToBotEndpointAsync();
            }
            else
            {
                logService.LogInformation(commandResult.Message);
                await logService.ErrorAsync(commandResult, command);
            }
        }
        catch (Exception ex)
        {
            await logService.ErrorAsync(ex);
        }
    }

    public async Task VoidingBetIDExecuteJobAsync()
    {
        try
        {
            var systemSettingInfo = await systemSettingService.GetSystemSettingAsync(BackgroundJobConstants.ServiceName_AutoVoidBetID, Environment);
            if (!systemSettingInfo.ServiceFlag)
            {
                logService.LogInformation("Service VoidBetID is shutdown.");
                return;
            }
            var message = await voidingBetIDService.GetMessageVoidBetIDAsync(systemSettingInfo.LastMsgTargetId, systemSettingInfo.BatchSize);
            var (messageNotDuplicated, maxMsgTargetId) = await voidingBetIDService.CleanMessageVoidBetIDAsync(message);
            var (commandResult, command) = await voidingBetIDService.ImportVoidBetIDAsync(messageNotDuplicated, Environment, maxMsgTargetId);
            if (commandResult.IsSuccessful)
            {
                await sendNotificationService.ScanAndSendMessageToBotEndpointAsync();
            }
            else
            {
                logService.LogInformation(commandResult.Message);
                await logService.ErrorAsync(commandResult, command);
            }
        }
        catch (Exception ex)
        {
            await logService.ErrorAsync(ex);
        }
    }

    public async Task AbnormalBetsExecuteJobAsync()
    {
        try
        {
            var systemSettingInfo = await systemSettingService.GetSystemSettingAsync(BackgroundJobConstants.ServiceName_AutoAbnormalBets, Environment);
            if (!systemSettingInfo.ServiceFlag)
            {
                logService.LogInformation("Service AutoFeedback AbnormalBets is shutdown.");
                return;
            }
            var (voidedCollectionInfo, abnormalMessages, maxVoidingHistoryCollectionId) =
                await abnormalBetService.GetVoidedInfoAsync(systemSettingInfo.LastVoidingHistoryCollectionId, systemSettingInfo.BatchSize);
            var messages = await abnormalBetService.GetMessageAbnormalBetAsync(abnormalMessages);
            var (commandResult, command) = await abnormalBetService.ImportAbnormalBetsAsync(messages, voidedCollectionInfo, Environment, maxVoidingHistoryCollectionId);
            if (commandResult.IsSuccessful)
            {
                await sendNotificationService.ScanAndSendMessageToBotEndpointAsync();
            }
            else
            {
                logService.LogInformation(commandResult.Message);
                await logService.ErrorAsync(commandResult, command);
            }
        }
        catch (Exception ex)
        {
            await logService.ErrorAsync(ex);
        }
    }

    public async Task MessageNotificationCenterExecuteJobAsync()
    {
        try
        {
            var systemSettingInfo = await systemSettingService.GetSystemSettingAsync(
                BackgroundJobConstants.ServiceName_AutoMessageNotificationCenter, Environment);

            if (!systemSettingInfo.ServiceFlag)
            {
                logService.LogInformation("Service AutoFeedback MessageNotificationCenter is shutdown.");
                return;
            }
            var message = await autoFbMessageNotificationService.GetMsgInfoMessageNotificationAsync(systemSettingInfo.LastMsgTargetId, systemSettingInfo.BatchSize);
            var (commandResult, command) = await autoFbMessageNotificationService.ImportMessageNotificationAsync(message, Environment);
            if (commandResult.IsSuccessful)
            {
                await sendNotificationService.ScanAndSendMessageToBotEndpointAsync();
            }
            else
            {
                logService.LogInformation(commandResult.Message);
                await logService.ErrorAsync(commandResult, command);
            }
        }
        catch (Exception ex)
        {
            await logService.ErrorAsync(ex);
        }
    }

    public async Task RebateRetentionExecuteJobAsync()
    {
        try
        {
            var systemSettingInfo = await systemSettingService.GetSystemSettingAsync(
                BackgroundJobConstants.ServiceName_AutoRebateRetention, Environment);

            if (!systemSettingInfo.ServiceFlag)
            {
                logService.LogInformation("Service AutoFeedback RebateRetention is shutdown.");
                return;
            }
            var (rrMessages, newLogSIDs) = await autoFbRebateRetentionService.GetRebateRetentionInfoAsync(systemSettingInfo.LastLogSId);
            var (commandResult, command) = await autoFbRebateRetentionService.ImportRebateRetentionCommand(rrMessages, newLogSIDs, Environment);

            if (commandResult.IsSuccessful)
            {
                await sendNotificationService.ScanAndSendMessageToBotEndpointAsync();
            }
            else
            {
                logService.LogInformation(commandResult.Message);
                await logService.ErrorAsync(commandResult, command);
            }
        }
        catch (Exception ex)
        {
            await logService.ErrorAsync(ex);
        }
    }
}