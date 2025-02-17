namespace CRM.BackgroundJob.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CRM.BackgroundJob.Criteria.AbnormalBet;
    using CRM.BackgroundJob.Models;
    using CRM.BackgroundJob.Models.AbnormalBet;
    using CRM.BackgroundJob.Core.Extensions.Xml;
    using CRM.BackgroundJob.Core.Models;
    using CRM.BackgroundJob.Core.Services;

    public interface IAbnormalBetService
    {
        Task<ValueTuple<IEnumerable<VoidedInfo>, IEnumerable<VoidingMessage>, int>> GetVoidedInfoAsync(int lastVoidingHistoryCollectionId, int batchSize);
        Task<IEnumerable<MessageAbnormelBet>> GetMessageAbnormalBetAsync(IEnumerable<VoidingMessage> messages);
        Task<ValueTuple<CommandResult, ImportAbnormalBetCommand>> ImportAbnormalBetsAsync(
            IEnumerable<MessageAbnormelBet> messages, 
            IEnumerable<VoidedInfo> voidedCollectionInfo, 
            string environment, int maxVoidingHistoryCollectionId);
    }

    public class AbnormalBetService : IAbnormalBetService
    {
        private readonly IBaseService baseService;

        public AbnormalBetService(IBaseService baseService)
        {
            this.baseService = baseService;
        }

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

        public async Task<IEnumerable<MessageAbnormelBet>> GetMessageAbnormalBetAsync(IEnumerable<VoidingMessage> messages)
        {
            var criteria = new GetAbnormalBetMessageCriteria
            {
                Message = messages.ToXML("root").ToString()
            };
            return await baseService.GetDataAsync<MessageAbnormelBet>(criteria);
        }

        public async Task<ValueTuple<CommandResult, ImportAbnormalBetCommand>> ImportAbnormalBetsAsync(
            IEnumerable<MessageAbnormelBet> messages, 
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
            return new ValueTuple<CommandResult, ImportAbnormalBetCommand>(commandResult, command);
        }
    }
    
}
