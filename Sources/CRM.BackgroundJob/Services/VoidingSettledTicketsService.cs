namespace CRM.BackgroundJob.Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.BackgroundJob.Criteria.ChatBot_ResultAmendment;
using CRM.BackgroundJob.Criteria.VoidingSettledTickets;
using CRM.BackgroundJob.Models.ChatBotResulAmendment;
using CRM.BackgroundJob.Models.AmendmentNotiFB;
using CRM.BackgroundJob.Models.VoidingSettledTickets;
using CRM.Extensions.Xml;
using CRM.Models;
using CRM.Services;

public interface IVoidingSettledTicketsService
{
    Task<IEnumerable<MessageVoidSettled>> GetMessageVoidSettledAsync();
    Task<IEnumerable<MessageVoidSettled>> GetAutoVoidingMessageAsync(AmendmentNotiFbSetting voidingSettledTicketSetting, string environment);
    Task<ValueTuple<IEnumerable<VoidingTicketEventInfo>, IEnumerable<MsgTarget>>> GetAutoVoidingTicketAsync(
        AmendmentNotiFbSetting voidingSettledTicketSetting, List<MessageVoidSettled> voidSettleds, string environment
    );
    Task<IEnumerable<MessageVoidSettled>> GetFailVoidingMessageAsync();
    Task<IEnumerable<MessageVoidSettled>> GetVoidingMessasgeCheckAsync(IEnumerable<MessageVoidSettled> messageVoidSettleds, string environment);
    Task<IEnumerable<MessageVoidSettled>> VoidingMessageGetSettledTicketAsync(IEnumerable<MessageVoidSettled> messageVoidSettleds);
    Task<ValueTuple<IEnumerable<VoidingAutoFeedbackInfo>, IEnumerable<AutoVoidingEventDetails>>> GetVoidingAutoFeedbackInfoAsync(int batchSize);
    Task<CommandResult> ImportMessageVoidSettledAsync(IEnumerable<MessageVoidSettled> messages);
    Task<CommandResult> UpdateVoidingAutoFeedbackInfoBatchAsync(int logVoidingID, string status, string description, string environment);
    Task<CommandResult> UpdateAutoVoidingMessageAsync(IEnumerable<MsgTarget> messages, string environment);
    Task<CommandResult> ImportVoidingAutoFeedbackInfoAsync(IEnumerable<VoidingTicketEventInfo> ticketEventInfos, string environment);
}
public class VoidingSettledTicketsService(IBaseService baseService, ICrmLogService logService)
    : IVoidingSettledTicketsService
{
    public async Task<IEnumerable<MessageVoidSettled>> GetMessageVoidSettledAsync()
    {
        var criteria = new GetMesssagesVoidingSettledCriteria();
        return await baseService.GetDataAsync<MessageVoidSettled>(criteria);
    }

    public async Task<IEnumerable<MessageVoidSettled>> GetAutoVoidingMessageAsync(AmendmentNotiFbSetting voidingSettledTicketSetting, string environment)
    {
        var criteria = new GetAutoVoidingMessageCriteria
        {
            Environment = environment,
            BatchSize = voidingSettledTicketSetting.BatchSize,
            LastDateUAT = voidingSettledTicketSetting.LastDateUAT
        };

        return await baseService.GetDataAsync<MessageVoidSettled>(criteria);
    }

    public async Task<ValueTuple<IEnumerable<VoidingTicketEventInfo>, IEnumerable<MsgTarget>>> GetAutoVoidingTicketAsync(
        AmendmentNotiFbSetting voidingSettledTicketSetting, List<MessageVoidSettled> voidSettleds, string environment
    )
    {
        var criteria = new GetAutoVoidingTicketCriteria
        {
            MessageIDs = voidSettleds.ToXML("ROOT").ToString(),
            Environment = environment,
            ConfigUAT = voidingSettledTicketSetting.ConfigUAT,
            ConfigPRO = voidingSettledTicketSetting.ConfigPRO
        };
        logService.LogInformation($"Acc_CRM_AutoVoidingTicketSel Criteria: {criteria.MessageIDs}");

        return await baseService.FetchMultipleAsync<VoidingTicketEventInfo, MsgTarget>(criteria);
    }

    public async Task<IEnumerable<MessageVoidSettled>> GetFailVoidingMessageAsync()
    {
        var criteria = new GetFailVoidingMessageCriteria();

        return await baseService.GetDataAsync<MessageVoidSettled>(criteria);
    }

    public async Task<IEnumerable<MessageVoidSettled>> GetVoidingMessasgeCheckAsync(IEnumerable<MessageVoidSettled> messageVoidSettleds, string environment)
    {
        var criteria = new GetChatbotVoidingMessageCheck
        {
            Message = messageVoidSettleds.ToXML("root").ToString(),
            Environment = environment
        };
        return await baseService.GetDataAsync<MessageVoidSettled>(criteria);
    }

    public async Task<IEnumerable<MessageVoidSettled>> VoidingMessageGetSettledTicketAsync(IEnumerable<MessageVoidSettled> messageVoidSettleds)
    {
        var criteria = new VoidingGetSettledTicketMessageCriteria
        {
            Message = messageVoidSettleds.ToXML("root").ToString()
        };
        return await baseService.GetDataAsync<MessageVoidSettled>(criteria);
    }

    public async Task<ValueTuple<IEnumerable<VoidingAutoFeedbackInfo>, IEnumerable<AutoVoidingEventDetails>>> GetVoidingAutoFeedbackInfoAsync(int batchSize)
    {
        var criteria = new GetVoidingAutoFeedbackInfoCriteria
        {
            BatchSize = batchSize
        };

        return await baseService.FetchMultipleAsync<VoidingAutoFeedbackInfo, AutoVoidingEventDetails>(criteria);
    }

    public async Task<CommandResult> ImportMessageVoidSettledAsync(IEnumerable<MessageVoidSettled> messages)
    {
        var criteria = new ImportMessageVoidSettledCommand
        {
            Message = messages.ToXML("root").ToString()
        };
        return await baseService.ExecuteNonQueryAsync(criteria);
    }

    public async Task<CommandResult> UpdateAutoVoidingMessageAsync(IEnumerable<MsgTarget> messages, string environment)
    {
        var criteria = new AutoVoidingMessageUpdCommand
        {
            MessageIds = messages.ToXML("ROOT").ToString(),
            Environment = environment
        };
        return await baseService.ExecuteNonQueryAsync(criteria);
    }
    public async Task<CommandResult> UpdateVoidingAutoFeedbackInfoBatchAsync(int logVoidingID, string status,string description, string environment)
    {
        var criteria = new VoidingAutoFeedbackInfoBatchUpdCommand
        {
            LogVoidingID = logVoidingID,
            Description = description,
            Status = status,
            Environment = environment
        };
        return await baseService.ExecuteNonQueryAsync(criteria);
    }
        

    public async Task<CommandResult>ImportVoidingAutoFeedbackInfoAsync(IEnumerable<VoidingTicketEventInfo> ticketEventInfos, string environment)
    {
        var criteria = new VoidingAutoFeedbackInfoInsCommand
        {
            Message = ticketEventInfos.ToXML("ROOT").ToString(),
            Environment = environment
        };
        return await baseService.ExecuteNonQueryAsync(criteria);
    }
}