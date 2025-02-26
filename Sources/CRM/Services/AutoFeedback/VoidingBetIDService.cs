namespace CRM.Services.AutoFeedback;

using CRM.Criteria.VoidingBetID;
using CRM.Extensions.Xml;
using CRM.Models.Common;
using CRM.Services.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IVoidingBetIDService
{
    Task<IEnumerable<MessageInfoID>> GetMessageVoidBetIDAsync(int lastMsgTargetId, int batchSize);

    Task<ValueTuple<IEnumerable<MessageInfoID>, int>> CleanMessageVoidBetIDAsync(IEnumerable<MessageInfoID> messages);

    Task<ValueTuple<CRM.Models.Common.CommandResult, ImportVoidBetIDCommand>> ImportVoidBetIDAsync(IEnumerable<MessageInfoID> messages, string environment, int maxMsgTargetId);
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

    public async Task<ValueTuple<CRM.Models.Common.CommandResult, ImportVoidBetIDCommand>> ImportVoidBetIDAsync(IEnumerable<MessageInfoID> messages, string environment, int maxMsgTargetId)
    {
        var command = new ImportVoidBetIDCommand
        {
            Messages = messages.ToXML("root").ToString(),
            Environment = environment,
            MAXMsgTargetId = maxMsgTargetId
        };
        var commandResult = await baseService.ExecuteNonQueryAsync(command);
        return new ValueTuple<CRM.Models.Common.CommandResult, ImportVoidBetIDCommand>(commandResult, command);
    }
}