namespace CRM.BackgroundJob.UnitTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using AutoFixture;
    using CRM.BackgroundJob.Core.Extensions.Xml;
    using CRM.BackgroundJob.Core.Models;
    using CRM.BackgroundJob.Core.Services;
    using CRM.BackgroundJob.Criteria.ChatBot;
    using CRM.BackgroundJob.Models;
    using CRM.BackgroundJob.Models.ChatBot;
    using Fanex.CRM.BackgroundJob.Constants;
    using Fanex.CRM.BackgroundJob.Criteria.ChatBot;
    using Fanex.CRM.BackgroundJob.Models;
    using Fanex.CRM.BackgroundJob.Models.ChatBot;
    using Fanex.CRM.BackgroundJob.Services;
    using NSubstitute;
    using Xunit;

    public class ChatBotTicketVoidingServiceTests
    {
        private IBaseService baseService;

        [Fact]
        public async Task GetVoidingMessageAsync_WhenCalled_ReturnExpectedMessages()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture.Create<IEnumerable<ChatBotVoidingHistoryCollection>>();
            var service = GetService();
            baseService.GetDataAsync<ChatBotVoidingHistoryCollection>(Arg.Any<GetChatBotVoidingMessageCriteria>()).Returns(expected);

            // Act
            var actual = await service.GetVoidingMessageAsync(1, 1);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ImportVoidingMessageHistoryCheckAsync_WhenCalled_ReturnExpectedVoidingHistory()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture.Create<IEnumerable<ChatBotVoidingHistory>>();
            var xml = expected.ToXML("root").ToString();
            var criteria = new GetVoidingMessageHistoryCheckCriteria();
            var service = GetService();
            baseService.GetDataAsync<ChatBotVoidingHistory>(Arg.Do<GetVoidingMessageHistoryCheckCriteria>(x => criteria = x)).Returns(expected);

            // Act
            var actual = await service.ImportVoidingMessageHistoryCheckAsync(expected);

            // Assert
            Assert.Equal(expected, actual);
            Assert.Equal(xml, criteria.Messages);
        }

        [Fact]
        public async Task GetChatBotAbnormalAccountGetUsernameList_WhenCalled_ReturnExpectedUsernameList()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture.Create<IEnumerable<ChatBotUserNameList>>();
            var criteria = new GetChatBotAbnormalAccountGetUsernameListCriteria();
            var service = GetService();
            baseService.GetDataFromSiteDbAsync<ChatBotUserNameList>(Arg.Do<GetChatBotAbnormalAccountGetUsernameListCriteria>(x => criteria = x)).Returns(expected);

            // Act
            var actual = await service.GetChatBotAbnormalAccountUsernameAsync(criteria);

            // Assert
            Assert.Equal(expected, actual);
        }


        [Fact]
        public async Task EditVoidingMessageAsync_WhenCalled_ReturnExpectedCommandResult()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture.Create<CommandResult>();
            var voidingMessages = fixture.Create<IEnumerable<ChatBotVoidingMessage>>();
            var xml = voidingMessages.ToXML("root").ToString();
            var criteria = new EditChatBotVoidingMessageCommand();

            var service = GetService();
            baseService.ExecuteNonQueryAsync(Arg.Do<EditChatBotVoidingMessageCommand>(x => criteria = x)).Returns(expected);

            // Act
            var actual = await service.EditVoidingMessageAsync(voidingMessages);

            // Assert
            Assert.Equal(expected, actual);
            Assert.Equal(xml, criteria.Messages);
        }

        [Fact]
        public async Task GetMessageInfoAsync_WhenCalled_ReturnExpectedMessages()
        {
            // Arrange
            var fixture = new Fixture();
            var message = new List<Message>
            {
                new Message
                {
                    Content_en = "@@1",
                    Content_cs = "@@1",
                    Content_th = "@@1",
                    Content_vn = "@@1"
                }
            };
            var expected = new List<Message>
            {
                new Message
                {
                    Content_en = "1",
                    Content_cs = "1",
                    Content_th = "1",
                    Content_vn = "1"
                }
            };
            var voidingMessages = fixture.Create<IEnumerable<VoidingMessage>>();
            var xml = voidingMessages.ToXML("root").ToString();
            var criteria = new GetChatBotMessageInfoCriteria();
            var service = GetService();
            baseService.GetDataAsync<Message>(Arg.Do<GetChatBotMessageInfoCriteria>(x => criteria = x)).Returns(message);

            // Act
            var actual = await service.GetMessageInfoAsync(voidingMessages);

            // Assert
            Assert.Equal(expected.ToString(), actual.ToString());
            Assert.Equal(xml, criteria.Messages);
        }

        [Fact]
        public async Task GetTicketVoidingInfoAsync_WhenCalled_ReturnExpectedTicketInfos()
        {
            // Arrange
            var fixture = new Fixture();
            var data = new ValueTuple<IEnumerable<ChatBotVoidingTicketInfo>, IEnumerable<VoidingMessage>>();
            var expected = new ValueTuple<IEnumerable<ChatBotVoidingTicketInfo>, IEnumerable<VoidingMessage>, int, int>();
            var messages = fixture.Create<IEnumerable<ChatBotVoidingHistoryCollection>>();
            var chatBotVoidingBetIdHistory = fixture.Create<IEnumerable<ChatBotVoidingBetIdHistory>>();
            var xml = messages.ToXML("root").ToString();
            var criteria = new GetChatBotTicketVoidingInfoCriteria
            {
                Messages = xml
            };
            var service = GetService();
            baseService.FetchMultipleAsync<ChatBotVoidingTicketInfo, VoidingMessage>(Arg.Any<GetChatBotTicketVoidingInfoCriteria>()).Returns(data);

            // Act
            var actual = await service.GetTicketVoidingInfoAsync(messages, chatBotVoidingBetIdHistory, "Live", true);

            // Assert
            Assert.Equal(expected, actual);
            Assert.Equal(xml, criteria.Messages);
        }

        [Fact]
        public async Task ImportTicketVoidingAsync_WhenCalled_ReturnExpectedCommandResult()
        {
            // Arrange
            const string XmlTemplate = "<r VoidingHistoryCollectionId='{0}' MessageId='{1}' UsernameList='{2}' CustId='{3}' " +
                "Recommend='{4}' Mrecommend='{5}' Srecommend='{6}' IsTest='{7}' " +
                "Content_en='{8}' Content_cs='{9}' Content_th='{10}' Content_vn='{11}' Type='{12}'/>";
            var messageStringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag.ToLowerInvariant());
            var fixture = new Fixture();

            var MAXVoidingHistoryCollectionId = fixture.Create<int>();
            var MAXMsgTargetId = fixture.Create<int>();
            messageStringBuilder.Append(BackgroundJobConstants.EndRootTag.ToLowerInvariant());

            var messages = new List<Message>
            {
               new Message
               {
                    MessageId = 1,
                    RequestId = 1,
                    VoidingHistoryCollectionId = 1
               }
            };
            var ticketInfos = new List<ChatBotVoidingTicketInfo>
            {
                fixture.Create<ChatBotVoidingTicketInfo>()
            };

            ticketInfos.First().MessageId = messages.First().MessageId;
            var ticketInfo = ticketInfos.FirstOrDefault();
            var message = messages.FirstOrDefault();
            var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag.ToLowerInvariant());
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
                    ticketInfo.Type);
            stringBuilder.Append(BackgroundJobConstants.EndRootTag.ToLowerInvariant());
            var commandResult = fixture.Create<CommandResult>();
            var criteria = new ImportChatBotTicketVoidingInfoCommand();
            var valueTupe = new ValueTuple<CommandResult, ImportChatBotTicketVoidingInfoCommand>(commandResult, criteria);

            var service = GetService();
            baseService.ExecuteNonQueryAsync(Arg.Do<ImportChatBotTicketVoidingInfoCommand>(x => criteria = x)).Returns(valueTupe.Item1);

            // Act
            var actual = await service.ImportTicketVoidingAsync(messages, ticketInfos, MAXVoidingHistoryCollectionId, MAXMsgTargetId, BackgroundJobConstants.DefaultEnvironment);

            // Assert
            Assert.Equal(valueTupe.Item1, actual.Item1);
            Assert.Equal(stringBuilder.ToString(), criteria.XmlData);
            Assert.Equal(BackgroundJobConstants.DefaultEnvironment, criteria.Environment);
            Assert.Equal(0, criteria.ErrCode);
            Assert.Null(criteria.ErrMsg);
        }

        [Fact]
        public async Task ImportTicketVoidingAsync_WhenMessageIdNotEqual_ReturnExpectedXml()
        {
            // Arrange
            const string XmlTemplate = "<r VoidingHistoryCollectionId='{0}' MessageId='{1}' UsernameList='{2}' CustId='{3}' " +
                "Recommend='{4}' Mrecommend='{5}' Srecommend='{6}' IsTest='{7}' " +
                "Content_en='{8}' Content_cs='{9}' Content_th='{10}' Content_vn='{11}' Type='{12}'/>";
            var fixture = new Fixture();

            var messages = new List<Message>
            {
               new Message
               {
                   MessageId = 1,
                   RequestId = 1,
                   VoidingHistoryCollectionId = 1
               }
            };
            var ticketInfos = new List<ChatBotVoidingTicketInfo>
            {
                new ChatBotVoidingTicketInfo
                {
                    MessageId = 2,
                    VoidingHistoryCollectionId = 1
                }
            };

            var MAXVoidingHistoryCollectionId = fixture.Create<int>();
            var MAXMsgTargetId = fixture.Create<int>();

            ticketInfos.First().MessageId = messages.First().MessageId;
            var ticketInfo = ticketInfos.FirstOrDefault();
            var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag.ToLowerInvariant());
            stringBuilder.AppendFormat(XmlTemplate,
                    ticketInfo.VoidingHistoryCollectionId,
                    ticketInfo.MessageId,
                    ticketInfo.UsernameList,
                    ticketInfo.CustId,
                    ticketInfo.Recommend,
                    ticketInfo.Mrecommend,
                    ticketInfo.Srecommend,
                    ticketInfo.IsTest,
                    "",
                    "",
                    "",
                    "",
                    ticketInfo.Type);
            stringBuilder.Append(BackgroundJobConstants.EndRootTag.ToLowerInvariant());
            var commandResult = fixture.Create<CommandResult>();
            var criteria = new ImportChatBotTicketVoidingInfoCommand();
            var valueTupe = new ValueTuple<CommandResult, ImportChatBotTicketVoidingInfoCommand>(commandResult, criteria);

            var service = GetService();
            baseService.ExecuteNonQueryAsync(Arg.Do<ImportChatBotTicketVoidingInfoCommand>(x => criteria = x)).Returns(valueTupe.Item1);

            // Act
            var actual = await service.ImportTicketVoidingAsync(messages, ticketInfos, MAXVoidingHistoryCollectionId, MAXMsgTargetId, BackgroundJobConstants.DefaultEnvironment);

            // Assert
            Assert.Equal(valueTupe.Item1, actual.Item1);
            Assert.Equal(stringBuilder.ToString(), criteria.XmlData);
            Assert.Equal(BackgroundJobConstants.DefaultEnvironment, criteria.Environment);
            Assert.Equal(0, criteria.ErrCode);
            Assert.Null(criteria.ErrMsg);
        }

        #region Helpers

        private ChatBotTicketVoidingService GetService()
        {
            baseService = Substitute.For<IBaseService>();
            return new ChatBotTicketVoidingService(baseService);
        }

        #endregion Helpers
    }
}