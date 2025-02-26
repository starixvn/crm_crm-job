namespace CRM.UnitTests.Services.AutoFeedback
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using CRM.Constants;
    using CRM.Criteria.AutoFeedback;
    using CRM.Models.AbnormalBet;
    using CRM.Models.Common;
    using CRM.Services.AutoFeedback;
    using CRM.Services.Common;
    using Microsoft.Extensions.Configuration;
    using NSubstitute;
    using Xunit;

    public class FeedbackTicketVoidingServiceTests
    {
        private IBaseService baseService;
        private IMessageService messageService;
        private ICrmLogService logService;
        private IConfiguration configuration;

        private readonly IFixture fixture = new Fixture();

        [Fact]
        public async Task GetTicketVoidingInfoAsync_MessageDoNotHaveTransactionInfo_ExpectedUpdateStatusAndWriteLog()
        {
            // Arrange
            fixture.Customize<VoidingMessage>(x => x.With(b => b.MessageId, 1));
            fixture.Customize<MatchInfo>(x => x.With(b => b.MessageId, "1"));
            var setting = fixture.Create<AbnormalBetSystemSetting>();
            var voidingMessageUserNames = new List<VoidingMessageUserName>
            {
                new()
                {
                    VoidingHistoryCollectionId = 1,
                    MessageId = 1,
                    RequestId = 1,
                    CustIdList = "CustIdList",
                    UsernameList = "UsernameList"
                }
            };
            const string XmlTemplate = "<r VoidingHistoryCollectionId='1' MsgID='1' ReqID='1' UsernameList='UsernameList' CustIdList='CustIdList' />";
            var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);
            stringBuilder.Append(XmlTemplate);
            stringBuilder.Append(BackgroundJobConstants.EndRootTag);
            var matchInfos = fixture.Create<IEnumerable<MatchInfo>>();
            var duplicateTicketDetail = fixture.Create<IEnumerable<DuplicateTicketDetail>>();
            var ticketInfo = new ValueTuple<IEnumerable<MatchInfo>, IEnumerable<TransactionInfo>, IEnumerable<DuplicateTicketDetail>>(matchInfos, new List<TransactionInfo>(), duplicateTicketDetail);
            var criteria = new GetAbnormalTicketInfoCriteria();
            var service = GetService();
            baseService.FetchMultipleAsync<MatchInfo, TransactionInfo, DuplicateTicketDetail>(Arg.Do<GetAbnormalTicketInfoCriteria>(x => criteria = x)).Returns(ticketInfo);

            // Act
            var actual = await service.GetTicketVoidingInfoAsync(voidingMessageUserNames, setting);

            // Assert
            logService.Received().LogInformation(Arg.Any<string>());
            await messageService.Received().EditStatusVoidingMessageAsync(Arg.Any<IEnumerable<VoidingMessageUserName>>(), Arg.Any<string>());

            Assert.Equal(matchInfos, actual.Item1);
            Assert.Empty(actual.Item2);
            Assert.Equal(stringBuilder.ToString(), criteria.MessageIds);
            Assert.Equal(setting.ConfigUAT, criteria.ConfigUAT);
            Assert.Equal(setting.ConfigPRO, criteria.ConfigPRO);
            Assert.Equal(BackgroundJobConstants.DefaultEnvironment, criteria.Environment);
        }

        [Fact]
        public async Task GetTicketVoidingInfoAsync_MessageHaveTransactionInfo_ReturnExpectedTicketInfos()
        {
            // Arrange
            fixture.Customize<MatchInfo>(x => x.With(b => b.VoidingHistoryCollectionId, 1));
            fixture.Customize<TransactionInfo>(x => x.With(b => b.VoidingHistoryCollectionId, 1));
            var setting = fixture.Create<AbnormalBetSystemSetting>();
            var voidingMessageUserNames = new List<VoidingMessageUserName>
            {
                new()
                {
                    VoidingHistoryCollectionId = 1,
                    MessageId = 1,
                    RequestId = 1,
                    CustIdList = "CustIdList",
                    UsernameList = "UsernameList"
                }
            };
            const string XmlTemplate = "<r VoidingHistoryCollectionId='1' MsgID='1' ReqID='1' UsernameList='UsernameList' CustIdList='CustIdList' />";
            var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);
            stringBuilder.Append(XmlTemplate);
            stringBuilder.Append(BackgroundJobConstants.EndRootTag);
            var matchInfos = fixture.Create<IEnumerable<MatchInfo>>();
            var transactionInfos = fixture.Create<IEnumerable<TransactionInfo>>();
            var duplicateTicketDetail = fixture.Create<IEnumerable<DuplicateTicketDetail>>();
            var ticketInfo = new ValueTuple<IEnumerable<MatchInfo>, IEnumerable<TransactionInfo>, IEnumerable<DuplicateTicketDetail>>(matchInfos, transactionInfos, duplicateTicketDetail);
            var criteria = new GetAbnormalTicketInfoCriteria();
            var service = GetService();
            baseService.FetchMultipleAsync<MatchInfo, TransactionInfo, DuplicateTicketDetail>(Arg.Do<GetAbnormalTicketInfoCriteria>(x => criteria = x)).Returns(ticketInfo);

            // Act
            var actual = await service.GetTicketVoidingInfoAsync(voidingMessageUserNames, setting);

            // Assert
            logService.DidNotReceive().LogInformation(Arg.Any<string>());
            await messageService.DidNotReceive().EditStatusVoidingMessageAsync(Arg.Any<IEnumerable<VoidingMessageUserName>>(), Arg.Any<string>());

            Assert.Equal(transactionInfos, actual.Item2);
            Assert.Equal(stringBuilder.ToString(), criteria.MessageIds);
            Assert.Equal(setting.ConfigUAT, criteria.ConfigUAT);
            Assert.Equal(setting.ConfigPRO, criteria.ConfigPRO);
            Assert.Equal(BackgroundJobConstants.DefaultEnvironment, criteria.Environment);
        }

        [Fact]
        public async Task ImportTicketVoidingInfoAsync_FeedbackCreated_ExpectedUpdateStatusAndWriteLog()
        {
            // Arrange
            fixture.Customize<VoidingMessage>(x => x.With(b => b.MessageId, 1));
            fixture.Customize<MatchInfo>(x => x.With(b => b.MessageId, "1"));
            var voidingMessageUserNames = fixture.Create<IEnumerable<VoidingMessageUserName>>();
            var matchInfos = fixture.Create<IEnumerable<MatchInfo>>();
            var siteSettingMappings = fixture.Create<IEnumerable<SiteSettingMapping>>();
            var headlines = fixture.Create<IEnumerable<GroupHeadline>>();
            var licenseeInfo = fixture.Create<CustomerInfo>();
            var criteria = new ImportFeedbackInfoCriteria();
            var service = GetService();
            baseService.GetDataAsync<AutoFeedbackInfo>(Arg.Do<ImportFeedbackInfoCriteria>(x => criteria = x)).Returns(new List<AutoFeedbackInfo>());

            // Act
            var actual = await service.ImportTicketVoidingInfoAsync(
                voidingMessageUserNames,
                matchInfos,
                new List<TransactionInfo>(),
                siteSettingMappings,
                headlines,
                licenseeInfo);

            // Assert
            await messageService.Received().EditStatusVoidingMessageAsync(Arg.Any<IEnumerable<VoidingMessageUserName>>(), Arg.Any<string>());

            Assert.Empty(actual.Item1);
            Assert.Empty(actual.Item2);
            Assert.Equal("<ROOT></ROOT>", criteria.Xml);
        }

        [Fact]
        public async Task ImportTicketVoidingInfoAsync_TicketVoidingForCredit_ReturnExpectedInfo()
        {
            // Arrange
            var voidingMessageUserNames = fixture.Create<IEnumerable<VoidingMessageUserName>>();
            var matchInfos = new List<MatchInfo>
            {
                new() { MessageId = "1", HomeName = "home", AwayName = "away", IssueTypeId = 7, VoidingHistoryCollectionId = 1 }
            };
            var transactionInfos = new List<TransactionInfo>
            {
                new() { MessageId = 1, RequestId = 1, CustId = 1, VoidingHistoryCollectionId = 1 }
            };
            var siteSettingMappings = fixture.Create<IEnumerable<SiteSettingMapping>>();
            var headlines = new List<GroupHeadline>
            {
                new() { CustId = 1, HeadlineId = 1 }
            };
            var licenseeInfo = fixture.Create<CustomerInfo>();
            var feedbackInfos = new List<AutoFeedbackInfo>
            {
                new() { VoidingHistoryCollectionId = 1 ,MessageId = 1, CustId = 1 }
            };
            var xml = "<ROOT><r msgid='1' reqid='1' custid='1' hl='1' related='1' event='home -vs- away' IssueTypeId='7' /></ROOT>";
            var criteria = new ImportFeedbackInfoCriteria();
            var service = GetService();
            baseService.GetDataAsync<AutoFeedbackInfo>(Arg.Do<ImportFeedbackInfoCriteria>(x => criteria = x)).Returns(feedbackInfos);

            // Act
            var actual = await service.ImportTicketVoidingInfoAsync(
                voidingMessageUserNames,
                matchInfos,
                transactionInfos,
                siteSettingMappings,
                headlines,
                licenseeInfo);

            // Assert
            logService.Received(2).LogInformation(Arg.Any<string>());
            await messageService.DidNotReceive().EditStatusVoidingMessageAsync(Arg.Any<IEnumerable<VoidingMessageUserName>>(), Arg.Any<string>());

            Assert.Equal(matchInfos, actual.Item1);
            Assert.Equal(transactionInfos, actual.Item2);
            Assert.Equal(xml, criteria.Xml);
        }

        [Fact]
        public async Task ImportTicketVoidingInfoAsync_TicketVoidingForDeposit_ReturnExpectedInfo()
        {
            // Arrange
            var voidingMessageUserNames = fixture.Create<IEnumerable<VoidingMessageUserName>>();
            var matchInfos = new List<MatchInfo>
            {
                new() { MessageId = "1", HomeName = "home", AwayName = "away", IssueTypeId = 7, VoidingHistoryCollectionId = 1 }
            };
            var transactionInfos = new List<TransactionInfo>
            {
                new() { MessageId = 1, RequestId = 1, CustId = 1, Site = "Site", IsDeposit = true , VoidingHistoryCollectionId = 1}
            };
            var siteSettingMappings = new List<SiteSettingMapping>
            {
                new() { SettingSource = "Site", LicenseeManagerId = 1 }
            };
            var headlines = fixture.Create<IEnumerable<GroupHeadline>>();
            var licenseeInfo = new CustomerInfo { CustId = 1 };
            var feedbackInfos = new List<AutoFeedbackInfo>
            {
                new() { VoidingHistoryCollectionId = 1, MessageId = 1, CustId = 1 }
            };
            var xml = "<ROOT><r msgid='1' reqid='1' custid='1' hl='1' related='1' event='home -vs- away' IssueTypeId='7' /></ROOT>";
            var criteria = new ImportFeedbackInfoCriteria();
            var service = GetService();
            baseService.GetDataAsync<AutoFeedbackInfo>(Arg.Do<ImportFeedbackInfoCriteria>(x => criteria = x)).Returns(feedbackInfos);

            // Act
            var actual = await service.ImportTicketVoidingInfoAsync(
                voidingMessageUserNames,
                matchInfos,
                transactionInfos,
                siteSettingMappings,
                headlines,
                licenseeInfo);

            // Assert
            logService.Received(2).LogInformation(Arg.Any<string>());
            await messageService.DidNotReceive().EditStatusVoidingMessageAsync(Arg.Any<IEnumerable<VoidingMessageUserName>>(), Arg.Any<string>());

            Assert.Equal(matchInfos, actual.Item1);
            Assert.Equal(transactionInfos, actual.Item2);
            Assert.Equal(xml, criteria.Xml);
        }

        [Fact]
        public async Task EditFeedbackInfoAsync_WhenCalled_ReturnExpectedCommandResult()
        {
            // Arrange
            var expected = fixture.Create<CRM.Models.Common.CommandResult>();
            var messageId = fixture.Create<string>();
            var requestIds = fixture.Create<IEnumerable<int>>();
            var custIds = fixture.Create<IEnumerable<int>>();
            var status = fixture.Create<string>();
            var description = fixture.Create<string>();
            var criteria = new EditFeedbackInfoCommand();
            var service = GetService();
            baseService.ExecuteNonQueryAsync(Arg.Do<EditFeedbackInfoCommand>(x => criteria = x)).Returns(expected);

            // Act
            var actual = await service.EditFeedbackInfoAsync(messageId, requestIds, custIds, status, description);

            // Assert
            Assert.Equal(expected, actual);
            Assert.Equal(messageId, criteria.MessageId);
            Assert.Equal(string.Join(",", requestIds), criteria.RequestIds);
            Assert.Equal(string.Join(",", custIds), criteria.CustIds);
            Assert.Equal(string.Join(",", status), criteria.Status);
            Assert.Equal(string.Join(",", description), criteria.Description);
            Assert.Equal(string.Join(",", "LIVE"), criteria.Environment);
        }

        [Fact]
        public async Task GetAbnormalTicketsByTransAsync_WhenCalled_ReturnExpectedTransaction()
        {
            // Arrange
            var expected = fixture.Create<IEnumerable<TransactionInfo>>();
            fixture.Create<GetAbnormalTicketsCheckCriteria>();
            var setting = fixture.Create<AbnormalBetSystemSetting>();
            var checkTrans = fixture.Create<string>();
            var service = GetService();
            baseService.GetDataFromSiteDbAsync<TransactionInfo>(Arg.Do<GetAbnormalTicketsCheckCriteria>(x => _ = x)).Returns(expected);

            // Act
            var actual = await service.GetAbnormalTicketsByTransAsync(setting, checkTrans);

            // Assert
            Assert.Equal(expected, actual);
        }

        #region Helpers

        private FeedbackTicketVoidingService GetService()
        {
            baseService = Substitute.For<IBaseService>();
            messageService = Substitute.For<IMessageService>();
            logService = Substitute.For<ICrmLogService>();
            configuration = Substitute.For<IConfiguration>();
            return new FeedbackTicketVoidingService(baseService, messageService, logService, configuration);
        }

        #endregion Helpers
    }
}