using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace CRM.BackgroundJob.DependencyInjection;

using CRM.BackgroundJob.Models;
using CRM.BackgroundJob.Models.HealthCheck;
using CRM.BackgroundJob.Services;
using CRM.BackgroundJob.Workers;
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
        services.AddCoreServices();
        services.AddChatBotServices();
        services.AddFeedbackServices();
    }

    private static void AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<IDynamicRepository, DynamicRepository>();
        services.AddSingleton<IBaseRepository, BaseRepository>();
        services.AddSingleton<IBaseService, BaseService>();
        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<ILogger, Logger>();
        services.AddSingleton<ICrmLogService, CrmLogService>();

        services.AddSingleton<IBotService, BotService>();
        services.AddSingleton<BotServiceHttpClientBase>();

        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddSingleton<IRemotingRefService, RemotingRefService>();
        services.AddSingleton<IJobRunningChecker, JobRunningChecker>();
        services.AddSingleton<IBotHealthCheckService, BotHealthCheckService>();
    }

    private static void AddChatBotServices(this IServiceCollection services)
    {
        services.AddSingleton<IChatBotTicketVoidingService, ChatBotTicketVoidingService>();
        services.AddSingleton<IChatBotCWLService, ChatBotCWLService>();
        services.AddSingleton<IChatBotJobService, ChatBotJobService>();
        services.AddSingleton<IChatBotCTOService, ChatBotCTOService>();
        services.AddSingleton<IChatBotArchivingDAService, ChatBotArchivingDAService>();
        services.AddSingleton<IChatBotSendMessageService, ChatBotSendMessageService>();
        services.AddSingleton<IChatBotProblemAccountService, ChatBotProblemAccountService>();
        services.AddSingleton<IChatBotResultAmendmentService, ChatBotResultAmendmentService>();
        services.AddSingleton<IChatBotRebateRetentionService, ChatBotRebateRetentionService>();
    }

    private static void AddFeedbackServices(this IServiceCollection services)
    {
        services.AddSingleton<ICommonFeedback, CommonFeedback>();
        services.AddSingleton<ISettingService, SettingService>();
        services.AddSingleton<ISystemSettingService, SystemSettingService>();
        services.AddSingleton<ISendNotificationService, SendNotificationService>();
        services.AddSingleton<IMessageService, MessageService>();
        services.AddSingleton<ICustomerService, CustomerService>();
        services.AddSingleton<IFeedbackService, FeedbackService>();
        services.AddSingleton<IAutoFeedbackService, AutoFeedbackService>();
        services.AddSingleton<IWinLossFeedbackService, WinLossFeedbackService>();
        services.AddSingleton<IFeedbackTicketVoidingService, FeedbackTicketVoidingService>();
        services.AddSingleton<IFeedbackTransactionService, FeedbackTransactionService>();
        services.AddSingleton<ITOControlFeedbackService, TOControlFeedbackService>();
        services.AddSingleton<IProblemsAccountFeedbackService, ProblemsAccountFeedbackService>();
        services.AddSingleton<IAmendmentNotiFbService, AmendmentNotiFbService>();
        services.AddSingleton<ISynchronizeLicenseeService, SynchronizeLicenseeService>();
        services.AddSingleton<IVoidingSettledTicketsFeedbackService, VoidingSettledTicketsFeedbackService>();
        services.AddSingleton<IVoidingSettledTicketsService, VoidingSettledTicketsService>();
        services.AddSingleton<IArchivingDAService, ArchivingDAService>();
        services.AddSingleton<ISendAnnouncementService, SendAnnouncementService>();
        services.AddSingleton<IRefundSettledTicketService, RefundSettledTicketService>();
        services.AddSingleton<IVoidingBetIDService, VoidingBetIDService>();
        services.AddSingleton<IAbnormalBetService, AbnormalBetService>();
        services.AddSingleton<IAutoFbRebateRetentionService, AutoFbRebateRetentionService>();
        services.AddSingleton<IAutoFbMessageNotificationCenterService, AutoFbMessageNotificationCenterService>();
        services.AddSingleton<IMessageNotificationCenterService, MessageNotificationCenterService>();
        services.AddSingleton<ICaptureSettlementDataService, CaptureSettlementDataService>();
        services.AddSingleton<IGenerateResultAmendmentService, GenerateResultAmendmentService>();
        services.AddSingleton<IGenerateLapNotificationService, GenerateLapNotificationService>();
        services.AddSingleton<IPushNotificationResultAmendmentService, PushNotificationResultAmendmentService>();
        services.AddSingleton<ISendNotificationCenterService, SendNotificationCenterService>();
    }

    public static void AddWorkers(this IServiceCollection services)
    {
        services.AddHostedService<AutoFbAmendmentNotiFbWorker>();
        services.AddHostedService<AutoFbProblemAccountWorker>();
        services.AddHostedService<AutoFbVoidingAbnormalBetWorker>();
        services.AddHostedService<AutoFbWinLossLimitWorker>();
        services.AddHostedService<AutoFbTurnoverControlWorker>();
        services.AddHostedService<AutoFbVoidingBetIDWorker>();
        services.AddHostedService<AutoFbAbnormalBetsWorker>();
        services.AddHostedService<AutoFbMessageNotificationCenterWorker>();

        services.AddHostedService<AutoChatbotWorker>();
        services.AddHostedService<AutoChatbotCWLWorker>();
        services.AddHostedService<AutoChatbotCTOWorker>();
        services.AddHostedService<AutoChatbotProblemAccountWorker>();
        services.AddHostedService<AutoChatbotResultAmendmentWorker>();
        services.AddHostedService<AutoChatbotRebateRetentionWorker>();
        services.AddHostedService<ScanAndSendLetstalkNotificationWorker>();
        services.AddHostedService<ScanAndSendAnnouncementWorker>();
        services.AddHostedService<SynchronizeNewLicenseeWorker>();
        services.AddHostedService<AutoFbVoidingSettledTicketsWorker>();
        services.AddHostedService<AutoFbArchivingDAWorker>();
        services.AddHostedService<AutoChatbotArchivingDAWorker>();
        services.AddHostedService<LetstalkHealthCheckWorker>();
        services.AddHostedService<AutoFbRefundSettledTicketWorker>();
        services.AddHostedService<AutoFbRebateRetentionWorker>();

        services.AddHostedService<MncCaptureSettlementDataWorker>();
        services.AddHostedService<MncGenerateResultAmendmentWorker>();
        services.AddHostedService<MncPushNotificationResultAmendmentWorker>();
        services.AddHostedService<MncGenerateLapNotificationWorker>();
        services.AddHostedService<MncSendNotificationCenterWorker>();
    }

    public static void AddRemotingRef(this IConfiguration configuration)
    {
        RefDataTypeContainer
            .ConnectTo(new Uri(configuration["RemotingRefService:Fanex.RemotingRef.Service.Url"] ??
                               throw new MissingFieldException("Miss RemotingRefService configuration")))
            .FromApplication("CRM.BackgroundJob");
    }
}