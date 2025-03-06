namespace CRM.Services.AutoFeedback;

using CRM.Constants;
using CRM.Criteria.AutoFeedback;
using CRM.Extensions.Xml;
using CRM.Models.AbnormalBet;
using CRM.Models.Common;
using CRM.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IMessageService
{
    Task<ValueTuple<IEnumerable<VoidingMessage>, IEnumerable<VoidingHisMessage>>> GetVoidingMessageAsync(int maxVoidingHistoryCollectionId);

    Task<IEnumerable<VoidingHisMessage>> ImportVoidingMessageHistoryCheckAsync(IEnumerable<VoidingHisMessage> voidingHisMessage);

    Task<IEnumerable<VoidingMessageUserName>> VoidingHisMessageUserNameList(IEnumerable<VoidingHisMessage> messageImportCheck);

    Task<IEnumerable<VoidingMessageUserName>> ImportVoidingMessageAsync(
        IEnumerable<VoidingMessageUserName> voidingMessageUserName,
        IEnumerable<VoidingMessage> voidingMessages,
        int batchSize,
        string Environment,
        DateTime LastDateUAT);

    Task<IEnumerable<VoidingMessageUserName>> GetFailMessageAsync();

    Task<IEnumerable<Message>> GetMessageInfoAsync(IEnumerable<int> messageIds);

    Task<CRM.Models.Common.CommandResult> EditStatusVoidingMessageAsync(IEnumerable<VoidingMessageUserName> voidingMessageUserNames, string environment);
}

public class MessageService(IBaseService baseService) : IMessageService
{
    public async Task<ValueTuple<IEnumerable<VoidingMessage>, IEnumerable<VoidingHisMessage>>> GetVoidingMessageAsync(int maxVoidingHistoryCollectionId)
    {
        var criteria = new GetVoidingMessageCriteria
        {
            MaxVoidingHistoryCollectionId = maxVoidingHistoryCollectionId
        };
        return await baseService.FetchMultipleAsync<VoidingMessage, VoidingHisMessage>(criteria);
    }

    public async Task<IEnumerable<VoidingHisMessage>> ImportVoidingMessageHistoryCheckAsync(
        IEnumerable<VoidingHisMessage> voidingHisMessage
    )
    {
        var criteria = new GetVoidingMessageHistoryCheckFbCriteria
        {
            Messages = voidingHisMessage.ToXML("root").ToString()
        };
        return await baseService.GetDataAsync<VoidingHisMessage>(criteria);
    }

    public async Task<IEnumerable<VoidingMessageUserName>> VoidingHisMessageUserNameList(
        IEnumerable<VoidingHisMessage> messageImportCheck
    )
    {
        var criteria = new GetVoidingHisMessageUserNameCriteria
        {
            Messages = messageImportCheck.ToXML("root").ToString()
        };

        return await baseService.GetDataFromSiteDbAsync<VoidingMessageUserName>(criteria);
    }

    public async Task<IEnumerable<VoidingMessageUserName>> ImportVoidingMessageAsync(
        IEnumerable<VoidingMessageUserName> voidingMessageUserName,
        IEnumerable<VoidingMessage> voidingMessages,
        int batchSize,
        string Environment,
        DateTime LastDateUAT)
    {
        const string XmlTemplate = "<r VoidingHistoryCollectionId='{0}' MsgID='{1}' ReqID='{2}' UsernameList='{3}' CustIdList='{4}'/>";
        var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);
        var voidingMessageGroupBy = voidingMessageUserName.GroupBy(x => new { x.VoidingHistoryCollectionId, x.RequestId, x.MessageId }).Select(y => new VoidingMessageUserName
        {
            VoidingHistoryCollectionId = y.Key.VoidingHistoryCollectionId,
            CustIdList = string.Join(",", y.Select(u => u.CustIdList)),
            RequestId = y.Key.RequestId,
            MessageId = y.Key.MessageId,
            UsernameList = string.Join(",", y.Select(u => u.UsernameList))
        });
        foreach (var message in voidingMessages.Where(x => x.MessageId != 0))
        {
            var userName = voidingMessageGroupBy.FirstOrDefault(
                x => x.VoidingHistoryCollectionId == message.VoidingHistoryCollectionId &&
                     x.MessageId == message.MessageId && x.RequestId == message.RequestId
            );

            stringBuilder.AppendFormat(XmlTemplate,
                message.VoidingHistoryCollectionId,
                message.MessageId,
                message.RequestId,
                userName?.UsernameList,
                userName?.CustIdList);
        }
        stringBuilder.Append(BackgroundJobConstants.EndRootTag);
        var criteria = new ImportVoidingMessageCriteria
        {
            MessageIds = stringBuilder.ToString(),
            BatchSize = batchSize,
            Environment = Environment,
            LastDateUAT = LastDateUAT
        };
        return await baseService.GetDataAsync<VoidingMessageUserName>(criteria);
    }

    public async Task<IEnumerable<VoidingMessageUserName>> GetFailMessageAsync()
    {
        var criteria = new GetFailMessageCriteria();
        return await baseService.GetDataAsync<VoidingMessageUserName>(criteria);
    }

    public async Task<IEnumerable<Message>> GetMessageInfoAsync(IEnumerable<int> messageIds)
    {
        var criteria = new GetMessageInfoCriteria
        {
            MessageIds = string.Join(",", messageIds)
        };

        var messages = (await baseService.GetDataAsync<Message>(criteria)).ToList();
        foreach (var message in messages)
        {
            message.Content_en = message.Content_en?.Trim().Replace("@@", string.Empty);
            message.Content_cs = message.Content_cs?.Trim().Replace("@@", string.Empty);
        }

        return messages;
    }

    public async Task<CRM.Models.Common.CommandResult> EditStatusVoidingMessageAsync(IEnumerable<VoidingMessageUserName> voidingMessageUserNames, string environment)
    {
        const string XmlTemplate = "<r VoidingHistoryCollectionId='{0}' MsgID='{1}' ReqID='{2}' />";
        var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);
        foreach (var voidingMessageUserName in voidingMessageUserNames)
        {
            stringBuilder.AppendFormat(XmlTemplate, voidingMessageUserName.VoidingHistoryCollectionId, voidingMessageUserName.MessageId, voidingMessageUserName.RequestId);
        }
        stringBuilder.Append(BackgroundJobConstants.EndRootTag);
        var criteria = new EditStatusVoidingMessageCommand
        {
            MessageIds = stringBuilder.ToString(),
            Environment = environment
        };
        return await baseService.ExecuteNonQueryAsync(criteria);
    }
}