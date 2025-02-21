namespace CRM.BackgroundJob.UnitTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using CRM.Extensions.Xml;
    using CRM.Models;
    using CRM.Services;
    using CRM.BackgroundJob.Criteria.AutoFeedback;
    using CRM.BackgroundJob.Models;
    using CRM.BackgroundJob.Models.AbnormalBet;
    using CRM.BackgroundJob.Constants;
    using CRM.BackgroundJob.Criteria;
    using CRM.BackgroundJob.Models;
    using CRM.BackgroundJob.Services;
    using NSubstitute;
    using Xunit;

    public class MessageServiceTests
    {
        private IBaseService baseService;

        [Fact]
        public async Task GetVoidingMessageAsync_WhenCalled_ReturnExpectedMessages()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture.Create<ValueTuple<IEnumerable<VoidingMessage>, IEnumerable<VoidingHisMessage>>>();
            var service = GetMessageService();
            baseService.FetchMultipleAsync<VoidingMessage, VoidingHisMessage>(Arg.Any<GetVoidingMessageCriteria>()).Returns(expected);

            // Act
            var actual = await service.GetVoidingMessageAsync(1);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ImportVoidingMessageHistoryCheckAsync_WhenCalled_ReturnExpectedMessages()
        {
            // Arrange
            var fixture = new Fixture();
            var voidingHisMessages = fixture.Create<IEnumerable<VoidingHisMessage>>();
            var xml = voidingHisMessages.ToXML("root").ToString();
            var criteria = new GetVoidingMessageHistoryCheckFbCriteria();
            var service = GetMessageService();
            baseService.GetDataAsync<VoidingHisMessage>(Arg.Do <GetVoidingMessageHistoryCheckFbCriteria>(x => criteria = x)).Returns(voidingHisMessages);

            // Act
            var actual = await service.ImportVoidingMessageHistoryCheckAsync(voidingHisMessages);

            // Assert
            Assert.Equal(voidingHisMessages, actual);
            Assert.Equal(xml, criteria.Messages);
        }

        [Fact]
        public async Task ImportVoidingMessageAsync_WhenCalled_ReturnExpectedMessages()
        {
            // Arrange
            var fixture = new Fixture();
            var batchSize = fixture.Create<int>();
            var voidingMessages = new List<VoidingMessage>
            {
                new VoidingMessage
                {
                    MessageId = 1,
                    RequestId = 1,
                    VoidingHistoryCollectionId = 1
                }
            };

            var voidingMessageUserName = new List<VoidingMessageUserName>
            {
                new VoidingMessageUserName
                {
                    VoidingHistoryCollectionId = 1,
                    MessageId = 1,
                    RequestId = 1,
                    CustIdList = "CustIdList",
                    UsernameList = "UsernameList"
                }
            };
            const string XmlTemplate = "<r VoidingHistoryCollectionId='1' MsgID='1' ReqID='1' UsernameList='UsernameList' CustIdList='CustIdList'/>";
            var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);
            stringBuilder.Append(XmlTemplate);
            stringBuilder.Append(BackgroundJobConstants.EndRootTag);
            var date = new DateTime();
            var criteria = new ImportVoidingMessageCriteria();
            var service = GetMessageService();
            baseService.GetDataAsync<VoidingMessageUserName>(Arg.Do<ImportVoidingMessageCriteria>(x => criteria = x)).Returns(voidingMessageUserName);
            // Act
            var actual = await service.ImportVoidingMessageAsync(voidingMessageUserName, voidingMessages, batchSize, "Live", date);

            // Assert
            Assert.Equal(voidingMessageUserName, actual);
            Assert.Equal(stringBuilder.ToString(), criteria.MessageIds);
            Assert.Equal(batchSize, criteria.BatchSize);
        }

        [Fact]
        public async Task GetFailMessageAsync_WhenCalled_ReturnExpectedMessages()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture.Create<IEnumerable<VoidingMessageUserName>>();
            var service = GetMessageService();
            baseService.GetDataAsync<VoidingMessageUserName>(Arg.Any<GetFailMessageCriteria>()).Returns(expected);

            // Act
            var actual = await service.GetFailMessageAsync();

            // Assert
            Assert.Equal(expected, actual);
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
                    Content_cs = "@@1"
                }
            };
            var expected = new List<Message>
            {
                new Message
                {
                    Content_en = "1",
                    Content_cs = "1"
                }
            };
            var messageIds = fixture.Create<IEnumerable<int>>();
            var criteria = new GetMessageInfoCriteria();
            var service = GetMessageService();
            baseService.GetDataAsync<Message>(Arg.Do<GetMessageInfoCriteria>(x => criteria = x)).Returns(message);

            // Act
            var actual = await service.GetMessageInfoAsync(messageIds);

            // Assert
            Assert.Equal(expected.ToString(), actual.ToString());
            Assert.Equal(string.Join(",", messageIds), criteria.MessageIds);
        }

        [Fact]
        public async Task EditStatusVoidingMessageAsync_WhenCalled_ReturnExpectedCommandResult()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture.Create<CommandResult>();
            var voidingMessageUserNames = new List<VoidingMessageUserName>{
                new VoidingMessageUserName { MessageId = 1, RequestId = 1, VoidingHistoryCollectionId= 1}
            };
            var xml = "<ROOT><r VoidingHistoryCollectionId='1' MsgID='1' ReqID='1' /></ROOT>";
            var criteria = new EditStatusVoidingMessageCommand();
            var service = GetMessageService();
            baseService.ExecuteNonQueryAsync(Arg.Do<EditStatusVoidingMessageCommand>(x => criteria = x)).Returns(expected);

            // Act
            var actual = await service.EditStatusVoidingMessageAsync(voidingMessageUserNames, "Live");

            // Assert
            Assert.Equal(expected, actual);
            Assert.Equal(xml, criteria.MessageIds);
        }

        #region Helpers

        private MessageService GetMessageService()
        {
            baseService = Substitute.For<IBaseService>();
            return new MessageService(baseService);
        }

        #endregion Helpers
    }
}