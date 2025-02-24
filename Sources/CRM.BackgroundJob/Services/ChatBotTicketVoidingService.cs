namespace CRM.BackgroundJob.Services;

using Constants;
using Criteria.ChatBot;
using CRM.Extensions.Xml;
using CRM.Models;
using CRM.Services;
using Models;
using Models.ChatBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

public interface IChatBotTicketVoidingService
{
    Task<IEnumerable<ChatBotUserNameList>> GetChatBotAbnormalAccountUsernameAsync(GetChatBotAbnormalAccountGetUsernameListCriteria criteria);

    Task<IEnumerable<ChatBotVoidingHistoryCollection>> GetVoidingMessageAsync(int LastVoidingHistoryCollectionId, int BatchSize);

    Task<CommandResult> EditVoidingMessageAsync(IEnumerable<ChatBotVoidingMessage> voidingMessages);

    Task<IEnumerable<Message>> GetMessageInfoAsync(IEnumerable<VoidingMessage> voidingMessages);

    Task<ValueTuple<IEnumerable<ChatBotVoidingTicketInfo>, IEnumerable<VoidingMessage>, int, int>> GetTicketVoidingInfoAsync(
        IEnumerable<ChatBotVoidingHistoryCollection> messages,
        IEnumerable<ChatBotVoidingBetIdHistory> voidingBetIdMessages,
        string environment,
        bool IncludeUUSCurrency
    );

    Task<ValueTuple<CommandResult, ImportChatBotTicketVoidingInfoCommand>> ImportTicketVoidingAsync(
        IEnumerable<Message> messages,
        IEnumerable<ChatBotVoidingTicketInfo> ticketInfos,
        int MAXVoidingHistoryCollectionId,
        int MAXMsgTargetId,
        string Environment = BackgroundJobConstants.DefaultEnvironment);

    Task<IEnumerable<ChatBotVoidingBetIdHistory>> GetVoidingBetIdMessageAsync(int lastMsgTargetId, int batchSize);
}

public class ChatBotTicketVoidingService(IBaseService baseService) : IChatBotTicketVoidingService
{
    public async Task<IEnumerable<ChatBotUserNameList>> GetChatBotAbnormalAccountUsernameAsync(GetChatBotAbnormalAccountGetUsernameListCriteria criteria)
    {
        return await baseService.GetDataFromSiteDbAsync<ChatBotUserNameList>(criteria);
    }

    public async Task<IEnumerable<ChatBotVoidingHistoryCollection>> GetVoidingMessageAsync(int LastVoidingHistoryCollectionId, int BatchSize)
    {
        var criteria = new GetChatBotVoidingMessageCriteria
        {
            LastVoidingHistoryCollectionId = LastVoidingHistoryCollectionId,
            BatchSize = BatchSize
        };
        return await baseService.GetDataAsync<ChatBotVoidingHistoryCollection>(criteria);
    }

    public async Task<IEnumerable<ChatBotVoidingHistory>> ImportVoidingMessageHistoryCheckAsync(
        IEnumerable<ChatBotVoidingHistory> voidingMessagesHis
    )
    {
        var criteria = new GetVoidingMessageHistoryCheckCriteria
        {
            Messages = voidingMessagesHis.ToXML("root").ToString()
        };
        return await baseService.GetDataAsync<ChatBotVoidingHistory>(criteria);
    }

    public async Task<CommandResult> EditVoidingMessageAsync(IEnumerable<ChatBotVoidingMessage> voidingMessages)
    {
        var criteria = new EditChatBotVoidingMessageCommand
        {
            Messages = voidingMessages.ToXML("root").ToString()
        };
        return await baseService.ExecuteNonQueryAsync(criteria);
    }

    public async Task<IEnumerable<Message>> GetMessageInfoAsync(IEnumerable<VoidingMessage> voidingMessages)
    {
        var criteria = new GetChatBotMessageInfoCriteria
        {
            Messages = voidingMessages.ToXML("root").ToString()
        };
        var messages = await baseService.GetDataAsync<Message>(criteria);
        foreach (var message in messages)
        {
            message.Content_en = message.Content_en?.Trim().Replace("@@", string.Empty);
            message.Content_cs = message.Content_cs?.Trim().Replace("@@", string.Empty);
            message.Content_th = message.Content_th?.Trim().Replace("@@", string.Empty);
            message.Content_vn = message.Content_vn?.Trim().Replace("@@", string.Empty);
        }

        return messages;
    }

