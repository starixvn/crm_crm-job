namespace CRM.BackgroundJob.Services;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Constants;
using CRM.BackgroundJob.Criteria.ChatBot_ResultAmendment;
using CRM.BackgroundJob.Models.AbnormalBet;
using CRM.BackgroundJob.Models.ChatBotResulAmendment;
using CRM.Extensions.Xml;
using CRM.Models;
using CRM.Services;

public interface IChatBotResultAmendmentService
{
    Task<(IEnumerable<MessageVoidSettled>, IEnumerable<MessageVoidSettled>)> GetChatBotMessageVoidSettledAsync(int lastMsgTargetId, int batchSizeGetMsg);
    Task<IEnumerable<MessageVoidSettled>> GetVoidingMessasgeCheckAsync(IEnumerable<MessageVoidSettled> messageVoidSettleds, string environment);
    Task<IEnumerable<MessageVoidSettled>> VoidingMessageGetSettledTicketAsync(IEnumerable<MessageVoidSettled> messageVoidSettleds, int siteDBGroupId);
    Task<CommandResult> ImportMessageVoidSettledAsync(IEnumerable<MessageVoidSettled> messages);
    Task<ValueTuple<IEnumerable<int>, IEnumerable<TargetMessage>, IEnumerable<TargetMessageInfo>>>
        GetChatbotMessageVoidSettledInfoAsync(IEnumerable<MessageVoidSettled> messageVoidSettleds);

    Task<ValueTuple<ValueTuple<IEnumerable<AmendmentResultTicketEvent>, IEnumerable<AmendmentResultTransaction>, IEnumerable<int>>, string>>
        GetChatBotResultAmendmentInfoAsync(AbnormalBetSystemSetting settingResultAmendment);

    Task<IEnumerable<MessageContent>> GetChatBotMessageInfoAsync(IEnumerable<int> messageIds);

    Task<ValueTuple<CommandResult, ImportChatBotResultAmendmentCommand>> ImportChatBotAmendmentResultAsync(
        IEnumerable<AmendmentResultTicketEvent> ticketEvents,
        IEnumerable<AmendmentResultTransaction> transactions,
        IEnumerable<MessageContent> messages,
        IEnumerable<TargetMessage> targetMessages,
        IEnumerable<TargetMessageInfo> targetMessagesInfo,
        string Environment,
        string Servername
    );
}

