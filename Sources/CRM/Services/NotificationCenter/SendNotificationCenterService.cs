namespace CRM.Services.NotificationCenter;

using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Criteria.SendNotificationCenter;
using CRM.Models.MessageNotificationCenter;
using CRM.Services.Common;
using Models.Common;

public interface ISendNotificationCenterService
{
    Task<CommandResult> InsertNotificationAlertAsync(string environment);

    Task<IEnumerable<MessageBotInfo>> GetNotificationAlertAsync(string environment);
}

public class SendNotificationCenterService(IBaseService baseService) : ISendNotificationCenterService
{
    public async Task<CommandResult> InsertNotificationAlertAsync(string environment)
    {
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