    public async Task<ValueTuple<IEnumerable<ChatBotVoidingTicketInfo>, IEnumerable<VoidingMessage>, int, int>> GetTicketVoidingInfoAsync(
        IEnumerable<ChatBotVoidingHistoryCollection> messages,
        IEnumerable<ChatBotVoidingBetIdHistory> voidingBetIdMessages,
        string environment,
        bool IncludeUUSCurrency
    )
    {
        var criteria = new GetChatBotTicketVoidingInfoCriteria
        {
            Messages = messages.ToXML("root").ToString(),
            Environment = environment,
            IncludeUUSCurrency = IncludeUUSCurrency,
            Messages_VoidByBetID = voidingBetIdMessages.ToXML("root").ToString()
        };
        var data = await baseService.FetchMultipleAsync<ChatBotVoidingTicketInfo, VoidingMessage>(criteria);
        return new ValueTuple<IEnumerable<ChatBotVoidingTicketInfo>, IEnumerable<VoidingMessage>, int, int>
            (data.Item1, data.Item2, criteria.MAXVoidingHistoryCollectionId, criteria.MAXMsgTargetId);
    }

    public async Task<ValueTuple<CommandResult, ImportChatBotTicketVoidingInfoCommand>> ImportTicketVoidingAsync(
        IEnumerable<Message> messages,
        IEnumerable<ChatBotVoidingTicketInfo> ticketInfos,
        int MAXVoidingHistoryCollectionId,
        int MAXMsgTargetId,
        string Environment = BackgroundJobConstants.DefaultEnvironment)
    {
        const string XmlTemplate = "<r VoidingHistoryCollectionId='{0}' MessageId='{1}' UsernameList='{2}' CustId='{3}' " +
                                   "Recommend='{4}' Mrecommend='{5}' Srecommend='{6}' IsTest='{7}' " +
                                   "Content_en='{8}' Content_cs='{9}' Content_th='{10}' Content_vn='{11}' Type='{12}'/>";
        var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag.ToLowerInvariant());

        foreach (var ticketInfo in ticketInfos)
        {
            var message = messages.FirstOrDefault(x => x.VoidingHistoryCollectionId == ticketInfo.VoidingHistoryCollectionId);
            stringBuilder.AppendFormat(XmlTemplate,
                ticketInfo.VoidingHistoryCollectionId,
                ticketInfo.MessageId,
                ticketInfo.UsernameList,
                ticketInfo.CustId,
                ticketInfo.Recommend,
                ticketInfo.Mrecommend,
                ticketInfo.Srecommend,
                ticketInfo.IsTest,
                HttpUtility.HtmlEncode(message?.Content_en),
                HttpUtility.HtmlEncode(message?.Content_cs),
                HttpUtility.HtmlEncode(message?.Content_th),
                HttpUtility.HtmlEncode(message?.Content_vn),
                ticketInfo.Type
            );
        }

        stringBuilder.Append(BackgroundJobConstants.EndRootTag.ToLowerInvariant());
        var criteria = new ImportChatBotTicketVoidingInfoCommand
        {
            XmlData = stringBuilder.ToString(),
            MAXVoidingHistoryCollectionId = MAXVoidingHistoryCollectionId,
            MAXMsgTargetId = MAXMsgTargetId,
            Environment = Environment
        };
        var commandResult = await baseService.ExecuteNonQueryAsync(criteria);
        var valueTuple = new ValueTuple<CommandResult, ImportChatBotTicketVoidingInfoCommand>(commandResult, criteria);
        return valueTuple;
    }

    public async Task<IEnumerable<ChatBotVoidingBetIdHistory>> GetVoidingBetIdMessageAsync(int lastMsgTargetId, int batchSize)
    {
        var criteria = new GetChatBotVoidingBetIdMessageCriteria
        {
            LastMsgTargetId = lastMsgTargetId,
            BatchSize = batchSize
        };
        return await baseService.GetDataAsync<ChatBotVoidingBetIdHistory>(criteria);
    }
}