public class ChatBotResultAmendmentService(IBaseService baseService, ICommonFeedback commonFeedback)
    : IChatBotResultAmendmentService
{
    private const int EventType5 = 5;

    public async Task<(IEnumerable<MessageVoidSettled>, IEnumerable<MessageVoidSettled>)> GetChatBotMessageVoidSettledAsync(int lastMsgTargetId, int batchSizeGetMsg)
    {
        var criteria = new GetMesssagesVoidSettledCriteria
        {
            LastMsgTargetId = lastMsgTargetId,
            BatchSizeGetMsg = batchSizeGetMsg
        };
        return await baseService.FetchMultipleAsync<MessageVoidSettled, MessageVoidSettled>(criteria);
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

    public async Task<IEnumerable<MessageVoidSettled>> VoidingMessageGetSettledTicketAsync(IEnumerable<MessageVoidSettled> messageVoidSettleds, int siteDBGroupId)
    {
        var criteria = new VoidingMessageGetSettledTicket
        {
            Message = messageVoidSettleds.ToXML("root").ToString(),
            SiteDBGroupId = siteDBGroupId
        };
        return await baseService.GetDataAsync<MessageVoidSettled>(criteria);
    }

    public async Task<CommandResult> ImportMessageVoidSettledAsync(IEnumerable<MessageVoidSettled> messages)
    {
        var criteria = new ImportMessageVoidSettledCommand
        {
            Message = messages.ToXML("root").ToString()
        };
        return await baseService.ExecuteNonQueryAsync(criteria);
    }

    public async Task<ValueTuple<IEnumerable<int>, IEnumerable<TargetMessage>, IEnumerable<TargetMessageInfo>>> GetChatbotMessageVoidSettledInfoAsync(
        IEnumerable<MessageVoidSettled> messageVoidSettleds)
    {
        var criteria = new GetChatbotMessageVoidSettledInfo
        {
            Message = messageVoidSettleds.ToXML("root").ToString()
        };
        return await baseService.FetchMultipleAsync<int, TargetMessage, TargetMessageInfo>(criteria);
    }

    public async Task<ValueTuple<ValueTuple<IEnumerable<AmendmentResultTicketEvent>, IEnumerable<AmendmentResultTransaction>, IEnumerable<int>>, string>> 
        GetChatBotResultAmendmentInfoAsync(AbnormalBetSystemSetting settingResultAmendment)
    {
        var criteria = new GetChatBotResultAmendmentCriteria
        {
            BatchSize = settingResultAmendment.BatchSize,
            SubmitDate = settingResultAmendment.SubmitDate.ToString("yyyy-MM-dd H:mm:ss.fffffff"),
            Seq = settingResultAmendment.Seq
        };
        var result =  await baseService.FetchMultipleAsync<AmendmentResultTicketEvent, AmendmentResultTransaction, int>(criteria);
        return new ValueTuple<ValueTuple<IEnumerable<AmendmentResultTicketEvent>, IEnumerable<AmendmentResultTransaction>, IEnumerable<int>>, string>(result, criteria.ServerName);
    }

    public async Task<IEnumerable<MessageContent>> GetChatBotMessageInfoAsync(IEnumerable<int> messageIds)
    {
        var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag.ToLowerInvariant());
        const string Xml = "<r MessageId='{0}'/>";
        foreach (var messageId in messageIds)
        {
            stringBuilder.AppendFormat(Xml, messageId);
        }
        stringBuilder.Append(BackgroundJobConstants.EndRootTag.ToLowerInvariant());
        var criteria = new GetChatBotMsgCriteria
        {
            Messages = stringBuilder.ToString()
        };
        return await baseService.GetDataAsync<MessageContent>(criteria);
    }

    public async Task<ValueTuple<CommandResult, ImportChatBotResultAmendmentCommand>> ImportChatBotAmendmentResultAsync(
        IEnumerable<AmendmentResultTicketEvent> ticketEvents,
        IEnumerable<AmendmentResultTransaction> transactions,
        IEnumerable<MessageContent> messages,
        IEnumerable<TargetMessage> targetMessages,
        IEnumerable<TargetMessageInfo> targetMessagesInfo,
        string Environment,
        string Servername
    )
    {
        var stringBuilderEventData = new StringBuilder(BackgroundJobConstants.BeginRootTag.ToLowerInvariant());
        var stringBuilderMessage = new StringBuilder(BackgroundJobConstants.BeginRootTag.ToLowerInvariant());

        const string XmlTemplate = "<r TicketEventId='{0}' ProductId='{1}' EventDate='{2}' " +
                                   "SubmitDate='{3}' EventType='{4}' ActualResult='{5}' WrongResult='{6}' MessageId='{7}' " +
                                   "SportNameEN='{8}' SportNameSC='{9}' SportNameTH='{10}' SportNameYN='{11}' " +
                                   "HomeNameEN='{12}' HomeNameSC='{13}' HomeNameTH='{14}' HomeNameYN='{15}' " +
                                   "AwayNameEN='{16}' AwayNameSC='{17}' AwayNameTH='{18}' AwayNameYN='{19}' " +
                                   "LeagueNameEN='{20}' LeagueNameSC='{21}' LeagueNameTH='{22}' LeagueNameYN='{23}' MatchCode='{24}' " +
                                   "MatchInfoEN='{25}' MatchInfoSC='{26}' MatchInfoTH='{27}' MatchInfoYN='{28}' Type='{29}' LeagueGroupID='{30}'/>";

        foreach (var transaction in transactions)
        {
            var getTeamNameFunc = transaction.EventType == EventType5
                ? commonFeedback.GetTeamName3rd
                : new Func<string, string, string>(commonFeedback.GetTeamName);

            var getLeagueNameFunc = transaction.EventType == EventType5
                ? commonFeedback.GetLeagueName3rd
                : new Func<string, string, string>(commonFeedback.GetLeagueName);

            var getMatchCodeFunc = transaction.EventType == EventType5
                ? commonFeedback.GetMatchCodeVirtualSportFormat
                : new Func<string, string, string>(commonFeedback.GetMatchCodeFormat);

            var sportNameEN = commonFeedback.GetSportName(transaction.Sporttype.ToString(), Language.en_US);
            var sportNameSC = commonFeedback.GetSportName(transaction.Sporttype.ToString(), Language.zh_CN);
            var sportNameTH = commonFeedback.GetSportName(transaction.Sporttype.ToString(), Language.th_TH);
            var sportNameYN = commonFeedback.GetSportName(transaction.Sporttype.ToString(), Language.vi_VN);

            var leagueNameEN = HttpUtility.HtmlEncode(getLeagueNameFunc(transaction.LeagueId.ToString(), Language.en_US));
            var leagueNameSC = HttpUtility.HtmlEncode(getLeagueNameFunc(transaction.LeagueId.ToString(), Language.zh_CN));
            var leagueNameTH = HttpUtility.HtmlEncode(getLeagueNameFunc(transaction.LeagueId.ToString(), Language.th_TH));
            var leagueNameYN = HttpUtility.HtmlEncode(getLeagueNameFunc(transaction.LeagueId.ToString(), Language.vi_VN));

            var homeNameEN = HttpUtility.HtmlEncode(getTeamNameFunc(transaction.HomeId.ToString(), Language.en_US));
            var homeNameSC = HttpUtility.HtmlEncode(getTeamNameFunc(transaction.HomeId.ToString(), Language.zh_CN));
            var homeNameTH = HttpUtility.HtmlEncode(getTeamNameFunc(transaction.HomeId.ToString(), Language.th_TH));
            var homeNameYN = HttpUtility.HtmlEncode(getTeamNameFunc(transaction.HomeId.ToString(), Language.vi_VN));

            var awayNameEN = HttpUtility.HtmlEncode(getTeamNameFunc(transaction.AwayId.ToString(), Language.en_US));
            var awayNameSC = HttpUtility.HtmlEncode(getTeamNameFunc(transaction.AwayId.ToString(), Language.zh_CN));
            var awayNameTH = HttpUtility.HtmlEncode(getTeamNameFunc(transaction.AwayId.ToString(), Language.th_TH));
            var awayNameYN = HttpUtility.HtmlEncode(getTeamNameFunc(transaction.AwayId.ToString(), Language.vi_VN));

            var matchInfoEN = HttpUtility.HtmlEncode(getMatchCodeFunc(transaction.MatchCode, Language.en_US));
            var matchInfoSC = HttpUtility.HtmlEncode(getMatchCodeFunc(transaction.MatchCode, Language.zh_CN));
            var matchInfoTH = HttpUtility.HtmlEncode(getMatchCodeFunc(transaction.MatchCode, Language.th_TH));
            var matchInfoYN = HttpUtility.HtmlEncode(getMatchCodeFunc(transaction.MatchCode, Language.vi_VN));

            stringBuilderEventData.AppendFormat(XmlTemplate,
                transaction.TicketEventId, transaction.ProductId, transaction.EventDate,
                transaction.SubmitDate.ToString("yyyy-MM-dd H:mm:ss.fffffff"), transaction.EventType,
                HttpUtility.HtmlEncode(transaction.ActualResult), HttpUtility.HtmlEncode(transaction.WrongResult),
                transaction.MessageId, sportNameEN, sportNameSC, sportNameTH, sportNameYN,
                homeNameEN, homeNameSC, homeNameTH, homeNameYN,
                awayNameEN, awayNameSC, awayNameTH, awayNameYN,
                leagueNameEN, leagueNameSC, leagueNameTH, leagueNameYN, transaction.MatchCode,
                matchInfoEN, matchInfoSC, matchInfoTH, matchInfoYN, transaction.Type, transaction.LeagueGroupID
            );
        }
        stringBuilderEventData.Append(BackgroundJobConstants.EndRootTag.ToLowerInvariant());

        const string xmlTemplateMessage = "<r MessageId='{0}' Content_en='{1}' Content_cs='{2}' Content_th='{3}' Content_vn='{4}'/>";

        foreach (var message in messages)
        {
            stringBuilderMessage.AppendFormat(
                xmlTemplateMessage,
                message.MessageId,
                HttpUtility.HtmlEncode(message.Content_en?.Trim().Replace("@@", string.Empty)),
                HttpUtility.HtmlEncode(message.Content_cs?.Trim().Replace("@@", string.Empty)),
                HttpUtility.HtmlEncode(message.Content_th?.Trim().Replace("@@", string.Empty)),
                HttpUtility.HtmlEncode(message.Content_vn?.Trim().Replace("@@", string.Empty))
            );
        }

        stringBuilderMessage.Append(BackgroundJobConstants.EndRootTag.ToLowerInvariant());

        var criteria = new ImportChatBotResultAmendmentCommand
        {
            TicketData = ticketEvents.ToXML("root").ToString(),
            EventData = stringBuilderEventData.ToString(),
            MessageData = stringBuilderMessage.ToString(),
            TargetMessage = targetMessages.ToXML("root").ToString(),
            InfoMessage = targetMessagesInfo.ToXML("root").ToString(),
            Environment = Environment,
            Servername = Servername
        };
        var commandResult = await baseService.ExecuteNonQueryAsync(criteria);
        return new ValueTuple<CommandResult, ImportChatBotResultAmendmentCommand>(commandResult, criteria);
    }
}