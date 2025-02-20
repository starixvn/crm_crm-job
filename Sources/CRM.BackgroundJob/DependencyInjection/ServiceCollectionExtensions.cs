using System;
using System.Collections.Generic;
using CRM.BackgroundJob.Models.HealthCheck;
using CRM.BackgroundJob.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CRM.BackgroundJob.DependencyInjection;

using CRM.Models.BotServices;
using CRM.Services;
using Fanex.BotService.Client.Core.Services;
using Fanex.BotService.Client.Core.Services.Interfaces;
using Fanex.Caching;
using Fanex.Data.Repository;
using Fanex.Logging;
using Fanex.RemotingRef;
using Microsoft.AspNetCore.Http;
using Repositories;
using Services;
using Workers;

public static class ServiceCollectionExtensions
{
    public static void AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AppJobSettings>(options => configuration.GetSection("AppSettings").Bind(options));
        services.Configure<List<JobSetting>>(configuration.GetSection("JobSettings"));

        services.AddSingleton(configuration.GetSection("LetstalkHealthCheckSettings").Get<LetstalkHealthCheckSettings>());
        services.AddSingleton(configuration.GetSection("Fanex.BotService").Get<BotServiceSetting>());
        services.AddSingleton(configuration.GetSection("BotServiceSettings").Get<BotEndpointSettings>());
    }

    public static void AddDependencyInjection(this IServiceCollection services)
    {
        services.AddHttpClient();

        #region Core services

        services.AddSingleton<IDynamicRepository, DynamicRepository>();
        services.AddSingleton<IBaseRepository, BaseRepository>();
        services.AddSingleton<IBaseService, BaseService>();
        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<ILogger, Logger>();
        services.AddSingleton<ICrmLogService, CrmLogService>();

        #endregion Core services

        #region BotService

        services.AddSingleton<IBotService, BotService>();

        #endregion BotService

        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddSingleton<IRemotingRefService, RemotingRefService>();
        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<IJobRunningChecker, JobRunningChecker>();
        services.AddSingleton<ICommonFeedback, CommonFeedback>();
        services.AddSingleton<ISettingService, SettingService>();
        services.AddSingleton<ISystemSettingService, SystemSettingService>();
        services.AddSingleton<IMessageService, MessageService>();
        services.AddSingleton<ICustomerService, CustomerService>();
        services.AddSingleton<IFeedbackService, FeedbackService>();
        services.AddSingleton<IWinLossFeedbackService, WinLossFeedbackService>();
        services.AddSingleton<IFeedbackTicketVoidingService, FeedbackTicketVoidingService>();
        services.AddSingleton<IAutoFeedbackService, AutoFeedbackService>();
        services.AddSingleton<IChatBotTicketVoidingService, ChatBotTicketVoidingService>();
        services.AddSingleton<IChatBotCWLService, ChatBotCWLService>();
        services.AddSingleton<IChatBotJobService, ChatBotJobService>();
        services.AddSingleton<IBotHealthCheckService, BotHealthCheckService>();
        services.AddSingleton<IFeedbackTransactionService, FeedbackTransactionService>();
        services.AddSingleton<ICommonFeedback, CommonFeedback>();
        services.AddSingleton<ITOControlFeedbackService, TOControlFeedbackService>();
        services.AddSingleton<IChatBotCTOService, ChatBotCTOService>();
        services.AddSingleton<IChatBotArchivingDAService, ChatBotArchivingDAService>();
        services.AddSingleton<IProblemsAccountFeedbackService, ProblemsAccountFeedbackService>();
        services.AddSingleton<IAmendmentNotiFbService, AmendmentNotiFbService>();
        services.AddSingleton<IChatBotProblemAccountService, ChatBotProblemAccountService>();
        services.AddSingleton<IChatBotResultAmendmentService, ChatBotResultAmendmentService>();
        services.AddSingleton<ISynchronizeLicenseeService, SynchronizeLicenseeService>();
        services.AddSingleton<IVoidingSettledTicketsFeedbackService, VoidingSettledTicketsFeedbackService>();
        services.AddSingleton<IVoidingSettledTicketsService, VoidingSettledTicketsService>();
        services.AddSingleton<IArchivingDAService, ArchivingDAService>();
        services.AddSingleton<ISendAnnouncementService, SendAnnouncementService>();
        services.AddSingleton<IRefundSettledTicketService, RefundSettledTicketService>();
        services.AddSingleton<IVoidingBetIDService, VoidingBetIDService>();
        services.AddSingleton<IAbnormalBetService, AbnormalBetService>();
        services.AddSingleton<IMessageNotificationCenterService, MessageNotificationCenterService>();
        services.AddSingleton<ICaptureSettlementDataService, CaptureSettlementDataService>();
        services.AddSingleton<IGenerateResultAmendmentService, GenerateResultAmendmentService>();
        services.AddSingleton<IPushNotificationResultAmendmentService, PushNotificationResultAmendmentService>();
        services.AddSingleton<IGenerateLapNotificationService, GenerateLapNotificationService>();
        services.AddSingleton<ISendNotificationCenterService, SendNotificationCenterService>();
        services.AddSingleton<IAutoFbMessageNotificationCenterService, AutoFbMessageNotificationCenterService>();
        services.AddSingleton<IAutoFbRebateRetentionService, AutoFbRebateRetentionService>();
        services.AddSingleton<BotServiceHttpClientBase>();
        services.AddSingleton<ISendNotificationService, SendNotificationService>();
        services.AddSingleton<IChatBotSendMessageService, ChatBotSendMessageService>();
        services.AddSingleton<IChatBotRebateRetentionService, ChatBotRebateRetentionService>();
    }

    public static void AddWorkers(this IServiceCollection services)
    {
        services.AddHostedService<AutoFbAmendmentNotiFbWoker>();
        services.AddHostedService<AutoFbProblemAccountWoker>();
        services.AddHostedService<AutoFbVoidingAbnormalBetWorker>();
        services.AddHostedService<AutoFbWinLossLimitWorker>();
        services.AddHostedService<AutoFbTurnoverControlWorker>();
        services.AddHostedService<AutoFbVoidingBetIDWorker>();
        services.AddHostedService<AutoFbAbnormalBetsWorker>();
        services.AddHostedService<AutoFbMessageNotificationCenterWoker>();

        services.AddHostedService<AutoChatbotWorker>();
        services.AddHostedService<AutoChatbotCWLWoker>();
        services.AddHostedService<AutoChatbotCTOWoker>();
        services.AddHostedService<AutoChatbotProblemAccountWorker>();
        services.AddHostedService<AutoChatbotResultAmendmentWoker>();
        services.AddHostedService<AutoChatbotRebateRetentionWorker>();
        services.AddHostedService<ScanAndSendLetstalkNotificationWorker>();
        services.AddHostedService<ScanAndSendAnnouncementWorker>();
        services.AddHostedService<SynchronizeNewLicenseeWorker>();
        services.AddHostedService<AutoFbVoidingSettledTicketsWorker>();
        services.AddHostedService<AutoFbArchivingDAWoker>();
        services.AddHostedService<AutoChatbotArchivingDAWoker>();
        services.AddHostedService<LetstalkHealthCheckWorker>();
        services.AddHostedService<AutoFbRefundSettledTicketWoker>();
        services.AddHostedService<AutoFbRebateRetentionWorker>();

        services.AddHostedService<MncCaptureSettlementDataWorker>();
        services.AddHostedService<MncGenerateResultAmendmentWorker>();
        services.AddHostedService<MncPushNotificationResultAmendmentWorker>();
        services.AddHostedService<MncGenerateLapNotificationWorker>();
        services.AddHostedService<MncSendNotificationCenterWorker>();
    }


    public static void AddRemotingRef(IConfiguration configuration)
    {
        RefDataTypeContainer
            .ConnectTo(new Uri(configuration["RemotingRefService:Fanex.RemotingRef.Service.Url"]))
            .FromApplication("CRM.BackgroundJob");
    }
}