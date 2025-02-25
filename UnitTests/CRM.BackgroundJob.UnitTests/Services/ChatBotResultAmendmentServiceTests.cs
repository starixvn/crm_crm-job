namespace CRM.BackgroundJob.UnitTests.Services
{
    using AutoFixture;
    using CRM.BackgroundJob.Constants;
    using CRM.BackgroundJob.Criteria.ChatBot_ResultAmendment;
    using CRM.BackgroundJob.Models.AbnormalBet;
    using CRM.BackgroundJob.Models.ChatBotResulAmendment;
    using CRM.BackgroundJob.Services;
    using CRM.Extensions.Xml;
    using CRM.Models;
    using CRM.Services;
    using NSubstitute;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;

    public class ChatBotResultAmendmentServiceTests
    {
        private IBaseService baseService;
        private ICommonFeedback commonFeedback;

        [Fact]
        public async Task GetChatBotMessageVoidSettledAsync_WhenCalled_ReturnExpectedMessages()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture.Create<(IEnumerable<MessageVoidSettled>, IEnumerable<MessageVoidSettled>)>();
            var service = GetService();
            baseService.FetchMultipleAsync<MessageVoidSettled, MessageVoidSettled>(Arg.Any<GetMessagesVoidSettledCriteria>()).Returns(expected);

            // Act
            var actual = await service.GetChatBotMessageVoidSettledAsync(1, 1);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task VoidingMessageGetSettledTicketAsync_WhenCalled_ReturnExpectedMessages()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture.Create<IEnumerable<MessageVoidSettled>>();
            var messageVoidSettles = fixture.Create<IEnumerable<MessageVoidSettled>>();
            var service = GetService();
            baseService.GetDataAsync<MessageVoidSettled>(Arg.Any<VoidingMessageGetSettledTicket>()).Returns(expected);

            // Act
            var actual = await service.VoidingMessageGetSettledTicketAsync(messageVoidSettles, 1);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task GetVoidingMessageCheckAsync_WhenCalled_ReturnExpectedMessages()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture.Create<IEnumerable<MessageVoidSettled>>();
            var messageVoidSettles = fixture.Create<IEnumerable<MessageVoidSettled>>();
            var service = GetService();
            baseService.GetDataAsync<MessageVoidSettled>(Arg.Any<GetChatbotVoidingMessageCheck>()).Returns(expected);

            // Act
            var actual = await service.GetVoidingMessageCheckAsync(messageVoidSettles, "LIVE");

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ImportMessageVoidSettledAsync_WhenCalled_ReturnExpectedCommandResult()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture.Create<CommandResult>();
            var messages = fixture.Create<IEnumerable<MessageVoidSettled>>();
            var service = GetService();
            var criteria = new ImportMessageVoidSettledCommand();
            baseService.ExecuteNonQueryAsync(Arg.Do<ImportMessageVoidSettledCommand>(x => criteria = x)).Returns(expected);

            // Act
            var actual = await service.ImportMessageVoidSettledAsync(messages);

            // Assert
            Assert.Equal(expected, actual);
            Assert.Equal(criteria.Message, messages.ToXML("root").ToString());
        }

        [Fact]
        public async Task GetChatbotMessageVoidSettledInfoAsync_WhenCalled_ReturnExpectedData()
        {
            // Arrange
            var fixture = new Fixture();
            fixture.Create<AbnormalBetSystemSetting>();
            var targetMessages = fixture.Create<IEnumerable<TargetMessage>>();
            var targetMessageInfos = fixture.Create<IEnumerable<TargetMessageInfo>>();
            var messageVoidSettles = fixture.Create<IEnumerable<MessageVoidSettled>>();
            var messageIds = fixture.Create<IEnumerable<int>>();
            var expected = new ValueTuple<IEnumerable<int>, IEnumerable<TargetMessage>, IEnumerable<TargetMessageInfo>>(messageIds, targetMessages, targetMessageInfos);
            var service = GetService();
            baseService.FetchMultipleAsync<int, TargetMessage, TargetMessageInfo>(Arg.Any<GetChatbotMessageVoidSettledInfo>()).Returns(expected);

            // Act
            var actual = await service.GetChatbotMessageVoidSettledInfoAsync(messageVoidSettles);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task GetChatBotResultAmendmentInfoAsync_WhenCalled_ReturnExpectedData()
        {
            // Arrange
            var fixture = new Fixture();
            var settingResultAmendment = fixture.Create<AbnormalBetSystemSetting>();
            var ticketEvents = fixture.Create<IEnumerable<AmendmentResultTicketEvent>>();
            var ticketTransactions = fixture.Create<IEnumerable<AmendmentResultTransaction>>();
            var messageIds = fixture.Create<IEnumerable<int>>();
            fixture.Create<GetChatBotResultAmendmentCriteria>();
            var expected = new ValueTuple<IEnumerable<AmendmentResultTicketEvent>, IEnumerable<AmendmentResultTransaction>, IEnumerable<int>>(ticketEvents, ticketTransactions, messageIds);
            var service = GetService();
            baseService.FetchMultipleAsync<AmendmentResultTicketEvent, AmendmentResultTransaction, int>(Arg.Do<GetChatBotResultAmendmentCriteria>(x => _ = x)).Returns(expected);

            // Act
            var actual = await service.GetChatBotResultAmendmentInfoAsync(settingResultAmendment);

            // Assert
            Assert.Equal(expected, actual.Item1);
        }

        [Fact]
        public async Task GetChatBotMessageInfoAsync_WhenCalled_ReturnExpectedData()
        {
            // Arrange
            var fixture = new Fixture();
            var service = GetService();
            var criteria = new GetChatBotMsgCriteria();
            var expected = fixture.Create<IEnumerable<MessageContent>>();
            baseService.GetDataAsync<MessageContent>(Arg.Do<GetChatBotMsgCriteria>(x => criteria = x)).Returns(expected);
            const string Xml = "<root><r MessageId='1'/></root>";
            var messages = new List<int> { 1 };

            // Act
            var actual = await service.GetChatBotMessageInfoAsync(messages);

            // Assert
            Assert.Equal(expected, actual);
            Assert.Equal(Xml, criteria.Messages);
        }

        [Fact]
        public async Task ImportChatBotAmendmentResultAsync_WhenCalled_ReturnExpectedCommandResult()
        {
            // Arrange
            var fixture = new Fixture();
            var service = GetService();
            var ticketEvents = fixture.Create<IEnumerable<AmendmentResultTicketEvent>>();
            var transaction = new AmendmentResultTransaction
            {
                TicketEventId = 1,
                ProductId = 1,
                EventDate = new DateTime(),
                SubmitDate = new DateTime(),
                EventType = 2,
                ActualResult = "WrongResult",
                WrongResult = "WrongResult",
                MessageId = 1,
                SportType = 1,
                HomeId = 1,
                AwayId = 2,
                LeagueId = 1,
                MatchCode = "1",
                MatchId = 1,
                Type = 1,
                LeagueGroupID = 1
            };
            var transactions = new List<AmendmentResultTransaction> { transaction };

            var messages = new List<MessageContent> { new()
            {
                MessageId = 1, RequestId = 1, Content_en = "Content_en", Content_cs ="Content_cs", Content_th = "Content_th", Content_vn = "Content_vn"
            } };

            const string XmlTemplate = "<r TicketEventId='{0}' ProductId='{1}' EventDate='{2}' " +
               "SubmitDate='{3}' EventType='{4}' ActualResult='{5}' WrongResult='{6}' MessageId='{7}' " +
               "SportNameEN='{8}' SportNameSC='{9}' SportNameTH='{10}' SportNameYN='{11}' " +
               "HomeNameEN='{12}' HomeNameSC='{13}' HomeNameTH='{14}' HomeNameYN='{15}' " +
               "AwayNameEN='{16}' AwayNameSC='{17}' AwayNameTH='{18}' AwayNameYN='{19}' " +
               "LeagueNameEN='{20}' LeagueNameSC='{21}' LeagueNameTH='{22}' LeagueNameYN='{23}' MatchCode='{24}' " +
               "MatchInfoEN='{25}' MatchInfoSC='{26}' MatchInfoTH='{27}' MatchInfoYN='{28}' Type='{29}' LeagueGroupID='{30}'/>";

            var stringBuilderEventData = new StringBuilder(BackgroundJobConstants.BeginRootTag.ToLowerInvariant());

            commonFeedback.GetSportName(transaction.SportType.ToString(), Language.en_US).Returns("sportNameEN");
            commonFeedback.GetSportName(transaction.SportType.ToString(), Language.zh_CN).Returns("sportNameSC");
            commonFeedback.GetSportName(transaction.SportType.ToString(), Language.th_TH).Returns("sportNameTH");
            commonFeedback.GetSportName(transaction.SportType.ToString(), Language.vi_VN).Returns("sportNameYN");

            commonFeedback.GetLeagueName(transaction.LeagueId.ToString(), Language.en_US).Returns("leagueNameEN");
            commonFeedback.GetLeagueName(transaction.LeagueId.ToString(), Language.zh_CN).Returns("leagueNameSC");
            commonFeedback.GetLeagueName(transaction.LeagueId.ToString(), Language.th_TH).Returns("leagueNameTH");
            commonFeedback.GetLeagueName(transaction.LeagueId.ToString(), Language.vi_VN).Returns("leagueNameYN");

            commonFeedback.GetTeamName(transaction.HomeId.ToString(), Language.en_US).Returns("homeNameEN");
            commonFeedback.GetTeamName(transaction.HomeId.ToString(), Language.zh_CN).Returns("homeNameSC");
            commonFeedback.GetTeamName(transaction.HomeId.ToString(), Language.th_TH).Returns("homeNameTH");
            commonFeedback.GetTeamName(transaction.HomeId.ToString(), Language.vi_VN).Returns("homeNameYN");

            commonFeedback.GetTeamName(transaction.AwayId.ToString(), Language.en_US).Returns("awayNameEN");
            commonFeedback.GetTeamName(transaction.AwayId.ToString(), Language.zh_CN).Returns("awayNameSC");
            commonFeedback.GetTeamName(transaction.AwayId.ToString(), Language.th_TH).Returns("awayNameTH");
            commonFeedback.GetTeamName(transaction.AwayId.ToString(), Language.vi_VN).Returns("awayNameYN");

            commonFeedback.GetMatchCodeFormat(transaction.MatchCode, Language.en_US).Returns("matchInfoEN");
            commonFeedback.GetMatchCodeFormat(transaction.MatchCode, Language.zh_CN).Returns("matchInfoSC");
            commonFeedback.GetMatchCodeFormat(transaction.MatchCode, Language.th_TH).Returns("matchInfoTH");
            commonFeedback.GetMatchCodeFormat(transaction.MatchCode, Language.vi_VN).Returns("matchInfoYN");

            stringBuilderEventData.AppendFormat(XmlTemplate,
                        transaction.TicketEventId, transaction.ProductId, transaction.EventDate,
                        transaction.SubmitDate.ToString("yyyy-MM-dd H:mm:ss.fffffff"), transaction.EventType, transaction.ActualResult,
                        transaction.WrongResult, transaction.MessageId, "sportNameEN", "sportNameSC",
                        "sportNameTH", "sportNameYN", "homeNameEN", "homeNameSC", "homeNameTH", "homeNameYN",
                        "awayNameEN", "awayNameSC", "awayNameTH", "awayNameYN", "leagueNameEN", "leagueNameSC",
                        "leagueNameTH", "leagueNameYN", transaction.MatchCode, "matchInfoEN", "matchInfoSC",
                        "matchInfoTH", "matchInfoYN", transaction.Type, transaction.LeagueGroupID
                        );

            stringBuilderEventData.Append(BackgroundJobConstants.EndRootTag.ToLowerInvariant());

            const string xmlTemplateMessage = "<root><r MessageId='1' Content_en='Content_en' Content_cs='Content_cs' Content_th='Content_th' Content_vn='Content_vn'/></root>";
            var targetMessages = fixture.Create<IEnumerable<TargetMessage>>();
            var targetMessagesInfo = fixture.Create<IEnumerable<TargetMessageInfo>>();
            var criteria = new ImportChatBotResultAmendmentCommand();
            var commandResult = fixture.Create<CommandResult>();
            var valueTuple = new ValueTuple<CommandResult, ImportChatBotResultAmendmentCommand>(commandResult, criteria);
            baseService.ExecuteNonQueryAsync(Arg.Do<ImportChatBotResultAmendmentCommand>(x => criteria = x)).Returns(valueTuple.Item1);

            // Act
            var actual = await service.ImportChatBotAmendmentResultAsync(ticketEvents, transactions, messages, targetMessages, targetMessagesInfo, BackgroundJobConstants.DefaultEnvironment, "ServerName");
            // Assert
            Assert.Equal(valueTuple.Item1, actual.Item1);
            Assert.Equal(ticketEvents.ToXML("root").ToString(), criteria.TicketData);
            Assert.Equal(stringBuilderEventData.ToString(), criteria.EventData);
            Assert.Equal(xmlTemplateMessage, criteria.MessageData);
            Assert.Equal(targetMessages.ToXML("root").ToString(), criteria.TargetMessage);
            Assert.Equal(targetMessagesInfo.ToXML("root").ToString(), criteria.InfoMessage);
            Assert.Equal(BackgroundJobConstants.DefaultEnvironment, criteria.Environment);
            Assert.Equal("ServerName", criteria.ServerName);

            Assert.Equal(0, criteria.ErrCode);
            Assert.Null(criteria.ErrMsg);
        }

        #region Helpers

        private ChatBotResultAmendmentService GetService()
        {
            baseService = Substitute.For<IBaseService>();
            commonFeedback = Substitute.For<ICommonFeedback>();
            return new ChatBotResultAmendmentService(baseService, commonFeedback);
        }

        #endregion Helpers
    }
}