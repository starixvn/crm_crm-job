namespace CRM.Services.AutoFeedback;

using CRM.Criteria.RefundSettledTicket;
using CRM.Extensions.Xml;
using CRM.Models.ChatBotResulAmendment;
using CRM.Models.RefundSettledTicket;
using CRM.Services.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IRefundSettledTicketService
{
    Task<ValueTuple<IEnumerable<MessageRefundSettledTicket>, IEnumerable<MessageVoidSettled>>> GetMessageRefundSettledTicketAsync(int lastMsgTargetId, int batchSize);

    Task<ValueTuple<CRM.Models.Common.CommandResult, ImportRefundSettledTicketCommand>> ImportRefundSettledTicketAsync(
        IEnumerable<MessageRefundSettledTicket> messageContent,
        IEnumerable<MessageVoidSettled> message, string environment);
}

public class RefundSettledTicketService(IBaseService baseService) : IRefundSettledTicketService
{
    public async Task<ValueTuple<IEnumerable<MessageRefundSettledTicket>, IEnumerable<MessageVoidSettled>>> GetMessageRefundSettledTicketAsync(int lastMsgTargetId, int batchSize)
    {
        var criteria = new GetMessageRefundSettledTicketCriteria
        {
            LastMsgTargetId = lastMsgTargetId,
            BatchSize = batchSize
        };
        return await baseService.FetchMultipleAsync<MessageRefundSettledTicket, MessageVoidSettled>(criteria);
    }

    public async Task<ValueTuple<CRM.Models.Common.CommandResult, ImportRefundSettledTicketCommand>> ImportRefundSettledTicketAsync
        (IEnumerable<MessageRefundSettledTicket> messageContent, IEnumerable<MessageVoidSettled> message, string environment)
    {
        var command = new ImportRefundSettledTicketCommand
        {
            Message = message.ToXML("root").ToString(),
            MessageContent = messageContent.ToXML("root").ToString(),
            Environment = environment
        };
        var commandResult = await baseService.ExecuteNonQueryAsync(command);
        return new ValueTuple<CRM.Models.Common.CommandResult, ImportRefundSettledTicketCommand>(commandResult, command);
    }
}