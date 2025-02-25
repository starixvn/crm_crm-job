namespace CRM.BackgroundJob.UnitTests.Services
{
    using AutoFixture;
    using CRM.BackgroundJob.Constants;
    using CRM.BackgroundJob.Criteria.AbnormalBet;
    using CRM.BackgroundJob.Criteria.AutoFbMessageNotificationCenter;
    using CRM.BackgroundJob.Criteria.AutoFbRebateRetention;
    using CRM.BackgroundJob.Criteria.RefundSettledTicket;
    using CRM.BackgroundJob.Criteria.VoidingBetID;
    using CRM.BackgroundJob.Models;
    using CRM.BackgroundJob.Models.AbnormalBet;
    using CRM.BackgroundJob.Models.AmendmentNotiFB;
    using CRM.BackgroundJob.Models.ChatBotRebateRetention;
    using CRM.BackgroundJob.Models.ChatBotResulAmendment;
    using CRM.BackgroundJob.Models.LicenseeInfo;
    using CRM.BackgroundJob.Models.ProblemsAccount;
    using CRM.BackgroundJob.Models.RefundSettledTicket;
    using CRM.BackgroundJob.Models.WinLossLimit;
    using CRM.BackgroundJob.Services;
    using CRM.BackgroundJob.UnitTests.Testable;
    using CRM.Models;
    using CRM.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using NSubstitute;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;

    public class AutoFeedbackServiceTests
    {
        private AppJobSettings appSettings;
        private IJobRunningChecker jobRunningChecker;
        private ISystemSettingService systemSettingService;
        private IWinLossFeedbackService winLossService;
        private ICrmLogService logService;
        private ITOControlFeedbackService tOControlFeedbackService;
        private IProblemsAccountFeedbackService problemsAccountFeedbackService;
        private ISynchronizeLicenseeService licenseeService;
        private IConfiguration configuration;
        private IAmendmentNotiFbService amendmentNotiFbService;
        private IFeedbackService feedbackService;
        private IVoidingSettledTicketsFeedbackService voidingSettledTicketsService;
        private IArchivingDAService archivingDAService;
        private ISendNotificationService sendNotificationService;
        private IRefundSettledTicketService refundSettledTicketService;
        private IVoidingBetIDService voidingBetIDService;
        private IAbnormalBetService abnormalBetService;
        private IAutoFbMessageNotificationCenterService autoFbMessageNotificationCenterService;
        private IAutoFbRebateRetentionService autoFbRebateRetentionService;

        private readonly IFixture fixture = new Fixture();

        #region ExecuteJobAsync

        [Fact]
        public async Task ExecuteJobAsync_Exception_ExpectedWriteLogAndClearJob()
        {
            // Arrange
            var exception = fixture.Create<Exception>();
            var service = GetService();
            systemSettingService.When(x => x.GetAbnormalBetSettingAsync(Arg.Any<string>())).Throw(exception);

            // Assert
            await service.AbnormalBetExecuteJobAsync();

            // Act
            await logService.Received().ErrorAsync(exception);
            await jobRunningChecker.Received().ClearRunning(JobName.CreateFeedbackForVoidingAbnormalBet);
        }

        [Fact]
        public async Task ExecuteJobAsync_ServiceIsShutdown_ExpectedWriteLog()
        {
            // Arrange
            var setting = new AbnormalBetSystemSetting { ServiceFlag = false };
            var service = GetService();
            systemSettingService.GetAbnormalBetSettingAsync(Arg.Any<string>()).Returns(setting);

            // Act
            await service.AbnormalBetExecuteJobAsync();

            // Assert
            logService.Received().LogInformation("Abnormal Bet - Service is shutdown.");
        }

        [Fact]
        public async Task ExecuteJobAsync_LIVEEnvironment_ExpectedCallExactFunction()
        {
            // Arrange
            var setting = new AbnormalBetSystemSetting
            {
                ServiceFlag = true,
                JobStatus = "running",
                JobStatusUAT = "completed"
            };
            var service = GetService();
            var env = BackgroundJobConstants.DefaultEnvironment;
            systemSettingService.GetAbnormalBetSettingAsync(Arg.Any<string>()).Returns(setting);
            configuration.GetValue<string>("SiteEnvironment").Returns(env);

            // Act
            await service.AbnormalBetExecuteJobAsync();

            // Assert
            service.Received().Protected("AbnormalBetRunJobByEnvironmentAsync",
                false, false, true, setting, BackgroundJobConstants.DefaultEnvironment
                );
        }

        [Fact]
        public async Task ExecuteJobAsync_EnvironmentIsLIVEAndJobIsRunningAndLastRuntimeIsLowerThanNow_DoNothing()
        {
            // Arrange
            var setting = new AbnormalBetSystemSetting
            {
                ServiceFlag = true,
                LastRuntime = DateTime.Now,
                JobStatus = JobStatus.Running,
                JobStatusUAT = JobStatus.Running
            };
            var service = GetService();
            systemSettingService.GetAbnormalBetSettingAsync(Arg.Any<string>()).Returns(setting);
            jobRunningChecker.IsRunning(JobName.CreateFeedbackForVoidingAbnormalBet).Returns(false);

            // Act
            await service.Public_AbnormalBetRunJobByEnvironmentAsync(false, true, false, setting, "LIVE");

            // Assert
            await systemSettingService.DidNotReceive().EditSystemSettingAsync(Arg.Any<IEnumerable<SystemSettingItem>>());
        }

        [Fact]
        public async Task ExecuteJobAsync_EnvironmentIsUATAndJobIsRunningAndLastRuntimeIsGreaterThanNow_CallExactFunction()
        {
            // Arrange
            var setting = new AbnormalBetSystemSetting
            {
                ServiceFlag = true,
                LastRuntimeUAT = DateTime.Now.AddMinutes(-6),
                JobStatus = JobStatus.Running,
                JobStatusUAT = JobStatus.Running
            };
            var service = GetService();
            jobRunningChecker.IsRunning(JobName.CreateFeedbackForVoidingAbnormalBet).Returns(false);

            // Act
            await service.Public_AbnormalBetRunJobByEnvironmentAsync(true, true, false, setting, "UAT");

            // Assert
            await systemSettingService.Received().EditSystemSettingAsync(Arg.Any<IEnumerable<SystemSettingItem>>());
        }

        [Fact]
        public async Task AbnormalBetRunJobByEnvironmentAsync_EnvironmentIsUATAndJobIsRunningAndLastRuntimeIsLowerThanNow_DoNothing()
        {
            // Arrange
            var setting = new AbnormalBetSystemSetting
            {
                ServiceFlag = true,
                LastRuntimeUAT = DateTime.Now,
                JobStatus = JobStatus.Running,
                JobStatusUAT = JobStatus.Running
            };
            var service = GetService();
            systemSettingService.GetAbnormalBetSettingAsync(Arg.Any<string>()).Returns(setting);
            jobRunningChecker.IsRunning(JobName.CreateFeedbackForVoidingAbnormalBet).Returns(false);

            // Act
            await service.Public_AbnormalBetRunJobByEnvironmentAsync(true, true, false, setting, "UAT");

            // Assert
            await systemSettingService.DidNotReceive().EditSystemSettingAsync(Arg.Any<IEnumerable<SystemSettingItem>>());
        }

        [Fact]
        public async Task AbnormalBetRunJobByEnvironmentAsync_EnvironmentIsLIVEAndJobIsRunningAndLastRuntimeIsGreaterThanNow_CallExactFunction()
        {
            // Arrange
            var setting = new AbnormalBetSystemSetting
            {
                ServiceFlag = true,
                LastRuntime = DateTime.Now.AddMinutes(-6),
                JobStatus = JobStatus.Running,
                JobStatusUAT = JobStatus.Running
            };
            var service = GetService();
            systemSettingService.GetAbnormalBetSettingAsync(Arg.Any<string>()).Returns(setting);
            jobRunningChecker.IsRunning(JobName.CreateFeedbackForVoidingAbnormalBet).Returns(false);

            // Act
            await service.Public_AbnormalBetRunJobByEnvironmentAsync(false, true, true, setting, "LIVE");

            // Assert
            await systemSettingService.Received().EditSystemSettingAsync(Arg.Any<IEnumerable<SystemSettingItem>>());
        }

        [Fact]
        public async Task AbnormalBetRunJobByEnvironmentAsync_EnvironmentIsLIVEAndJobIsNotRunning_CallExactFunction()
        {
            // Arrange
            var setting = new AbnormalBetSystemSetting
            {
                ServiceFlag = true,
                LastRuntime = DateTime.Now,
                JobStatus = JobStatus.Running,
                JobStatusUAT = JobStatus.Running
            };
            var service = GetService();
            systemSettingService.GetAbnormalBetSettingAsync(Arg.Any<string>()).Returns(setting);
            jobRunningChecker.IsRunning(JobName.CreateFeedbackForVoidingAbnormalBet).Returns(false);

            // Act
            await service.Public_AbnormalBetRunJobByEnvironmentAsync(false, false, false, setting, "LIVE");

            // Assert
            await jobRunningChecker.Received().MarkRunning(JobName.CreateFeedbackForVoidingAbnormalBet);
            await feedbackService.Received().CreateFeedbackForVoidingAbnormalBetAsync(setting, false, setting.LastRuntime, SystemSettingName.LastRuntime, SystemSettingName.JobStatus, "LIVE");
            await jobRunningChecker.Received().ClearRunning(JobName.CreateFeedbackForVoidingAbnormalBet);
        }

        [Fact]
        public async Task AbnormalBetRunJobByEnvironmentAsync_EnvironmentIsUATAndJobIsNotRunning_CallExactFunction()
        {
            // Arrange
            var setting = new AbnormalBetSystemSetting
            {
                ServiceFlag = true,
                LastRuntimeUAT = DateTime.Now,
                JobStatus = JobStatus.Running,
                JobStatusUAT = JobStatus.Running
            };
            var service = GetService();
            systemSettingService.GetAbnormalBetSettingAsync(Arg.Any<string>()).Returns(setting);
            jobRunningChecker.IsRunning(JobName.CreateFeedbackForVoidingAbnormalBet).Returns(false);

            // Act
            await service.Public_AbnormalBetRunJobByEnvironmentAsync(true, false, false, setting, "UAT");

            // Assert
            await jobRunningChecker.Received().MarkRunning(JobName.CreateFeedbackForVoidingAbnormalBet);
            await feedbackService.Received().CreateFeedbackForVoidingAbnormalBetAsync(setting, true, setting.LastRuntimeUAT, SystemSettingName.LastRuntimeUAT, SystemSettingName.JobStatusUAT, "UAT");
            await jobRunningChecker.Received().ClearRunning(JobName.CreateFeedbackForVoidingAbnormalBet);
        }

        #endregion ExecuteJobAsync

        #region WinLossExecuteJobAsync

        [Fact]
        public async Task WinLossExecuteJobAsync_Exception_ExpectedWriteLogAndClearJob()
        {
            // Arrange
            var exception = fixture.Create<Exception>();
            var service = GetService();
            systemSettingService.When(x => x.GetWinLossSettingAsync()).Throw(exception);

            // Assert
            await service.WinLossExecuteJobAsync();

            // Act
            await logService.Received().ErrorAsync(exception);
            await jobRunningChecker.Received().ClearRunning(JobName.CreateFeedbackForWinLossLimit);
        }

        [Fact]
        public async Task WinLossExecuteJobAsync_ServiceIsShutdown_ExpectedWriteLog()
        {
            // Arrange
            var setting = new WinLossSystemSetting { ServiceFlag = false };
            var service = GetService();
            systemSettingService.GetWinLossSettingAsync().Returns(setting);

            // Act
            await service.WinLossExecuteJobAsync();

            // Assert
            logService.Received().LogInformation("Service Company Winloss is shutdown.");
        }

        [Fact]
        public async Task WinLossExecuteJobAsync_JobIsRunningAndLastRuntimeIsLowerThanNow_DoNothing()
        {
            // Arrange
            var setting = new WinLossSystemSetting
            {
                ServiceFlag = true,
                LastRuntime = DateTime.Now,
                JobStatus = JobStatus.Running
            };
            var service = GetService();
            systemSettingService.GetWinLossSettingAsync().Returns(setting);
            jobRunningChecker.IsRunning(JobName.CreateFeedbackForWinLossLimit).Returns(true);

            // Act
            await service.WinLossExecuteJobAsync();

            // Assert
            logService.DidNotReceive().LogInformation(Arg.Any<string>());
            await systemSettingService.DidNotReceive().EditSystemSettingAsync(Arg.Any<IEnumerable<SystemSettingItem>>());
        }

        [Fact]
        public async Task WinLossExecuteJobAsync_JobIsRunningAndLastRuntimeIsGreaterThanNow_DoNothing()
        {
            // Arrange
            var service = GetService();
            var setting = new WinLossSystemSetting
            {
                ServiceFlag = true,
                LastRuntime = DateTime.Now.AddMinutes(-appSettings.RetryRuntime - 1),
                JobStatus = JobStatus.Running
            };
            systemSettingService.GetWinLossSettingAsync().Returns(setting);
            jobRunningChecker.IsRunning(JobName.CreateFeedbackForWinLossLimit).Returns(false);

            // Act
            await service.WinLossExecuteJobAsync();

            // Assert
            logService.Received().LogInformation("Rerun Company Winloss service.");
            await systemSettingService.Received().EditSystemSettingAsync(Arg.Any<IEnumerable<SystemSettingItem>>(), BackgroundJobConstants.ServiceName_WinLimitFB);
        }

        [Fact]
        public async Task WinLossExecuteJobAsync_JobIsCompleted_ExpectedCallCreateFeedback()
        {
            // Arrange
            var service = GetService();
            var setting = new WinLossSystemSetting
            {
                ServiceFlag = true,
                JobStatus = JobStatus.Completed
            };
            systemSettingService.GetWinLossSettingAsync().Returns(setting);

            // Act
            await service.WinLossExecuteJobAsync();

            // Assert
            await jobRunningChecker.Received().MarkRunning(JobName.CreateFeedbackForWinLossLimit);
            await jobRunningChecker.Received().ClearRunning(JobName.CreateFeedbackForWinLossLimit);
            await winLossService.Received().CreateFeedbackForWinLossLimitAsync(setting);
        }

        #endregion WinLossExecuteJobAsync

        #region TurnoverControlExecuteJobAsync

        [Fact]
        public async Task TurnoverControlExecuteJobAsync_Exception_ExpectedWriteLogAndClearJob()
        {
            // Arrange
            var exception = fixture.Create<Exception>();
            var service = GetService();
            systemSettingService.When(x => x.GetTurnoverSettingAsync()).Throw(exception);

            // Assert
            await service.TurnoverControlExecuteJobAsync();

            // Act
            await logService.Received().ErrorAsync(exception);
            await jobRunningChecker.Received().ClearRunning(JobName.CreateFeedbackForTurnoverControl);
        }

        [Fact]
        public async Task TurnoverControlExecuteJobAsync_ServiceIsShutdown_ExpectedWriteLog()
        {
            // Arrange
            var setting = new WinLossSystemSetting { ServiceFlag = false };
            var service = GetService();
            systemSettingService.GetTurnoverSettingAsync().Returns(setting);

            // Act
            await service.TurnoverControlExecuteJobAsync();

            // Assert
            logService.Received().LogInformation("Service Turnover Control is shutdown.");
        }

        [Fact]
        public async Task TurnoverControlExecuteJobAsync_JobIsRunningAndLastRuntimeIsLowerThanNow_DoNothing()
        {
            // Arrange
            var setting = new WinLossSystemSetting
            {
                ServiceFlag = true,
                LastRuntime = DateTime.Now,
                JobStatus = JobStatus.Running
            };
            var service = GetService();
            systemSettingService.GetTurnoverSettingAsync().Returns(setting);
            jobRunningChecker.IsRunning(JobName.CreateFeedbackForTurnoverControl).Returns(true);

            // Act
            await service.TurnoverControlExecuteJobAsync();

            // Assert
            await systemSettingService.DidNotReceive().EditSystemSettingAsync(Arg.Any<IEnumerable<SystemSettingItem>>());
        }

        [Fact]
        public async Task TurnoverControlExecuteJobAsync_JobIsRunningAndLastRuntimeIsGreaterThanNow_DoNothing()
        {
            // Arrange
            var service = GetService();
            var setting = new WinLossSystemSetting
            {
                ServiceFlag = true,
                LastRuntime = DateTime.Now.AddMinutes(-appSettings.RetryRuntime - 1),
                JobStatus = JobStatus.Running
            };
            systemSettingService.GetTurnoverSettingAsync().Returns(setting);
            jobRunningChecker.IsRunning(JobName.CreateFeedbackForTurnoverControl).Returns(false);

            // Act
            await service.TurnoverControlExecuteJobAsync();

            // Assert
            logService.Received().LogInformation("Rerun Turnover Control service.");
            await systemSettingService.Received().EditSystemSettingAsync(Arg.Any<IEnumerable<SystemSettingItem>>(), BackgroundJobConstants.ServiceName_TOControlFB);
        }

        [Fact]
        public async Task TurnoverControlExecuteJobAsync_JobIsCompleted_ExpectedCallCreateFeedback()
        {
            // Arrange
            var service = GetService();
            var setting = new WinLossSystemSetting
            {
                ServiceFlag = true,
                JobStatus = JobStatus.Completed
            };
            systemSettingService.GetTurnoverSettingAsync().Returns(setting);

            // Act
            await service.TurnoverControlExecuteJobAsync();

            // Assert
            await jobRunningChecker.Received().MarkRunning(JobName.CreateFeedbackForTurnoverControl);
            await jobRunningChecker.Received().ClearRunning(JobName.CreateFeedbackForTurnoverControl);
            await tOControlFeedbackService.Received().CreateFeedbackForTurnoverControlAsync(setting);
        }

        #endregion TurnoverControlExecuteJobAsync

        #region AmendmentNotiFbExecuteJobAsync

        [Fact]
        public async Task VoidingSettledTicketsExecuteJobAsync_Exception_ExpectedWriteLogAndClearJob()
        {
            // Arrange
            var exception = fixture.Create<Exception>();
            var service = GetService();
            systemSettingService.When(x => x.GetVoidingSettledTicketsSettingAsync()).Throw(exception);

            // Assert
            await service.VoidingSettledTicketsExecuteJobAsync();

            // Act
            await logService.Received().ErrorAsync(exception);
            await jobRunningChecker.Received().ClearRunning(JobName.CreateFeedbackForVoidingSettledTickets);
        }

        [Fact]
        public async Task VoidingSettledTicketsExecuteJobAsync_ServiceIsShutdown_ExpectedWriteLog()
        {
            // Arrange
            var setting = new AmendmentNotiFbSetting { ServiceFlag = false };
            var service = GetService();
            systemSettingService.GetVoidingSettledTicketsSettingAsync().Returns(setting);

            // Act
            await service.VoidingSettledTicketsExecuteJobAsync();

            // Assert
            logService.Received().LogInformation("Service Voiding Settled Tickets is shutdown.");
        }

        [Fact]
        public async Task VoidingSettledTicketsExecuteJobAsync_LIVEEnvironment_ExpectedCallExactFunction()
        {
            // Arrange
            var setting = new AmendmentNotiFbSetting
            {
                ServiceFlag = true,
                JobStatus = "running",
                JobStatusUAT = "completed"
            };
            var service = GetService();
            var env = BackgroundJobConstants.DefaultEnvironment;
            systemSettingService.GetVoidingSettledTicketsSettingAsync().Returns(setting);
            configuration.GetValue<string>("SiteEnvironment").Returns(env);

            // Act
            await service.VoidingSettledTicketsExecuteJobAsync();

            // Assert
            service.Received().Protected("VoidingSettledTicketsRunJobByEnvironmentAsync",
                false, false, true, setting, BackgroundJobConstants.DefaultEnvironment
                );
        }

        [Fact]
        public async Task VoidingSettledTicketsRunJobByEnvironmentAsync_EnvironmentIsLIVEAndJobIsRunningAndLastRuntimeIsLowerThanNow_DoNothing()
        {
            // Arrange
            var setting = new AmendmentNotiFbSetting
            {
                ServiceFlag = true,
                LastRuntime = DateTime.Now,
                JobStatus = JobStatus.Running,
                JobStatusUAT = JobStatus.Running
            };
            var service = GetService();
            systemSettingService.GetVoidingSettledTicketsSettingAsync().Returns(setting);
            jobRunningChecker.IsRunning(JobName.CreateFeedbackForVoidingSettledTickets).Returns(false);

            // Act
            await service.Public_VoidingSettledTicketsRunJobByEnvironmentAsync(false, true, false, setting, "LIVE");

            // Assert
            await systemSettingService.DidNotReceive().EditSystemSettingAsync(Arg.Any<IEnumerable<SystemSettingItem>>());
        }

        [Fact]
        public async Task VoidingSettledTicketsRunJobByEnvironmentAsync_EnvironmentIsUATAndJobIsRunningAndLastRuntimeIsGreaterThanNow_CallExactFunction()
        {
            // Arrange
            var setting = new AmendmentNotiFbSetting
            {
                ServiceFlag = true,
                LastRuntimeUAT = DateTime.Now.AddMinutes(-6),
                JobStatus = JobStatus.Running,
                JobStatusUAT = JobStatus.Running
            };
            var service = GetService();
            jobRunningChecker.IsRunning(JobName.CreateFeedbackForVoidingSettledTickets).Returns(false);

            // Act
            await service.Public_VoidingSettledTicketsRunJobByEnvironmentAsync(true, true, false, setting, "UAT");

            // Assert
            await systemSettingService.Received().EditSystemSettingAsync(Arg.Any<IEnumerable<SystemSettingItem>>(), BackgroundJobConstants.ServiceName_AutoVoidFB);
        }

        [Fact]
        public async Task VoidingSettledTicketsRunJobByEnvironmentAsync_EnvironmentIsUATAndJobIsRunningAndLastRuntimeIsLowerThanNow_DoNothing()
        {
            // Arrange
            var setting = new AmendmentNotiFbSetting
            {
                ServiceFlag = true,
                LastRuntimeUAT = DateTime.Now,
                JobStatus = JobStatus.Running,
                JobStatusUAT = JobStatus.Running
            };
            var service = GetService();
            systemSettingService.GetVoidingSettledTicketsSettingAsync().Returns(setting);
            jobRunningChecker.IsRunning(JobName.CreateFeedbackForVoidingSettledTickets).Returns(false);

            // Act
            await service.Public_VoidingSettledTicketsRunJobByEnvironmentAsync(true, true, false, setting, "UAT");

            // Assert
            await systemSettingService.DidNotReceive().EditSystemSettingAsync(Arg.Any<IEnumerable<SystemSettingItem>>());
        }

        [Fact]
        public async Task VoidingSettledTicketsRunJobByEnvironmentAsync_EnvironmentIsLIVEAndJobIsRunningAndLastRuntimeIsGreaterThanNow_CallExactFunction()
        {
            // Arrange
            var setting = new AmendmentNotiFbSetting
            {
                ServiceFlag = true,
                LastRuntime = DateTime.Now.AddMinutes(-6),
                JobStatus = JobStatus.Running,
                JobStatusUAT = JobStatus.Running
            };
            var service = GetService();
            systemSettingService.GetVoidingSettledTicketsSettingAsync().Returns(setting);
            jobRunningChecker.IsRunning(JobName.CreateFeedbackForVoidingSettledTickets).Returns(false);

            // Act
            await service.Public_VoidingSettledTicketsRunJobByEnvironmentAsync(false, true, true, setting, "LIVE");

            // Assert
            logService.Received().LogInformation("Update  status and return service voiding settled tickets.");
            await systemSettingService.Received().EditSystemSettingAsync(Arg.Any<IEnumerable<SystemSettingItem>>(), BackgroundJobConstants.ServiceName_AutoVoidFB);
        }

        [Fact]
        public async Task VoidingSettledTicketsRunJobByEnvironmentAsync_EnvironmentIsLIVEAndJobIsNotRunning_CallExactFunction()
        {
            // Arrange
            var setting = new AmendmentNotiFbSetting
            {
                ServiceFlag = true,
                LastRuntime = DateTime.Now,
                JobStatus = JobStatus.Running,
                JobStatusUAT = JobStatus.Running
            };
            var service = GetService();
            systemSettingService.GetVoidingSettledTicketsSettingAsync().Returns(setting);
            jobRunningChecker.IsRunning(JobName.CreateFeedbackForVoidingSettledTickets).Returns(false);

            // Act
            await service.Public_VoidingSettledTicketsRunJobByEnvironmentAsync(false, false, false, setting, "LIVE");

            // Assert
            await jobRunningChecker.Received().MarkRunning(JobName.CreateFeedbackForVoidingSettledTickets);
            await voidingSettledTicketsService.Received().CreateFeedbackForVoidingSettledTickets(setting, false, setting.LastRuntime, SystemSettingName.LastRuntime, SystemSettingName.JobStatus, "LIVE");
            await jobRunningChecker.Received().ClearRunning(JobName.CreateFeedbackForVoidingSettledTickets);
        }

        [Fact]
        public async Task VoidingSettledTicketsRunJobByEnvironmentAsync_EnvironmentIsUATAndJobIsNotRunning_CallExactFunction()
        {
            // Arrange
            var setting = new AmendmentNotiFbSetting
            {
                ServiceFlag = true,
                LastRuntimeUAT = DateTime.Now,
                JobStatus = JobStatus.Running,
                JobStatusUAT = JobStatus.Running
            };
            var service = GetService();
            systemSettingService.GetVoidingSettledTicketsSettingAsync().Returns(setting);
            jobRunningChecker.IsRunning(JobName.CreateFeedbackAmendmentNotiFB).Returns(false);

            // Act
            await service.Public_VoidingSettledTicketsRunJobByEnvironmentAsync(true, false, false, setting, "UAT");

            // Assert
            await jobRunningChecker.Received().MarkRunning(JobName.CreateFeedbackForVoidingSettledTickets);
            await voidingSettledTicketsService.Received().CreateFeedbackForVoidingSettledTickets(setting, true, setting.LastRuntimeUAT, SystemSettingName.LastRuntimeUAT, SystemSettingName.JobStatusUAT, "UAT");
            await jobRunningChecker.Received().ClearRunning(JobName.CreateFeedbackForVoidingSettledTickets);
        }

        #endregion AmendmentNotiFbExecuteJobAsync

        #region ProblemsAcccountExecuteJobAsync

        [Fact]
        public async Task ProblemsAccountExecuteJobAsync_Exception_ExpectedWriteLogAndClearJob()
        {
            // Arrange
            var exception = fixture.Create<Exception>();
            var service = GetService();
            systemSettingService.When(x => x.GetProblemsAccountSettingAsync()).Throw(exception);

            // Assert
            await service.ProblemsAccountExecuteJobAsync();

            // Act
            await logService.Received().ErrorAsync(exception);
            await jobRunningChecker.Received().ClearRunning(JobName.CreateFeedbackForProblemsAccount);
        }

        [Fact]
        public async Task ProblemsAccountExecuteJobAsync_ServiceIsShutdown_ExpectedWriteLog()
        {
            // Arrange
            var setting = new ProblemsAccountSetting { ServiceFlag = false };
            var service = GetService();
            systemSettingService.GetProblemsAccountSettingAsync().Returns(setting);

            // Act
            await service.ProblemsAccountExecuteJobAsync();

            // Assert
            logService.Received().LogInformation("Service Problems Account is shutdown.");
        }

        [Fact]
        public async Task ProblemsAccountExecuteJobAsync_LIVEEnvironment_ExpectedCallExactFunction()
        {
            // Arrange
            var setting = new ProblemsAccountSetting
            {
                ServiceFlag = true,
                JobStatus = "running",
                JobStatusUAT = "completed"
            };
            var service = GetService();
            var env = BackgroundJobConstants.DefaultEnvironment;
            systemSettingService.GetProblemsAccountSettingAsync().Returns(setting);
            configuration.GetValue<string>("SiteEnvironment").Returns(env);

            // Act
            await service.ProblemsAccountExecuteJobAsync();

            // Assert
            service.Received().Protected("ProblemsAccountRunJobByEnvironmentAsync",
                false, false, true, setting, BackgroundJobConstants.DefaultEnvironment
                );
        }

        [Fact]
        public async Task ProblemsAccountRunJobByEnvironmentAsync_EnvironmentIsLIVEAndJobIsRunningAndLastRuntimeIsLowerThanNow_DoNothing()
        {
            // Arrange
            var setting = new ProblemsAccountSetting
            {
                ServiceFlag = true,
                LastRuntime = DateTime.Now,
                JobStatus = JobStatus.Running,
                JobStatusUAT = JobStatus.Running
            };
            var service = GetService();
            systemSettingService.GetTurnoverSettingAsync().Returns(setting);
            jobRunningChecker.IsRunning(JobName.CreateFeedbackForProblemsAccount).Returns(false);

            // Act
            await service.Public_ProblemsAccountRunJobByEnvironmentAsync(false, true, false, setting, "LIVE");

            // Assert
            await systemSettingService.DidNotReceive().EditSystemSettingAsync(Arg.Any<IEnumerable<SystemSettingItem>>());
        }

        [Fact]
        public async Task ProblemsAccountRunJobByEnvironmentAsync_EnvironmentIsUATAndJobIsRunningAndLastRuntimeIsLowerThanNow_DoNothing()
        {
            // Arrange
            var setting = new ProblemsAccountSetting
            {
                ServiceFlag = true,
                LastRuntimeUAT = DateTime.Now,
                JobStatus = JobStatus.Running,
                JobStatusUAT = JobStatus.Running
            };
            var service = GetService();
            systemSettingService.GetTurnoverSettingAsync().Returns(setting);
            jobRunningChecker.IsRunning(JobName.CreateFeedbackForProblemsAccount).Returns(false);

            // Act
            await service.Public_ProblemsAccountRunJobByEnvironmentAsync(true, true, false, setting, "UAT");

            // Assert
            await systemSettingService.DidNotReceive().EditSystemSettingAsync(Arg.Any<IEnumerable<SystemSettingItem>>());
        }

        [Fact]
        public async Task ProblemsAccountRunJobByEnvironmentAsync_EnvironmentIsUATAndJobIsRunningAndLastRuntimeIsGreaterThanNow_CallExactFunction()
        {
            // Arrange
            var setting = new ProblemsAccountSetting
            {
                ServiceFlag = true,
                LastRuntimeUAT = DateTime.Now.AddMinutes(-6),
                JobStatus = JobStatus.Running,
                JobStatusUAT = JobStatus.Running
            };
            var service = GetService();
            systemSettingService.GetTurnoverSettingAsync().Returns(setting);
            jobRunningChecker.IsRunning(JobName.CreateFeedbackForProblemsAccount).Returns(false);

            // Act
            await service.Public_ProblemsAccountRunJobByEnvironmentAsync(true, true, false, setting, "UAT");

            // Assert
            await systemSettingService.Received().EditSystemSettingAsync(Arg.Any<IEnumerable<SystemSettingItem>>(), BackgroundJobConstants.ServiceName_ProblemsAccount);
        }

        [Fact]
        public async Task ProblemsAccountRunJobByEnvironmentAsync_EnvironmentIsLIVEAndJobIsRunningAndLastRuntimeIsGreaterThanNow_CallExactFunction()
        {
            // Arrange
            var setting = new ProblemsAccountSetting
            {
                ServiceFlag = true,
                LastRuntime = DateTime.Now.AddMinutes(-6),
                JobStatus = JobStatus.Running,
                JobStatusUAT = JobStatus.Running
            };
            var service = GetService();
            systemSettingService.GetTurnoverSettingAsync().Returns(setting);
            jobRunningChecker.IsRunning(JobName.CreateFeedbackForProblemsAccount).Returns(false);

            // Act
            await service.Public_ProblemsAccountRunJobByEnvironmentAsync(false, true, true, setting, "LIVE");

            // Assert
            logService.Received().LogInformation("Update  status and return service problems account.");
            await systemSettingService.Received().EditSystemSettingAsync(Arg.Any<IEnumerable<SystemSettingItem>>(), BackgroundJobConstants.ServiceName_ProblemsAccount);
        }

        [Fact]
        public async Task ProblemsAccountRunJobByEnvironmentAsync_EnvironmentIsLIVEAndJobIsNotRunning_CallExactFunction()
        {
            // Arrange
            var setting = new ProblemsAccountSetting
            {
                ServiceFlag = true,
                LastRuntime = DateTime.Now,
                JobStatus = JobStatus.Running,
                JobStatusUAT = JobStatus.Running
            };
            var service = GetService();
            systemSettingService.GetTurnoverSettingAsync().Returns(setting);
            jobRunningChecker.IsRunning(JobName.CreateFeedbackForProblemsAccount).Returns(false);

            // Act
            await service.Public_ProblemsAccountRunJobByEnvironmentAsync(false, false, false, setting, "LIVE");

            // Assert
            await jobRunningChecker.Received().MarkRunning(JobName.CreateFeedbackForProblemsAccount);
            await problemsAccountFeedbackService.Received().CreateFeedbackForProblemsAccountAsync(setting, false, setting.LastRuntime, SystemSettingName.LastRuntime, SystemSettingName.JobStatus, "LIVE");
            await jobRunningChecker.Received().ClearRunning(JobName.CreateFeedbackForProblemsAccount);
        }

        [Fact]
        public async Task ProblemsAccountRunJobByEnvironmentAsync_EnvironmentIsUATAndJobIsNotRunning_CallExactFunction()
        {
            // Arrange
            var setting = new ProblemsAccountSetting
            {
                ServiceFlag = true,
                LastRuntimeUAT = DateTime.Now,
                JobStatus = JobStatus.Running,
                JobStatusUAT = JobStatus.Running
            };
            var service = GetService();
            systemSettingService.GetTurnoverSettingAsync().Returns(setting);
            jobRunningChecker.IsRunning(JobName.CreateFeedbackForProblemsAccount).Returns(false);

            // Act
            await service.Public_ProblemsAccountRunJobByEnvironmentAsync(true, false, false, setting, "UAT");

            // Assert
            await jobRunningChecker.Received().MarkRunning(JobName.CreateFeedbackForProblemsAccount);
            await problemsAccountFeedbackService.Received().CreateFeedbackForProblemsAccountAsync(setting, true, setting.LastRuntimeUAT, SystemSettingName.LastRuntimeUAT, SystemSettingName.JobStatusUAT, "UAT");
            await jobRunningChecker.Received().ClearRunning(JobName.CreateFeedbackForProblemsAccount);
        }

        #endregion ProblemsAcccountExecuteJobAsync

        #region SynchronizeNewLicenseeOICExecuteJobAsync

        [Fact]
        public async Task SynchronizeNewLicenseeOICExecuteJobAsync_Exception_ExpectedWriteLog()
        {
            // Arrange
            var exception = fixture.Create<Exception>();
            var service = GetService();
            licenseeService.When(x => x.GetOICLicenseeInfo()).Throw(exception);

            // Assert
            await service.SynchronizeNewLicenseeOICExecuteJobAsync();

            // Act
            await logService.Received().ErrorAsync(exception);
        }

        [Fact]
        public async Task SynchronizeNewLicenseeOICExecuteJobAsync_ExceptionImportData_ExpectedWriteLog()
        {
            // Arrange
            var exception = fixture.Create<Exception>();
            var service = GetService();
            var licensee = fixture.Create<IEnumerable<LicenseeStatus>>();
            licenseeService.GetOICLicenseeInfo().Returns(licensee);
            licenseeService.When(x => x.ImportLicenseeStatus(licensee, "LIVE")).Throw(exception);

            // Assert
            await service.SynchronizeNewLicenseeOICExecuteJobAsync();

            // Act
            await logService.Received().ErrorAsync(exception);
        }

        [Fact]
        public async Task SynchronizeNewLicenseeOICExecuteJobAsync_ImportDataIsNotSuccessful_ExpectedWriteLog()
        {
            // Arrange
            var service = GetService();
            var commandResult = new CommandResult { Code = 1 };
            var licensee = fixture.Create<IEnumerable<LicenseeStatus>>();
            licenseeService.GetOICLicenseeInfo().Returns(licensee);
            licenseeService.ImportLicenseeStatus(licensee, "LIVE").Returns(commandResult);

            // Assert
            await service.SynchronizeNewLicenseeOICExecuteJobAsync();

            // Act
            logService.Received().LogInformation(commandResult.Message);
        }

        [Fact]
        public async Task SynchronizeNewLicenseeOICExecuteJobAsync_ImportDataIsSuccessful_DoNothing()
        {
            // Arrange
            var service = GetService();
            var commandResult = new CommandResult { Code = 0 };
            var licensee = fixture.Create<IEnumerable<LicenseeStatus>>();
            licenseeService.GetOICLicenseeInfo().Returns(licensee);
            licenseeService.ImportLicenseeStatus(licensee, "LIVE").Returns(commandResult);

            // Assert
            await service.SynchronizeNewLicenseeOICExecuteJobAsync();

            // Act
            logService.DidNotReceive();
        }

        #endregion SynchronizeNewLicenseeOICExecuteJobAsync

        #region VoidingSettledTicketsExecuteJobAsync

        [Fact]
        public async Task AmendmentNotiFbExecuteJobAsync_Exception_ExpectedWriteLogAndClearJob()
        {
            // Arrange
            var exception = fixture.Create<Exception>();
            var service = GetService();
            systemSettingService.When(x => x.GetResultAmendmentSettingAsync()).Throw(exception);

            // Assert
            await service.AmendmentNotiFbExecuteJobAsync();

            // Act
            await logService.Received().ErrorAsync(exception);
            await jobRunningChecker.Received().ClearRunning(JobName.CreateFeedbackAmendmentNotiFB);
        }

        [Fact]
        public async Task AmendmentNotiFbExecuteJobAsync_ServiceIsShutdown_ExpectedWriteLog()
        {
            // Arrange
            var setting = new AmendmentNotiFbSetting { ServiceFlag = false };
            var service = GetService();
            systemSettingService.GetResultAmendmentSettingAsync().Returns(setting);

            // Act
            await service.AmendmentNotiFbExecuteJobAsync();

            // Assert
            logService.Received().LogInformation("Service amendment notiFB is shutdown.");
        }

        [Fact]
        public async Task AmendmentNotiFbExecuteJobAsync_LIVEEnvironment_ExpectedCallExactFunction()
        {
            // Arrange
            var setting = new AmendmentNotiFbSetting
            {
                ServiceFlag = true,
                JobStatus = "running",
                JobStatusUAT = "completed"
            };
            var service = GetService();
            var env = BackgroundJobConstants.DefaultEnvironment;
            systemSettingService.GetResultAmendmentSettingAsync().Returns(setting);
            configuration.GetValue<string>("SiteEnvironment").Returns(env);

            // Act
            await service.AmendmentNotiFbExecuteJobAsync();

            // Assert
            service.Received().Protected("ResultAmendmentRunJobByEnvironmentAsync",
                false, false, true, setting, BackgroundJobConstants.DefaultEnvironment
                );
        }

        [Fact]
        public async Task AmendmentNotiFbExecuteJobAsync_EnvironmentIsLIVEAndJobIsRunningAndLastRuntimeIsLowerThanNow_DoNothing()
        {
            // Arrange
            var setting = new AmendmentNotiFbSetting
            {
                ServiceFlag = true,
                LastRuntime = DateTime.Now,
                JobStatus = JobStatus.Running,
                JobStatusUAT = JobStatus.Running
            };
            var service = GetService();
            systemSettingService.GetResultAmendmentSettingAsync().Returns(setting);
            jobRunningChecker.IsRunning(JobName.CreateFeedbackForProblemsAccount).Returns(false);

            // Act
            await service.Public_ResultAmendmentRunJobByEnvironmentAsync(false, true, false, setting, "LIVE");

            // Assert
            await systemSettingService.DidNotReceive().EditSystemSettingAsync(Arg.Any<IEnumerable<SystemSettingItem>>());
        }

        [Fact]
        public async Task AmendmentNotiFbExecuteJobAsync_EnvironmentIsUATAndJobIsRunningAndLastRuntimeIsGreaterThanNow_CallExactFunction()
        {
            // Arrange
            var setting = new AmendmentNotiFbSetting
            {
                ServiceFlag = true,
                LastRuntimeUAT = DateTime.Now.AddMinutes(-6),
                JobStatus = JobStatus.Running,
                JobStatusUAT = JobStatus.Running
            };
            var service = GetService();
            jobRunningChecker.IsRunning(JobName.CreateFeedbackAmendmentNotiFB).Returns(false);

            // Act
            await service.Public_ResultAmendmentRunJobByEnvironmentAsync(true, true, false, setting, "UAT");

            // Assert
            await systemSettingService.Received().EditSystemSettingAsync(Arg.Any<IEnumerable<SystemSettingItem>>(), BackgroundJobConstants.ServiceName_AmendmentNotiFB);
        }

        [Fact]
        public async Task ResultAmendmentRunJobByEnvironmentAsync_EnvironmentIsUATAndJobIsRunningAndLastRuntimeIsLowerThanNow_DoNothing()
        {
            // Arrange
            var setting = new AmendmentNotiFbSetting
            {
                ServiceFlag = true,
                LastRuntimeUAT = DateTime.Now,
                JobStatus = JobStatus.Running,
                JobStatusUAT = JobStatus.Running
            };
            var service = GetService();
            systemSettingService.GetResultAmendmentSettingAsync().Returns(setting);
            jobRunningChecker.IsRunning(JobName.CreateFeedbackForProblemsAccount).Returns(false);

            // Act
            await service.Public_ResultAmendmentRunJobByEnvironmentAsync(true, true, false, setting, "UAT");

            // Assert
            await systemSettingService.DidNotReceive().EditSystemSettingAsync(Arg.Any<IEnumerable<SystemSettingItem>>());
        }

        [Fact]
        public async Task ResultAmendmentRunJobByEnvironmentAsync_EnvironmentIsLIVEAndJobIsRunningAndLastRuntimeIsGreaterThanNow_CallExactFunction()
        {
            // Arrange
            var setting = new AmendmentNotiFbSetting
            {
                ServiceFlag = true,
                LastRuntime = DateTime.Now.AddMinutes(-6),
                JobStatus = JobStatus.Running,
                JobStatusUAT = JobStatus.Running
            };
            var service = GetService();
            systemSettingService.GetResultAmendmentSettingAsync().Returns(setting);
            jobRunningChecker.IsRunning(JobName.CreateFeedbackAmendmentNotiFB).Returns(false);

            // Act
            await service.Public_ResultAmendmentRunJobByEnvironmentAsync(false, true, true, setting, "LIVE");

            // Assert
            logService.Received().LogInformation("Update  status and return service result amendment.");
            await systemSettingService.Received().EditSystemSettingAsync(Arg.Any<IEnumerable<SystemSettingItem>>(), BackgroundJobConstants.ServiceName_AmendmentNotiFB);
        }

        [Fact]
        public async Task AmendmentNotiFbExecuteJobAsync_EnvironmentIsLIVEAndJobIsNotRunning_CallExactFunction()
        {
            // Arrange
            var setting = new AmendmentNotiFbSetting
            {
                ServiceFlag = true,
                LastRuntime = DateTime.Now,
                JobStatus = JobStatus.Running,
                JobStatusUAT = JobStatus.Running
            };
            var service = GetService();
            systemSettingService.GetResultAmendmentSettingAsync().Returns(setting);
            jobRunningChecker.IsRunning(JobName.CreateFeedbackForProblemsAccount).Returns(false);

            // Act
            await service.Public_ResultAmendmentRunJobByEnvironmentAsync(false, false, false, setting, "LIVE");

            // Assert
            await jobRunningChecker.Received().MarkRunning(JobName.CreateFeedbackAmendmentNotiFB);
            await amendmentNotiFbService.Received().CreateFeedbackForAmendmentNotiFbAsync(setting, false, setting.LastRuntime, SystemSettingName.LastRuntime, SystemSettingName.JobStatus, "LIVE");
            await jobRunningChecker.Received().ClearRunning(JobName.CreateFeedbackAmendmentNotiFB);
        }

        [Fact]
        public async Task AmendmentNotiFbExecuteJobAsync_EnvironmentIsUATAndJobIsNotRunning_CallExactFunction()
        {
            // Arrange
            var setting = new AmendmentNotiFbSetting
            {
                ServiceFlag = true,
                LastRuntimeUAT = DateTime.Now,
                JobStatus = JobStatus.Running,
                JobStatusUAT = JobStatus.Running
            };
            var service = GetService();
            systemSettingService.GetResultAmendmentSettingAsync().Returns(setting);
            jobRunningChecker.IsRunning(JobName.CreateFeedbackAmendmentNotiFB).Returns(false);

            // Act
            await service.Public_ResultAmendmentRunJobByEnvironmentAsync(true, false, false, setting, "UAT");

            // Assert
            await jobRunningChecker.Received().MarkRunning(JobName.CreateFeedbackAmendmentNotiFB);
            await amendmentNotiFbService.Received().CreateFeedbackForAmendmentNotiFbAsync(setting, true, setting.LastRuntimeUAT, SystemSettingName.LastRuntimeUAT, SystemSettingName.JobStatusUAT, "UAT");
            await jobRunningChecker.Received().ClearRunning(JobName.CreateFeedbackAmendmentNotiFB);
        }

        #endregion VoidingSettledTicketsExecuteJobAsync

        #region RefundSettledTicketJobAsync

        [Fact]
        public async Task RefundSettledTicketJobAsync_ServiceIsShutdown_ExpectedWriteLog()
        {
            // Arrange
            var setting = new SystemSettingInfo { ServiceFlag = false };
            var service = GetService();
            systemSettingService.GetSystemSettingAsync(BackgroundJobConstants.ServiceName_RefundSettledTickets, BackgroundJobConstants.DefaultEnvironment).Returns(setting);

            // Act
            await service.RefundSettledTicketJobAsync();

            // Assert
            logService.Received().LogInformation("Service Refund Settled Tickets is shutdown.");
        }

        [Fact]
        public async Task RefundSettledTicketJobAsync_ImportRefundSettledTicketAsyncUnsuccessful_ExpectedWriteLog()
        {
            // Arrange
            var setting = new SystemSettingInfo { ServiceFlag = true };
            var messageContent = fixture.Create<ValueTuple<IEnumerable<MessageRefundSettledTicket>, IEnumerable<MessageVoidSettled>>>();
            var commandResult = new CommandResult { Code = 1 };
            var criteria = fixture.Create<ImportRefundSettledTicketCommand>();
            var valueTuple = new ValueTuple<CommandResult, ImportRefundSettledTicketCommand>(commandResult, criteria);
            var service = GetService();
            systemSettingService.GetSystemSettingAsync(BackgroundJobConstants.ServiceName_RefundSettledTickets, BackgroundJobConstants.DefaultEnvironment).Returns(setting);
            refundSettledTicketService.GetMessageRefundSettledTicketAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(messageContent);
            refundSettledTicketService.ImportRefundSettledTicketAsync(messageContent.Item1, messageContent.Item2, BackgroundJobConstants.DefaultEnvironment).Returns(valueTuple);

            // Act
            await service.RefundSettledTicketJobAsync();

            // Assert
            logService.Received().LogInformation(valueTuple.Item1.Message);
            await logService.Received().ErrorAsync(valueTuple.Item1, valueTuple.Item2);
        }

        [Fact]
        public async Task RefundSettledTicketJobAsync_ImportRefundSettledTicketAsyncSuccessful_ExpectedToCallScanAndSendMessageToBotEndpointAsync()
        {
            // Arrange
            var setting = new SystemSettingInfo { ServiceFlag = true };
            var messageContent = fixture.Create<ValueTuple<IEnumerable<MessageRefundSettledTicket>, IEnumerable<MessageVoidSettled>>>();
            var commandResult = new CommandResult { Code = 0 };
            var criteria = fixture.Create<ImportRefundSettledTicketCommand>();
            var valueTuple = new ValueTuple<CommandResult, ImportRefundSettledTicketCommand>(commandResult, criteria);
            var service = GetService();
            systemSettingService.GetSystemSettingAsync(BackgroundJobConstants.ServiceName_RefundSettledTickets, BackgroundJobConstants.DefaultEnvironment).Returns(setting);
            refundSettledTicketService.GetMessageRefundSettledTicketAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(messageContent);
            refundSettledTicketService.ImportRefundSettledTicketAsync(messageContent.Item1, messageContent.Item2, BackgroundJobConstants.DefaultEnvironment).Returns(valueTuple);

            // Act
            await service.RefundSettledTicketJobAsync();

            // Assert
            await sendNotificationService.Received().ScanAndSendMessageToBotEndpointAsync();
        }

        #endregion RefundSettledTicketJobAsync

        #region VoidingBetIDExecuteJobAsync

        [Fact]
        public async Task VoidingBetIDExecuteJobAsync_ServiceIsShutdown_ExpectedWriteLog()
        {
            // Arrange
            var setting = new SystemSettingInfo { ServiceFlag = false };
            var service = GetService();
            systemSettingService.GetSystemSettingAsync(BackgroundJobConstants.ServiceName_AutoVoidBetID, BackgroundJobConstants.DefaultEnvironment).Returns(setting);

            // Act
            await service.VoidingBetIDExecuteJobAsync();

            // Assert
            logService.Received().LogInformation("Service VoidBetID is shutdown.");
        }

        [Fact]
        public async Task VoidingBetIDExecuteJobAsync_ImportVoidBetIDAsyncUnsuccessful_ExpectedWriteLog()
        {
            // Arrange
            var setting = new SystemSettingInfo { ServiceFlag = true };
            var messages = fixture.Create<IEnumerable<MessageInfoID>>();
            var cleanMessages = fixture.Create<ValueTuple<IEnumerable<MessageInfoID>, int>>();
            var commandResult = new CommandResult { Code = 1 };
            var criteria = fixture.Create<ImportVoidBetIDCommand>();
            var valueTuple = new ValueTuple<CommandResult, ImportVoidBetIDCommand>(commandResult, criteria);
            var service = GetService();
            systemSettingService.GetSystemSettingAsync(BackgroundJobConstants.ServiceName_AutoVoidBetID, BackgroundJobConstants.DefaultEnvironment).Returns(setting);
            voidingBetIDService.GetMessageVoidBetIDAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(messages);
            voidingBetIDService.CleanMessageVoidBetIDAsync(messages).Returns(cleanMessages);
            voidingBetIDService.ImportVoidBetIDAsync(cleanMessages.Item1, BackgroundJobConstants.DefaultEnvironment, cleanMessages.Item2).Returns(valueTuple);

            // Act
            await service.VoidingBetIDExecuteJobAsync();

            // Assert
            logService.Received().LogInformation(valueTuple.Item1.Message);
            await logService.Received().ErrorAsync(valueTuple.Item1, valueTuple.Item2);
        }

        [Fact]
        public async Task VoidingBetIDExecuteJobAsync_ImportVoidBetIDAsyncSuccessful_ExpectedToCallScanAndSendMessageToBotEndpointAsync()
        {
            // Arrange
            var setting = new SystemSettingInfo { ServiceFlag = true };
            var messages = fixture.Create<IEnumerable<MessageInfoID>>();
            var cleanMessages = fixture.Create<ValueTuple<IEnumerable<MessageInfoID>, int>>();
            var commandResult = new CommandResult { Code = 0 };
            var criteria = fixture.Create<ImportVoidBetIDCommand>();
            var valueTuple = new ValueTuple<CommandResult, ImportVoidBetIDCommand>(commandResult, criteria);
            var service = GetService();
            systemSettingService.GetSystemSettingAsync(BackgroundJobConstants.ServiceName_AutoVoidBetID, BackgroundJobConstants.DefaultEnvironment).Returns(setting);
            voidingBetIDService.GetMessageVoidBetIDAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(messages);
            voidingBetIDService.CleanMessageVoidBetIDAsync(messages).Returns(cleanMessages);
            voidingBetIDService.ImportVoidBetIDAsync(cleanMessages.Item1, BackgroundJobConstants.DefaultEnvironment, cleanMessages.Item2).Returns(valueTuple);

            // Act
            await service.VoidingBetIDExecuteJobAsync();

            // Assert
            await sendNotificationService.Received().ScanAndSendMessageToBotEndpointAsync();
        }

        #endregion VoidingBetIDExecuteJobAsync

        #region AbnormalBetsExecuteJobAsync

        [Fact]
        public async Task AbnormalBetsExecuteJobAsync_ServiceIsShutdown_ExpectedWriteLog()
        {
            // Arrange
            var setting = new SystemSettingInfo { ServiceFlag = false };
            var service = GetService();
            systemSettingService.GetSystemSettingAsync(BackgroundJobConstants.ServiceName_AutoAbnormalBets, BackgroundJobConstants.DefaultEnvironment).Returns(setting);

            // Act
            await service.AbnormalBetsExecuteJobAsync();

            // Assert
            logService.Received().LogInformation("Service AutoFeedback AbnormalBets is shutdown.");
        }

        [Fact]
        public async Task AbnormalBetsExecuteJobAsync_ImportAbnormalBetsAsyncUnsuccessful_ExpectedWriteLog()
        {
            // Arrange
            var setting = new SystemSettingInfo { ServiceFlag = true };
            var voidedInfo = fixture.Create<ValueTuple<IEnumerable<VoidedInfo>, IEnumerable<VoidingMessage>, int>>();
            var messages = fixture.Create<IEnumerable<MessageAbnormalBet>>();
            var commandResult = new CommandResult { Code = 1 };
            var criteria = fixture.Create<ImportAbnormalBetCommand>();
            var valueTuple = new ValueTuple<CommandResult, ImportAbnormalBetCommand>(commandResult, criteria);
            var service = GetService();
            systemSettingService.GetSystemSettingAsync(BackgroundJobConstants.ServiceName_AutoAbnormalBets, BackgroundJobConstants.DefaultEnvironment).Returns(setting);
            abnormalBetService.GetVoidedInfoAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(voidedInfo);
            abnormalBetService.GetMessageAbnormalBetAsync(voidedInfo.Item2).Returns(messages);
            abnormalBetService.ImportAbnormalBetsAsync(messages, voidedInfo.Item1, BackgroundJobConstants.DefaultEnvironment, voidedInfo.Item3).Returns(valueTuple);

            // Act
            await service.AbnormalBetsExecuteJobAsync();

            // Assert
            logService.Received().LogInformation(valueTuple.Item1.Message);
            await logService.Received().ErrorAsync(valueTuple.Item1, valueTuple.Item2);
        }

        [Fact]
        public async Task AbnormalBetsExecuteJobAsync_ImportAbnormalBetsAsyncSuccessful_ExpectedToCallScanAndSendMessageToBotEndpointAsync()
        {
            // Arrange
            var setting = new SystemSettingInfo { ServiceFlag = true };
            var voidedInfo = fixture.Create<ValueTuple<IEnumerable<VoidedInfo>, IEnumerable<VoidingMessage>, int>>();
            var messages = fixture.Create<IEnumerable<MessageAbnormalBet>>();
            var commandResult = new CommandResult { Code = 0 };
            var criteria = fixture.Create<ImportAbnormalBetCommand>();
            var valueTuple = new ValueTuple<CommandResult, ImportAbnormalBetCommand>(commandResult, criteria);
            var service = GetService();
            systemSettingService.GetSystemSettingAsync(BackgroundJobConstants.ServiceName_AutoAbnormalBets, BackgroundJobConstants.DefaultEnvironment).Returns(setting);
            abnormalBetService.GetVoidedInfoAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(voidedInfo);
            abnormalBetService.GetMessageAbnormalBetAsync(voidedInfo.Item2).Returns(messages);
            abnormalBetService.ImportAbnormalBetsAsync(messages, voidedInfo.Item1, BackgroundJobConstants.DefaultEnvironment, voidedInfo.Item3).Returns(valueTuple);

            // Act
            await service.AbnormalBetsExecuteJobAsync();

            // Assert
            await sendNotificationService.Received().ScanAndSendMessageToBotEndpointAsync();
        }

        #endregion AbnormalBetsExecuteJobAsync

        #region MessageNotificationCenterExecuteJobAsync

        [Fact]
        public async Task MessageNotificationCenterExecuteJobAsync_ServiceIsShutdown_ExpectedWriteLog()
        {
            // Arrange
            var setting = new SystemSettingInfo { ServiceFlag = false };
            var service = GetService();
            systemSettingService.GetSystemSettingAsync(BackgroundJobConstants.ServiceName_AutoMessageNotificationCenter, BackgroundJobConstants.DefaultEnvironment).Returns(setting);

            // Act
            await service.MessageNotificationCenterExecuteJobAsync();

            // Assert
            logService.Received().LogInformation("Service AutoFeedback MessageNotificationCenter is shutdown.");
        }

        [Fact]
        public async Task MessageNotificationCenterExecuteJobAsync_ImportAbnormalBetsAsyncUnsuccessful_ExpectedWriteLog()
        {
            // Arrange
            var setting = new SystemSettingInfo { ServiceFlag = true, LastMsgTargetId = 1, BatchSize = 100 };
            var messageInfoID = fixture.Create<IEnumerable<MessageInfoID>>();
            var commandResult = new CommandResult { Code = 1 };
            var command = fixture.Create<ImportMessageNotificationCenterCommand>();
            var valueTuple = new ValueTuple<CommandResult, ImportMessageNotificationCenterCommand>(commandResult, command);
            var service = GetService();
            systemSettingService.GetSystemSettingAsync(BackgroundJobConstants.ServiceName_AutoMessageNotificationCenter, BackgroundJobConstants.DefaultEnvironment).Returns(setting);
            autoFbMessageNotificationCenterService.GetMsgInfoMessageNotificationAsync(setting.LastMsgTargetId, setting.BatchSize).Returns(messageInfoID);
            autoFbMessageNotificationCenterService.ImportMessageNotificationAsync(messageInfoID, BackgroundJobConstants.DefaultEnvironment).Returns(valueTuple);

            // Act
            await service.MessageNotificationCenterExecuteJobAsync();

            // Assert
            logService.Received().LogInformation(valueTuple.Item1.Message);
            await logService.Received().ErrorAsync(valueTuple.Item1, valueTuple.Item2);
        }

        [Fact]
        public async Task MessageNotificationCenterExecuteJobAsync_ExpectedScanAndSendMessageToBotEndpoint()
        {
            // Arrange
            var setting = new SystemSettingInfo { ServiceFlag = true, LastMsgTargetId = 1, BatchSize = 100 };
            var messageInfoID = fixture.Create<IEnumerable<MessageInfoID>>();
            var commandResult = new CommandResult { Code = 0 };
            var command = fixture.Create<ImportMessageNotificationCenterCommand>();
            var valueTuple = new ValueTuple<CommandResult, ImportMessageNotificationCenterCommand>(commandResult, command);
            var service = GetService();
            systemSettingService.GetSystemSettingAsync(BackgroundJobConstants.ServiceName_AutoMessageNotificationCenter, BackgroundJobConstants.DefaultEnvironment).Returns(setting);
            autoFbMessageNotificationCenterService.GetMsgInfoMessageNotificationAsync(setting.LastMsgTargetId, setting.BatchSize).Returns(messageInfoID);
            autoFbMessageNotificationCenterService.ImportMessageNotificationAsync(messageInfoID, BackgroundJobConstants.DefaultEnvironment).Returns(valueTuple);

            // Act
            await service.MessageNotificationCenterExecuteJobAsync();

            // Assert
            await sendNotificationService.Received().ScanAndSendMessageToBotEndpointAsync();
        }

        #endregion MessageNotificationCenterExecuteJobAsync

        #region RebateRetentionExecuteJobAsync

        [Fact]
        public async Task RebateRetentionExecuteJobAsync_ServiceIsShutdown_ExpectedWriteLog()
        {
            // Arrange
            var setting = new SystemSettingInfo { ServiceFlag = false };
            var service = GetService();
            systemSettingService.GetSystemSettingAsync(BackgroundJobConstants.ServiceName_AutoRebateRetention, BackgroundJobConstants.DefaultEnvironment).Returns(setting);

            // Act
            await service.RebateRetentionExecuteJobAsync();

            // Assert
            logService.Received().LogInformation("Service AutoFeedback RebateRetention is shutdown.");
        }

        [Fact]
        public async Task RebateRetentionExecuteJobAsync_ImportAbnormalBetsAsyncUnsuccessful_ExpectedWriteLog()
        {
            // Arrange
            var setting = new SystemSettingInfo { ServiceFlag = true, LastMsgTargetId = 1, BatchSize = 100, LastLogSId = "1" };
            var rrMessages = fixture.Create<IEnumerable<ChatBotRebateRetentionInfo>>();
            var newLogSIDs = "123";
            var commandResult = new CommandResult { Code = 1 };
            var command = fixture.Create<ImportAutoFbRebateRetentionCommand>();
            var valueTuple = new ValueTuple<CommandResult, ImportAutoFbRebateRetentionCommand>(commandResult, command);
            var service = GetService();
            systemSettingService.GetSystemSettingAsync(BackgroundJobConstants.ServiceName_AutoRebateRetention, BackgroundJobConstants.DefaultEnvironment).Returns(setting);
            autoFbRebateRetentionService.GetRebateRetentionInfoAsync(setting.LastLogSId).Returns((rrMessages, newLogSIDs));
            autoFbRebateRetentionService.ImportRebateRetentionCommand(rrMessages, newLogSIDs, BackgroundJobConstants.DefaultEnvironment).Returns(valueTuple);

            // Act
            await service.RebateRetentionExecuteJobAsync();

            // Assert
            logService.Received().LogInformation(valueTuple.Item1.Message);
            await logService.Received().ErrorAsync(valueTuple.Item1, valueTuple.Item2);
        }

        [Fact]
        public async Task RebateRetentionExecuteJobAsync_ExpectedScanAndSendMessageToBotEndpoint()
        {
            // Arrange
            var setting = new SystemSettingInfo { ServiceFlag = true, LastMsgTargetId = 1, BatchSize = 100, LastLogSId = "1" };
            var rrMessages = fixture.Create<IEnumerable<ChatBotRebateRetentionInfo>>();
            var newLogSIDs = "123";
            var commandResult = new CommandResult { Code = 0 };
            var command = fixture.Create<ImportAutoFbRebateRetentionCommand>();
            var valueTuple = new ValueTuple<CommandResult, ImportAutoFbRebateRetentionCommand>(commandResult, command);
            var service = GetService();
            systemSettingService.GetSystemSettingAsync(BackgroundJobConstants.ServiceName_AutoRebateRetention, BackgroundJobConstants.DefaultEnvironment).Returns(setting);
            autoFbRebateRetentionService.GetRebateRetentionInfoAsync(setting.LastLogSId).Returns((rrMessages, newLogSIDs));
            autoFbRebateRetentionService.ImportRebateRetentionCommand(rrMessages, newLogSIDs, BackgroundJobConstants.DefaultEnvironment).Returns(valueTuple);

            // Act
            await service.RebateRetentionExecuteJobAsync();

            // Assert
            await sendNotificationService.Received().ScanAndSendMessageToBotEndpointAsync();
        }

        #endregion RebateRetentionExecuteJobAsync

        #region Helpers

        private AutoFeedbackServiceTestable GetService()
        {
            appSettings = new AppJobSettings
            {
                RetryRuntime = 5
            };
            var appJobSettings = Substitute.For<IOptionsSnapshot<AppJobSettings>>();
            appJobSettings.Value.Returns(appSettings);
            jobRunningChecker = Substitute.For<IJobRunningChecker>();
            systemSettingService = Substitute.For<ISystemSettingService>();
            winLossService = Substitute.For<IWinLossFeedbackService>();
            logService = Substitute.For<ICrmLogService>();
            tOControlFeedbackService = Substitute.For<ITOControlFeedbackService>();
            problemsAccountFeedbackService = Substitute.For<IProblemsAccountFeedbackService>();
            amendmentNotiFbService = Substitute.For<IAmendmentNotiFbService>();
            feedbackService = Substitute.For<IFeedbackService>();
            licenseeService = Substitute.For<ISynchronizeLicenseeService>();
            configuration = Substitute.For<IConfiguration>();
            voidingSettledTicketsService = Substitute.For<IVoidingSettledTicketsFeedbackService>();
            archivingDAService = Substitute.For<IArchivingDAService>();
            sendNotificationService = Substitute.For<ISendNotificationService>();
            refundSettledTicketService = Substitute.For<IRefundSettledTicketService>();
            voidingBetIDService = Substitute.For<IVoidingBetIDService>();
            abnormalBetService = Substitute.For<IAbnormalBetService>();
            autoFbMessageNotificationCenterService = Substitute.For<IAutoFbMessageNotificationCenterService>();
            autoFbRebateRetentionService = Substitute.For<IAutoFbRebateRetentionService>();
            return Substitute.For<AutoFeedbackServiceTestable>(
                appJobSettings,
                jobRunningChecker,
                systemSettingService,
                winLossService,
                logService,
                tOControlFeedbackService,
                amendmentNotiFbService,
                problemsAccountFeedbackService,
                feedbackService,
                licenseeService,
                voidingSettledTicketsService,
                archivingDAService,
                sendNotificationService,
                refundSettledTicketService,
                voidingBetIDService,
                abnormalBetService,
                autoFbMessageNotificationCenterService,
                autoFbRebateRetentionService,
                configuration);
        }

        #endregion Helpers
    }
}