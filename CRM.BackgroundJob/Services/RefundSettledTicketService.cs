namespace CRM.BackgroundJob.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CRM.BackgroundJob.Criteria.RefundSettledTicket;
    using CRM.BackgroundJob.Models.ChatBotResulAmendment;
    using CRM.BackgroundJob.Models.RefundSettledTicket;
    using CRM.BackgroundJob.Core.Extensions.Xml;
    using CRM.BackgroundJob.Core.Models;
    using CRM.BackgroundJob.Core.Services;

    public interface IRefundSettledTicketService
    {
        Task<ValueTuple<IEnumerable<MessageRefundSettledTicket>, IEnumerable<MessageVoidSettled>>> GetMessageRefundSettledTicketAsync(int lastMsgTargetId, int batchSize);

        Task<ValueTuple<CommandResult, ImportRefundSettledTicketCommand>> ImportRefundSettledTicketAsync(
            IEnumerable<MessageRefundSettledTicket> messageContent, 
            IEnumerable<MessageVoidSettled> message, string environment);
    }

    public class RefundSettledTicketService : IRefundSettledTicketService
    {
        private readonly IBaseService baseService;

        public RefundSettledTicketService(IBaseService baseService)
        {
            this.baseService = baseService;
        }

        public async Task<ValueTuple<IEnumerable<MessageRefundSettledTicket>, IEnumerable<MessageVoidSettled>>> GetMessageRefundSettledTicketAsync(int lastMsgTargetId, int batchSize)
        {
            var criteria = new GetMessageRefundSettledTicketCriteria
            {
                LastMsgTargetId = lastMsgTargetId,
                BatchSize = batchSize
            };
            return await baseService.FetchMultipleAsync<MessageRefundSettledTicket, MessageVoidSettled>(criteria);
        }

        public async Task<ValueTuple<CommandResult, ImportRefundSettledTicketCommand>> ImportRefundSettledTicketAsync
            (IEnumerable<MessageRefundSettledTicket> messageContent, IEnumerable<MessageVoidSettled> message, string environment)
        {
            var command = new ImportRefundSettledTicketCommand
            {
                Message = message.ToXML("root").ToString(),
                MessageContent = messageContent.ToXML("root").ToString(),
                Environment = environment
            };
            var commandResult = await baseService.ExecuteNonQueryAsync(command);
            return new ValueTuple<CommandResult, ImportRefundSettledTicketCommand>(commandResult, command);
        }
    }
}
