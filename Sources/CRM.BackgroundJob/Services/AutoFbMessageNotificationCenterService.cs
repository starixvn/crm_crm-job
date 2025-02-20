namespace CRM.BackgroundJob.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.BackgroundJob.Criteria.AutoFbMessageNotificationCenter;
using CRM.BackgroundJob.Models;
using CRM.Extensions.Xml;
using CRM.Models;
using CRM.Services;

public interface IAutoFbMessageNotificationCenterService
{
    Task<IEnumerable<MessageInfoID>> GetMsgInfoMessageNotificationAsync(int lastMsgTargetId, int batchSize);

    Task<(CommandResult commandResult, ImportMessageNotificationCenterCommand command)> ImportMessageNotificationAsync(IEnumerable<MessageInfoID> messages, string environment);
}

public class AutoFbMessageNotificationCenterService(IBaseService baseService) : IAutoFbMessageNotificationCenterService
{
    public async Task<IEnumerable<MessageInfoID>> GetMsgInfoMessageNotificationAsync(int lastMsgTargetId, int batchSize)
    {
        var criteria = new GetMsgInfoMessageNotificationCriteria
        {
            LastMsgTargetId = lastMsgTargetId,
            BatchSize = batchSize
        };
        return await baseService.GetDataAsync<MessageInfoID>(criteria);
    }

    public async Task<(CommandResult commandResult, ImportMessageNotificationCenterCommand command)> ImportMessageNotificationAsync(IEnumerable<MessageInfoID> messages, string environment)
    {
        var command = new ImportMessageNotificationCenterCommand
        {
            MsgInfo = messages.ToXML("root").ToString(),
            Environment = environment
        };
        var commandResult = await baseService.ExecuteNonQueryAsync(command);
        return (commandResult, command);
    }
}