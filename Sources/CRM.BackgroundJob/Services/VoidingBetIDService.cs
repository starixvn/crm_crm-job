namespace CRM.BackgroundJob.Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.BackgroundJob.Criteria.VoidingBetID;
using CRM.BackgroundJob.Models;
using CRM.Extensions.Xml;
using CRM.Models;
using CRM.Services;

public interface IVoidingBetIDService
{
    Task<IEnumerable<MessageInfoID>> GetMessageVoidBetIDAsync(int lastMsgTargetId, int batchSize);

    Task<ValueTuple<IEnumerable<MessageInfoID>, int>> CleanMessageVoidBetIDAsync(IEnumerable<MessageInfoID> messages);

    Task<ValueTuple<CommandResult, ImportVoidBetIDCommand>> ImportVoidBetIDAsync(IEnumerable<MessageInfoID> messages, string environment, int maxMsgTargetId);
}

public class VoidingBetIDService(IBaseService baseService) : IVoidingBetIDService
{
    public async Task<IEnumerable<MessageInfoID>> GetMessageVoidBetIDAsync(int lastMsgTargetId, int batchSize)
    {
        var criteria = new GetMessageVoidByBetIDCriteria
        {
            LastMsgTargetId = lastMsgTargetId,
            BatchSize = batchSize
        };
        return await baseService.GetDataAsync<MessageInfoID>(criteria);
    }

    public async Task<ValueTuple<IEnumerable<MessageInfoID>, int>> CleanMessageVoidBetIDAsync(IEnumerable<MessageInfoID> messages)
    {
        var criteria = new CleanMessageVoidByBetIDCriteria
        {
            Messages = messages.ToXML("root").ToString()
        };
        var cleanMessage = await baseService.GetDataAsync<MessageInfoID>(criteria);
        return new ValueTuple<IEnumerable<MessageInfoID>, int>(cleanMessage, criteria.MAXMsgTargetId);
    }

    public async Task<ValueTuple<CommandResult, ImportVoidBetIDCommand>> ImportVoidBetIDAsync(IEnumerable<MessageInfoID> messages, string environment, int maxMsgTargetId)
    {
        var command = new ImportVoidBetIDCommand
        {
            Messages = messages.ToXML("root").ToString(),
            Environment = environment,
            MAXMsgTargetId = maxMsgTargetId
        };
        var commandResult = await baseService.ExecuteNonQueryAsync(command);
        return new ValueTuple<CommandResult, ImportVoidBetIDCommand>(commandResult, command);
    }
}