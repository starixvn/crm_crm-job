namespace CRM.BackgroundJob.UnitTests.Services
{
    using AutoFixture;
    using CRM.BackgroundJob.Core.Models;
    using CRM.BackgroundJob.Core.Services;
    using CRM.BackgroundJob.Models;
    using CRM.BackgroundJob.Models.MessageNotificationCenter;
    using CRM.BackgroundJob.Services;
    using CRM.BackgroundJob.UnitTests.Testable;
    using Fanex.BotService.Client.Core.Services.Interfaces;
    using Fanex.CRM.BackgroundJob.Services;
    using Microsoft.Extensions.Configuration;
    using NSubstitute;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;

    public class MessageNotificationCenterServiceTests
    {
        private ICrmLogService logService;
        private IConfiguration configuration;
        private ICommonFeedback commonFeedback;
        private ICaptureSettlementDataService captureSettlementDataService;
        private IGenerateResultAmendmentService generateResultAmendmentService;
        private IPushNotificationResultAmendmentService pushNotificationResultAmendmentService;
        private IGenerateLapNotificationService generateLapNotificationService;
        private ISendNotificationCenterService sendNotificationCenterService;
        private ISystemSettingService systemSettingService;
        private IBotService botService;
        private BotServiceSetting botSetting;

        private readonly IFixture fixture = new Fixture();

        [Fact]
        public async Task CaptureSettlementDataAsync_ExpectedCallExactFunction()
        {
            // Arrange
            var service = GetService();

            var serviceFlag = true;
            var initSiteDbConfigs = new List<SiteDbConfig>
            {
                new() {SiteDBGroupID = 1, LinkedServerName = "LinkedServerName", ClientAccessName = "ClientAccessName", AGIP = "1", Environment = "dev", FDTime = new DateTime()}
            };
            var sessionResultHistorys = new List<SettlResultHistory>
            {
                new () { Environment = "dev", JSSettlResultHist = "JSSettlResultHist"}
            };
            systemSettingService.GetSystemSettingNotificationAsync(Arg.Any<string>()).Returns(serviceFlag);
            captureSettlementDataService.GetInitialSiteDbConfigsAsync(Arg.Any<string>()).Returns(initSiteDbConfigs);
            captureSettlementDataService.GetSessionByDateAsync(initSiteDbConfigs[0]).Returns(sessionResultHistorys);

            // Act
            await service.CaptureSettlementDataAsync();

            // Assert
            await captureSettlementDataService.Received(1).InsertDataLoadAsync("dev", "JSSettlResultHist");
        }

        [Fact]
        public async Task CaptureSettlementDataAsync_ExpectedReturnLogService()
        {
            // Arrange
            var service = GetService();

            var serviceFlag = false;
            systemSettingService.GetSystemSettingNotificationAsync(Arg.Any<string>()).Returns(serviceFlag);

            // Act
            await service.CaptureSettlementDataAsync();

            // Assert
            logService.Received(1).LogInformation(Arg.Any<string>());
        }

        [Fact]
        public async Task CaptureSettlementDataAsync_ThrowException()
        {
            // Arrange
            var service = GetService();
            var exception = fixture.Create<Exception>();

            systemSettingService.When(x => x.GetSystemSettingNotificationAsync(Arg.Any<string>())).Throw(exception);

            // Act
            await service.CaptureSettlementDataAsync();

            // Assert
            await logService.Received(1).ErrorAsync(Arg.Any<Exception>());
        }

        [Theory]
        [InlineData("LeagueID", "1", "EN", "LeagueName")]
        [InlineData("SportType", "1", "EN", "SportName")]
        [InlineData("TeamID", "1", "EN", "TeamName")]
        [InlineData("ResourceCricket", "1", "EN", "ResourceCricketName")]
        [InlineData("ResultSettlement", "1", "EN", "ResultSettlementName")]
        [InlineData("test", "1", "EN", "")]
        public void GetResourceRemotingRef_ReturnExpected(string type, string id, string languageCode, string expected)
        {
            // Arrange
            var service = GetService();

            commonFeedback.GetLeagueName(id, languageCode).Returns("LeagueName");
            commonFeedback.GetSportName(id, languageCode).Returns("SportName");
            commonFeedback.GetTeamName(id, languageCode).Returns("TeamName");
            commonFeedback.GetResourceByRefIdAndResourceId(type, id, languageCode).Returns(expected);

            // Act
            var actual = service.Public_GetResourceRemotingRef(type, id, languageCode);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task GenerateResultAmendmentAsync_ExpectedCallExactFunction()
        {
            // Arrange
            var service = GetService();
            var serviceFlag = true;
            var settlementRemotingRefInfo = fixture.Create<IEnumerable<SettlementRemotingRefInfo>>();
            var refLanguage = "MNC";
            foreach (var item in settlementRemotingRefInfo)
            {
                item.RefId1 = "LeagueID";
                item.RefId2 = "1";
                item.RefValueEN = refLanguage;
                item.RefValueCH = refLanguage;
                item.RefValueCS = refLanguage;
                item.RefValueZHCN = refLanguage;
            }
            generateResultAmendmentService.GetPrepareGenNotificationAsync(Arg.Any<string>()).Returns(settlementRemotingRefInfo);
            systemSettingService.GetSystemSettingNotificationAsync(Arg.Any<string>()).Returns(serviceFlag);
            service.Public_GetResourceRemotingRef("LeagueID", "1", Arg.Any<string>()).Returns(refLanguage);

            // Act
            await service.GenerateResultAmendmentAsync();

            // Assert
            await generateResultAmendmentService.Received(1).InsertGenResultAmendmentAsync(Arg.Any<string>(), settlementRemotingRefInfo);
        }

        [Fact]
        public async Task GenerateResultAmendmentAsync_ExpectedReturnLogService()
        {
            // Arrange
            var service = GetService();

            var serviceFlag = false;
            systemSettingService.GetSystemSettingNotificationAsync(Arg.Any<string>()).Returns(serviceFlag);

            // Act
            await service.GenerateResultAmendmentAsync();

            // Assert
            logService.Received(1).LogInformation(Arg.Any<string>());
        }

        [Fact]
        public async Task GenerateResultAmendmentAsync_ThrowException()
        {
            // Arrange
            var service = GetService();
            var exception = fixture.Create<Exception>();

            systemSettingService.When(x => x.GetSystemSettingNotificationAsync(Arg.Any<string>())).Throw(exception);

            // Act
            await service.GenerateResultAmendmentAsync();

            // Assert
            await logService.Received(1).ErrorAsync(Arg.Any<Exception>());
        }

        [Fact]
        public async Task PushNotificationResultAmendmentAsync_ExpectedCallExactFunction()
        {
            // Arrange
            var service = GetService();
            var serviceFlag = true;
            var mncMessageTarget = new List<MncMessageTarget>
            {
                new () { NotificationID = 1, JSNotification = "MNC" }
            };
            var approveTest = fixture.Create<(bool, byte)>();
            var adminMessageSite = fixture.Create<AdminMessageSite>();

            systemSettingService.GetSystemSettingNotificationAsync(Arg.Any<string>()).Returns(serviceFlag);
            pushNotificationResultAmendmentService.GetPrepareNotificationAsync(Arg.Any<string>()).Returns(mncMessageTarget);
            pushNotificationResultAmendmentService.EditPushNotificationResultAmendmentAsync(1, Arg.Any<int>(), Arg.Any<string>(), "AfterInsMsg").Returns(approveTest);
            pushNotificationResultAmendmentService.EditPushMessageResultAmendmentAsync(Arg.Any<int>(), "AfterUpdNoti", approveTest.Item1, approveTest.Item2).Returns(adminMessageSite);

            // Act
            await service.PushNotificationResultAmendmentAsync();

            // Assert
            await pushNotificationResultAmendmentService.Received(1).InsertAdminMessageSiteAsync(adminMessageSite);
            await pushNotificationResultAmendmentService.Received(1).InsertCompleteResultAmendmentAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task PushNotificationResultAmendmentAsync_ExpectedReturnLogService()
        {
            // Arrange
            var service = GetService();

            var serviceFlag = false;
            systemSettingService.GetSystemSettingNotificationAsync(Arg.Any<string>()).Returns(serviceFlag);

            // Act
            await service.PushNotificationResultAmendmentAsync();

            // Assert
            logService.Received(1).LogInformation(Arg.Any<string>());
        }

        [Fact]
        public async Task PushNotificationResultAmendmentAsync_ThrowException()
        {
            // Arrange
            var service = GetService();
            var exception = fixture.Create<Exception>();

            systemSettingService.When(x => x.GetSystemSettingNotificationAsync(Arg.Any<string>())).Throw(exception);

            // Act
            await service.PushNotificationResultAmendmentAsync();

            // Assert
            await logService.Received(1).ErrorAsync(Arg.Any<Exception>());
        }

        [Fact]
        public async Task GenerateLapNotificationAsync_ExpectedCallExactFunction()
        {
            // Arrange
            var service = GetService();
            var serviceFlag = true;
            var settlementGenLap = fixture.Create<IEnumerable<SettlementRemotingRefInfo>>();

            systemSettingService.GetSystemSettingNotificationAsync(Arg.Any<string>()).Returns(serviceFlag);
            generateLapNotificationService.GetPrepareGenLapNotificationAsync(Arg.Any<string>()).Returns(settlementGenLap);

            // Act
            await service.GenerateLapNotificationAsync();

            // Assert
            await generateLapNotificationService.Received(1).InsertGenLapResultAmendmentAsync(settlementGenLap, Arg.Any<string>());
        }

        [Fact]
        public async Task GenerateLapNotificationAsync_ExpectedReturnLogService()
        {
            // Arrange
            var service = GetService();

            var serviceFlag = false;
            systemSettingService.GetSystemSettingNotificationAsync(Arg.Any<string>()).Returns(serviceFlag);

            // Act
            await service.GenerateLapNotificationAsync();

            // Assert
            logService.Received(1).LogInformation(Arg.Any<string>());
        }

        [Fact]
        public async Task GenerateLapNotificationAsync_ThrowException()
        {
            // Arrange
            var service = GetService();
            var exception = fixture.Create<Exception>();

            systemSettingService.When(x => x.GetSystemSettingNotificationAsync(Arg.Any<string>())).Throw(exception);

            // Act
            await service.GenerateLapNotificationAsync();

            // Assert
            await logService.Received(1).ErrorAsync(Arg.Any<Exception>());
        }

        [Fact]
        public async Task SendNotificationCenterAsync_ExpectedCallExactFunction()
        {
            // Arrange
            var service = GetService();
            var serviceFlag = true;
            var messages = fixture.Create<IEnumerable<MessageBotInfo>>();

            systemSettingService.GetSystemSettingNotificationAsync(Arg.Any<string>()).Returns(serviceFlag);
            sendNotificationCenterService.GetNotificationAlertAsync(Arg.Any<string>()).Returns(messages);

            // Act
            await service.SendNotificationCenterAsync();

            // Assert
            await sendNotificationCenterService.Received(1).InsertNotificationAlertAsync(Arg.Any<string>());
            await service.Received(1).Public_ProcessNotificationItemAsync(messages);
        }

        [Fact]
        public async Task SendNotificationCenterAsync_ExpectedReturnLogService()
        {
            // Arrange
            var service = GetService();

            var serviceFlag = false;
            systemSettingService.GetSystemSettingNotificationAsync(Arg.Any<string>()).Returns(serviceFlag);

            // Act
            await service.SendNotificationCenterAsync();

            // Assert
            logService.Received(1).LogInformation(Arg.Any<string>());
        }

        [Fact]
        public async Task SendNotificationCenterAsync_ThrowException()
        {
            // Arrange
            var service = GetService();
            var serviceFlag = true;
            var exception = fixture.Create<Exception>();

            systemSettingService.GetSystemSettingNotificationAsync(Arg.Any<string>()).Returns(serviceFlag);
            sendNotificationCenterService.When(x => x.GetNotificationAlertAsync(Arg.Any<string>())).Throw(exception);

            // Act
            await service.SendNotificationCenterAsync();

            // Assert
            await logService.Received(1).ErrorAsync(Arg.Any<Exception>());
        }

        [Fact]
        public async Task ProcessNotificationItemAsync_CallNextFunction()
        {
            // Arrange
            var service = GetService();
            var messages = new List<MessageBotInfo>
            {
                new() { ChatGroupID = "MNC", Content = "Content MNC", IsTest = 1 }
            };
            var conversationConfigs = new List<ConversationConfig>
            {
                new() { DisplayName = "MNC", ConversationId = "1", IsOnlySendTest = true}
            };

            // Act
            await service.Public_ProcessNotificationItemAsync(messages);

            // Assert
            await service.Received(1).Public_ProcessMessagesAsync(conversationConfigs, "Content MNC", 1);
        }

        [Fact]
        public async Task ProcessMessagesAsync_CallNextFunctionWithIsTestTrue()
        {
            // Arrange
            var service = GetService();
            var conversationConfigs = new List<ConversationConfig>
            {
                new() { DisplayName = "MNC", ConversationId = "1", IsOnlySendTest = true}
            };

            // Act
            await service.Public_ProcessMessagesAsync(conversationConfigs, "Content MNC", 1);

            // Assert
            await service.Received(1).Public_ProcessSendRealTestMessagesAsync(conversationConfigs, "Content MNC");
        }

        [Fact]
        public async Task ProcessMessagesAsync_CallNextFunctionWithIsTestFalse()
        {
            // Arrange
            var service = GetService();
            var conversationConfigs = new List<ConversationConfig>
            {
                new() { DisplayName = "MMM", ConversationId = "2", IsOnlySendTest = false}
            };

            // Act
            await service.Public_ProcessMessagesAsync(conversationConfigs, "Content MMM", 1);

            // Assert
            await service.Received(1).Public_ProcessSendOnlyTestMessagesAsync(conversationConfigs, "Content MNC");
        }

        [Fact]
        public async Task ProcessSendRealTestMessagesAsync_SendLogService()
        {
            // Arrange
            var service = GetService();
            var conversationConfigs = new List<ConversationConfig>
            {
                new() { DisplayName = "SYS", ConversationId = "2", IsOnlySendTest = false}
            };

            // Act
            await service.Public_ProcessSendRealTestMessagesAsync(conversationConfigs, "Content SYS");

            // Assert
            await logService.Received(1).ErrorMessageAsync("Content SYS");
        }

        [Fact]
        public async Task ProcessSendRealTestMessagesAsync_SendSkypeService()
        {
            // Arrange
            var service = GetService();
            var conversationConfigs = new List<ConversationConfig>
            {
                new() { DisplayName = "MNC", ConversationId = "1", IsOnlySendTest = true}
            };

            // Act
            await service.Public_ProcessSendRealTestMessagesAsync(conversationConfigs, "Content MNC");

            // Assert
            await botService.Received(1).SendSkypeAsync("1", "Content MNC");
        }

        [Fact]
        public async Task ProcessSendOnlyTestMessagesAsync_SendSkypeService()
        {
            // Arrange
            var service = GetService();
            var conversationConfigs = new List<ConversationConfig>
            {
                new() { DisplayName = "MNC", ConversationId = "1", IsOnlySendTest = true}
            };

            // Act
            await service.Public_ProcessSendOnlyTestMessagesAsync(conversationConfigs, "Content MNC");

            // Assert
            await botService.Received(1).SendSkypeAsync("1", "Content MNC");
        }

        #region Helpers

        private MessageNotificationCenterServiceTestable GetService()
        {
            botSetting = new BotServiceSetting
            {
                AppId = 1,
                ServiceUrl = "/test",
                ConversationConfigs = new List<ConversationConfig>
                {
                    new() { DisplayName = "MNC", ConversationId = "1", IsOnlySendTest = true},
                    new() { DisplayName = "MMM", ConversationId = "2", IsOnlySendTest = false}
                }
            };
            logService = Substitute.For<ICrmLogService>();
            configuration = Substitute.For<IConfiguration>();
            commonFeedback = Substitute.For<ICommonFeedback>();
            captureSettlementDataService = Substitute.For<ICaptureSettlementDataService>();
            generateResultAmendmentService = Substitute.For<IGenerateResultAmendmentService>();
            pushNotificationResultAmendmentService = Substitute.For<IPushNotificationResultAmendmentService>();
            generateLapNotificationService = Substitute.For<IGenerateLapNotificationService>();
            sendNotificationCenterService = Substitute.For<ISendNotificationCenterService>();
            systemSettingService = Substitute.For<ISystemSettingService>();
            botService = Substitute.For<IBotService>();
            return Substitute.For<MessageNotificationCenterServiceTestable>(
                logService,
                configuration,
                commonFeedback,
                captureSettlementDataService,
                generateResultAmendmentService,
                pushNotificationResultAmendmentService,
                generateLapNotificationService,
                sendNotificationCenterService,
                systemSettingService,
                botService,
                botSetting);
        }

        #endregion Helpers
    }
}