namespace CRM.Services.AutoFeedback;

using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Constants;
using CRM.Criteria.AmendmentNotiFB;
using CRM.Criteria.ArchivingDA;
using CRM.Criteria.AutoFeedback;
using CRM.Criteria.ChatBot;
using CRM.Criteria.Common;
using CRM.Criteria.ProblemsAccount;
using CRM.Criteria.TurnoverControl;
using CRM.Criteria.VoidingSettledTickets;
using CRM.Criteria.WinLossLimit;
using CRM.Extensions.Xml;
using CRM.Models.AbnormalBet;
using CRM.Models.AmendmentNotiFB;
using CRM.Models.ArchivingDA;
using CRM.Models.Common;
using CRM.Models.ProblemsAccount;
using CRM.Models.WinLossLimit;
using CRM.Services.Common;

public interface ISystemSettingService
{
    Task<AbnormalBetSystemSetting> GetAbnormalBetSettingAsync(string serviceName);

    Task<CRM.Models.Common.CommandResult> EditSystemSettingAsync(
        IEnumerable<SystemSettingItem> settings,
        string serviceName = BackgroundJobConstants.ServiceName_AutoFB);

    Task<CRM.Models.Common.CommandResult> EditJobStatusAsync(string status);

    Task<AbnormalBetSystemSetting> GetChatBotSystemSettingAsync(
        string serviceName,
        string Environment = BackgroundJobConstants.DefaultEnvironment);

    Task<WinLossSystemSetting> GetWinLossSettingAsync();

    Task<WinLossSystemSetting> GetTurnoverSettingAsync();

    Task<ProblemsAccountSetting> GetProblemsAccountSettingAsync();

    Task<AmendmentNotiFbSetting> GetResultAmendmentSettingAsync();

    Task<AmendmentNotiFbSetting> GetVoidingSettledTicketsSettingAsync();

    Task<ArchivingDASetting> GetArchivingDASettingAsync();

    Task<SystemSettingInfo> GetSystemSettingAsync(string serviceName, string environment);

    Task<bool> GetSystemSettingNotificationAsync(string environment);
}

public class SystemSettingService(IBaseService baseService) : ISystemSettingService
{
    public async Task<AbnormalBetSystemSetting> GetAbnormalBetSettingAsync(string serviceName)
    {
        var criteria = new GetSystemSettingByServiceNameCriteria
        {
            ServiceName = serviceName
        };
        return await baseService.GetDataEntityAsync<AbnormalBetSystemSetting>(criteria);
    }

    public async Task<CRM.Models.Common.CommandResult> EditSystemSettingAsync(
        IEnumerable<SystemSettingItem> settings,
        string serviceName = BackgroundJobConstants.ServiceName_AutoFB)
    {
        var criteria = new EditSystemSettingCommand
        {
            Settings = settings.ToXML("ROOT").ToString(),
            ServiceName = serviceName
        };
        return await baseService.ExecuteNonQueryAsync(criteria);
    }

    public async Task<CRM.Models.Common.CommandResult> EditJobStatusAsync(string status)
    {
        var settings = new List<SystemSettingItem>
        {
            new() { SettingName = SystemSettingName.JobStatus, Value = status }
        };
        return await EditSystemSettingAsync(settings);
    }

    public async Task<AbnormalBetSystemSetting> GetChatBotSystemSettingAsync(
        string serviceName,
        string Environment = BackgroundJobConstants.DefaultEnvironment)
    {
        var criteria = new GetChatBotSystemSettingCriteria
        {
            ServiceName = serviceName,
            Environment = Environment
        };
        return await baseService.GetDataEntityAsync<AbnormalBetSystemSetting>(criteria);
    }

    public async Task<WinLossSystemSetting> GetWinLossSettingAsync()
    {
        var criteria = new GetWinLossSystemSettingCriteria();
        return await baseService.GetDataEntityAsync<WinLossSystemSetting>(criteria);
    }

    public async Task<WinLossSystemSetting> GetTurnoverSettingAsync()
    {
        var criteria = new GetTOControlSystemSettingCriteria();
        return await baseService.GetDataEntityAsync<WinLossSystemSetting>(criteria);
    }

    public async Task<ProblemsAccountSetting> GetProblemsAccountSettingAsync()
    {
        var criteria = new GetProblemsAccountSystemSettingCriteria();
        return await baseService.GetDataEntityAsync<ProblemsAccountSetting>(criteria);
    }

    public async Task<AmendmentNotiFbSetting> GetResultAmendmentSettingAsync()
    {
        var criteria = new GetResultAmendmentSystemSettingCriteria();
        return await baseService.GetDataEntityAsync<AmendmentNotiFbSetting>(criteria);
    }

    public async Task<AmendmentNotiFbSetting> GetVoidingSettledTicketsSettingAsync()
    {
        var criteria = new GetVoidingSettledTicketsSettingCriteria();
        return await baseService.GetDataEntityAsync<AmendmentNotiFbSetting>(criteria);
    }

    public async Task<ArchivingDASetting> GetArchivingDASettingAsync()
    {
        var criteria = new GetSystemSettingByServiceNameCriteria
        {
            ServiceName = BackgroundJobConstants.ServiceName_ArchivingDA
        };
        return await baseService.GetDataEntityAsync<ArchivingDASetting>(criteria);
    }

    public async Task<SystemSettingInfo> GetSystemSettingAsync(string serviceName, string environment)
    {
        var criteria = new GetSystemSettingCriteria
        {
            ServiceName = serviceName,
            Environment = environment
        };
        return await baseService.GetDataEntityAsync<SystemSettingInfo>(criteria);
    }

    public async Task<bool> GetSystemSettingNotificationAsync(string environment)
    {
        var criteria = new GetSystemSettingNotificationCriteria
        {
            Environment = environment
        };
        return await baseService.GetDataEntityAsync<bool>(criteria);
    }
}