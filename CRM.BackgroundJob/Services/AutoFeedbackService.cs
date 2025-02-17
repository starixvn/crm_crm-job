namespace Fanex.CRM.BackgroundJob.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fanex.CRM.BackgroundJob.Constants;
    using Fanex.CRM.BackgroundJob.Models;
    using global::CRM.BackgroundJob.Core.Models;
    using global::CRM.BackgroundJob.Core.Services;
    using global::CRM.BackgroundJob.Models;
    using global::CRM.BackgroundJob.Models.AbnormalBet;
    using global::CRM.BackgroundJob.Models.AmendmentNotiFB;
    using global::CRM.BackgroundJob.Models.ArchivingDA;
    using global::CRM.BackgroundJob.Models.ProblemsAccount;
    using global::CRM.BackgroundJob.Models.WinLossLimit;
    using global::CRM.BackgroundJob.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;

    public interface IAutoFeedbackService
    {
        Task AbnormalbetExecuteJobAsync();

        Task WinLossExecuteJobAsync();

        Task TurnoverControlExecuteJobAsync();

        Task ProblemsAcccountExecuteJobAsync();

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

        public async Task AbnormalbetExecuteJobAsync()
        {
            try
            {
                logService.LogInformation("Abnormal Bet - Service is Start.");
                AbnormalBetSystemSetting setting = await systemSettingService.GetAbnormalBetSettingAsync(BackgroundJobConstants.ServiceName_AutoFB);
                if (!setting.ServiceFlag)
                {
                    logService.LogInformation("Abnormal Bet - Service is shutdown.");
                    return;
                }

                var isRunningLive = setting.JobStatus.Equals(JobStatus.Running, StringComparison.OrdinalIgnoreCase);
                var isRunningUAT = setting.JobStatusUAT.Equals(JobStatus.Running, StringComparison.OrdinalIgnoreCase);

                if (Environment == DefaultEnvironment)
                {
                    await AbnormalbetRunJobByEnviromentAsync(false, isRunningUAT, isRunningLive, setting, Environment);
                }
                else
                {
                    await AbnormalbetRunJobByEnviromentAsync(true, isRunningUAT, isRunningLive, setting, Environment);
                }
            }
            catch (Exception ex)
            {
                await jobRunningChecker.ClearRunning(JobName.CreateFeedbackForVoidingAbnormalBet);
                await logService.ErrorAsync(ex);
            }
        }

        protected internal virtual async Task AbnormalbetRunJobByEnviromentAsync(bool isUAT, bool isRunningUAT,
            bool isRunningLive, AbnormalBetSystemSetting setting, string Environment)
        {
            logService.LogInformation("AbnormalbetRunJobByEnviromentAsync");
            var LastRuntimeEnv = isUAT ? setting.LastRuntimeUAT : setting.LastRuntime;
            var LastRuntimeNameEnv = isUAT ? SystemSettingName.LastRuntimeUAT : SystemSettingName.LastRuntime;
            var JobStatusNameEnv = isUAT ? SystemSettingName.JobStatusUAT : SystemSettingName.JobStatus;
            var isRunning = isUAT ? isRunningUAT : isRunningLive;

            if (!isRunning)
            {
                await Task.WhenAll(
                         jobRunningChecker.MarkRunning(JobName.CreateFeedbackForVoidingAbnormalBet),
                         feedbackService.CreateFeedbackForVoidingAbnormalBetAsync(setting, isUAT, LastRuntimeEnv, LastRuntimeNameEnv,
                            JobStatusNameEnv, Environment)
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
                           new SystemSettingItem { SettingName = JobStatusNameEnv, Value = JobStatus.Completed },
                           new SystemSettingItem { SettingName = LastRuntimeNameEnv, Value = DateTimeOffset.Now.DateTime.ToString("yyyy-MM-dd hh:mm:ss.fff tt") },
                        };
                    await systemSettingService.EditSystemSettingAsync(settingJobStatus);
                }
            }
        }

        public async Task WinLossExecuteJobAsync()
        {
            try
            {
                WinLossSystemSetting setting = await systemSettingService.GetWinLossSettingAsync();
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
                           new SystemSettingItem { SettingName = SystemSettingName.JobStatus, Value = JobStatus.Completed },
                           new SystemSettingItem { SettingName = SystemSettingName.LastRuntime, Value = DateTimeOffset.Now.DateTime.ToString(dateTimeParse) },
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
                WinLossSystemSetting turnOverSetting = await systemSettingService.GetTurnoverSettingAsync();
                if (!turnOverSetting.ServiceFlag)
                {
                    logService.LogInformation("Service Turnover Controll is shutdown.");
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
                        logService.LogInformation("Rerun Turnover Controll service.");
                        var settingJobStatus = new List<SystemSettingItem>
                        {
                           new SystemSettingItem { SettingName = SystemSettingName.JobStatus, Value = JobStatus.Completed },
                           new SystemSettingItem { SettingName = SystemSettingName.LastRuntime, Value = DateTimeOffset.Now.DateTime.ToString(dateTimeParse)},
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
                AmendmentNotiFbSetting resultAmendmentSetting = await systemSettingService.GetResultAmendmentSettingAsync();
                if (!resultAmendmentSetting.ServiceFlag)
                {
                    logService.LogInformation("Service amendment notiFB is shutdown.");
                    return;
                }

                var isRunningLive = resultAmendmentSetting.JobStatus.Equals(JobStatus.Running, StringComparison.OrdinalIgnoreCase);
                var isRunningUAT = resultAmendmentSetting.JobStatusUAT.Equals(JobStatus.Running, StringComparison.OrdinalIgnoreCase);

                if (Environment == DefaultEnvironment)
                {
                    await ResultAmendmentRunJobByEnviromentAsync(false, isRunningUAT, isRunningLive, resultAmendmentSetting, Environment);
                }
                else
                {
                    await ResultAmendmentRunJobByEnviromentAsync(true, isRunningUAT, isRunningLive, resultAmendmentSetting, Environment);
                }
            }
            catch (Exception ex)
            {
                await jobRunningChecker.ClearRunning(JobName.CreateFeedbackAmendmentNotiFB);
                await logService.ErrorAsync(ex);
            }
        }

        protected internal virtual async Task ResultAmendmentRunJobByEnviromentAsync(bool isUAT, bool isRunningUAT,
            bool isRunningLive, AmendmentNotiFbSetting resultAmendmentSetting, string Environment)
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
                            JobStatusNameEnv, Environment)
                     );
                await jobRunningChecker.ClearRunning(JobName.CreateFeedbackAmendmentNotiFB);
            }
            else
            {
                logService.LogInformation("Service amendment notiFB is running.");
                isRunning = await jobRunningChecker.IsRunning(JobName.CreateFeedbackAmendmentNotiFB);
                if (!isRunning && LastRuntimeEnv.AddMinutes(appSettings.RetryRuntime) < DateTimeOffset.Now.DateTime)
                {
                    logService.LogInformation("Update  status and return serive result amendment.");
                    var settingJobStatus = new List<SystemSettingItem>
                        {
                           new SystemSettingItem { SettingName = JobStatusNameEnv, Value = JobStatus.Completed },
                           new SystemSettingItem { SettingName = LastRuntimeNameEnv, Value = DateTimeOffset.Now.DateTime.ToString(dateTimeParse) },
                        };
                    await systemSettingService.EditSystemSettingAsync(settingJobStatus, BackgroundJobConstants.ServiceName_AmendmentNotiFB);
                }
            }
        }

        public async Task ProblemsAcccountExecuteJobAsync()
        {
            try
            {
                ProblemsAccountSetting problemsAccountSetting = await systemSettingService.GetProblemsAccountSettingAsync();
                if (!problemsAccountSetting.ServiceFlag)
                {
                    logService.LogInformation("Service Problems Account is shutdown.");
                    return;
                }

                var isRunningLive = problemsAccountSetting.JobStatus.Equals(JobStatus.Running, StringComparison.OrdinalIgnoreCase);
                var isRunningUAT = problemsAccountSetting.JobStatusUAT.Equals(JobStatus.Running, StringComparison.OrdinalIgnoreCase);

                if (Environment == DefaultEnvironment)
                {
                    await ProblemsAcccountRunJobByEnviromentAsync(false, isRunningUAT, isRunningLive, problemsAccountSetting, Environment);
                }
                else
                {
                    await ProblemsAcccountRunJobByEnviromentAsync(true, isRunningUAT, isRunningLive, problemsAccountSetting, Environment);
                }
            }
            catch (Exception ex)
            {
                await jobRunningChecker.ClearRunning(JobName.CreateFeedbackForProblemsAccount);
                await logService.ErrorAsync(ex);
            }
        }

        protected internal virtual async Task ProblemsAcccountRunJobByEnviromentAsync(bool isUAT, bool isRunningUAT,
            bool isRunningLive, ProblemsAccountSetting problemsAccountSetting, string Environment)
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
                            JobStatusNameEnv, Environment
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
                    logService.LogInformation("Update  status and return serive problems account.");
                    var settingJobStatus = new List<SystemSettingItem>
                        {
                           new SystemSettingItem { SettingName = JobStatusNameEnv, Value = JobStatus.Completed },
                           new SystemSettingItem { SettingName = LastRuntimeNameEnv, Value = DateTimeOffset.Now.DateTime.ToString(dateTimeParse) },
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
                CommandResult commandResult = await licenseeService.ImportLicenseeStatus(
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
                    await VoidingSettledTicketsRunJobByEnviromentAsync(false, isRunningUAT, isRunningLive, voidingSettledTicketSetting, Environment);
                }
                else
                {
                    await VoidingSettledTicketsRunJobByEnviromentAsync(true, isRunningUAT, isRunningLive, voidingSettledTicketSetting, Environment);
                }
            }
            catch (Exception ex)
            {
                await jobRunningChecker.ClearRunning(JobName.CreateFeedbackForVoidingSettledTickets);
                await logService.ErrorAsync(ex);
            }
        }

        protected internal virtual async Task VoidingSettledTicketsRunJobByEnviromentAsync(bool isUAT, bool isRunningUAT,
            bool isRunningLive, AmendmentNotiFbSetting voidingSettledTicketSetting, string Environment)
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
                            JobStatusNameEnv, Environment)
                     );
                await jobRunningChecker.ClearRunning(JobName.CreateFeedbackForVoidingSettledTickets);
            }
            else
            {
                logService.LogInformation("Voidding settled tickets service is running.");
                isRunning = await jobRunningChecker.IsRunning(JobName.CreateFeedbackForVoidingSettledTickets);
                if (!isRunning && LastRuntimeEnv.AddMinutes(appSettings.RetryRuntime) < DateTimeOffset.Now.DateTime)
                {
                    logService.LogInformation("Update  status and return serive voidding settled tickets.");
                    var settingJobStatus = new List<SystemSettingItem>
                        {
                           new SystemSettingItem { SettingName = JobStatusNameEnv, Value = JobStatus.Completed },
                           new SystemSettingItem { SettingName = LastRuntimeNameEnv, Value = DateTimeOffset.Now.DateTime.ToString("yyyy-MM-dd hh:mm:ss") },
                        };
                    await systemSettingService.EditSystemSettingAsync(settingJobStatus, BackgroundJobConstants.ServiceName_AutoVoidFB);
                }
            }
        }

        public async Task ArchivingDAExecuteJobAsync()
        {
            try
            {
                ArchivingDASetting setting = await systemSettingService.GetArchivingDASettingAsync();
                if (!setting.ServiceFlag)
                {
                    logService.LogInformation("Service Archiving DA is shutdown.");
                    return;
                }

                var isRunningLive = setting.JobStatus.Equals(JobStatus.Running, StringComparison.OrdinalIgnoreCase);
                var isRunningUAT = setting.JobStatusUAT.Equals(JobStatus.Running, StringComparison.OrdinalIgnoreCase);

                if (Environment == DefaultEnvironment)
                {
                    await ArchivingDARunJobByEnviromentAsync(false, isRunningUAT, isRunningLive, setting, Environment);
                }
                else
                {
                    await ArchivingDARunJobByEnviromentAsync(true, isRunningUAT, isRunningLive, setting, Environment);
                }
            }
            catch (Exception ex)
            {
                await jobRunningChecker.ClearRunning(JobName.CreateFeedbackForArchivingDA);
                await logService.ErrorAsync(ex);
            }
        }

        protected internal virtual async Task ArchivingDARunJobByEnviromentAsync(bool isUAT, bool isRunningUAT,
            bool isRunningLive, ArchivingDASetting setting, string Environment)
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
                            JobStatusNameEnv, Environment
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
                    logService.LogInformation("Update  status and return serive archiving DA.");
                    var settingJobStatus = new List<SystemSettingItem>
                        {
                           new SystemSettingItem { SettingName = JobStatusNameEnv, Value = JobStatus.Completed },
                           new SystemSettingItem { SettingName = LastRuntimeNameEnv, Value = DateTimeOffset.Now.DateTime.ToString(dateTimeParse) },
                        };
                    await systemSettingService.EditSystemSettingAsync(settingJobStatus, BackgroundJobConstants.ServiceName_ArchivingDA);
                }
            }
        }

        public async Task RefundSettledTicketJobAsync()
        {
            try
            {
                SystemSettingInfo refundSettledSetting = await systemSettingService.GetSystemSettingAsync(BackgroundJobConstants.ServiceName_RefundSettledTickets, Environment);
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
                SystemSettingInfo systemSettingInfo = await systemSettingService.GetSystemSettingAsync(BackgroundJobConstants.ServiceName_AutoVoidBetID, Environment);
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
                SystemSettingInfo systemSettingInfo = await systemSettingService.GetSystemSettingAsync(BackgroundJobConstants.ServiceName_AutoAbnormalBets, Environment);
                if (!systemSettingInfo.ServiceFlag)
                {
                    logService.LogInformation("Service AutoFeedback AbnormalBets is shutdown.");
                    return;
                }
                var (voidedCollectionInfo, messageAbnormals, maxVoidingHistoryCollectionId) =
                    await abnormalBetService.GetVoidedInfoAsync(systemSettingInfo.LastVoidingHistoryCollectionId, systemSettingInfo.BatchSize);
                var messages = await abnormalBetService.GetMessageAbnormalBetAsync(messageAbnormals);
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
                SystemSettingInfo systemSettingInfo = await systemSettingService.GetSystemSettingAsync(
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
                SystemSettingInfo systemSettingInfo = await systemSettingService.GetSystemSettingAsync(
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
}