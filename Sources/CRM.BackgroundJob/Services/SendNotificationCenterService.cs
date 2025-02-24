namespace CRM.BackgroundJob.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.BackgroundJob.Criteria.SendNotificationCenter;
using CRM.BackgroundJob.Models.MessageNotificationCenter;
using CRM.Models;
using CRM.Services;

public interface ISendNotificationCenterService
{
    Task<CommandResult> InsertNotificationAlertAsync(string environment);

    Task<IEnumerable<MessageBotInfo>> GetNotificationAlertAsync(string environment);
}

public class SendNotificationCenterService(IBaseService baseService) : ISendNotificationCenterService
{
    public async Task<CommandResult> InsertNotificationAlertAsync(string environment) {
        var command = new InsertNotificationAlertCommand
        {
            Environment = environment
        };
        return await baseService.ExecuteNonQueryAsync(command);
    }

    public async Task<IEnumerable<MessageBotInfo>> GetNotificationAlertAsync(string environment)
    {
        var criteria = new GetNotificationAlertCriteria
        {
            Environment = environment
        };
        return await baseService.GetDataAsync<MessageBotInfo>(criteria);
    }
}