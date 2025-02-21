namespace CRM.BackgroundJob.UnitTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using CRM.BackgroundJob.Constants;
    using CRM.BackgroundJob.Criteria.AutoFeedback;
    using CRM.BackgroundJob.Models;
    using CRM.BackgroundJob.Models.AbnormalBet;
    using CRM.BackgroundJob.Models.MixParlay;
    using CRM.BackgroundJob.Services;
    using CRM.BackgroundJob.UnitTests.Testable;
    using CRM.Models;
    using CRM.Services;
    using Microsoft.Extensions.Options;
    using NSubstitute;
    using Xunit;

    public class FeedbackServiceTests
    {
        private AppJobSettings appSettings;
        private IBaseService baseService;
        private ISettingService settingService;
        private IMessageService messageService;
        private ICustomerService customerService;
        private ICrmLogService logService;
        private IFeedbackTicketVoidingService ticketVoidingService;
        private ICommonFeedback commonFeedback;
        private readonly IFixture fixture = new Fixture();

        #region CreateFeedbackForVoidingAbnormalBetAsync

        [Fact]
        public async Task CreateFeedbackForVoidingAbnormalBetAsync_NotExistedVoidingMessage_ExpectedUpdateSystemSetting()
        {
            // Arrange
            var setting = fixture.Create<AbnormalBetSystemSetting>();
            var voidingMessages = new ValueTuple<IEnumerable<VoidingMessage>, IEnumerable<VoidingHisMessage>>();
            var messageImportCheck = fixture.Create<IEnumerable<VoidingHisMessage>>();
            var failMessages = new List<VoidingMessageUserName>();
            var voidingMessageUserName = fixture.Create<IEnumerable<VoidingMessageUserName>>();
            var transactionTickets = fixture.Create<IEnumerable<TransactionInfo>>();
            var service = MockService();
            messageService.GetVoidingMessageAsync(Arg.Any<int>()).Returns(voidingMessages);
            messageService.ImportVoidingMessageHistoryCheckAsync(voidingMessages.Item2).Returns(messageImportCheck);
            messageService.ImportVoidingMessageAsync(voidingMessageUserName, voidingMessages.Item1, setting.BatchSize, "Live", setting.LastDateUAT).Returns(voidingMessageUserName);
            messageService.GetFailMessageAsync().Returns(failMessages);
            ticketVoidingService.GetAbnormalTicketsByTransAsync(setting, Arg.Any<string>()).Returns(transactionTickets);
            // Act
            await service.CreateFeedbackForVoidingAbnormalBetAsync(setting, false, setting.LastDateUAT, "LastRuntime", setting.JobStatus, "Live");

            // Assert
            await commonFeedback.Received().EditSystemSettingAsyncByEnv(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            service.DidNotReceive().Protected(
               "CreateFeedbackForCreditDepositSiteAsync",
               Arg.Any<IEnumerable<Message>>(),
               Arg.Any<IEnumerable<MatchInfo>>(),
               Arg.Any<IEnumerable<TransactionInfo>>(),
               Arg.Any<List<VoidingMessageUserName>>(),
               Arg.Any<bool>(),
               Arg.Any<AbnormalBetSystemSetting>(),
               Arg.Any<IEnumerable<DuplicateTicketDetail>>()
               );
        }
        [Fact]
        public async Task CreateFeedbackForVoidingAbnormalBetAsync_NotExistedTicketVoiding_ExpectedUpdateSystemSetting()
        {
            // Arrange
            var setting = fixture.Create<AbnormalBetSystemSetting>();
            var voidingMessages = new ValueTuple<IEnumerable<VoidingMessage>, IEnumerable<VoidingHisMessage>>();
            var messageImportCheck = fixture.Create<IEnumerable<VoidingHisMessage>>();
            var failMessages = fixture.Create<IEnumerable<VoidingMessageUserName>>();
            var voidingMessageUserName = new List<VoidingMessageUserName>();
            var matchInfos = new List<MatchInfo>();
            var transactionInfos = fixture.Create<IEnumerable<TransactionInfo>>();
            var transCheck = fixture.Create<string>();
            var transactionTickets = fixture.Create<IEnumerable<TransactionInfo>>();
            var duplicateTicketDetail = fixture.Create<IEnumerable<DuplicateTicketDetail>>();
            var ticketInfo = new ValueTuple<IEnumerable<MatchInfo>, IEnumerable<TransactionInfo>, IEnumerable<DuplicateTicketDetail>, string>(matchInfos, transactionInfos, duplicateTicketDetail, transCheck);
            var service = MockService();
            messageService.GetVoidingMessageAsync(Arg.Any<int>()).Returns(voidingMessages);
            messageService.ImportVoidingMessageHistoryCheckAsync(voidingMessages.Item2).Returns(messageImportCheck);
            messageService.ImportVoidingMessageAsync(voidingMessageUserName, voidingMessages.Item1, setting.BatchSize, "Live", setting.LastDateUAT).Returns(voidingMessageUserName);
            messageService.GetFailMessageAsync().Returns(failMessages);
            ticketVoidingService.GetTicketVoidingInfoAsync(Arg.Any<IEnumerable<VoidingMessageUserName>>(), setting).Returns(ticketInfo);
            ticketVoidingService.GetAbnormalTicketsByTransAsync(setting, ticketInfo.Item4).Returns(transactionTickets);

            // Act
            await service.CreateFeedbackForVoidingAbnormalBetAsync(setting, false, setting.LastDateUAT, "LastRuntime", SystemSettingName.JobStatus, "LIVE");

            // Assert
            await commonFeedback.Received().EditSystemSettingAsyncByEnv(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            service.DidNotReceive().Protected(
               "CreateFeedbackForCreditDepositSiteAsync",
               Arg.Any<IEnumerable<Message>>(),
               Arg.Any<IEnumerable<MatchInfo>>(),
               Arg.Any<IEnumerable<TransactionInfo>>(),
               Arg.Any<List<VoidingMessageUserName>>(),
               Arg.Any<bool>(),
               Arg.Any<AbnormalBetSystemSetting>(),
               Arg.Any<IEnumerable<DuplicateTicketDetail>>()
               );
        }

        [Fact]
        public async Task CreateFeedbackForVoidingAbnormalBetAsync_ExistedTicketVoiding_ExpectedCreateFeedbackForCreditDepositSiteAsync()
        {
            // Arrange
            var setting = fixture.Create<AbnormalBetSystemSetting>();
            var matchInfos = new List<MatchInfo>
            {
                new MatchInfo { MessageId = "1" }
            };
            var transactionInfos = new List<TransactionInfo>
            {
                new TransactionInfo { MessageId = 1, CustId = 1 },
                new TransactionInfo { MessageId = 1, IsDeposit = true }
            };
            var voidingMessages = new ValueTuple<IEnumerable<VoidingMessage>, IEnumerable<VoidingHisMessage>>();
            var messageImportCheck = fixture.Create<IEnumerable<VoidingHisMessage>>();
            var failMessages = fixture.Create<IEnumerable<VoidingMessageUserName>>();
            var voidingMessageUserName = new List<VoidingMessageUserName>();
            var transCheck = fixture.Create<string>();
            var transactionTickets = fixture.Create<IEnumerable<TransactionInfo>>();
            var duplicateTicketDetail = fixture.Create<IEnumerable<DuplicateTicketDetail>>();
            var ticketInfo = new ValueTuple<IEnumerable<MatchInfo>, IEnumerable<TransactionInfo>, IEnumerable<DuplicateTicketDetail>, string>(matchInfos, transactionInfos, duplicateTicketDetail, transCheck);
            var service = MockService();
            messageService.GetVoidingMessageAsync(Arg.Any<int>()).Returns(voidingMessages);
            messageService.ImportVoidingMessageHistoryCheckAsync(voidingMessages.Item2).Returns(messageImportCheck);
            messageService.ImportVoidingMessageAsync(voidingMessageUserName, voidingMessages.Item1, setting.BatchSize, "Live", setting.LastDateUAT).Returns(voidingMessageUserName);
            messageService.GetFailMessageAsync().Returns(failMessages);
            ticketVoidingService.GetTicketVoidingInfoAsync(Arg.Any<IEnumerable<VoidingMessageUserName>>(), setting).Returns(ticketInfo);
            ticketVoidingService.GetAbnormalTicketsByTransAsync(setting, ticketInfo.Item4).Returns(transactionTickets);

            // Act
            await service.CreateFeedbackForVoidingAbnormalBetAsync(setting, false, setting.LastDateUAT, "LastRuntime", setting.JobStatus, "Live");

            // Assert
            await commonFeedback.Received().EditSystemSettingAsyncByEnv(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());

            service.Received().Protected(
               "CreateFeedbackForCreditDepositSiteAsync",
               Arg.Any<IEnumerable<Message>>(),
               Arg.Any<IEnumerable<MatchInfo>>(),
               Arg.Any<IEnumerable<TransactionInfo>>(),
               Arg.Any<List<VoidingMessageUserName>>(),
               Arg.Any<bool>(),
               Arg.Any<AbnormalBetSystemSetting>(),
               Arg.Any<IEnumerable<DuplicateTicketDetail>>()
               );
        }

        #endregion CreateFeedbackForVoidingAbnormalBetAsync

        #region CreateFeedbackForCreditDepositSiteAsync

        [Fact]
        public async Task CreateFeedbackForCreditDepositSiteAsync_WhenCalled_ExpectedCallExpectedCreateFeedbackForCreditAndDeposit()
        {
            // Arrange
            var setting = fixture.Create<AbnormalBetSystemSetting>();
            var matchInfos = new List<MatchInfo>
            {
                new MatchInfo { MessageId = "1" }
            };
            var transactionInfos = new List<TransactionInfo>
            {
                new TransactionInfo { MessageId = 1, CustId = 1 },
                new TransactionInfo { MessageId = 1, IsDeposit = true }
            };
            var settings = fixture.Create<IEnumerable<SettingItem>>();
            var messages = new List<Message> {
                new Message
                {
                    MessageId = 1
                }
            };
            var voidingMessageUserName = new List<VoidingMessageUserName>();
            var ticketInfo = new ValueTuple<IEnumerable<MatchInfo>, IEnumerable<TransactionInfo>>(matchInfos, transactionInfos);
            var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();
            var siteSettingMappings = fixture.Create<IEnumerable<SiteSettingMapping>>();
            var duplicateTicketDetails = fixture.Create<IEnumerable<DuplicateTicketDetail>>();
            var licenseeInfo = fixture.Create<CustomerInfo>();
            var headlines = new List<GroupHeadline>
            {
                new GroupHeadline { CustId = 1 }
            };
            var service = MockService();
            settingService.GetAllSettingAsync().Returns(settings);
            settingService.GetSystemSettingMappingAsync(Arg.Any<string[]>()).Returns(settingMappings);
            settingService.GetSiteSettingMappingAsync().Returns(siteSettingMappings);
            customerService.GetCustomerInfoAsync(appSettings.LicenseeUsername, true).Returns(licenseeInfo);
            customerService.GetGroupHeadlineVipAsync(Arg.Any<IEnumerable<int>>()).Returns(headlines);
            ticketVoidingService.ImportTicketVoidingInfoAsync(
                    voidingMessageUserName,
                    ticketInfo.Item1,
                    ticketInfo.Item2,
                    siteSettingMappings,
                    headlines,
                    licenseeInfo)
                .Returns(ticketInfo);

            // Act
            await service.Public_CreateFeedbackForCreditDepositSiteAsync(messages,
                        ticketInfo.Item1,
                        ticketInfo.Item2,
                        voidingMessageUserName,
                        false,
                        setting,
                        duplicateTicketDetails
                        );

            // Assert
            service.Received().Protected(
                    "CreateFeedbackForCreditSiteAsync",
                Arg.Any<IEnumerable<Message>>(),
                Arg.Any<MatchInfo>(),
                Arg.Any<IEnumerable<TransactionInfo>>(),
                Arg.Any<IEnumerable<GroupHeadline>>(),
                settings,
                settingMappings);
            service.Received().Protected(
                    "CreateFeedbackForDepositSiteAsync",
                licenseeInfo,
                Arg.Any<IEnumerable<Message>>(),
                Arg.Any<MatchInfo>(),
                Arg.Any<IEnumerable<TransactionInfo>>(),
                settings,
                settingMappings,
                Arg.Any<IEnumerable<DuplicateTicketDetail>>());
        }
        #endregion CreateFeedbackForCreditDepositSiteAsync

        #region CreateFeedbackForCreditSiteAsync
        [Fact]
        public async Task CreateFeedbackForCreditSiteAsync_WhenCalled_ExpectedCallCreateFeedbackForCreditSiteByBatchAsync()
        {
            // Arrange
            var messages = fixture.Create<IEnumerable<Message>>();
            var matchInfo = fixture.Create<MatchInfo>();
            var transactionInfos = fixture.Create<IEnumerable<TransactionInfo>>();
            var headlines = new List<GroupHeadline>
            {
                new GroupHeadline { HeadlineId = 1, CustId = 1 }
            };
            var settings = new List<SettingItem>
            {
                new SettingItem { SettingId = 1, SettingName = "Status", SettingValue = "Closed" },
                new SettingItem { SettingId = 2, SettingName = "Response", SettingValue = "Neutral" },
                new SettingItem { SettingId = 2022, SettingName = "Station" }
            };
            var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();
            var service = MockService();
            service.Protected("GetCategorySubCategory", settings, matchInfo.IssueTypeId).Returns(new Tuple<int, int, string>(1, 1, "xml"));

            // Act
            await service.Public_CreateFeedbackForCreditSiteAsync(
                messages,
                matchInfo,
                transactionInfos,
                headlines,
                settings,
                settingMappings);

            // Assert
            logService.Received(2).LogInformation(Arg.Any<string>());
            service.Received().Protected(
                "CreateFeedbackForCreditSiteByBatchAsync",
                Arg.Any<GroupHeadline>(),
                Arg.Any<IEnumerable<int>>(),
                messages,
                matchInfo,
                transactionInfos,
                settings,
                settingMappings,
                Arg.Any<CreateMutipleFeedbackCommand>());
        }
        #endregion CreateFeedbackForCreditSiteAsync

        #region CreateFeedbackForCreditSiteByBatchAsync

        [Fact]
        public async Task CreateFeedbackForCreditSiteByBatchAsync_CreateSuccessful_ExpectedWriteLog()
        {
            // Arrange
            var headline = new GroupHeadline { HeadlineId = 1 };
            var relatedCustomerIds = new List<int> { 1 };
            var messages = new List<Message>{
                new Message
                {
                    MessageId = 1, RequestId = 1
                }
            };
            var matchInfo = fixture.Create<MatchInfo>();
            var settings = fixture.Create<IEnumerable<SettingItem>>();
            var transactionInfos = new List<TransactionInfo>
            {
                new TransactionInfo { RequestId = 1, CustId = 1 }
            };
            var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();
            var criteria = fixture.Create<CreateMutipleFeedbackCommand>();
            var commandResult = new CommandResult { Code = 0 };
            var service = MockService();
            baseService.ExecuteNonQueryAsync(criteria).Returns(commandResult);
            service.Protected("GetProductXml", false, matchInfo, Arg.Any<IEnumerable<TransactionInfo>>(), settings, settingMappings).Returns("product xml");
            service.Protected("GetSolutionForCreditSite", messages.FirstOrDefault(), Arg.Any<IEnumerable<TransactionInfo>>(), Arg.Any<int>()).Returns("solution");
            service.Protected("GetRelatedCustomerXml", headline.HeadlineId, Arg.Any<IEnumerable<TransactionInfo>>(), matchInfo.IssueTypeId).Returns("related customer xml");
            service.Protected("GetDescriptionForCreditSite", Arg.Any<string>(), "1", matchInfo, Arg.Any<IEnumerable<TransactionInfo>>()).Returns("description");

            // Act
            await service.Public_CreateFeedbackForCreditSiteByBatchAsync(
                headline,
                relatedCustomerIds,
                messages,
                matchInfo,
                transactionInfos,
                settings,
                settingMappings,
                criteria);

            // Assert
            Assert.Equal("product xml", criteria.ProductXml);
            Assert.Equal("solution", criteria.Solution);
            Assert.Equal("related customer xml", criteria.RelatedCustomerXml);
            Assert.Equal("1", criteria.MessageId);
            Assert.Equal("1", criteria.ListOrigCustIds);
            Assert.Equal("description", criteria.Description);
            logService.Received(4).LogInformation(Arg.Any<string>());
        }

        [Fact]
        public async Task CreateFeedbackForCreditSiteByBatchAsync_CreateUnsuccessful_ExpectedWriteLogAndUpdateStatus()
        {
            // Arrange
            var headline = new GroupHeadline { HeadlineId = 1 };
            var relatedCustomerIds = new List<int> { 1 };
            var messages = new List<Message>{
                new Message
                {
                    MessageId = 1, RequestId = 1
                }
            };
            var matchInfo = fixture.Create<MatchInfo>();
            var transactionInfos = new List<TransactionInfo>
            {
                new TransactionInfo { RequestId = 1, CustId = 1 }
            };
            var settings = fixture.Create<IEnumerable<SettingItem>>();
            var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();
            var criteria = fixture.Create<CreateMutipleFeedbackCommand>();
            var commandResult = new CommandResult();
            var service = MockService();
            baseService.ExecuteNonQueryAsync(criteria).Returns(commandResult);

            // Act
            await service.Public_CreateFeedbackForCreditSiteByBatchAsync(
                headline,
                relatedCustomerIds,
                messages,
                matchInfo,
                transactionInfos,
                settings,
                settingMappings,
                criteria);

            // Assert
            logService.Received(4).LogInformation(Arg.Any<string>());
            await ticketVoidingService.EditFeedbackInfoAsync(Arg.Any<string>(), Arg.Any<IEnumerable<int>>(), Arg.Any<IEnumerable<int>>(), CreateFeedbackJobStatus.Fail, commandResult.Message);
            await logService.ErrorAsync(commandResult, criteria);
        }

        [Fact]
        public async Task CreateFeedbackForCreditSiteByBatchAsync_Exception_ExpectedWriteLogAndUpdateStatus()
        {
            // Arrange
            var exception = fixture.Create<Exception>();
            var headline = new GroupHeadline { HeadlineId = 1 };
            var relatedCustomerIds = new List<int> { 1 };
            var messages = new List<Message>{
                new Message { MessageId = 1, RequestId = 1 }
            };
            var matchInfo = fixture.Create<MatchInfo>();
            var transactionInfos = new List<TransactionInfo>
            {
                new TransactionInfo { RequestId = 1, CustId = 1 }
            };
            var settings = fixture.Create<IEnumerable<SettingItem>>();
            var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();
            var criteria = fixture.Create<CreateMutipleFeedbackCommand>();
            var service = MockService();
            baseService.When(x => x.ExecuteNonQueryAsync(criteria)).Throw(exception);

            // Act
            await service.Public_CreateFeedbackForCreditSiteByBatchAsync(
                headline,
                relatedCustomerIds,
                messages,
                matchInfo,
                transactionInfos,
                settings,
                settingMappings,
                criteria);

            // Assert
            logService.Received(4).LogInformation(Arg.Any<string>());
            await ticketVoidingService.EditFeedbackInfoAsync(Arg.Any<string>(), Arg.Any<IEnumerable<int>>(), Arg.Any<IEnumerable<int>>(), CreateFeedbackJobStatus.Fail, exception.Message);
            await logService.ErrorAsync(exception);
        }

        #endregion CreateFeedbackForCreditSiteByBatchAsync

        #region CreateFeedbackForDepositSiteAsync
        [Fact]
        public async Task CreateFeedbackForDepositSiteAsync_WhenCalled_ExpectedCallCreateFeedbackForDepositByBatchAsync()
        {
            // Arrange
            var licenseeInfo = fixture.Create<CustomerInfo>();
            var messages = fixture.Create<IEnumerable<Message>>();
            var matchInfo = fixture.Create<MatchInfo>();
            var transactionInfos = fixture.Create<IEnumerable<TransactionInfo>>();
            var communicationTool = fixture.Create<SettingItem>();
            var category = fixture.Create<Tuple<int, int, string>>();
            var duplicateTicketDetails = fixture.Create<IEnumerable<DuplicateTicketDetail>>();
            var settings = new List<SettingItem>
            {
                new SettingItem { SettingId = 1, SettingName = "Status", SettingValue = "Closed" },
                new SettingItem { SettingId = 2, SettingName = "Response", SettingValue = "Neutral" },
                new SettingItem { SettingId = 2022, SettingName = "Station" }
            };
            var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();
            var service = MockService();
            service.Protected("GetCommunicationTool", settings, matchInfo.IssueTypeId).Returns(communicationTool);
            service.Protected("GetCategorySubCategory", settings, matchInfo.IssueTypeId).Returns(category);

            // Act
            await service.Public_CreateFeedbackForDepositSiteAsync(
                licenseeInfo,
                messages,
                matchInfo,
                transactionInfos,
                settings,
                settingMappings,
                duplicateTicketDetails);

            // Assert
            logService.Received(2).LogInformation(Arg.Any<string>());
            service.Received().Protected("GetCategorySubCategory", settings, matchInfo.IssueTypeId);
            service.Received().Protected("GetProductXml", false, matchInfo, transactionInfos, settings, settingMappings);
            service.Received().Protected("CreateFeedbackForDepositByBatchAsync", messages, transactionInfos, Arg.Any<string>(), Arg.Any<CreateFeedbackCommand>(), duplicateTicketDetails);
        }
        #endregion CreateFeedbackForDepositSiteAsync

        #region GetLicenseeManager

        [Fact]
        public void GetLicenseeManager_ExistedMapping_ReturnExpectedLicenseeManager()
        {
            // Arrange
            var transactionInfos = new List<TransactionInfo>
            {
                new TransactionInfo { Site = "Site" }
            };
            var siteSettingMappings = new List<SiteSettingMapping>
            {
                new SiteSettingMapping { SettingSource = "Site", LicenseeManagerId = 1, LicenseeManager = "starixIT" }
            };
            var service = MockService();

            // Act
            service.Public_GetLicenseeManager(transactionInfos, siteSettingMappings);

            // Assert
            Assert.Equal(1, transactionInfos.First().LicenseeManagerId);
            Assert.Equal("starixIT", transactionInfos.First().LicenseeManager);
        }

        [Fact]
        public void GetLicenseeManager_NotExistedMapping_ReturnExpectedLicenseeManager()
        {
            // Arrange
            var transactionInfos = new List<TransactionInfo>
            {
                new TransactionInfo()
            };
            var siteSettingMappings = new List<SiteSettingMapping>();
            var service = MockService();

            // Act
            service.Public_GetLicenseeManager(transactionInfos, siteSettingMappings);

            // Assert
            Assert.Equal(0, transactionInfos.First().LicenseeManagerId);
            Assert.Null(transactionInfos.First().LicenseeManager);
        }

        #endregion GetLicenseeManager

        #region GetCommunicationTool

        [Fact]
        public void GetCommunicationTool_NotExistedSetting_ReturnDefaultCommunicationTool()
        {
            // Arrange
            var service = MockService();
            var settings = new List<SettingItem>();

            // Act
            var actual = service.Public_GetCommunicationTool(settings);

            // Assert
            Assert.Equal(BackgroundJobConstants.CommunicationNA.ToString(), actual.SettingId.ToString());
        }

        [Fact]
        public void GetCommunicationTool_ExistedSetting_ReturnExpectedCommunicationTool()
        {
            // Arrange
            var service = MockService();
            var settings = new List<SettingItem>
            {
                new SettingItem { SettingId = 1, SettingName = "Communication", SettingValue = "Skype" }
            };

            // Act
            var actual = service.Public_GetCommunicationTool(settings);

            // Assert
            Assert.Equal(settings.First().SettingId.ToString(), actual.SettingId.ToString());
        }

        #endregion GetCommunicationTool

        #region GetProductXml

        [Fact]
        public void GetProductXml_NotExistedSubProduct_ReturnExpectedXml()
        {
            // Arrange
            var matchInfo = fixture.Create<MatchInfo>();
            var transactionInfos = fixture.Create<IEnumerable<TransactionInfo>>();
            var settings = fixture.Create<IEnumerable<SettingItem>>();
            var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();
            var sportsbook = fixture.Create<SettingItem>();
            var subProducts = new List<SettingItem>();
            var service = MockService();
            service.Protected("GetSportsbookProduct", settings).Returns(sportsbook);
            service.Protected("GetSubProduct", false, sportsbook.SettingId, matchInfo, settings, settingMappings).Returns(subProducts);

            // Act
            var actual = service.Public_GetProductXml(false, matchInfo, transactionInfos, settings, settingMappings);

            // Assert
            Assert.Equal("<ROOT></ROOT>", actual);
        }

        [Fact]
        public void GetProductXml_NotExistedBettype_ReturnExpectedXml()
        {
            // Arrange
            var matchInfo = fixture.Create<MatchInfo>();
            var transactionInfos = fixture.Create<IEnumerable<TransactionInfo>>();
            var settings = fixture.Create<IEnumerable<SettingItem>>();
            var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();
            var sportsbook = new SettingItem { SettingId = 1 };
            var subProducts = new List<SettingItem>
            {
                new SettingItem { SettingId = 1 }
            };
            var bettypes = new List<SettingItem>();
            var service = MockService();
            service.Protected("GetSportsbookProduct", settings).Returns(sportsbook);
            service.Protected("GetSubProduct", Arg.Any<bool>(), sportsbook.SettingId, matchInfo, settings, settingMappings).Returns(subProducts);
            service.Protected("GetBettype", Arg.Any<bool>(), Arg.Any<IEnumerable<int>>(), transactionInfos, settings, settingMappings).Returns(bettypes);

            // Act
            var actual = service.Public_GetProductXml(false, matchInfo, transactionInfos, settings, settingMappings);

            // Assert
            Assert.Equal("<ROOT><Product ProductID='1' SubProductID='1' BetTypeID='9144' /></ROOT>", actual);
        }

        [Fact]
        public void GetProductXml_ExistedBettype_ReturnExpectedXml()
        {
            // Arrange
            var matchInfo = fixture.Create<MatchInfo>();
            var transactionInfos = fixture.Create<IEnumerable<TransactionInfo>>();
            var settings = fixture.Create<IEnumerable<SettingItem>>();
            var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();
            var sportsbook = new SettingItem { SettingId = 1 };
            var subProducts = new List<SettingItem>
            {
                new SettingItem { SettingId = 1 }
            };
            var bettypes = new List<SettingItem>
            {
                new SettingItem { SettingId = 1, ParentIds = new List<int> { 1 } }
            };
            var service = MockService();
            service.Protected("GetSportsbookProduct", settings).Returns(sportsbook);
            service.Protected("GetSubProduct", Arg.Any<bool>(), sportsbook.SettingId, matchInfo, settings, settingMappings).Returns(subProducts);
            service.Protected("GetBettype", Arg.Any<bool>(), Arg.Any<IEnumerable<int>>(), transactionInfos, settings, settingMappings).Returns(bettypes);

            // Act
            var actual = service.Public_GetProductXml(false, matchInfo, transactionInfos, settings, settingMappings);

            // Assert
            Assert.Equal("<ROOT><Product ProductID='1' SubProductID='1' BetTypeID='1' /></ROOT>", actual);
        }

        #endregion GetProductXml

        #region GetSportsbookProduct

        [Fact]
        public void GetSportsbookProduct_NotExistedSetting_ReturnDefaultProduct()
        {
            // Arrange
            var service = MockService();
            var settings = new List<SettingItem>();

            // Act
            var actual = service.Public_GetSportsbookProduct(settings);

            // Assert
            Assert.Equal(BackgroundJobConstants.ProductNA, actual.SettingId);
        }

        [Fact]
        public void GetSportsbookProduct_ExistedSetting_ReturnExpectedProduct()
        {
            // Arrange
            var service = MockService();
            var settings = new List<SettingItem>
            {
                new SettingItem { SettingId = 1, SettingName = "Product", SettingValue = "Sportsbook" }
            };

            // Act
            var actual = service.Public_GetSportsbookProduct(settings);

            // Assert
            Assert.Equal(settings.First(), actual);
        }

        #endregion GetSportsbookProduct

        #region GetSubProduct
        [Fact]
        public void GetSubProduct_NotExistedMapping_ReturnDefaultSubProduct()
        {
            // Arrange
            var productId = 1;
            var matchInfo = fixture.Create<MatchInfo>();
            var settings = fixture.Create<IEnumerable<SettingItem>>();
            var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();
            var service = MockService();

            // Act
            var actual = service.Public_GetSubProduct(false, productId, matchInfo, settings, settingMappings);

            // Assert
            Assert.Single(actual);
            Assert.Equal(BackgroundJobConstants.SubProductNA, actual.First().SettingId);

            Assert.Single(actual.First().ParentIds);
            Assert.Equal(BackgroundJobConstants.ProductNA, actual.First().ParentIds.First());
        }

        [Fact]
        public void GetSubProduct_MixParlayAndNotExistedMapping_ReturnDefaultSubProduct()
        {
            // Arrange
            var productId = 1;
            var matchInfo = fixture.Create<MatchInfo>();
            var settings = fixture.Create<IEnumerable<SettingItem>>();
            var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();
            var service = MockService();

            // Act
            var actual = service.Public_GetSubProduct(true, productId, matchInfo, settings, settingMappings);

            // Assert
            Assert.Single(actual);
            Assert.Equal(BackgroundJobConstants.SubProductNA, actual.First().SettingId);

            Assert.Single(actual.First().ParentIds);
            Assert.Equal(BackgroundJobConstants.ProductNA, actual.First().ParentIds.First());
        }

        [Fact]
        public void GetBettype_ExistedMapping_ReturnExpectedSubProducts()
        {
            // Arrange
            var productId = 1;
            var matchInfo = new MatchInfo { SportName = "sport" };
            var settings = new List<SettingItem>
            {
                new SettingItem { SettingId = 1, SettingName = "SubProduct", ParentIds = new List<int> { 1 } }
            };
            var settingMappings = new List<SystemSettingMapping>
            {
                new SystemSettingMapping { SettingId = 1, SettingName = "SubProduct", SettingSource = "sport" }
            };
            var service = MockService();

            // Act
            var actual = service.Public_GetSubProduct(false, productId, matchInfo, settings, settingMappings);

            // Assert
            Assert.Single(actual);
            Assert.Equal(settings.First(), actual.First());
        }

        #endregion GetSubProduct

        #region GetBettype

        [Fact]
        public void GetBettype_IsMixParlay_ReturnDefaultBettype()
        {
            // Arrange
            var subProducts = fixture.Create<IEnumerable<int>>();
            var transactionInfos = fixture.Create<IEnumerable<TransactionInfo>>();
            var settings = fixture.Create<IEnumerable<SettingItem>>();
            var bettypeMixparlay = new List<SettingItem>();
            var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();
            var service = MockService();
            service.Protected("GetBettypeParlay", subProducts, settings, settingMappings).Returns(bettypeMixparlay);

            // Act
            var actual = service.Public_GetBettype(true, subProducts, transactionInfos, settings, settingMappings);

            // Assert
            Assert.Single(actual);
            Assert.Equal(BackgroundJobConstants.BettypeNA, actual.First().SettingId);

            Assert.Single(actual.First().ParentIds);
            Assert.Equal(BackgroundJobConstants.SubProductNA, actual.First().ParentIds.First());
        }

        [Fact]
        public void GetBettype_NotExistedMapping_ReturnDefaultBettype()
        {
            // Arrange
            var subProducts = fixture.Create<IEnumerable<int>>();
            var transactionInfos = fixture.Create<IEnumerable<TransactionInfo>>();
            var settings = fixture.Create<IEnumerable<SettingItem>>();
            var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();
            var service = MockService();

            // Act
            var actual = service.Public_GetBettype(false, subProducts, transactionInfos, settings, settingMappings);

            // Assert
            Assert.Single(actual);
            Assert.Equal(BackgroundJobConstants.BettypeNA, actual.First().SettingId);

            Assert.Single(actual.First().ParentIds);
            Assert.Equal(BackgroundJobConstants.SubProductNA, actual.First().ParentIds.First());
        }

        [Fact]
        public void GetBettype_ExistedMapping_ReturnExpectedBettypes()
        {
            // Arrange
            var subProducts = new List<int> { 1 };
            var settings = new List<SettingItem>
            {
                new SettingItem { SettingId = 1, SettingName = "Bettype", ParentIds = new List<int> { 1 } }
            };
            var settingMappings = new List<SystemSettingMapping>
            {
                new SystemSettingMapping { SettingId = 1, SettingName = "Bettype", SettingSource = "Bettype" }
            };
            var transactionInfos = new List<TransactionInfo>
            {
                new TransactionInfo { Bettype = "Bettype" }
            };
            var service = MockService();

            // Act
            var actual = service.Public_GetBettype(false, subProducts, transactionInfos, settings, settingMappings);

            // Assert
            Assert.Single(actual);
            Assert.Equal(settings.First(), actual.First());
        }

        #endregion GetBettype

        #region GetBettypeParlay

        [Fact]
        public void GetBettypeParlay_IsMixParlayAndExistMapping_ReturnDefaultBettype()
        {
            // Arrange
            var subProducts = new List<int> { 1 };
            var settings = new List<SettingItem>
            {
                new SettingItem { SettingId = 1, SettingName = "Bettype", ParentIds = new List<int> { 1 } }
            };
            var settingMappings = new List<SystemSettingMapping>
            {
                new SystemSettingMapping { SettingId = 1, SettingName = "Bettype", SettingSource = "Mix Parlay" }
            };
            var service = MockService();

            // Act
            var actual = service.Public_GetBettypeParlay(subProducts, settings, settingMappings);

            // Assert
            Assert.Single(actual);
            Assert.Equal(settings.First(), actual.First());
        }

        #endregion GetBettypeParlay

        #region GetCategorySubCategory

        [Fact]
        public void GetCategorySubCategory_NotExistedCategory_ReturnExpectedTuple()
        {
            // Arrange
            var settings = new List<SettingItem>();
            var service = MockService();

            // Act
            var actual = service.Public_GetCategorySubCategory(settings, 13);

            // Assert
            Assert.Equal(0, actual.Item1);
            Assert.Equal(0, actual.Item2);
            Assert.Equal("<ROOT><Category MainCategoryID='0' SubCategoryID='0' /></ROOT>", actual.Item3);
        }

        [Fact]
        public void GetCategorySubCategory_NotExistedSubCategory_ReturnExpectedTuple()
        {
            // Arrange
            var settings = new List<SettingItem>
            {
                new SettingItem { SettingId = 1, SettingName = "MainCategory", SettingValue = "Special Notification" }
            };
            var service = MockService();

            // Act
            var actual = service.Public_GetCategorySubCategory(settings, 11);

            // Assert
            Assert.Equal(1, actual.Item1);
            Assert.Equal(0, actual.Item2);
            Assert.Equal("<ROOT><Category MainCategoryID='1' SubCategoryID='0' /></ROOT>", actual.Item3);
        }

        [Fact]
        public void GetCategorySubCategory_ExistedMainAndSubCategory_ReturnExpectedTuple()
        {
            // Arrange
            var settings = new List<SettingItem>
            {
                new SettingItem { SettingId = 1, SettingName = "MainCategory", SettingValue = "Special Notification" },
                new SettingItem { SettingId = 2, SettingName = "SubCategory", SettingValue = "Abnormal Bets", ParentIds = new List<int>{ 1 } }
            };
            var service = MockService();

            // Act
            var actual = service.Public_GetCategorySubCategory(settings, 11);

            // Assert
            Assert.Equal(1, actual.Item1);
            Assert.Equal(2, actual.Item2);
            Assert.Equal("<ROOT><Category MainCategoryID='1' SubCategoryID='2' /></ROOT>", actual.Item3);
        }

        [Fact]
        public void GetCategorySubCategory_ExistedMainAndSubCategoryAndIssuesTypeEqual13_ReturnExpectedTuple()
        {
            // Arrange
            var settings = new List<SettingItem>
            {
                new SettingItem { SettingId = 1, SettingName = "MainCategory", SettingValue = "Special Notification" },
                new SettingItem { SettingId = 2, SettingName = "SubCategory", SettingValue = "Abnormal Account", ParentIds = new List<int>{ 1 } }
            };
            var service = MockService();

            // Act
            var actual = service.Public_GetCategorySubCategory(settings, 13);

            // Assert
            Assert.Equal(1, actual.Item1);
            Assert.Equal(2, actual.Item2);
            Assert.Equal("<ROOT><Category MainCategoryID='1' SubCategoryID='2' /></ROOT>", actual.Item3);
        }

        #endregion GetCategorySubCategory

        #region GetRelatedCustomerXml
        [Fact]
        public void GetRelatedCustomerXml_WhenCalled_ReturnExpectedXml()
        {
            // Arrange
            var headlineId = 1;
            var transactions = new List<TransactionInfo>
            {
                new TransactionInfo { CustId = 1, Username = "starixIT" }
            };
            var xml = "<ROOT><Row CustId='1' RelatedCustId='1' Username='starixIT' /></ROOT>";
            var service = MockService();

            // Act
            var actual = service.Public_GetRelatedCustomerXml(headlineId, transactions, 11);

            // Assert
            Assert.Equal(xml, actual);
        }

        [Fact]
        public void GetRelatedCustomerXml_IssuesType13_ReturnExpectedXml()
        {
            // Arrange
            var headlineId = 1;
            var transactions = new List<TransactionInfo>
            {
                new TransactionInfo { CustId = 1, Username = "starixIT", RelatedUserName = "RelatedUserName", RelatedCustID = 2 }
            };
            var xml = "<ROOT><Row CustId='1' RelatedCustId='2' Username='RelatedUserName' /></ROOT>";
            var service = MockService();

            // Act
            var actual = service.Public_GetRelatedCustomerXml(headlineId, transactions, 13);

            // Assert
            Assert.Equal(xml, actual);
        }
        #endregion GetRelatedCustomerXml

        [Fact]
        public void GetAutoField_WhenCalled_ReturnExpectedAutoField()
        {
            // Arrange
            var feedbackDate = fixture.Create<DateTime>();
            var communicationTool = fixture.Create<string>();
            var licenseeManagers = fixture.Create<IEnumerable<string>>();
            var autoField = new StringBuilder("Date Time & Tool: ");
            autoField.AppendFormat("{0}/{1}/{2}", feedbackDate.ToString("yyyyMMdd"), feedbackDate.ToString("hh:mmtt"), communicationTool);
            autoField.Append(Environment.NewLine);
            autoField.AppendFormat("System ID: {0}", string.Join(", ", licenseeManagers));
            var service = MockService();

            // Act
            var actual = service.Public_GetAutoField(feedbackDate, communicationTool, licenseeManagers);

            // Assert
            Assert.Equal(autoField.ToString(), actual);
        }

        #region GetDescription

        [Fact]
        public void GetDescriptionForDepositeSite_WhenCalled_ReturnExpectedDescription()
        {
            // Arrange
            var messageId = fixture.Create<string>();
            var requestIds = fixture.Create<string>();
            var description = new StringBuilder("<span style=\"color: rgb(255, 0, 0);\">This is an auto-generated entry.</span>");
            description.AppendFormat("<br /><span style=\"color: rgb(255, 0, 0);\">Msg ID: {0}, Request No.: {1}</span>", messageId, requestIds);
            description.Append("<br />Announcement sent to affected Licensees and informed our related department found tickets of abnormal accounts.");
            description.Append("<br /><br />Please refer to the system email subject \"Voided Bets Report\" for those affected customers &amp; tickets for reference.");
            var service = MockService();

            // Act
            var actual = service.Public_GetDescriptionForDepositeSite(messageId, requestIds);

            // Assert
            Assert.Equal(description.ToString(), actual);
        }

        [Fact]
        public void GetDescriptionForCreditSite_WhenCalled_ReturnExpectedDescription()
        {
            // Arrange
            var messageId = fixture.Create<string>();
            var requestIds = fixture.Create<string>();
            fixture.Customize<MatchInfo>(x => x.With(b => b.IssueTypeId, 1));
            var matchInfo = fixture.Create<MatchInfo>();
            var transactionInfos = fixture.Create<IEnumerable<TransactionInfo>>();
            var description = new StringBuilder("<span style=\"color: rgb(255, 0, 0);\">This is an auto-generated entry.</span>");
            description.AppendFormat("<br /><span style=\"color: rgb(255, 0, 0);\">Msg ID: {0}, Request No.: {1}</span>", messageId, requestIds);
            description.AppendFormat("<br />Date: {0}", matchInfo.EventDate.ToString("dd-MMM-yyyy"));
            description.AppendFormat("<br />Sport: {0}", matchInfo.SportName);
            description.AppendFormat("<br />Event Start Time: {0}", matchInfo.KickOffTime);
            description.AppendFormat("<br />League: {0}", matchInfo.LeagueName);
            description.AppendFormat("<br />Match: {0} - vs - {1}", matchInfo.HomeName, matchInfo.AwayName);
            var service = MockService();

            // Act
            var actual = service.Public_GetDescriptionForCreditSite(messageId, requestIds, matchInfo, transactionInfos);

            // Assert
            Assert.Equal(description.ToString(), actual);
        }

        [Fact]
        public void GetDescriptionForCreditSite_IssuesTypeAbnormalAccount_ReturnExpectedDescription()
        {
            // Arrange
            var messageId = fixture.Create<string>();
            var requestIds = fixture.Create<string>();
            var matchInfo = new MatchInfo
            {
                IssueTypeId = 13,
                MaxWinlostdate = new DateTime(2020 - 01 - 01),
                MinWinlostdate = new DateTime(2020 - 01 - 01),
                DecidedBy = "DecidedBy",
                Requestby = "Requestby",
                VoidBy = "VoidBy"
            };

            var transactionInfos = new List<TransactionInfo> {
                new TransactionInfo
                {
                    CustId = 1,
                    RelatedUserName = "RelatedUserName"
                }
            };

            var description = new StringBuilder("<span style=\"color: rgb(255, 0, 0);\">This is an auto-generated entry.</span>");
            description.AppendFormat("<br /><span style=\"color: rgb(255, 0, 0);\">Msg ID: {0}, Request No.: {1}</span>", messageId, requestIds);
            description.AppendFormat("<br />Event Date: {0} - {1}", matchInfo.MinWinlostdate.ToString("dd-MMM-yyyy"), matchInfo.MaxWinlostdate.ToString("dd-MMM-yyyy"));
            description.AppendFormat("<br />Affected accounts: {0}", "RelatedUserName");
            description.AppendFormat("<br />Decided By: {0}", matchInfo.DecidedBy);
            description.AppendFormat("<br />Requested by: {0}", matchInfo.Requestby);
            description.AppendFormat("<br />Voided by: {0}", matchInfo.VoidBy);
            var service = MockService();

            // Act
            var actual = service.Public_GetDescriptionForCreditSite(messageId, requestIds, matchInfo, transactionInfos);

            // Assert
            Assert.Equal(description.ToString(), actual);
        }

        #endregion GetDescription

        #region GetSolutionMixParlayForCreditSite
        [Fact]
        public void GetSolutionMixParlayForCreditSite_WhenCalled_ReturnExpectedSolution()
        {
            // Arrange
            var message = fixture.Create<Message>();
            var testDescTemp = new MixParlayTemplate
            {
                SolutionCreditENSC = "SolutionCreditENSC"
            };
            var transactionInfo = new List<TransactionInfo>
            {
                new TransactionInfo{ TransId = 1, Username = "test"}
            };
            var service = MockService();
            commonFeedback.GetFeedbackTemplates<MixParlayTemplate>("MixParlayTemplate.xml").Returns(testDescTemp);

            // Act
            var actual = service.Public_GetSolutionMixParlayForCreditSite(message, transactionInfo, false);

            // Assert
            Assert.Equal("SolutionCreditENSC", actual);
        }
        #endregion GetSolutionMixParlayForCreditSite

        #region GetDescriptionMixParlayForCreditSite
        [Fact]
        public void GetDescriptionMixParlayForCreditSite_WhenCalled_ReturnExpectedSolution()
        {
            // Arrange
            var testDescTemp = new MixParlayTemplate
            {
                DescriptionCredit = "DescriptionCredit"
            };
            var messageId = fixture.Create<string>();
            var requestIds = fixture.Create<string>();
            var matchInfo = fixture.Create<MatchInfo>();

            var service = MockService();
            commonFeedback.GetFeedbackTemplates<MixParlayTemplate>("MixParlayTemplate.xml").Returns(testDescTemp);
            var description = new StringBuilder("<span style=\"color: rgb(255, 0, 0);\">This is an auto-generated entry.</span>");
            description.AppendFormat("<br /><span style=\"color: rgb(255, 0, 0);\">Msg ID: {0}, Request No.: {1}</span>", messageId, requestIds);
            description.Append(BackgroundJobConstants.LineBreak);
            description.AppendFormat(testDescTemp.DescriptionCredit, matchInfo.DecidedBy, matchInfo.Requestby, matchInfo.VoidBy, BackgroundJobConstants.Break);

            // Act
            var actual = service.Public_GetDescriptionMixParlayForCreditSite(messageId, requestIds, matchInfo);

            // Assert
            Assert.Equal(description.ToString(), actual);
        }
        #endregion GetDescriptionMixParlayForCreditSite

        #region GetSolution

        [Fact]
        public void GetSolutionForDeposit_WhenCalled_ReturnExpectedSolution()
        {
            // Arrange
            var message = fixture.Create<Message>();
            var solution = new StringBuilder(message.Content_en);
            solution.Append("<br /><br />").Append(message.Content_cs);
            var service = MockService();

            // Act
            var actual = service.Public_GetSolution(message);

            // Assert
            Assert.Equal(solution.ToString(), actual);
        }

        [Fact]
        public void GetSolutionForCreditSite_AbnormalAccount_ReturnExpectedSolution()
        {
            // Arrange
            var message = fixture.Create<Message>();
            var transactions = new List<TransactionInfo>
            {
                new TransactionInfo {TransId = 1, Username = "starixIT"}
            };
            var stringBuilder = new StringBuilder(message.Content_en);
            stringBuilder.Append("<br /><br />").Append(message.Content_cs);
            stringBuilder.Append("<br /><br />Kindly refer to Agency site \"Cancelled Bets\" for Voided user.");
            var service = MockService();

            // Act
            var actual = service.Public_GetSolutionForCreditSite(message, transactions, 13);

            // Assert
            Assert.Equal(stringBuilder.ToString(), actual);
        }

        [Fact]
        public void GetSolutionForCreditSite_AbnormalBet_ReturnExpectedSolution()
        {
            // Arrange
            var message = fixture.Create<Message>();
            var transactions = new List<TransactionInfo>
            {
                new TransactionInfo {TransId = 1, Username = "starixIT"}
            };
            var stringBuilder = new StringBuilder(message.Content_en);
            stringBuilder.Append("<br /><br />").Append(message.Content_cs);
            stringBuilder.Append("<br /><br />Kindly refer to Agency site \"Cancelled Bets\" for Voided user.");
            stringBuilder.Append("<br />starixIT 1");
            var service = MockService();

            // Act
            var actual = service.Public_GetSolutionForCreditSite(message, transactions, 11);

            // Assert
            Assert.Equal(stringBuilder.ToString(), actual);
        }

        #endregion GetSolution

        #region Helpers

        private FeedbackServiceTestable MockService()
        {
            appSettings = new AppJobSettings
            {
                CreatorId = 8,
                LicenseeUsername = "starixIT",
                NoOfRelatedCustomerPerFeedback = 2,
                TimeSpan = 12
            };
            var appJobSettings = Substitute.For<IOptionsSnapshot<AppJobSettings>>();
            appJobSettings.Value.Returns(appSettings);
            baseService = Substitute.For<IBaseService>();
            settingService = Substitute.For<ISettingService>();
            messageService = Substitute.For<IMessageService>();
            customerService = Substitute.For<ICustomerService>();
            logService = Substitute.For<ICrmLogService>();
            ticketVoidingService = Substitute.For<IFeedbackTicketVoidingService>();
            commonFeedback = Substitute.For<ICommonFeedback>();

            return Substitute.For<FeedbackServiceTestable>(
                appJobSettings,
                baseService,
                settingService,
                messageService,
                customerService,
                logService,
                ticketVoidingService,
                commonFeedback);
        }

        #endregion Helpers
    }
}