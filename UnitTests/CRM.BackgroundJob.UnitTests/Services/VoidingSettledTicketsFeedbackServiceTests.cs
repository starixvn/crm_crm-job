namespace CRM.BackgroundJob.UnitTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using CRM.BackgroundJob.Models.ChatBotResulAmendment;
    using CRM.BackgroundJob.Models.AmendmentNotiFB;
    using CRM.BackgroundJob.Models.VoidingSettledTickets;
    using CRM.BackgroundJob.Services;
    using CRM.BackgroundJob.UnitTests.Testable;
    using CRM.BackgroundJob.Core.Models;
    using CRM.BackgroundJob.Core.Services;
    using Fanex.CRM.BackgroundJob.Constants;
    using Fanex.CRM.BackgroundJob.Criteria;
    using Fanex.CRM.BackgroundJob.Models;
    using Fanex.CRM.BackgroundJob.Services;
    using Microsoft.Extensions.Options;
    using NSubstitute;
    using Xunit;

    public class VoidingSettledTicketsFeedbackServiceTests
    {
        private AppJobSettings appSettings;
        private IBaseService baseService;
        private ISettingService settingService;
        private ICrmLogService logService;
        private ICommonFeedback commonFeedback;
        private IVoidingSettledTicketsService voidingSettledTicketsService;
        private IFeedbackTransactionService feedbackTransaction;
        private readonly IFixture fixture = new Fixture();

        #region CreateFeedbackForVoidingSettledTickets

        [Fact]
        public async Task CreateFeedbackForVoidingSettledTickets_WhenCalled_CallExactFunction()
        {
            // Arrange
            var service = MockService();
            var settingNames = new[] { SettingName.Product, SettingName.SubProduct, SettingName.Bettype };
            var voidingSettledTicketSetting = fixture.Create<AmendmentNotiFbSetting>();
            var settings = fixture.Create<IEnumerable<SettingItem>>();
            var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();
            var messageVoidSettled = fixture.Create<IEnumerable<MessageVoidSettled>>();
            var voidingMessages = fixture.Create<IEnumerable<MessageVoidSettled>>();
            var failVoidingMessages = new List<MessageVoidSettled>();
            var voidingTickets = fixture.Create<ValueTuple<IEnumerable<VoidingTicketEventInfo>, IEnumerable<MsgTarget>>>();
            var feedbackInfos = fixture.Create<ValueTuple<IEnumerable<VoidingAutoFeedbackInfo>, IEnumerable<AutoVoidingEventDetails>>>();
            settingService.GetAllSettingAsync().Returns(settings);
            settingService.GetSystemSettingMappingAsync(settingNames).Returns(settingMappings);
            voidingSettledTicketsService.GetMessageVoidSettledAsync().Returns(messageVoidSettled);
            voidingSettledTicketsService.GetAutoVoidingMessageAsync(voidingSettledTicketSetting, "LIVE").Returns(voidingMessages);
            voidingSettledTicketsService.GetFailVoidingMessageAsync().Returns(failVoidingMessages);
            voidingSettledTicketsService.GetAutoVoidingTicketAsync(voidingSettledTicketSetting, Arg.Any<List<MessageVoidSettled>>(), "LIVE").Returns(voidingTickets);
            voidingSettledTicketsService.GetVoidingAutoFeedbackInfoAsync(voidingSettledTicketSetting.BatchSize).Returns(feedbackInfos);

            // Act
            await service.CreateFeedbackForVoidingSettledTickets(voidingSettledTicketSetting, true, voidingSettledTicketSetting.LastRuntime, SystemSettingName.LastRuntime, SystemSettingName.JobStatus, "LIVE");

            // Assert
            service.Received().Protected(
                "CreateVoidingSettledTicketsAsync",
                Arg.Any<VoidingAutoFeedbackInfo>(),
                Arg.Any<IEnumerable<AutoVoidingEventDetails>>(),
                Arg.Any<IEnumerable<Message>>(),
                Arg.Any<IEnumerable<SettingItem>>(),
                Arg.Any<IEnumerable<SystemSettingMapping>>(),
                Arg.Any<string>());
            await commonFeedback.Received(2).EditSystemSettingAsyncByEnvFormatDate(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task CreateFeedbackForVoidingSettledTickets_MessageCheckHaveData_CallExactFunction()
        {
            // Arrange
            var service = MockService();
            var commandResult = new CommandResult { Code = 1 };
            var settingNames = new[] { SettingName.Product, SettingName.SubProduct, SettingName.Bettype };
            var voidingSettledTicketSetting = fixture.Create<AmendmentNotiFbSetting>();
            var settings = fixture.Create<IEnumerable<SettingItem>>();
            var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();
            var messageVoidSettled = fixture.Create<IEnumerable<MessageVoidSettled>>();
            var voidingMessageCheck = fixture.Create<IEnumerable<MessageVoidSettled>>();
            var settledTicket = fixture.Create<IEnumerable<MessageVoidSettled>>();
            settingService.GetAllSettingAsync().Returns(settings);
            settingService.GetSystemSettingMappingAsync(settingNames).Returns(settingMappings);
            voidingSettledTicketsService.GetMessageVoidSettledAsync().Returns(messageVoidSettled);
            voidingSettledTicketsService.GetVoidingMessasgeCheckAsync(messageVoidSettled, "LIVE").Returns(voidingMessageCheck);
            voidingSettledTicketsService.VoidingMessageGetSettledTicketAsync(voidingMessageCheck).Returns(settledTicket);
            voidingSettledTicketsService.ImportMessageVoidSettledAsync(settledTicket).Returns(commandResult);

            // Act
            await service.CreateFeedbackForVoidingSettledTickets(voidingSettledTicketSetting, true, voidingSettledTicketSetting.LastRuntime, SystemSettingName.LastRuntime, SystemSettingName.JobStatus, "LIVE");

            // Assert

            logService.Received(2).LogInformation(Arg.Any<string>());
        }

        #endregion CreateFeedbackForVoidingSettledTickets

        #region CreateVoidingSettledTicketsAsync

        public async Task CreateVoidingSettledTicketsAsync_WhenCalled_CallExactFunction()
        {
            // Arrange
            var service = MockService();
            var category = fixture.Create<Tuple<int, int, string>>();
            service.Protected("GetCategorySubCategoryVoidingSettledTicket", Arg.Any<IEnumerable<SettingItem>>(), DefaultValue.ResultAmended).Returns(category);
            service.Protected("GetVoidingSettledTicketsProductXml", Arg.Any<string>(), Arg.Any<short>(), Arg.Any<string>(), Arg.Any<IEnumerable<SettingItem>>(), Arg.Any<IEnumerable<SystemSettingMapping>>()).Returns("Product");
            service.Protected("GetVoidingSettledTicketSolution", Arg.Any<VoidingAutoFeedbackInfo>(), Arg.Any<IEnumerable<Message>>()).Returns("Solution");
            service.Protected("GetRelatedCustomerXml", Arg.Any<VoidingAutoFeedbackInfo>()).Returns("RelatedCustomer");
            service.Protected("GetVoidingSettledTicketDescription", Arg.Any<VoidingAutoFeedbackInfo>(), Arg.Any<IEnumerable<AutoVoidingEventDetails>>()).Returns("Description");

            // Act
            await service.Public_CreateVoidingSettledTicketsAsync(Arg.Any<VoidingAutoFeedbackInfo>(), Arg.Any<IEnumerable<AutoVoidingEventDetails>>(),
                Arg.Any<IEnumerable<Message>>(), Arg.Any<IEnumerable<SettingItem>>(), Arg.Any<IEnumerable<SystemSettingMapping>>(), "LIVE");

            // Assert
            service.Received().Protected(
                "CheckValidVoidingSettledParamsAndInsertFeedbackAsync",
                Arg.Any<VoidingAutoFeedbackInfo>(),
                Arg.Any<CreateMutipleFeedbackCommand>(),
                Arg.Any<string>());
        }

        #endregion CreateVoidingSettledTicketsAsync

        #region CheckValidVoidingSettledParamsAndInsertFeedbackAsync

        [Fact]
        public async Task CheckValidVoidingSettledParamsAndInsertFeedbackAsync_CreateSuccessfull_ExpectedWriteLog()
        {
            // Arrange
            var feedbackInfo = fixture.Create<VoidingAutoFeedbackInfo>();
            var criteria = fixture.Create<CreateMutipleFeedbackCommand>();
            var service = MockService();
            commonFeedback.CheckFeedbackValidParams(criteria).Returns(new ValueTuple<bool, string>(true, ""));

            var commandResult = new CommandResult { Code = 0 };
            baseService.ExecuteNonQueryAsync(criteria).Returns(commandResult);

            // Act
            await service.Public_CheckValidVoidingSettledParamsAndInsertFeedbackAsync(feedbackInfo, criteria, "LIVE");

            // Assert
            logService.Received(3).LogInformation(Arg.Any<string>());
        }

        [Fact]
        public async Task CheckValidVoidingSettledParamsAndInsertFeedbackAsync_CreateUnSuccessfull_ExpectedWriteLogAndUpdateStatus()
        {
            var feedbackInfo = fixture.Create<VoidingAutoFeedbackInfo>();
            var criteria = fixture.Create<CreateMutipleFeedbackCommand>();
            var service = MockService();
            commonFeedback.CheckFeedbackValidParams(criteria).Returns(new ValueTuple<bool, string>(true, ""));

            var commandResult = new CommandResult { Code = 1, Message = "" };
            baseService.ExecuteNonQueryAsync(criteria).Returns(commandResult);

            // Act
            await service.Public_CheckValidVoidingSettledParamsAndInsertFeedbackAsync(feedbackInfo, criteria, "LIVE");

            // Assert
            logService.Received(3).LogInformation(Arg.Any<string>());
            await voidingSettledTicketsService.UpdateVoidingAutoFeedbackInfoBatchAsync(1, CreateFeedbackJobStatus.Fail, commandResult.Message, "LIVE");
            await logService.ErrorAsync(commandResult, criteria);
        }

        [Fact]
        public async Task CheckValidVoidingSettledParamsAndInsertFeedbackAsync_InvalidParams_ExpectedWriteLogAndUpdateStatus()
        {
            var feedbackInfo = fixture.Create<VoidingAutoFeedbackInfo>();
            var criteria = fixture.Create<CreateMutipleFeedbackCommand>();
            var service = MockService();
            commonFeedback.CheckFeedbackValidParams(criteria).Returns(new ValueTuple<bool, string>(false, "invalid"));

            var commandResult = new CommandResult { Code = 1, Message = "" };
            baseService.ExecuteNonQueryAsync(criteria).Returns(commandResult);

            // Act
            await service.Public_CheckValidVoidingSettledParamsAndInsertFeedbackAsync(feedbackInfo, criteria, "LIVE");

            // Assert
            logService.Received(1).LogInformation(Arg.Any<string>());
            await voidingSettledTicketsService.UpdateVoidingAutoFeedbackInfoBatchAsync(1, CreateFeedbackJobStatus.Fail, commandResult.Message, "LIVE");
        }

        [Fact]
        public async Task CheckValidAmendmentParamsAndInsertFeedbackAsync_Exception_ExpectedWriteLogAndUpdateStatus()
        {
            // Arrange
            var feedbackInfo = fixture.Create<VoidingAutoFeedbackInfo>();
            var exception = fixture.Create<Exception>();
            var criteria = fixture.Create<CreateMutipleFeedbackCommand>();
            var service = MockService();
            commonFeedback.CheckFeedbackValidParams(criteria).Returns(new ValueTuple<bool, string>(true, ""));

            baseService.When(x => x.ExecuteNonQueryAsync(criteria)).Throw(exception);

            // Act
            await service.Public_CheckValidVoidingSettledParamsAndInsertFeedbackAsync(feedbackInfo, criteria, "LIVE");

            // Assert
            await voidingSettledTicketsService.UpdateVoidingAutoFeedbackInfoBatchAsync(feedbackInfo.LogVoidingID, CreateFeedbackJobStatus.Fail, exception.Message, "LIVE");
            await logService.ErrorAsync(exception);
        }

        #endregion CheckValidVoidingSettledParamsAndInsertFeedbackAsync

        #region GetVoidingSettledTicketsProductXml

        [Fact]
        public void GetVoidingSettledTicketsProductXml_ExistedProduct_ReturnExpectedXml()
        {
            // Arrange
            var service = MockService();
            var settings = fixture.Create<IEnumerable<SettingItem>>();
            var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();
            var isMixParlay = fixture.Create<bool>();
            var products = new List<SettingItem>
            {
                new SettingItem{ SettingId = 1 }
            };
            var subProducts = new List<SettingItem>
            {
                new SettingItem{ SettingId = 1 }
            };
            var bettypesMapping = new List<SystemSettingMapping>
            {
                new SystemSettingMapping
                {
                    SettingId = 1
                }
            };
            var productSubproductXml = "<Product ProductID='1' SubProductID='1' BetTypeID='1' />";
            var productIds = new List<int> { 1 };
            var sportTypeIds = new List<short> { 1 };
            service.Protected("GetVoidingSettledTicketsProduct", settings, settingMappings, productIds).Returns(products);
            service.Protected("GetVoidingSettledTicketsSubProduct", isMixParlay, products.Select(x => x.SettingId), sportTypeIds, settings, settingMappings).Returns(subProducts);
            service.Protected("GetVoidingSettledTicketsBettype", isMixParlay, Arg.Any<string[]>(), settingMappings).Returns(bettypesMapping);
            service.Protected("GetProductSubXml", Arg.Any<SettingItem>(), Arg.Any<IList<SettingItem>>(), Arg.Any<IList<SystemSettingMapping>>()).Returns(productSubproductXml);

            // Act
            var actual = service.Public_GetVoidingSettledTicketsProductXml(isMixParlay, productIds, sportTypeIds, "1,2", settings, settingMappings);

            // Assert
            Assert.Equal("<ROOT><Product ProductID='1' SubProductID='1' BetTypeID='1' /></ROOT>", actual);
        }

        [Fact]
        public void GetVoidingSettledTicketsProductXml_DoNotExistProduct_ReturnExpectedXml()
        {
            // Arrange
            var service = MockService();
            var settings = fixture.Create<IEnumerable<SettingItem>>();
            var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();
            var products = new List<SettingItem>();
            var isMixParlay = fixture.Create<bool>();
            var subProducts = new List<SettingItem>
            {
                new SettingItem{ SettingId = 1 }
            };
            var bettypesMapping = new List<SystemSettingMapping>
            {
                new SystemSettingMapping
                {
                    SettingId = 1
                }
            };
            var productIds = new List<int> { 1 };
            var sportTypeIds = new List<short> { 1 };
            service.Protected("GetVoidingSettledTicketsProduct", settings, settingMappings, productIds).Returns(products);
            service.Protected("GetVoidingSettledTicketsSubProduct", isMixParlay, products.Select(x => x.SettingId), sportTypeIds, settings, settingMappings).Returns(subProducts);
            service.Protected("GetVoidingSettledTicketsBettype", isMixParlay, Arg.Any<string[]>(), settingMappings).Returns(bettypesMapping);

            // Act
            var actual = service.Public_GetVoidingSettledTicketsProductXml(true, productIds, sportTypeIds, "1,2", settings, settingMappings);

            // Assert
            Assert.Equal("<ROOT><Product ProductID='46004' SubProductID='8063' BetTypeID='9144' /></ROOT>", actual);
        }

        #endregion GetVoidingSettledTicketsProductXml

        #region GetProductSubXml

        [Fact]
        public void GetProductSubXml_NotExistSubProduct_ReturnDefaultProduct()
        {
            // Arrange
            var service = MockService();
            var settingMappings = fixture.Create<IList<SystemSettingMapping>>();
            var products = new SettingItem { SettingId = 1 };
            var subProducts = new List<SettingItem>();

            // Act
            var actual = service.Public_GetProductSubXml(products, subProducts, settingMappings);

            // Assert
            Assert.Equal("<Product ProductID='1' SubProductID='8063' BetTypeID='9144' />", actual);
        }

        [Fact]
        public void GetProductSubXml_ExistSubProductAndBettype_ReturnDefaultProduct()
        {
            // Arrange
            var service = MockService();
            var settingMappings = fixture.Create<IList<SystemSettingMapping>>();
            var products = fixture.Create<SettingItem>();
            var subProducts = new List<SettingItem> { new SettingItem { SettingId = 1 } };
            service.Protected("GetBettpeXml", Arg.Any<SettingItem>(), Arg.Any<SettingItem>(), Arg.Any<IList<SystemSettingMapping>>()).Returns("test");

            // Act
            var actual = service.Public_GetProductSubXml(products, subProducts, settingMappings);

            // Assert
            Assert.Equal("test", actual);
        }

        #endregion GetProductSubXml

        #region GetVoidingSettledTicketsProduct

        [Fact]
        public void GetVoidingSettledTicketsProduct_NotExistProductId_ReturnDefaultProduct()
        {
            // Arrange
            var setting = new List<SettingItem>
                {
                    new SettingItem { SettingName = "1", SettingValue = "1" }
                };

            var settingMappings = new List<SystemSettingMapping>
                {
                    new SystemSettingMapping { SettingName = "2", SettingValue = "1" , SettingSource = "1"}
                };
            var productIds = new List<int> { 1 };
            var service = MockService();
            commonFeedback.GetProductName("1", Language.en_US).Returns("2");

            // Act
            var actual = service.Public_GetVoidingSettledTicketsProduct(setting, settingMappings, productIds);

            // Assert
            Assert.Equal(BackgroundJobConstants.ProductNA, actual.First().SettingId);
        }

        [Fact]
        public void GetVoidingSettledTicketsProduct_SettingNotEqualProductMapping_ReturnDefaultProduct()
        {
            // Arrange
            var setting = new List<SettingItem>
                {
                    new SettingItem { SettingName = "2", SettingValue = "2" }
                };

            var settingMappings = new List<SystemSettingMapping>
                {
                    new SystemSettingMapping { SettingName = "1", SettingValue = "1" , SettingSource = "1"}
                };
            var productIds = new List<int> { 1 };

            var service = MockService();
            commonFeedback.GetProductName("1", Language.en_US).Returns("1");

            // Act
            var actual = service.Public_GetVoidingSettledTicketsProduct(setting, settingMappings, productIds);

            // Assert
            Assert.Equal(BackgroundJobConstants.ProductNA, actual.First().SettingId);
        }

        [Fact]
        public void GetVoidingSettledTicketsProduct_SettingEqualProductMapping_ReturnProduct()
        {
            // Arrange
            var setting = new List<SettingItem>
                {
                    new SettingItem { SettingId = 1, SettingName = SettingName.Product, SettingValue = "1" }
                };

            var settingMappings = new List<SystemSettingMapping>
                {
                    new SystemSettingMapping { SettingId = 1, SettingName = SettingName.Product, SettingValue = "1" , SettingSource = "1"}
                };
            var productIds = new List<int> { 1 };
            var service = MockService();
            commonFeedback.GetProductName("1", Language.en_US).Returns("1");

            // Act
            var actual = service.Public_GetVoidingSettledTicketsProduct(setting, settingMappings, productIds);

            // Assert
            Assert.Equal(1, actual.First().SettingId);
        }

        #endregion GetVoidingSettledTicketsProduct

        #region GetVoidingSettledTicketsSubProduct

        [Fact]
        public void GetVoidingSettledTicketsSubProduct_NotExistsportName_ReturnDefaultSubProduct()
        {
            // Arrange
            var setting = new List<SettingItem>
                {
                    new SettingItem { SettingName = "1", SettingValue = "1" }
                };

            var sportTypeIds = new List<short> { 1 };
            var productIds = new List<int> { 1 };

            var settingMappings = new List<SystemSettingMapping>
                {
                    new SystemSettingMapping { SettingName = "2", SettingValue = "1" , SettingSource = "1"}
                };
            var service = MockService();
            commonFeedback.GetSportName("1", Language.en_US).Returns("1");

            // Act
            var actual = service.Public_GetVoidingSettledTicketsSubProduct(true, productIds, sportTypeIds, setting, settingMappings);

            // Assert
            Assert.Single(actual);
            Assert.Equal(BackgroundJobConstants.SubProductNA, actual.First().SettingId);
        }

        [Fact]
        public void GetVoidingSettledTicketsSubProduct_SettingNotEqualMapping_ReturnDefaultSubProduct()
        {
            // Arrange
            var setting = new List<SettingItem>
                {
                    new SettingItem { SettingName = SettingName.SubProduct, SettingValue = "1" , SettingId = 2, ParentIds = new List<int>{ 1,2} }
                };

            var sportTypeIds = new List<short> { 1 };
            var productIds = new List<int> { 1 };
            var settingMappings = new List<SystemSettingMapping>
                {
                    new SystemSettingMapping { SettingName = "1", SettingValue = "1" , SettingSource = "1", SettingId = 1}
                };
            var service = MockService();
            commonFeedback.GetProductName("1", Language.en_US).Returns("1");

            // Act
            var actual = service.Public_GetVoidingSettledTicketsSubProduct(true, productIds, sportTypeIds, setting, settingMappings);

            // Assert
            Assert.Equal(BackgroundJobConstants.SubProductNA, actual.First().SettingId);
        }

        [Fact]
        public void GetVoidingSettledTicketsSubProduct_SportNameNotEqualMapping_ReturnDefaultSubProduct()
        {
            // Arrange
            var setting = new List<SettingItem>
                {
                    new SettingItem { SettingName = SettingName.SubProduct, SettingValue = "1" , SettingId = 2, ParentIds = new List<int>{ 1,2} }
                };

            var sportTypeIds = new List<short> { 1 };
            var productIds = new List<int> { 1 };
            var settingMappings = new List<SystemSettingMapping>
                {
                    new SystemSettingMapping { SettingName = "1", SettingValue = "1" , SettingSource = "1", SettingId = 1}
                };
            var service = MockService();
            commonFeedback.GetProductName("2", Language.en_US).Returns("1");

            // Act
            var actual = service.Public_GetVoidingSettledTicketsSubProduct(true, productIds, sportTypeIds, setting, settingMappings);

            // Assert
            Assert.Equal(BackgroundJobConstants.SubProductNA, actual.First().SettingId);
        }

        [Fact]
        public void GetVoidingSettledTicketsSubProduct_ExistSportName_ReturnSubProduct()
        {
            // Arrange
            var setting = new List<SettingItem>
                {
                    new SettingItem { SettingName = SettingName.SubProduct, SettingValue = "1" , SettingId = 1, ParentIds = new List<int>{ 1,2} }
                };

            var sportTypeIds = new List<short> { 1 };
            var productIds = new List<int> { 1 };

            var settingMappings = new List<SystemSettingMapping>
                {
                    new SystemSettingMapping { SettingName = SettingName.SubProduct, SettingValue = "1" , SettingSource = "1", SettingId = 1}
                };

            var service = MockService();
            commonFeedback.GetSportName("1", Language.en_US).Returns("1");

            // Act
            var actual = service.Public_GetVoidingSettledTicketsSubProduct(false, productIds, sportTypeIds, setting, settingMappings);

            // Assert
            Assert.Equal(1, actual.First().SettingId);
        }

        [Fact]
        public void GetVoidingSettledTicketsSubProduct_NotExistSportName_ReturnSubProductNA()
        {
            // Arrange
            var setting = new List<SettingItem>
                {
                    new SettingItem { SettingName = SettingName.SubProduct, SettingValue = "1" , SettingId = 1, ParentIds = new List<int>{ 1,2} }
                };

            var sportTypeIds = new List<short> { 1 };
            var productIds = new List<int> { 1 };

            var settingMappings = new List<SystemSettingMapping>
                {
                    new SystemSettingMapping { SettingName = SettingName.SubProduct, SettingValue = "1" , SettingSource = "1", SettingId = 1}
                };

            var service = MockService();
            commonFeedback.GetSportName("1", Language.en_US).Returns("1");

            // Act
            var actual = service.Public_GetVoidingSettledTicketsSubProduct(true, productIds, sportTypeIds, setting, settingMappings);

            // Assert
            Assert.Equal(BackgroundJobConstants.SubProductNA, actual.First().SettingId);
        }

        #endregion GetVoidingSettledTicketsSubProduct

        #region GetVoidingSettledTicketsBettype

        [Fact]
        public void GetVoidingSettledTicketsBettype_betTypeMappingsNotEqualSettingSource_ReturnDefaultBetType()
        {
            // Arrange
            var settingMappings = new List<SystemSettingMapping>
                {
                    new SystemSettingMapping { SettingName = "Bettype", SettingValue = "1" , SettingSource = "1"}
                };

            var betTypeMappings = new string[] { "2", "3" };
            var service = MockService();

            // Act
            var actual = service.Public_GetVoidingSettledTicketsBettype(true, betTypeMappings, settingMappings);

            // Assert
            Assert.Single(actual);
            Assert.Equal(BackgroundJobConstants.BettypeNA, actual.First().SettingId);
        }

        [Fact]
        public void GetVoidingSettledTicketsBettype_SettingSourceIsNull_ReturnDefaultBetType()
        {
            // Arrange
            var settingMappings = new List<SystemSettingMapping>
                {
                    new SystemSettingMapping { SettingName = "Bettype", SettingValue = "1"}
                };

            var betTypeMappings = new string[] { "2", "3" };
            var service = MockService();

            // Act
            var actual = service.Public_GetVoidingSettledTicketsBettype(true, betTypeMappings, settingMappings);

            // Assert
            Assert.Single(actual);
            Assert.Equal(BackgroundJobConstants.BettypeNA, actual.First().SettingId);
        }

        [Fact]
        public void GetVoidingSettledTicketsBettype_SettingNameNotEqualBettype_ReturnDefaultBetType()
        {
            // Arrange
            var settingMappings = new List<SystemSettingMapping>
                {
                    new SystemSettingMapping { SettingName = "Bettype1", SettingValue = "1", SettingSource = "2"}
                };

            var betTypeMappings = new string[] { "2", "3" };
            var service = MockService();

            // Act
            var actual = service.Public_GetVoidingSettledTicketsBettype(true, betTypeMappings, settingMappings);

            // Assert
            Assert.Single(actual);
            Assert.Equal(BackgroundJobConstants.BettypeNA, actual.First().SettingId);
        }

        [Fact]
        public void GetVoidingSettledTicketsBettype_ExistBettypes_ReturnBetType()
        {
            // Arrange
            var settingMappings = new List<SystemSettingMapping>
                {
                    new SystemSettingMapping { SettingName = "Bettype", SettingValue = "1", SettingSource = "BettypeName", SettingId = 1}
                };

            var listBetTypes = new string[] { "2", "3" };
            var service = MockService();
            commonFeedback.GetBettypeName(Arg.Any<string>(), Language.en_US).Returns("BettypeName");

            // Act
            var actual = service.Public_GetVoidingSettledTicketsBettype(false, listBetTypes, settingMappings);

            // Assert
            Assert.Single(actual);
            Assert.Equal(1, actual.First().SettingId);
        }

        [Fact]
        public void GetVoidingSettledTicketsBettype_NotExistBettypes_ReturnBetTypeNA()
        {
            // Arrange
            var settingMappings = new List<SystemSettingMapping>
                {
                    new SystemSettingMapping { SettingName = "Bettype", SettingValue = "1", SettingSource = "BettypeName", SettingId = 1}
                };

            var listBetTypes = new string[] { "2", "3" };
            var service = MockService();
            commonFeedback.GetBettypeName(Arg.Any<string>(), Language.en_US).Returns("BettypeName");

            // Act
            var actual = service.Public_GetVoidingSettledTicketsBettype(true, listBetTypes, settingMappings);

            // Assert
            Assert.Single(actual);
            Assert.Equal(BackgroundJobConstants.BettypeNA, actual.First().SettingId);
        }

        #endregion GetVoidingSettledTicketsBettype

        #region GetCategorySubCategoryVoidingSettledTicket

        [Fact]
        public void GetCategorySubCategoryResultAmendment_NotExistSettingNameMainCategory_ReturnDefaultMainCategoryXML()
        {
            // Arrange
            var settings = new List<SettingItem>
                {
                    new SettingItem { SettingName = "MainCategory1", SettingValue = DefaultValue.ResultAmended }
                };
            var service = MockService();

            // Act
            var actual = service.Public_GetCategorySubCategoryVoidingSettledTicket(settings, DefaultValue.ResultAmended, 10);

            // Assert
            Assert.Equal(0, actual.Item1);
            Assert.Equal(0, actual.Item2);
            Assert.Equal("<ROOT><Category MainCategoryID='0' SubCategoryID='0' /></ROOT>", actual.Item3);
        }

        [Fact]
        public void GetCategorySubCategoryVoidingSettledTicket_NotExistsettingValueName_ReturnDefaultMainCategoryXML()
        {
            // Arrange
            var settings = new List<SettingItem>
                {
                    new SettingItem { SettingName = "MainCategory", SettingValue = DefaultValue.ResultAmended }
                };
            var service = MockService();

            // Act
            var actual = service.Public_GetCategorySubCategoryVoidingSettledTicket(settings, "test", 20);

            // Assert
            Assert.Equal(0, actual.Item1);
            Assert.Equal(0, actual.Item2);
            Assert.Equal("<ROOT><Category MainCategoryID='0' SubCategoryID='0' /></ROOT>", actual.Item3);
        }

        [Fact]
        public void GetCategorySubCategoryVoidingSettledTicket_NotExistsSubId_ReturnDefaultMainCategoryXML()
        {
            // Arrange
            var settings = new List<SettingItem>
                {
                    new SettingItem { SettingId = 1, SettingName = "MainCategory", SettingValue = DefaultValue.ResultAmended },
                    new SettingItem { SettingName = "SubCategory", SettingId =  1}
                };
            var service = MockService();

            // Act
            var actual = service.Public_GetCategorySubCategoryVoidingSettledTicket(settings, "test", 10);

            // Assert
            Assert.Equal(0, actual.Item1);
            Assert.Equal(0, actual.Item2);
            Assert.Equal("<ROOT><Category MainCategoryID='0' SubCategoryID='0' /></ROOT>", actual.Item3);
        }

        [Fact]
        public void GetCategorySubCategoryVoidingSettledTicket_ExistMainCategoryAndExistSubCategory_ReturnExpectedTuple()
        {
            // Arrange
            var settings = new List<SettingItem>
                {
                    new SettingItem { SettingId = 1, SettingName = "MainCategory", SettingValue = DefaultValue.ResultAmended },
                    new SettingItem { SettingName = "SubCategory", SettingId =  4029}
                };

            var service = MockService();

            // Act
            var actual = service.Public_GetCategorySubCategoryVoidingSettledTicket(settings, DefaultValue.ResultAmended, 10);

            // Assert
            Assert.Equal(1, actual.Item1);
            Assert.Equal(4029, actual.Item2);
            Assert.Equal("<ROOT><Category MainCategoryID='1' SubCategoryID='4029' /></ROOT>", actual.Item3);
        }

        [Fact]
        public void GetCategorySubCategoryVoidingSettledTicket_ExistMainCategoryAndNotExistSubCategory_ReturnExpectedTuple()
        {
            // Arrange
            var settings = new List<SettingItem>
                {
                    new SettingItem { SettingId = 1, SettingName = "MainCategory", SettingValue = DefaultValue.ResultAmended },
                    new SettingItem { SettingName = "SubCategory", SettingId =  2}
                };

            var service = MockService();

            // Act
            var actual = service.Public_GetCategorySubCategoryVoidingSettledTicket(settings, DefaultValue.ResultAmended, 10);

            // Assertg
            Assert.Equal(1, actual.Item1);
            Assert.Equal(0, actual.Item2);
            Assert.Equal("<ROOT><Category MainCategoryID='1' SubCategoryID='0' /></ROOT>", actual.Item3);
        }

        #endregion GetCategorySubCategoryVoidingSettledTicket

        #region GetRelatedCustomerXml

        [Fact]
        public void GetRelatedCustomerXml_IsEmptyOrNullAffectedId_ReturnExactDefaultTemplate()
        {
            // Arrange
            var feedbackInfo = new VoidingAutoFeedbackInfo();
            var service = MockService();

            // Act
            var actual = service.Public_GetRelatedCustomerXml(feedbackInfo);

            // Assert
            Assert.Equal("<ROOT></ROOT>", actual);
        }

        [Fact]
        public void GetRelatedCustomerXml_AffectedIdAreNotNull_ReturnExactTemplate()
        {
            // Arrange
            var feedbackInfo = new VoidingAutoFeedbackInfo
            {
                AffectedId = "1,2",
                HeadlineId = 1,
                AffectedName = "1,2"
            };
            var service = MockService();
            var expect = "<ROOT><Row CustId='1' RelatedCustId='1' Username='1' /><Row CustId='1' RelatedCustId='2' Username='2' /></ROOT>";

            // Act
            var actual = service.Public_GetRelatedCustomerXml(feedbackInfo);

            // Assert
            Assert.Equal(expect, actual);
        }

        #endregion GetRelatedCustomerXml

        #region GetVoidingSettledTicketSolution

        [Fact]
        public void GetVoidingSettledTicketSolution_IsVip_ReturnExactSolution()
        {
            // Arrange
            var service = MockService();
            var feedbackInfo = new VoidingAutoFeedbackInfo { IsVip = true, SiteLabel = "789y" };
            var ticketSolTemplate = new VoidingSettledTicketSolutionTemplate
            {
                SolutionENSC = "SolutionENSC",
                SolutionENYN = "SolutionENYN"
            };
            var messages = fixture.Create<IEnumerable<Message>>();

            commonFeedback.GetFeedbackTemplates<VoidingSettledTicketSolutionTemplate>("VoidingSettledTicketSolutionTemplate.xml").Returns(ticketSolTemplate);
            var expect = "SolutionENYN";

            // Act
            var actual = service.Public_GetVoidingSettledTicketSolution(feedbackInfo, messages);

            // Assert
            Assert.Equal(expect, actual);
        }

        [Fact]
        public void GetVoidingSettledTicketSolution_IsNotVip_ReturnExactSolution()
        {
            // Arrange
            var service = MockService();
            fixture.Customize<VoidingAutoFeedbackInfo>(x => x.With(b => b.IsVip, false));
            var feedbackInfo = fixture.Create<VoidingAutoFeedbackInfo>();
            var ticketSolTemplate = new VoidingSettledTicketSolutionTemplate
            {
                SolutionENSC = "SolutionENSC",
                SolutionENYN = "SolutionENYN"
            };
            var messages = fixture.Create<IEnumerable<Message>>();

            commonFeedback.GetFeedbackTemplates<VoidingSettledTicketSolutionTemplate>("VoidingSettledTicketSolutionTemplate.xml").Returns(ticketSolTemplate);
            var expect = "SolutionENSC";

            // Act
            var actual = service.Public_GetVoidingSettledTicketSolution(feedbackInfo, messages);

            // Assert
            Assert.Equal(expect, actual);
        }

        #endregion GetVoidingSettledTicketSolution

        #region GetVoidingSettledTicketDescription

        [Fact]
        public void GetVoidingSettledTicketDescription_IsMainParlay_ReturnExpectedDescription()
        {
            // Arrange
            var service = MockService();
            var feedbackInfo = fixture.Create<VoidingAutoFeedbackInfo>();
            var eventDetails = new List<AutoVoidingEventDetails> { new AutoVoidingEventDetails { MatchId = 9, MatchCode = "MatchCode" } };
            var ticketDescTemplate = fixture.Create<VoidingSettledTicketDescriptionTemplate>();
            commonFeedback.GetFeedbackTemplates<VoidingSettledTicketDescriptionTemplate>("VoidingSettledTicketDescriptionTemplate.xml").Returns(ticketDescTemplate);
            const string messageTemplate = "<span style=\"color: rgb(255, 0, 0);\">Msg ID: {0} {1}</span>";
            var stringBuilder = new StringBuilder(DefaultValue.autoGenerated);
            stringBuilder.Append(BackgroundJobConstants.Break);
            stringBuilder.AppendFormat(messageTemplate, feedbackInfo.MessageId, BackgroundJobConstants.Break);
            stringBuilder.Append(BackgroundJobConstants.Break);
            const string mainParlayticketsText = "<span style =\"font-weight:bold;\">Following Members have voided settled Main Parlay tickets: </span>";
            stringBuilder.Append(mainParlayticketsText);
            stringBuilder.Append(BackgroundJobConstants.Break);
            const string templateMainParlaytickets = "- {0}";
            stringBuilder.AppendFormat(templateMainParlaytickets, "MatchCode");

            // Act
            var actual = service.Public_GetVoidingSettledTicketDescription(feedbackInfo, eventDetails);

            // Assert
            Assert.Equal(stringBuilder.ToString(), actual);
        }

        [Fact]
        public void GetVoidingSettledTicketDescription_IsSettledTickets_ReturnExpectedDescription()
        {
            // Arrange
            var service = MockService();
            fixture.Customize<VoidingAutoFeedbackInfo>(x => x.With(b => b.ProductId, "1"));
            var feedbackInfo = fixture.Create<VoidingAutoFeedbackInfo>();
            var eventDetails = new List<AutoVoidingEventDetails> { new AutoVoidingEventDetails { MatchCode = "MatchCode", MatchId = 7 } };
            var ticketDescTemplate = fixture.Create<VoidingSettledTicketDescriptionTemplate>();
            commonFeedback.GetFeedbackTemplates<VoidingSettledTicketDescriptionTemplate>("VoidingSettledTicketDescriptionTemplate.xml").Returns(ticketDescTemplate);
            const string messageTemplate = "<span style=\"color: rgb(255, 0, 0);\">Msg ID: {0} {1}</span>";
            var stringBuilder = new StringBuilder(DefaultValue.autoGenerated);
            stringBuilder.Append(BackgroundJobConstants.Break);
            stringBuilder.AppendFormat(messageTemplate, feedbackInfo.MessageId, BackgroundJobConstants.Break);
            const string settledTicketsText = "<span style =\"font-weight:bold;\">Following events have voided settled tickets: </span>";
            stringBuilder.Append(settledTicketsText);
            stringBuilder.Append(BackgroundJobConstants.Break);
            service.Protected("GetVoidingSettledTicketDescriptionByProduct", Arg.Any<AutoVoidingEventDetails>(), ticketDescTemplate, feedbackInfo.Site).Returns("test");
            stringBuilder.Append("test");

            // Act
            var actual = service.Public_GetVoidingSettledTicketDescription(feedbackInfo, eventDetails);

            // Assert
            Assert.Equal(stringBuilder.ToString(), actual);
        }

        #endregion GetVoidingSettledTicketDescription

        #region GetVoidingSettledTicketDescriptionByProduct

        [Fact]
        public void GetVoidingSettledTicketDescriptionByProduct_ProductIsVirtualSports_ReturnExpectedDescription()
        {
            // Arrange
            var service = MockService();
            var stringBuilder = new StringBuilder();
            var eventDetail = new AutoVoidingEventDetails
            {
                Sporttype = 180,
                LeagueId = 1,
                HomeId = 1,
                AwayId = 1,
                ProductId = 1,
                VendorMatchId = "1"
            };
            var ticketDescTemplate = fixture.Create<VoidingSettledTicketDescriptionTemplate>();
            const string line = "-------------------------------------------------------- {0}";
            commonFeedback.GetSportName(eventDetail.Sporttype.ToString(), Language.en_US).Returns("sportName");
            commonFeedback.GetLeagueName(eventDetail.LeagueId.ToString(), Language.en_US).Returns("leagueName");
            commonFeedback.GetTeamName(eventDetail.HomeId.ToString(), Language.en_US).Returns("homeName");
            commonFeedback.GetTeamName(eventDetail.AwayId.ToString(), Language.en_US).Returns("awayName");
            commonFeedback.GetProductName(Arg.Any<string>(), Language.en_US).Returns("Virtual Sports");
            //service.Protected("GetDescriptionSportBook", eventDetail, Arg.Any<string>(),
            //    Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns("");
            stringBuilder.AppendFormat(line, BackgroundJobConstants.Break);
            stringBuilder.Append(ticketDescTemplate.VirtualSports);

            // Act
            var actual = service.Public_GetVoidingSettledTicketDescriptionByProduct(eventDetail, ticketDescTemplate, "");

            // Assert
            Assert.Equal(stringBuilder.ToString(), actual);
        }

        [Fact]
        public void GetVoidingSettledTicketDescriptionByProduct_ProductIsVirtualSports181185_ReturnExpectedDescription()
        {
            // Arrange
            var service = MockService();
            var stringBuilder = new StringBuilder();
            var eventDetail = new AutoVoidingEventDetails
            {
                Sporttype = 181,
                LeagueId = 1,
                HomeId = 1,
                AwayId = 1,
                ProductId = 1,
                VendorMatchId = "1"
            };
            var ticketDescTemplate = fixture.Create<VoidingSettledTicketDescriptionTemplate>();
            const string line = "-------------------------------------------------------- {0}";
            commonFeedback.GetSportName(eventDetail.Sporttype.ToString(), Language.en_US).Returns("sportName");
            commonFeedback.GetLeagueName(eventDetail.LeagueId.ToString(), Language.en_US).Returns("leagueName");
            commonFeedback.GetTeamName(eventDetail.HomeId.ToString(), Language.en_US).Returns("homeName");
            commonFeedback.GetTeamName(eventDetail.AwayId.ToString(), Language.en_US).Returns("awayName");
            commonFeedback.GetProductName(Arg.Any<string>(), Language.en_US).Returns("Virtual Sports");
            //service.Protected("GetDescriptionSportBook", eventDetail, Arg.Any<string>(),
            //    Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns("");
            stringBuilder.AppendFormat(line, BackgroundJobConstants.Break);
            stringBuilder.Append(ticketDescTemplate.VirtualSports181185);

            // Act
            var actual = service.Public_GetVoidingSettledTicketDescriptionByProduct(eventDetail, ticketDescTemplate, "");

            // Assert
            Assert.Equal(stringBuilder.ToString(), actual);
        }

        [Fact]
        public void GetVoidingSettledTicketDescriptionByProduct_ProductIsNumberGame_ReturnExpectedDescription()
        {
            // Arrange
            var service = MockService();
            var stringBuilder = new StringBuilder();
            var eventDetail = new AutoVoidingEventDetails
            {
                Sporttype = 181,
                LeagueId = 1,
                HomeId = 1,
                AwayId = 1,
                ProductId = 1,
                VendorMatchId = "1"
            };
            var ticketDescTemplate = fixture.Create<VoidingSettledTicketDescriptionTemplate>();
            const string line = "-------------------------------------------------------- {0}";
            commonFeedback.GetSportName(eventDetail.Sporttype.ToString(), Language.en_US).Returns("sportName");
            commonFeedback.GetLeagueName(eventDetail.LeagueId.ToString(), Language.en_US).Returns("leagueName");
            commonFeedback.GetTeamName(eventDetail.HomeId.ToString(), Language.en_US).Returns("homeName");
            commonFeedback.GetTeamName(eventDetail.AwayId.ToString(), Language.en_US).Returns("awayName");
            commonFeedback.GetProductName(Arg.Any<string>(), Language.en_US).Returns("Number Game");
            //service.Protected("GetDescriptionSportBook", eventDetail, Arg.Any<string>(),
            //    Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns("");
            stringBuilder.AppendFormat(line, BackgroundJobConstants.Break);
            stringBuilder.Append(ticketDescTemplate.NumberGames);

            // Act
            var actual = service.Public_GetVoidingSettledTicketDescriptionByProduct(eventDetail, ticketDescTemplate, "");

            // Assert
            Assert.Equal(stringBuilder.ToString(), actual);
        }

        #endregion GetVoidingSettledTicketDescriptionByProduct

        #region GetDescriptionSportBook

        [Fact]
        public void GetDescriptionSportBook_ProductIsOutright_ReturnExpectedDescription()
        {
            // Arrange
            var service = MockService();
            var stringBuilder = new StringBuilder();
            var eventDetail = new AutoVoidingEventDetails { EventType = 2 };
            const string line = "-------------------------------------------------------- {0}";
            var ticketDescTemplate = fixture.Create<VoidingSettledTicketDescriptionTemplate>();
            commonFeedback.GetFeedbackTemplates<VoidingSettledTicketDescriptionTemplate>("VoidingSettledTicketDescriptionTemplate.xml").Returns(ticketDescTemplate);
            stringBuilder.AppendFormat(line, BackgroundJobConstants.Break);
            stringBuilder.AppendFormat(ticketDescTemplate.Outright);

            // Act
            var actual = service.Public_GetDescriptionSportBook(eventDetail, "Sportsbook", "kO_time", "leagueName", "homeName", "sportName", "awayName", "macthCode");

            // Assert
            Assert.Equal(stringBuilder.ToString(), actual);
        }

        [Fact]
        public void GetDescriptionSportBook_ProductIsSportsbookOrVirtualSports2_ReturnExpectedDescription()
        {
            // Arrange
            var service = MockService();
            var stringBuilder = new StringBuilder();
            var eventDetail = new AutoVoidingEventDetails { EventType = 1 };
            const string line = "-------------------------------------------------------- {0}";
            var ticketDescTemplate = fixture.Create<VoidingSettledTicketDescriptionTemplate>();
            commonFeedback.GetFeedbackTemplates<VoidingSettledTicketDescriptionTemplate>("VoidingSettledTicketDescriptionTemplate.xml").Returns(ticketDescTemplate);
            stringBuilder.AppendFormat(line, BackgroundJobConstants.Break);
            stringBuilder.AppendFormat(ticketDescTemplate.SportsbookVirtualSports2);

            // Act
            var actual = service.Public_GetDescriptionSportBook(eventDetail, "Sportsbook", "kO_time", "leagueName", "homeName", "sportName", "awayName", "macthCode");

            // Assert
            Assert.Equal(stringBuilder.ToString(), actual);
        }

        #endregion GetDescriptionSportBook

        #region Helpers

        private VoidingSettledTicketsFeedbackServiceTestable MockService()
        {
            appSettings = new AppJobSettings
            {
                CreatorId = 8,
                TimeSpan = 12
            };
            var appJobSettings = Substitute.For<IOptionsSnapshot<AppJobSettings>>();
            appJobSettings.Value.Returns(appSettings);
            baseService = Substitute.For<IBaseService>();
            settingService = Substitute.For<ISettingService>();
            logService = Substitute.For<ICrmLogService>();
            voidingSettledTicketsService = Substitute.For<IVoidingSettledTicketsService>();
            commonFeedback = Substitute.For<ICommonFeedback>();
            feedbackTransaction = Substitute.For<IFeedbackTransactionService>();

            return Substitute.For<VoidingSettledTicketsFeedbackServiceTestable>(
                appJobSettings,
                baseService,
                settingService,
                logService,
                commonFeedback,
                voidingSettledTicketsService,
                feedbackTransaction
                );
        }

        #endregion Helpers
    }
}