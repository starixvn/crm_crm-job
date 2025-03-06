namespace CRM.Services.AutoFeedback;

using CRM.Criteria.AbnormalBet;
using CRM.Extensions.Xml;
using CRM.Models.AbnormalBet;
using CRM.Models.Common;
using CRM.Services.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IAbnormalBetService
{
    Task<ValueTuple<IEnumerable<VoidedInfo>, IEnumerable<VoidingMessage>, int>> GetVoidedInfoAsync(int lastVoidingHistoryCollectionId, int batchSize);

    Task<IEnumerable<MessageAbnormalBet>> GetMessageAbnormalBetAsync(IEnumerable<VoidingMessage> messages);

    Task<ValueTuple<CRM.Models.Common.CommandResult, ImportAbnormalBetCommand>> ImportAbnormalBetsAsync(
        IEnumerable<MessageAbnormalBet> messages,
        IEnumerable<VoidedInfo> voidedCollectionInfo,
        string environment, int maxVoidingHistoryCollectionId);
}

public class AbnormalBetService(IBaseService baseService) : IAbnormalBetService
{
    public async Task<ValueTuple<IEnumerable<VoidedInfo>, IEnumerable<VoidingMessage>, int>> GetVoidedInfoAsync(int lastVoidingHistoryCollectionId, int batchSize)
    {
        var criteria = new GetAbnormalBetVoidedInfoCriteria
        {
            LastVoidingHistoryCollectionId = lastVoidingHistoryCollectionId,
            BatchSize = batchSize
        };
        var data = await baseService.FetchMultipleAsync<VoidedInfo, VoidingMessage>(criteria);
        return new ValueTuple<IEnumerable<VoidedInfo>, IEnumerable<VoidingMessage>, int>(data.Item1, data.Item2, criteria.MaxVoidingHistoryCollectionId);
    }

    public async Task<IEnumerable<MessageAbnormalBet>> GetMessageAbnormalBetAsync(IEnumerable<VoidingMessage> messages)
    {
        var criteria = new GetAbnormalBetMessageCriteria
        {
            Message = messages.ToXML("root").ToString()
        };
        return await baseService.GetDataAsync<MessageAbnormalBet>(criteria);
    }

    public async Task<ValueTuple<CRM.Models.Common.CommandResult, ImportAbnormalBetCommand>> ImportAbnormalBetsAsync(
        IEnumerable<MessageAbnormalBet> messages,
        IEnumerable<VoidedInfo> voidedCollectionInfo,
        string environment, int maxVoidingHistoryCollectionId)
    {
        var command = new ImportAbnormalBetCommand
        {
            Message = messages.ToXML("root").ToString(),
            VoidedCollectionInfo = voidedCollectionInfo.ToXML("root").ToString(),
            Environment = environment,
            MaxVoidingHistoryCollectionId = maxVoidingHistoryCollectionId
        };
        var commandResult = await baseService.ExecuteNonQueryAsync(command);
        return new ValueTuple<CRM.Models.Common.CommandResult, ImportAbnormalBetCommand>(commandResult, command);
    }
}