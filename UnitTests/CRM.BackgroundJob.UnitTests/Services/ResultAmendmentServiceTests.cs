namespace CRM.BackgroundJob.UnitTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using CRM.BackgroundJob.Models.AmendmentNotiFB;
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

    public class ResultAmendmentServiceTests
    {
        private AppJobSettings appSettings;
        private IBaseService baseService;
        private ISettingService settingService;
        private ICustomerService customerService;
        private ICrmLogService logService;
        private ICommonFeedback commonFeedback;
        private IFeedbackTransactionService transactionService;
        private readonly IFixture fixture = new Fixture();

        #region CreateFeedbackForAmendmentNotiFbAsync

        [Fact]
        public async Task CreateFeedbackForAmendmentNotiFbAsync_isAnyNotCompeleteStatusIsTrue_EditSystemSettingAsyncCalled()
        {
            // Arrange
            var service = MockService();
            var resultAmendmentSetting = fixture.Create<AmendmentNotiFbSetting>();
            var custNotifications = new List<CusReachAmendmentNotification>
            {
                new CusReachAmendmentNotification
                {
                    TicketId = 1,
                    CustId = 1,
                    TicketEventId = 1,
                    Bettype = 1,
                    Status = "Fail"
                }
            };

            customerService.GetListResultAmendmentNotificationAsync(resultAmendmentSetting).Returns(custNotifications);
            // Act
            await service.CreateFeedbackForAmendmentNotiFbAsync(resultAmendmentSetting, false, resultAmendmentSetting.LastRuntime, SystemSettingName.LastRuntime, SystemSettingName.JobStatus, "LIVE");

            // Assert
            await commonFeedback.EditSystemSettingAsyncByEnv(Arg.Any<string>(), Arg.Any<string>(), JobStatus.Completed, BackgroundJobConstants.ServiceName_AmendmentNotiFB);
            logService.Received(1).LogInformation(Arg.Any<string>());
            service.DidNotReceive().Protected(
                "CreateResultAmendmentFeedbackCreditAsync",
                Arg.Any<AmendmentAutoFeedbackInfor>(),
                Arg.Any<IEnumerable<SettingItem>>(),
                Arg.Any<IEnumerable<SystemSettingMapping>>(),
                Arg.Any<bool>());
        }

        [Fact]
        public async Task CreateFeedbackForAmendmentNotiFbAsync_InserAmendmentNotiAutoFeedbackCreateUnSuccessfull_ExpectedWriteLog()
        {
            // Arrange
            var service = MockService();
            var resultAmendmentSetting = fixture.Create<AmendmentNotiFbSetting>();
            var custNotifications = new List<CusReachAmendmentNotification>
            {
                new CusReachAmendmentNotification
                {
                    TicketId = 1,
                    CustId = 1,
                    TicketEventId = 1,
                    Bettype = 1,
                    Status = "Completed"
                }
            };

            customerService.GetListResultAmendmentNotificationAsync(resultAmendmentSetting).Returns(custNotifications);

            var commandResult = new CommandResult { Code = 1, Message = "" };
            transactionService.InserAmendmentNotiAutoFeedback(custNotifications, "LIVE").Returns(commandResult);

            // Act
            await service.CreateFeedbackForAmendmentNotiFbAsync(resultAmendmentSetting, false, resultAmendmentSetting.LastRuntime, SystemSettingName.LastRuntime, SystemSettingName.JobStatus, "LIVE");

            // Assert
            logService.Received(2).LogInformation(Arg.Any<string>());
            service.DidNotReceive().Protected(
                "CreateResultAmendmentFeedbackCreditAsync",
                Arg.Any<AmendmentAutoFeedbackInfor>(),
                Arg.Any<IEnumerable<SettingItem>>(),
                Arg.Any<IEnumerable<SystemSettingMapping>>(),
                Arg.Any<bool>());
        }

        [Fact]
        public async Task CreateFeedbackForAmendmentNotiFbAsync_InserAmendmentNotiAutoFeedbackCreateSuccessfull_ExpectedWriteLogAndUpdate()
        {
            // Arrange
            var service = MockService();
            var resultAmendmentSetting = fixture.Create<AmendmentNotiFbSetting>();
            var custNotifications = new List<CusReachAmendmentNotification>
            {
                new CusReachAmendmentNotification
                {
                    Seq = 1,
                    SubmitDate = DateTime.Now,
                    TicketId = 1,
                    CustId = 1,
                    TicketEventId = 1,
                    Bettype = 1,
                    Status = "Completed"
                }
            };
            var amendmentAutoFeedbackInfors = fixture.Create<IEnumerable<AmendmentAutoFeedbackInfor>>();
            customerService.GetListResultAmendmentNotificationAsync(resultAmendmentSetting).Returns(custNotifications);
            transactionService.GetAmendmentNotiAutoFeedbackInfor(resultAmendmentSetting.BatchSize).Returns(amendmentAutoFeedbackInfors);

            var commandResult = new CommandResult { Code = 0, Message = "" };
            transactionService.InserAmendmentNotiAutoFeedback(custNotifications, "LIVE").Returns(commandResult);

            // Act
            await service.CreateFeedbackForAmendmentNotiFbAsync(resultAmendmentSetting, false, resultAmendmentSetting.LastRuntime, SystemSettingName.LastRuntime, SystemSettingName.JobStatus, "LIVE");

            // Assert
            logService.Received(2).LogInformation(Arg.Any<string>());

            service.Received().Protected("ResultAmendmentUpdateSystemSetting", false, resultAmendmentSetting, custNotifications, SystemSettingName.JobStatus);
            service.Received().Protected(
                 "CreateResultAmendmentFeedbackCreditAsync",
                 Arg.Any<AmendmentAutoFeedbackInfor>(),
                 Arg.Any<IEnumerable<SettingItem>>(),
                 Arg.Any<IEnumerable<SystemSettingMapping>>(),
                 Arg.Any<bool>());
        }

        [Fact]
        public async Task CreateFeedbackForAmendmentNotiFbAsync_InserAmendmentNotiAutoFeedbackThrowException_ExpectedWriteLogAndUpdateStatus()
        {
            // Arrange
            var service = MockService();
            var resultAmendmentSetting = fixture.Create<AmendmentNotiFbSetting>();
            var custNotifications = new List<CusReachAmendmentNotification>
            {
                new CusReachAmendmentNotification
                {
                    TicketId = 1,
                    CustId = 1,
                    TicketEventId = 1,
                    Bettype = 1,
                    Status = "Completed"
                }
            };
            var amendmentAutoFeedbackInfors = fixture.Create<IEnumerable<AmendmentAutoFeedbackInfor>>();
            customerService.GetListResultAmendmentNotificationAsync(resultAmendmentSetting).Returns(custNotifications);
            transactionService.GetAmendmentNotiAutoFeedbackInfor(resultAmendmentSetting.BatchSize).Returns(amendmentAutoFeedbackInfors);

            var exception = fixture.Create<Exception>();
            transactionService.When(x => x.InserAmendmentNotiAutoFeedback(custNotifications, "LIVE")).Throw(exception);

            // Act
            await service.CreateFeedbackForAmendmentNotiFbAsync(resultAmendmentSetting, false, resultAmendmentSetting.LastRuntime, SystemSettingName.LastRuntime, SystemSettingName.JobStatus, "LIVE");

            // Assert
            await logService.Received().ErrorAsync(exception);
        }

        #endregion CreateFeedbackForAmendmentNotiFbAsync

        #region ResultAmendmentUpdateSystemSetting

        [Fact]
        public async Task ResultAmendmentUpdateSystemSetting_IsUatEnv_ExpectedUpdateUatValue()
        {
            // Arrange
            var service = MockService();
            var resultAmendmentSetting = fixture.Create<AmendmentNotiFbSetting>();
            var custAmendmentNotifications = fixture.Create<IEnumerable<CusReachAmendmentNotification>>();

            // Act
            await service.Public_ResultAmendmentUpdateSystemSetting(true, resultAmendmentSetting, custAmendmentNotifications, "JobStatusNameEnv");

            // Assert
            await commonFeedback.Received().EditSystemSettingUpdateLastSeqAndLastSubmitDateAsync(
               Arg.Any<string>(),
               Arg.Any<string>(),
               Arg.Any<int>(),
               Arg.Any<string>(),
               Arg.Any<string>(),
               BackgroundJobConstants.ServiceName_AmendmentNotiFB
          );
        }

        [Fact]
        public async Task ResultAmendmentUpdateSystemSetting_IsNotUatEnv_ExpectedUpdateLiveValue()
        {
            // Arrange
            var service = MockService();
            var resultAmendmentSetting = fixture.Create<AmendmentNotiFbSetting>();
            var custAmendmentNotifications = fixture.Create<IEnumerable<CusReachAmendmentNotification>>();

            // Act
            await service.Public_ResultAmendmentUpdateSystemSetting(false, resultAmendmentSetting, custAmendmentNotifications, "JobStatusNameEnv");

            // Assert
            await commonFeedback.Received().EditSystemSettingUpdateLastSeqAndLastSubmitDateAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                BackgroundJobConstants.ServiceName_AmendmentNotiFB
           );
        }

        #endregion ResultAmendmentUpdateSystemSetting

        #region CreateResultAmendmentFeedbackCreditAsync

        [Fact]
        public async Task CreateResultAmendmentFeedbackCreditAsync_CreateNewCriteria_ExpectedToCallFunction()
        {
            // Arrange
            var amendment = new AmendmentAutoFeedbackInfor
            {
                LogId = 1,
                HeadlineName = "1",
                HeadlineId = 1,
                ProductId = 1,
                Listbettype = "1,2",
                MessageId = 1,
                Sporttype = 1,
                affectedId = "1"
            };

            var settings = new List<SettingItem>
            {
                new SettingItem { SettingId = 1, SettingName = "Status", SettingValue = "Closed" },
                new SettingItem { SettingId = 2, SettingName = "Response", SettingValue = "Neutral" },
                new SettingItem { SettingId = 2022, SettingName = "Station" }
            };
            var message = fixture.Create<IEnumerable<Message>>();

            var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();
            var category = fixture.Create<Tuple<int, int, string>>();
            var service = MockService();
            service.Protected("GetCategorySubCategoryResultAmendment", Arg.Any<IEnumerable<SettingItem>>(), DefaultValue.ResultAmended).Returns(category);
            transactionService.GetMessageContentInfoAsync(amendment.MessageId).Returns(message);
            commonFeedback.GetBettypeName(Arg.Any<string>(), Language.en_US).Returns("BettypeName");
            service.Protected("GetAmendmentProductXml", amendment.Sporttype, amendment.ProductId, Arg.Any<List<string>>(), settings, settingMappings).Returns("product");
            service.Protected("GetAmenmendSolution", amendment, message, false).Returns("product xml");
            service.Protected("GetAmendmentRelatedCustomerXml", amendment).Returns("relatedCustomerXml");
            service.Protected("GetAmendmentDescription", amendment, false).Returns("description");
            // Act
            await service.Public_CreateResultAmendmentFeedbackCreditAsync(
                amendment,
                settings,
                settingMappings,
                true
                );

            // Assert
            logService.Received(2).LogInformation(Arg.Any<string>());
            service.Received().Protected("CheckValidAmendmentParamsAndInsertFeedbackAsync", Arg.Any<AmendmentAutoFeedbackInfor>()
                , Arg.Any<CreateMutipleFeedbackCommand>());
        }

        #endregion CreateResultAmendmentFeedbackCreditAsync

        #region CheckValidAmendmentParamsAndInsertFeedbackAsync

        [Fact]
        public async Task CheckValidAmendmentParamsAndInsertFeedbackAsync_CreateSuccessfull_ExpectedWriteLog()
        {
            // Arrange
            var amendment = new AmendmentAutoFeedbackInfor
            {
                LogId = 1,
                HeadlineName = "1",
                HeadlineId = 1,
                ProductId = 1,
                Listbettype = "1,2",
                MessageId = 1,
                Sporttype = 1,
                affectedId = "1",
                affectedName = "1"
            };
            var criteria = fixture.Create<CreateMutipleFeedbackCommand>();
            var service = MockService();
            commonFeedback.CheckFeedbackValidParams(criteria).Returns(new ValueTuple<bool, string>(true, ""));

            var commandResult = new CommandResult { Code = 0 };
            baseService.ExecuteNonQueryAsync(criteria).Returns(commandResult);

            // Act
            await service.Public_CheckValidAmendmentParamsAndInsertFeedbackAsync(
                amendment,
                criteria
                );

            // Assert
            logService.Received(2).LogInformation(Arg.Any<string>());
        }

        [Fact]
        public async Task CheckValidAmendmentParamsAndInsertFeedbackAsync_CreateUnSuccessfull_ExpectedWriteLogAndUpdateStatus()
        {
            var amendment = new AmendmentAutoFeedbackInfor
            {
                LogId = 1,
                HeadlineName = "1",
                HeadlineId = 1,
                ProductId = 1,
                Listbettype = "1,2",
                MessageId = 1,
                Sporttype = 1,
                affectedId = "1",
                affectedName = "1"
            };
            var criteria = fixture.Create<CreateMutipleFeedbackCommand>();
            var service = MockService();
            commonFeedback.CheckFeedbackValidParams(criteria).Returns(new ValueTuple<bool, string>(true, ""));

            var commandResult = new CommandResult { Code = 1, Message = "" };
            baseService.ExecuteNonQueryAsync(criteria).Returns(commandResult);

            // Act
            await service.Public_CheckValidAmendmentParamsAndInsertFeedbackAsync(
                amendment,
                criteria
                );

            // Assert
            logService.Received(2).LogInformation(Arg.Any<string>());
            await transactionService.EditAmendmentNotiAutoFeedback(1, CreateFeedbackJobStatus.Fail, commandResult.Message);
            await logService.ErrorAsync(commandResult, criteria);
        }

        [Fact]
        public async Task CheckValidAmendmentParamsAndInsertFeedbackAsync_InvalidParams_ExpectedReturnAndUpdateStatus()
        {
            // Arrange
            var amendment = new AmendmentAutoFeedbackInfor
            {
                LogId = 1,
                HeadlineName = "1",
                HeadlineId = 1,
                ProductId = 1,
                Listbettype = "1,2",
                MessageId = 1,
                Sporttype = 1,
                affectedId = "1",
                affectedName = "1"
            };
            var criteria = fixture.Create<CreateMutipleFeedbackCommand>();
            var service = MockService();
            commonFeedback.CheckFeedbackValidParams(criteria).Returns(new ValueTuple<bool, string>(false, ""));

            var commandResult = new CommandResult { Code = 0 };
            baseService.ExecuteNonQueryAsync(criteria).Returns(commandResult);

            // Act
            await service.Public_CheckValidAmendmentParamsAndInsertFeedbackAsync(
                amendment,
                criteria
                );

            // Assert
            logService.Received(1).LogInformation(Arg.Any<string>());
            await transactionService.EditAmendmentNotiAutoFeedback(1, CreateFeedbackJobStatus.Fail, Arg.Any<string>());
        }

        [Fact]
        public async Task CheckValidAmendmentParamsAndInsertFeedbackAsync_Exception_ExpectedWriteLogAndUpdateStatus()
        {
            // Arrange
            var amendment = new AmendmentAutoFeedbackInfor
            {
                LogId = 1,
                HeadlineName = "1",
                HeadlineId = 1,
                ProductId = 1,
                Listbettype = "1,2",
                MessageId = 1,
                Sporttype = 1,
                affectedId = "1",
                affectedName = "1"
            };
            var exception = fixture.Create<Exception>();
            var criteria = fixture.Create<CreateMutipleFeedbackCommand>();
            var service = MockService();
            commonFeedback.CheckFeedbackValidParams(criteria).Returns(new ValueTuple<bool, string>(true, ""));

            baseService.When(x => x.ExecuteNonQueryAsync(criteria)).Throw(exception);

            // Act
            await service.Public_CheckValidAmendmentParamsAndInsertFeedbackAsync(
                amendment,
                criteria
                );

            // Assert
            logService.Received(1).LogInformation(Arg.Any<string>());
            await transactionService.EditAmendmentNotiAutoFeedback(1, CreateFeedbackJobStatus.Fail, exception.Message);
            await logService.ErrorAsync(exception);
        }

        #endregion CheckValidAmendmentParamsAndInsertFeedbackAsync

        #region GetAmendmentProductXml

        [Fact]
        public void GetAmendmentProductXml_NotExistedBetType_ReturnExpectedXml()
        {
            // Arrange
            var betTypeMappings = fixture.Create<List<string>>();
            var settings = fixture.Create<IEnumerable<SettingItem>>();
            var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();
            var product = new SettingItem { SettingId = 1 };
            var subProduct = new List<SettingItem>
            {
                new SettingItem{ SettingId = 1 }
            };
            var tempBettypes = new List<SystemSettingMapping>();
            var service = MockService();
            service.Protected("GetAmenmentProduct", settings, settingMappings, 1).Returns(product);
            service.Protected("GetAmendmentSubProduct", product.SettingId, 1, settings, settingMappings).Returns(subProduct);
            service.Protected("GetAmendmentBettype", betTypeMappings, settingMappings).Returns(tempBettypes);

            // Act
            var actual = service.Public_GetAmendmentProductXml(1, product.SettingId, betTypeMappings, settings, settingMappings);

            // Assert
            Assert.Equal("<ROOT><Product ProductID='1' SubProductID='1' BetTypeID='9144' /></ROOT>", actual);
        }

        [Fact]
        public void GetAmendmentProductXml_ExistedBetType_ReturnExpectedXml()
        {
            var betTypeMappings = fixture.Create<List<string>>();
            var settings = fixture.Create<IEnumerable<SettingItem>>();
            var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();
            var product = new SettingItem { SettingId = 1 };
            var subProduct = new List<SettingItem>
            {
                new SettingItem{ SettingId = 1 }
            };
            var tempBettypes = new List<SystemSettingMapping>
            {
                new SystemSettingMapping
                {
                    SettingId = 1
                }
            };
            var service = MockService();
            service.Protected("GetAmenmentProduct", settings, settingMappings, 1).Returns(product);
            service.Protected("GetAmendmentSubProduct", product.SettingId, 1, settings, settingMappings).Returns(subProduct);
            service.Protected("GetAmendmentBettype", betTypeMappings, settingMappings).Returns(tempBettypes);

            // Act
            var actual = service.Public_GetAmendmentProductXml(1, product.SettingId, betTypeMappings, settings, settingMappings);

            // Assert
            Assert.Equal("<ROOT><Product ProductID='1' SubProductID='1' BetTypeID='1' /></ROOT>", actual);
        }

        #endregion GetAmendmentProductXml

        #region GetAmenmentProduct

        [Fact]
        public void GetAmenmentProduct_NotExistProductId_ReturnDefaultProduct()
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
            var service = MockService();
            commonFeedback.GetProductName("1", Language.en_US).Returns("2");

            // Act
            var actual = service.Public_GetAmenmentProduct(setting, settingMappings, 1);

            // Assert
            Assert.Equal(BackgroundJobConstants.ProductNA, actual.SettingId);
        }

        [Fact]
        public void GetAmenmentProduct_SettingNotEqualProductMapping_ReturnDefaultProduct()
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
            var service = MockService();
            commonFeedback.GetProductName("1", Language.en_US).Returns("1");

            // Act
            var actual = service.Public_GetAmenmentProduct(setting, settingMappings, 1);

            // Assert
            Assert.Equal(BackgroundJobConstants.ProductNA, actual.SettingId);
        }

        [Fact]
        public void GetAmenmentProduct_SettingEqualProductMapping_ReturnProduct()
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
            var service = MockService();
            commonFeedback.GetProductName("1", Language.en_US).Returns("1");

            // Act
            var actual = service.Public_GetAmenmentProduct(setting, settingMappings, 1);

            // Assert
            Assert.Equal(1, actual.SettingId);
        }

        #endregion GetAmenmentProduct

        #region GetAmendmentSubProduct

        [Fact]
        public void GetAmendmentSubProduct_NotExistsportName_ReturnDefaultSubProduct()
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
            var service = MockService();
            commonFeedback.GetSportName("1", Language.en_US).Returns("1");

            // Act
            var actual = service.Public_GetAmendmentSubProduct(1, 1, setting, settingMappings);

            // Assert
            Assert.Single(actual);
            Assert.Equal(BackgroundJobConstants.SubProductNA, actual.First().SettingId);
        }

        [Fact]
        public void GetAmendmentSubProduct_SettingNotEqualMapping_ReturnDefaultSubProduct()
        {
            // Arrange
            var setting = new List<SettingItem>
                {
                    new SettingItem { SettingName = SettingName.SubProduct, SettingValue = "1" , SettingId = 2, ParentIds = new List<int>{ 1,2} }
                };

            var settingMappings = new List<SystemSettingMapping>
                {
                    new SystemSettingMapping { SettingName = "1", SettingValue = "1" , SettingSource = "1", SettingId = 1}
                };
            var service = MockService();
            commonFeedback.GetProductName("1", Language.en_US).Returns("1");

            // Act
            var actual = service.Public_GetAmendmentSubProduct(1, 1, setting, settingMappings);

            // Assert
            Assert.Equal(BackgroundJobConstants.SubProductNA, actual.First().SettingId);
        }

        [Fact]
        public void GetAmendmentSubProduct_SportNameNotEqualMapping_ReturnDefaultSubProduct()
        {
            // Arrange
            var setting = new List<SettingItem>
                {
                    new SettingItem { SettingName = SettingName.SubProduct, SettingValue = "1" , SettingId = 2, ParentIds = new List<int>{ 1,2} }
                };

            var settingMappings = new List<SystemSettingMapping>
                {
                    new SystemSettingMapping { SettingName = "1", SettingValue = "1" , SettingSource = "1", SettingId = 1}
                };
            var service = MockService();
            commonFeedback.GetProductName("2", Language.en_US).Returns("1");

            // Act
            var actual = service.Public_GetAmendmentSubProduct(1, 1, setting, settingMappings);

            // Assert
            Assert.Equal(BackgroundJobConstants.SubProductNA, actual.First().SettingId);
        }

        [Fact]
        public void GetAmendmentSubProduct_ExistSportName_ReturnSubProduct()
        {
            // Arrange
            var setting = new List<SettingItem>
                {
                    new SettingItem { SettingName = SettingName.SubProduct, SettingValue = "1" , SettingId = 1, ParentIds = new List<int>{ 1,2} }
                };

            var settingMappings = new List<SystemSettingMapping>
                {
                    new SystemSettingMapping { SettingName = SettingName.SubProduct, SettingValue = "1" , SettingSource = "1", SettingId = 1}
                };
            var service = MockService();
            commonFeedback.GetSportName("1", Language.en_US).Returns("1");

            // Act
            var actual = service.Public_GetAmendmentSubProduct(1, 1, setting, settingMappings);

            // Assert
            Assert.Equal(1, actual.First().SettingId);
        }

        #endregion GetAmendmentSubProduct

        #region GetAmendmentBettype

        [Fact]
        public void GetAmendmentBettype_betTypeMappingsNotEqualSettingSource_ReturnDefaultBetType()
        {
            // Arrange
            var settingMappings = new List<SystemSettingMapping>
                {
                    new SystemSettingMapping { SettingName = "Bettype", SettingValue = "1" , SettingSource = "1"}
                };

            var betTypeMappings = new List<string> { "2", "3" };
            var service = MockService();

            // Act
            var actual = service.Public_GetAmendmentBettype(betTypeMappings, settingMappings);

            // Assert
            Assert.Single(actual);
            Assert.Equal(BackgroundJobConstants.BettypeNA, actual.First().SettingId);
        }

        [Fact]
        public void GetAmendmentBettype_SettingSourceIsNull_ReturnDefaultBetType()
        {
            // Arrange
            var settingMappings = new List<SystemSettingMapping>
                {
                    new SystemSettingMapping { SettingName = "Bettype", SettingValue = "1"}
                };

            var betTypeMappings = new List<string> { "2", "3" };
            var service = MockService();

            // Act
            var actual = service.Public_GetAmendmentBettype(betTypeMappings, settingMappings);

            // Assert
            Assert.Single(actual);
            Assert.Equal(BackgroundJobConstants.BettypeNA, actual.First().SettingId);
        }

        [Fact]
        public void GetAmendmentBettype_SettingNameNotEqualBettype_ReturnDefaultBetType()
        {
            // Arrange
            var settingMappings = new List<SystemSettingMapping>
                {
                    new SystemSettingMapping { SettingName = "Bettype1", SettingValue = "1", SettingSource = "2"}
                };

            var betTypeMappings = new List<string> { "2", "3" };
            var service = MockService();

            // Act
            var actual = service.Public_GetAmendmentBettype(betTypeMappings, settingMappings);

            // Assert
            Assert.Single(actual);
            Assert.Equal(BackgroundJobConstants.BettypeNA, actual.First().SettingId);
        }

        [Fact]
        public void GetAmendmentBettype_ExistBettypes_ReturnBetType()
        {
            // Arrange
            var settingMappings = new List<SystemSettingMapping>
                {
                    new SystemSettingMapping { SettingName = "Bettype", SettingValue = "1", SettingSource = "2", SettingId = 1}
                };

            var betTypeMappings = new List<string> { "2", "3" };
            var service = MockService();

            // Act
            var actual = service.Public_GetAmendmentBettype(betTypeMappings, settingMappings);

            // Assert
            Assert.Single(actual);
            Assert.Equal(1, actual.First().SettingId);
        }

        #endregion GetAmendmentBettype

        #region GetCategorySubCategoryResultAmendment

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
            var actual = service.Public_GetCategorySubCategoryResultAmendment(settings, DefaultValue.ResultAmended);

            // Assert
            Assert.Equal(0, actual.Item1);
            Assert.Equal(0, actual.Item2);
            Assert.Equal("<ROOT><Category MainCategoryID='0' SubCategoryID='0' /></ROOT>", actual.Item3);
        }

        [Fact]
        public void GetCategorySubCategoryResultAmendment_NotExistsettingValueName_ReturnDefaultMainCategoryXML()
        {
            // Arrange
            var settings = new List<SettingItem>
                {
                    new SettingItem { SettingName = "MainCategory", SettingValue = DefaultValue.ResultAmended }
                };
            var service = MockService();

            // Act
            var actual = service.Public_GetCategorySubCategoryResultAmendment(settings, "test");

            // Assert
            Assert.Equal(0, actual.Item1);
            Assert.Equal(0, actual.Item2);
            Assert.Equal("<ROOT><Category MainCategoryID='0' SubCategoryID='0' /></ROOT>", actual.Item3);
        }

        [Fact]
        public void GetCategorySubCategoryResultAmendment_NotExistsSubId_ReturnDefaultMainCategoryXML()
        {
            // Arrange
            var settings = new List<SettingItem>
                {
                    new SettingItem { SettingId = 1, SettingName = "MainCategory", SettingValue = DefaultValue.ResultAmended },
                    new SettingItem { SettingName = "SubCategory", SettingId =  1}
                };
            var service = MockService();

            // Act
            var actual = service.Public_GetCategorySubCategoryResultAmendment(settings, "test");

            // Assert
            Assert.Equal(0, actual.Item1);
            Assert.Equal(0, actual.Item2);
            Assert.Equal("<ROOT><Category MainCategoryID='0' SubCategoryID='0' /></ROOT>", actual.Item3);
        }

        [Fact]
        public void GetCategorySubCategoryResultAmendment_ExistMainCategoryAndExistSubCategory_ReturnExpectedTuple()
        {
            // Arrange
            var settings = new List<SettingItem>
                {
                    new SettingItem { SettingId = 1, SettingName = "MainCategory", SettingValue = DefaultValue.ResultAmended },
                    new SettingItem { SettingName = "SubCategory", SettingId =  4029}
                };

            var service = MockService();

            // Act
            var actual = service.Public_GetCategorySubCategoryResultAmendment(settings, DefaultValue.ResultAmended);

            // Assert
            Assert.Equal(1, actual.Item1);
            Assert.Equal(4029, actual.Item2);
            Assert.Equal("<ROOT><Category MainCategoryID='1' SubCategoryID='4029' /></ROOT>", actual.Item3);
        }

        [Fact]
        public void GetCategorySubCategoryResultAmendment_ExistMainCategoryAndNotExistSubCategory_ReturnExpectedTuple()
        {
            // Arrange
            var settings = new List<SettingItem>
                {
                    new SettingItem { SettingId = 1, SettingName = "MainCategory", SettingValue = DefaultValue.ResultAmended },
                    new SettingItem { SettingName = "SubCategory", SettingId =  2}
                };

            var service = MockService();

            // Act
            var actual = service.Public_GetCategorySubCategoryResultAmendment(settings, DefaultValue.ResultAmended);

            // Assert
            Assert.Equal(1, actual.Item1);
            Assert.Equal(0, actual.Item2);
            Assert.Equal("<ROOT><Category MainCategoryID='1' SubCategoryID='0' /></ROOT>", actual.Item3);
        }

        #endregion GetCategorySubCategoryResultAmendment

        #region GetAmendmentRelatedCustomerXml

        [Fact]
        public void GetAmendmentRelatedCustomerXml_IsEmptyOrNullAffectedId_ReturnExactDefaultTemplate()
        {
            // Arrange
            var amendment = new AmendmentAutoFeedbackInfor();
            var service = MockService();

            // Act
            var actual = service.Public_GetAmendmentRelatedCustomerXml(amendment);

            // Assert
            Assert.Equal("<ROOT></ROOT>", actual);
        }

        [Fact]
        public void GetAmendmentRelatedCustomerXml_AffectedIdAreNotNull_ReturnExactTemplate()
        {
            // Arrange
            var amendment = new AmendmentAutoFeedbackInfor
            {
                affectedId = "1,2",
                HeadlineId = 1,
                affectedName = "1,2"
            };
            var service = MockService();
            var expect = "<ROOT><Row CustId='1' RelatedCustId='1' Username='1' /><Row CustId='1' RelatedCustId='2' Username='2' /></ROOT>";

            // Act
            var actual = service.Public_GetAmendmentRelatedCustomerXml(amendment);

            // Assert
            Assert.Equal(expect, actual);
        }

        #endregion GetAmendmentRelatedCustomerXml

        #region GetAmendmentDescription

        [Fact]
        public void GetAmendmentDescription_ProducIsNotExpected_ReturnDefaultTemplate()
        {
            // Arrange
            var amendment = new AmendmentAutoFeedbackInfor
            {
                LogId = 1,
                TicketEventId = 1,
                HeadlineId = 1,
                HeadlineName = "hl1",
                ProductId = 0,
                affectedId = "1,2",
                affectedName = "1,2",
                MatchId = 1,
                MatchCode = "0",
                EventType = 3,
                ActualResult = "win",
                WrongResult = "lose",
                Listbettype = "10",
                Currency = "HKK",
                MessageId = 338,
                Site = "789y",
                IsVip = true,
                Sporttype = 0,
                LeagueId = 93816,
                Leaguenametc = "Basketball",
                HomeId = 714227,
                Awayid = 0,
                KO_time = DateTime.Parse("2021-02-23 19:59:00.000"),
                Eventdate = DateTime.Parse("2021-02-24 19:59:00.000"),
                Race_time = DateTime.Parse("2021-02-25 19:59:00.000")
            };

            var service = MockService();
            var testDescTemp = fixture.Create<AmendmentDescriptionTemplate>();
            commonFeedback.GetFeedbackTemplates<AmendmentDescriptionTemplate>("test.xml").Returns(testDescTemp);
            var stringBuilder = new StringBuilder(DefaultValue.autoGenerated);
            stringBuilder.Append(BackgroundJobConstants.Break);
            stringBuilder.Append("");
            stringBuilder.Append(BackgroundJobConstants.Break);
            commonFeedback.GetProductName(amendment.ProductId.ToString(), Language.en_US).Returns("");

            // Act
            var actual = service.Public_GetAmendmentDescription(amendment, false);

            // Assert
            Assert.Equal(stringBuilder.ToString(), actual);
        }

        [Fact]
        public void GetAmendmentDescription_IsNoResult_ReturnExactTemplate()
        {
            // Arrange
            var amendment = new AmendmentAutoFeedbackInfor
            {
                LogId = 1,
                TicketEventId = 1,
                HeadlineId = 1,
                HeadlineName = "hl1",
                ProductId = 0,
                affectedId = "1,2",
                affectedName = "1,2",
                MatchId = 1,
                MatchCode = "0",
                EventType = 3,
                ActualResult = "win",
                WrongResult = "lose",
                Listbettype = "10",
                Currency = "HKK",
                MessageId = 338,
                Site = "789y",
                IsVip = true,
                Sporttype = 0,
                LeagueId = 93816,
                Leaguenametc = "Basketball",
                HomeId = 714227,
                Awayid = 0,
                KO_time = DateTime.Parse("2021-02-23 19:59:00.000"),
                Eventdate = DateTime.Parse("2021-02-24 19:59:00.000"),
                Race_time = DateTime.Parse("2021-02-25 19:59:00.000")
            };

            var service = MockService();
            var testDescTemp = new AmendmentDescriptionTemplate
            {
                NoResult = "NoResult"
            };
            commonFeedback.GetFeedbackTemplates<AmendmentDescriptionTemplate>("AmendmentDescriptionTemplate.xml").Returns(testDescTemp);
            var stringBuilder = new StringBuilder(DefaultValue.autoGenerated);
            stringBuilder.Append(BackgroundJobConstants.Break);
            const string messageIdColor = "<span style=\"color: rgb(255, 0, 0);\">Msg ID: {0}{1}</span>";
            stringBuilder.AppendFormat(messageIdColor, amendment.MessageId, BackgroundJobConstants.LineBreak);
            stringBuilder.Append("NoResult");
            stringBuilder.Append(BackgroundJobConstants.Break);

            // Act
            var actual = service.Public_GetAmendmentDescription(amendment, true);

            // Assert
            Assert.Equal(stringBuilder.ToString(), actual);
        }

        [Fact]
        public void GetAmendmentDescription_ProducIsSportsbook_ReturnExactTemplate()
        {
            // Arrange
            var amendment = new AmendmentAutoFeedbackInfor
            {
                LogId = 1,
                TicketEventId = 1,
                HeadlineId = 1,
                HeadlineName = "hl1",
                ProductId = 0,
                affectedId = "1,2",
                affectedName = "1,2",
                MatchId = 1,
                MatchCode = "0",
                EventType = 3,
                ActualResult = "win",
                WrongResult = "lose",
                Listbettype = "10",
                Currency = "HKK",
                MessageId = 338,
                Site = "789y",
                IsVip = true,
                Sporttype = 0,
                LeagueId = 93816,
                LeagueGroupId = 93816,
                Leaguenametc = "Basketball",
                HomeId = 714227,
                Awayid = 0,
                KO_time = DateTime.Parse("2021-02-23 19:59:00.000"),
                Eventdate = DateTime.Parse("2021-02-24 19:59:00.000"),
                Race_time = DateTime.Parse("2021-02-25 19:59:00.000")
            };

            var service = MockService();
            var testDescTemp = new AmendmentDescriptionTemplate
            {
                Sportsbook = "{0} {1} {2} {3} {4} {5} {6}"
            };
            commonFeedback.GetFeedbackTemplates<AmendmentDescriptionTemplate>("AmendmentDescriptionTemplate.xml").Returns(testDescTemp);
            var stringBuilder = new StringBuilder(DefaultValue.autoGenerated);
            stringBuilder.Append(BackgroundJobConstants.Break);
            service.Protected("GetTeamNameFunction", amendment.EventType).Returns(commonFeedback.GetTeamName);
            commonFeedback.GetProductName(amendment.ProductId.ToString(), Language.en_US).Returns("Sportsbook");
            commonFeedback.GetSportName(amendment.Sporttype.ToString(), Language.en_US).Returns("SportName");
            commonFeedback.GetLeagueNameBySiteName(amendment.Site, amendment.LeagueId, amendment.LeagueGroupId, Language.en_US).Returns("LeagueName");
            commonFeedback.GetTeamName(amendment.HomeId.ToString(), Language.en_US).Returns("HomeName");
            commonFeedback.GetTeamName(amendment.Awayid.ToString(), Language.en_US).Returns("AwayName");
            stringBuilder.Append("24-Feb-2021 SportName 07:59 AM LeagueName HomeName AwayName <br />");
            stringBuilder.Append(BackgroundJobConstants.Break);

            // Act
            var actual = service.Public_GetAmendmentDescription(amendment, false);

            // Assert
            Assert.Equal(stringBuilder.ToString(), actual);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(9)]
        public void GetAmendmentDescription_ProducIsSportsbookAndBetypeEqual2Or9_ReturnExactOutrightTemplate(byte EventType)
        {
            // Arrange
            var amendment = new AmendmentAutoFeedbackInfor
            {
                LogId = 1,
                TicketEventId = 1,
                HeadlineId = 1,
                HeadlineName = "hl1",
                ProductId = 0,
                affectedId = "1,2",
                affectedName = "1,2",
                MatchId = 1,
                MatchCode = "0",
                EventType = EventType,
                ActualResult = "win",
                WrongResult = "lose",
                Listbettype = "10",
                Currency = "HKK",
                MessageId = 338,
                Site = "789y",
                IsVip = true,
                Sporttype = 0,
                LeagueId = 93816,
                LeagueGroupId = 93816,
                Leaguenametc = "Basketball",
                HomeId = 714227,
                Awayid = 0,
                KO_time = DateTime.Parse("2021-02-23 19:59:00.000"),
                Eventdate = DateTime.Parse("2021-02-24 19:59:00.000"),
                Race_time = DateTime.Parse("2021-02-25 19:59:00.000")
            };

            var service = MockService();
            var testDescTemp = new AmendmentDescriptionTemplate
            {
                Sportsbook = "{0} {1} {2} {3} {4} {5} {6}"
            };
            commonFeedback.GetFeedbackTemplates<AmendmentDescriptionTemplate>("AmendmentDescriptionTemplate.xml").Returns(testDescTemp);
            var stringBuilder = new StringBuilder(DefaultValue.autoGenerated);
            stringBuilder.Append(BackgroundJobConstants.Break);
            service.Protected("GetTeamNameFunction", amendment.EventType).Returns(commonFeedback.GetTeamName);
            commonFeedback.GetProductName(amendment.ProductId.ToString(), Language.en_US).Returns("Sportsbook");
            commonFeedback.GetSportName(amendment.Sporttype.ToString(), Language.en_US).Returns("SportName");
            commonFeedback.GetLeagueNameBySiteName(amendment.Site, amendment.LeagueId, amendment.LeagueGroupId, Language.en_US).Returns("LeagueName");
            commonFeedback.GetTeamName(amendment.HomeId.ToString(), Language.en_US).Returns("HomeName");
            commonFeedback.GetTeamName(amendment.Awayid.ToString(), Language.en_US).Returns("AwayName");
            var outright = new StringBuilder("outright");

            service.Protected("GetAmendmentDescriptionSportOutRight", amendment, testDescTemp, "24 Feb 2021 07:59 AM", "LeagueName", "HomeName").Returns(outright);
            stringBuilder.Append("outright");
            stringBuilder.Append(BackgroundJobConstants.Break);

            // Act
            var actual = service.Public_GetAmendmentDescription(amendment, false);

            // Assert
            Assert.Equal(stringBuilder.ToString(), actual);
        }

        [Fact]
        public void GetAmendmentDescription_ProducIsVirtualSports2_ReturnExactTemplate()
        {
            // Arrange
            var amendment = new AmendmentAutoFeedbackInfor
            {
                LogId = 1,
                TicketEventId = 1,
                HeadlineId = 1,
                HeadlineName = "hl1",
                ProductId = 0,
                affectedId = "1,2",
                affectedName = "1,2",
                MatchId = 1,
                MatchCode = "0",
                EventType = 3,
                ActualResult = "win",
                WrongResult = "lose",
                Listbettype = "10",
                Currency = "HKK",
                MessageId = 338,
                Site = "789y",
                IsVip = true,
                Sporttype = 0,
                LeagueId = 93816,
                Leaguenametc = "Basketball",
                HomeId = 714227,
                Awayid = 0,
                KO_time = DateTime.Parse("2021-02-23 19:59:00.000"),
                Eventdate = DateTime.Parse("2021-02-24 19:59:00.000"),
                Race_time = DateTime.Parse("2021-02-25 19:59:00.000")
            };

            var service = MockService();
            var testDescTemp = new AmendmentDescriptionTemplate
            {
                VirtualSports2 = "Test VirtualSports2 Template"
            };
            commonFeedback.GetFeedbackTemplates<AmendmentDescriptionTemplate>("AmendmentDescriptionTemplate.xml").Returns(testDescTemp);
            var stringBuilder = new StringBuilder(DefaultValue.autoGenerated);
            stringBuilder.Append(BackgroundJobConstants.Break);
            service.Protected("GetTeamNameFunction", amendment.EventType).Returns(commonFeedback.GetTeamName);
            commonFeedback.GetProductName(amendment.ProductId.ToString(), Language.en_US).Returns("Virtual Sports 2");
            commonFeedback.GetSportName(amendment.Sporttype.ToString(), Language.en_US).Returns("SportName");
            commonFeedback.GetLeagueName(amendment.LeagueId.ToString(), Language.en_US).Returns("LeagueName");
            commonFeedback.GetTeamName(amendment.HomeId.ToString(), Language.en_US).Returns("HomeName");
            commonFeedback.GetTeamName(amendment.Awayid.ToString(), Language.en_US).Returns("AwayName");
            stringBuilder.Append(testDescTemp.VirtualSports2);
            stringBuilder.Append(BackgroundJobConstants.Break);

            // Act
            var actual = service.Public_GetAmendmentDescription(amendment, false);

            // Assert
            Assert.Equal(stringBuilder.ToString(), actual);
        }

        [Fact]
        public void GetAmendmentDescription_ProducIsVirtualSports5_ReturnExactTemplate()
        {
            // Arrange
            var amendment = new AmendmentAutoFeedbackInfor
            {
                LogId = 1,
                TicketEventId = 1,
                HeadlineId = 1,
                HeadlineName = "hl1",
                ProductId = 0,
                affectedId = "1,2",
                affectedName = "1,2",
                MatchId = 1,
                MatchCode = "0",
                EventType = 5,
                ActualResult = "win",
                WrongResult = "lose",
                Listbettype = "10",
                Currency = "HKK",
                MessageId = 338,
                Site = "789y",
                IsVip = true,
                Sporttype = 0,
                LeagueId = 93816,
                Leaguenametc = "Basketball",
                HomeId = 714227,
                Awayid = 0,
                KO_time = DateTime.Parse("2021-02-23 19:59:00.000"),
                Eventdate = DateTime.Parse("2021-02-24 19:59:00.000"),
                Race_time = DateTime.Parse("2021-02-25 19:59:00.000")
            };

            var service = MockService();
            var testDescTemp = new AmendmentDescriptionTemplate
            {
                VirtualSports2 = "Test VirtualSports5 Template"
            };
            commonFeedback.GetFeedbackTemplates<AmendmentDescriptionTemplate>("AmendmentDescriptionTemplate.xml").Returns(testDescTemp);
            var stringBuilder = new StringBuilder(DefaultValue.autoGenerated);
            stringBuilder.Append(BackgroundJobConstants.Break);
            service.Protected("GetTeamNameFunction", amendment.EventType).Returns(commonFeedback.GetTeamName);
            commonFeedback.GetProductName(amendment.ProductId.ToString(), Language.en_US).Returns("Virtual Sports 2");
            commonFeedback.GetSportName(amendment.Sporttype.ToString(), Language.en_US).Returns("SportName");
            commonFeedback.GetLeagueName3rd(amendment.LeagueId.ToString(), Language.en_US).Returns("LeagueName");
            commonFeedback.GetTeamName(amendment.HomeId.ToString(), Language.en_US).Returns("HomeName");
            commonFeedback.GetTeamName(amendment.Awayid.ToString(), Language.en_US).Returns("AwayName");
            stringBuilder.Append(testDescTemp.VirtualSports2);
            stringBuilder.Append(BackgroundJobConstants.Break);

            // Act
            var actual = service.Public_GetAmendmentDescription(amendment, false);

            // Assert
            Assert.Equal(stringBuilder.ToString(), actual);
        }

        [Fact]
        public void GetAmendmentDescription_ProducIsVirtualSports_ReturnExactTemplate()
        {
            // Arrange
            var amendment = new AmendmentAutoFeedbackInfor
            {
                LogId = 1,
                TicketEventId = 1,
                HeadlineId = 1,
                HeadlineName = "hl1",
                ProductId = 0,
                affectedId = "1,2",
                affectedName = "1,2",
                MatchId = 1,
                MatchCode = "0",
                EventType = 3,
                ActualResult = "win",
                WrongResult = "lose",
                Listbettype = "10",
                Currency = "HKK",
                MessageId = 338,
                Site = "789y",
                IsVip = true,
                Sporttype = 0,
                LeagueId = 93816,
                Leaguenametc = "Basketball",
                HomeId = 714227,
                Awayid = 0,
                KO_time = DateTime.Parse("2021-02-23 19:59:00.000"),
                Eventdate = DateTime.Parse("2021-02-24 19:59:00.000"),
                Race_time = DateTime.Parse("2021-02-25 19:59:00.000")
            };

            var service = MockService();
            var testDescTemp = new AmendmentDescriptionTemplate
            {
                VirtualSports180186 = "Test VirtualSports Template"
            };
            commonFeedback.GetFeedbackTemplates<AmendmentDescriptionTemplate>("AmendmentDescriptionTemplate.xml").Returns(testDescTemp);
            var stringBuilder = new StringBuilder(DefaultValue.autoGenerated);
            stringBuilder.Append(BackgroundJobConstants.Break);
            service.Protected("GetTeamNameFunction", amendment.EventType).Returns(commonFeedback.GetTeamName);
            commonFeedback.GetProductName(amendment.ProductId.ToString(), Language.en_US).Returns("Virtual Sports");
            commonFeedback.GetSportName(amendment.Sporttype.ToString(), Language.en_US).Returns("SportName");
            commonFeedback.GetLeagueName(amendment.LeagueId.ToString(), Language.en_US).Returns("LeagueName");
            commonFeedback.GetTeamName(amendment.HomeId.ToString(), Language.en_US).Returns("HomeName");
            commonFeedback.GetTeamName(amendment.Awayid.ToString(), Language.en_US).Returns("AwayName");
            var virtualSports = new StringBuilder("test virtualSports");
            service.Protected("GetAmendmentDescriptionVirtualSports", amendment, testDescTemp, "SportName", "HomeName", "AwayName", "07:59 AM", "24-Feb-2021").Returns(virtualSports);
            stringBuilder.Append(virtualSports);
            stringBuilder.Append(BackgroundJobConstants.Break);

            // Act
            var actual = service.Public_GetAmendmentDescription(amendment, false);

            // Assert
            Assert.Equal(stringBuilder.ToString(), actual);
        }

        [Fact]
        public void GetAmendmentDescription_ProducIsNumberGame_ReturnExactTemplate()
        {
            // Arrange
            var amendment = new AmendmentAutoFeedbackInfor
            {
                LogId = 1,
                TicketEventId = 1,
                HeadlineId = 1,
                HeadlineName = "hl1",
                ProductId = 0,
                affectedId = "1,2",
                affectedName = "1,2",
                MatchId = 1,
                MatchCode = "0",
                EventType = 3,
                ActualResult = "win",
                WrongResult = "lose",
                Listbettype = "10",
                Currency = "HKK",
                MessageId = 338,
                Site = "789y",
                IsVip = true,
                Sporttype = 0,
                LeagueId = 93816,
                LeagueGroupId = 19381,
                Leaguenametc = "Basketball",
                HomeId = 714227,
                Awayid = 0,
                KO_time = DateTime.Parse("2021-02-23 19:59:00.000"),
                Eventdate = DateTime.Parse("2021-02-24 19:59:00.000"),
                Race_time = DateTime.Parse("2021-02-25 19:59:00.000")
            };

            var service = MockService();
            var testDescTemp = new AmendmentDescriptionTemplate
            {
                NumberGame = "test number game"
            };
            commonFeedback.GetFeedbackTemplates<AmendmentDescriptionTemplate>("AmendmentDescriptionTemplate.xml").Returns(testDescTemp);
            var stringBuilder = new StringBuilder(DefaultValue.autoGenerated);
            stringBuilder.Append(BackgroundJobConstants.Break);
            service.Protected("GetTeamNameFunction", amendment.EventType).Returns(commonFeedback.GetTeamName);
            commonFeedback.GetProductName(amendment.ProductId.ToString(), Language.en_US).Returns("Number Game");
            commonFeedback.GetSportName(amendment.Sporttype.ToString(), Language.en_US).Returns("SportName");
            commonFeedback.GetLeagueNameBySiteName(amendment.Site, amendment.LeagueId, amendment.LeagueGroupId, Language.en_US).Returns("LeagueName");
            commonFeedback.GetTeamName(amendment.HomeId.ToString(), Language.en_US).Returns("HomeName");
            commonFeedback.GetTeamName(amendment.Awayid.ToString(), Language.en_US).Returns("AwayName");

            var numberGame = new StringBuilder("numberGame");

            service.Protected("GetAmendmentDescriptionNumberGame", amendment, testDescTemp, "LeagueName", "SportName", "24-Feb-2021").Returns(numberGame);

            stringBuilder.Append(numberGame);
            stringBuilder.Append(BackgroundJobConstants.Break);

            // Act
            var actual = service.Public_GetAmendmentDescription(amendment, false);

            // Assert
            Assert.Equal(stringBuilder.ToString(), actual);
        }

        #endregion GetAmendmentDescription

        #region GetAmendmentDescriptionVirtualSports

        [Theory]
        [InlineData(180, "test VirtualSports180186")]
        [InlineData(186, "test VirtualSports180186")]
        [InlineData(181, "test VirtualSports181To185")]
        [InlineData(182, "test VirtualSports181To185")]
        [InlineData(185, "test VirtualSports181To185")]
        public void GetAmendmentDescriptionVirtualSports_DependsSporttype_ReturnTempateForVirtualSports(byte Sporttype, string template)
        {
            // Arrange
            var amendment = new AmendmentAutoFeedbackInfor
            {
                Sporttype = Sporttype,
                Eventdate = DateTime.Parse("2021-02-24 19:59:00.000")
            };
            var amendmentDescTemplate = new AmendmentDescriptionTemplate
            {
                VirtualSports180186 = "test VirtualSports180186",
                VirtualSports181To185 = "test VirtualSports181To185"
            };

            var stringBuilder = new StringBuilder();
            stringBuilder.Append(template);
            var service = MockService();

            // Act
            var actual = service.Public_GetAmendmentDescriptionVirtualSports(amendment,
                amendmentDescTemplate,
                "sportName",
                "homeName",
                "awayName",
                "07:59 AM",
                "24 Feb 2021");

            // Assert
            Assert.Equal(stringBuilder.ToString(), actual.ToString());
        }

        #endregion GetAmendmentDescriptionVirtualSports

        #region GetAmendmentDescriptionNumberGame

        [Fact]
        public void GetAmendmentDescriptionNumberGame_WhenCalled_ReturnExactTemplate()
        {
            // Arrange
            var amendmentDescTemplate = new AmendmentDescriptionTemplate
            {
                NumberGame = "test numbergame"
            };
            var amendment = new AmendmentAutoFeedbackInfor
            {
                MatchCode = "MatchCode"
            };
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(amendmentDescTemplate.NumberGame);
            var service = MockService();

            // Act
            var actual = service.Public_GetAmendmentDescriptionNumberGame(amendment,
                amendmentDescTemplate,
                "SportName",
                "leagueName",
                "24 Feb 2021");

            // Assert
            Assert.Equal(stringBuilder.ToString(), actual.ToString());
        }

        #endregion GetAmendmentDescriptionNumberGame

        #region GetAmendmentDescriptionSportOutRight

        [Fact]
        public void GetAmendmentDescriptionSportOutRight_WhenCalled_ReturnExactTemplate()
        {
            // Arrange
            var amendmentDescTemplate = new AmendmentDescriptionTemplate
            {
                SportsbookOutrightEventType9 = "test SportsbookOutright",
                SportsbookOutrightEventType2 = "test SportsbookOutright"
            };
            var amendment = fixture.Create<AmendmentAutoFeedbackInfor>();
            fixture.Customize<AmendmentAutoFeedbackInfor>(x => x.With(b => b.EventType, 2));

            var stringBuilder = new StringBuilder();
            stringBuilder.Append(amendmentDescTemplate.SportsbookOutrightEventType9);
            var service = MockService();

            // Act
            var actual = service.Public_GetAmendmentDescriptionSportOutRight(
                amendment,
                amendmentDescTemplate,
                "kO_time",
                "leagueName",
                "homeName");

            // Assert
            Assert.Equal(stringBuilder.ToString(), actual.ToString());
        }

        #endregion GetAmendmentDescriptionSportOutRight

        #region GetAmenmendSolution

        [Fact]
        public void GetAmenmendSolution_WhenCalled_ReturnExactTemplate()
        {
            // Arrange
            var service = MockService();
            var amendmentSolTemplate = fixture.Create<AmendmentSolutionTemplate>();
            var amendment = fixture.Create<AmendmentAutoFeedbackInfor>();
            var solutionTemplateEN = new StringBuilder("solutionTemplateEN");
            var solutionTemplateSC = new StringBuilder("solutionTemplateSC");
            var messageContent = fixture.Create<IEnumerable<Message>>();
            var stringBuilder = new StringBuilder();
            commonFeedback.GetFeedbackTemplates<AmendmentSolutionTemplate>("AmendmentSolutionTemplate.xml").Returns(amendmentSolTemplate);
            service.Protected("GetAmendmentSolutionTemplateEN", amendment, amendmentSolTemplate, messageContent).Returns(solutionTemplateEN);
            service.Protected("GetAmendmentSolutionTemplateYNSC", amendment, amendmentSolTemplate, messageContent, false).Returns(solutionTemplateSC);
            stringBuilder.Append(solutionTemplateEN);
            stringBuilder.Append(solutionTemplateSC);

            // Act
            var actual = service.Public_GetAmenmendSolution(amendment, messageContent, false);

            // Assert
            Assert.Equal(stringBuilder.ToString(), actual.ToString());
        }

        [Theory]
        [InlineData(false, "789y", "NoResultENSC")]
        [InlineData(true, "789y", "NoResultENYN")]
        public void GetAmenmendSolution_IsNoResultTemplate_ReturnExactTemplate(bool isVip, string site, string expected)
        {
            // Arrange
            var service = MockService();
            var amendmentSolTemplate = new AmendmentSolutionTemplate
            {
                NoResultENSC = "NoResultENSC",
                NoResultENYN = "NoResultENYN"
            };
            var amendment = new AmendmentAutoFeedbackInfor { SiteLabel = site, IsVip = isVip };
            var messageContent = fixture.Create<IEnumerable<Message>>();
            var stringBuilder = new StringBuilder();
            commonFeedback.GetFeedbackTemplates<AmendmentSolutionTemplate>("AmendmentSolutionTemplate.xml").Returns(amendmentSolTemplate);
            stringBuilder.Append(expected);

            // Act
            var actual = service.Public_GetAmenmendSolution(amendment, messageContent, true);

            // Assert
            Assert.Equal(stringBuilder.ToString(), actual.ToString());
        }

        #endregion GetAmenmendSolution

        #region GetAmendmentSolutionTemplateEN

        [Theory]
        [InlineData("Sportsbook", "test Sportsbook")]
        [InlineData("Virtual Sports 2", "test Virtual Sports 2")]
        [InlineData("Virtual Sports", "test Virtual Sports")]
        [InlineData("Number Game", "Number Game")]
        [InlineData("", "")]
        public void GetAmendmentSolutionTemplateEN_DependProductName_ReturnExactTemplateEn(
            string productName, string expectedAppend)
        {
            // Arrange
            var amendment = new AmendmentAutoFeedbackInfor
            {
                LogId = 1,
                TicketEventId = 1,
                HeadlineId = 1,
                HeadlineName = "hl1",
                ProductId = 0,
                affectedId = "1,2",
                affectedName = "1,2",
                MatchId = 1,
                MatchCode = "0",
                EventType = 3,
                ActualResult = "win",
                WrongResult = "lose",
                Listbettype = "10",
                Currency = "HKK",
                MessageId = 338,
                Site = "789y",
                IsVip = true,
                Sporttype = 0,
                LeagueId = 93816,
                Leaguenametc = "Basketball",
                HomeId = 714227,
                Awayid = 0,
                KO_time = DateTime.Parse("2021-02-23 19:59:00.000"),
                Eventdate = DateTime.Parse("2021-02-24 19:59:00.000"),
                Race_time = DateTime.Parse("2021-02-25 19:59:00.000")
            };

            var amendmentSolTemplate = new AmendmentSolutionTemplate
            {
                SportsbookEN = expectedAppend,
                VirtualSports2EN = expectedAppend,
                VirtualSportsEN = expectedAppend,
                NumberGameEN = expectedAppend
            };
            var messageContent = fixture.Create<IEnumerable<Message>>();
            var service = MockService();
            commonFeedback.GetProductName(amendment.ProductId.ToString(), Language.en_US).Returns(productName);
            commonFeedback.GetSportName(amendment.Sporttype.ToString(), Language.en_US).Returns(productName);
            commonFeedback.GetLeagueName(amendment.LeagueId.ToString(), Language.en_US).Returns(productName);
            commonFeedback.GetTeamName(amendment.HomeId.ToString(), Language.en_US).Returns(productName);
            commonFeedback.GetTeamName(amendment.Awayid.ToString(), Language.en_US).Returns(productName);
            commonFeedback.GetMatchCode(amendment.MatchCode, Language.en_US).Returns("MatchCode");
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(expectedAppend);

            // Act
            var actual = service.Public_GetAmendmentSolutionTemplateEN(amendment, amendmentSolTemplate, messageContent);

            // Assert
            Assert.Equal(stringBuilder.ToString(), actual.ToString());
        }

        [Theory]
        [InlineData(2, "test OutrightEN")]
        [InlineData(9, "test OutrightEN")]
        [InlineData(5, "test Sportsbook")]
        public void GetAmendmentSolutionTemplateEN_DependEventType_ReturnExactTemplateOutRightEn(
            byte EventType, string expectedAppend)
        {
            // Arrange
            var amendment = new AmendmentAutoFeedbackInfor
            {
                LogId = 1,
                TicketEventId = 1,
                HeadlineId = 1,
                HeadlineName = "hl1",
                ProductId = 0,
                affectedId = "1,2",
                affectedName = "1,2",
                MatchId = 1,
                MatchCode = "0",
                EventType = EventType,
                ActualResult = "win",
                WrongResult = "lose",
                Listbettype = "10",
                Currency = "HKK",
                MessageId = 338,
                Site = "789y",
                IsVip = true,
                Sporttype = 0,
                LeagueId = 93816,
                Leaguenametc = "Basketball",
                HomeId = 714227,
                Awayid = 0,
                KO_time = DateTime.Parse("2021-02-23 19:59:00.000"),
                Eventdate = DateTime.Parse("2021-02-24 19:59:00.000"),
                Race_time = DateTime.Parse("2021-02-25 19:59:00.000")
            };

            var amendmentSolTemplate = new AmendmentSolutionTemplate
            {
                SportsbookEN = expectedAppend,
                VirtualSports2EN = expectedAppend,
                VirtualSportsEN = expectedAppend,
                NumberGameEN = expectedAppend,
                OutrightEN = expectedAppend
            };
            var messageContent = fixture.Create<IEnumerable<Message>>();
            var service = MockService();
            commonFeedback.GetProductName(amendment.ProductId.ToString(), Language.en_US).Returns("Sportsbook");

            var stringBuilder = new StringBuilder();
            stringBuilder.Append(expectedAppend);

            // Act
            var actual = service.Public_GetAmendmentSolutionTemplateEN(amendment, amendmentSolTemplate, messageContent);

            // Assert
            Assert.Equal(stringBuilder.ToString(), actual.ToString());
        }

        #endregion GetAmendmentSolutionTemplateEN

        #region GetAmendmentSolutionTemplateYNSC

        [Theory]
        [InlineData("Virtual Sports", "test Virtual Sports")]
        [InlineData("Number Game", "test Number Game")]
        public void GetAmendmentSolutionTemplateYNSC_DependProductName_ReturnExactTemplate(string productName, string expected)
        {
            // Arrange
            var service = MockService();

            var amendment = new AmendmentAutoFeedbackInfor
            {
                LogId = 1,
                TicketEventId = 1,
                HeadlineId = 1,
                HeadlineName = "hl1",
                ProductId = 0,
                affectedId = "1,2",
                affectedName = "1,2",
                MatchId = 1,
                MatchCode = "0",
                EventType = 3,
                ActualResult = "win",
                WrongResult = "lose",
                Listbettype = "10",
                Currency = "HKK",
                MessageId = 338,
                Site = "789y",
                IsVip = false,
                Sporttype = 161,
                LeagueId = 93816,
                LeagueGroupId = 93816,
                Leaguenametc = "Basketball",
                HomeId = 714227,
                Awayid = 0,
                KO_time = DateTime.Parse("2021-02-23 19:59:00.000"),
                Eventdate = DateTime.Parse("2021-02-24 19:59:00.000"),
                Race_time = DateTime.Parse("2021-02-25 19:59:00.000")
            };
            var amendmentSolTemplate = new AmendmentSolutionTemplate
            {
                NumberGameSC = "test NumberGameSC",
                NumberGameYN = "test NumberGameYN",
                VirtualSportsSC = "test VirtualSportsSC",
                VirtualSportsYN = "test VirtualSportsYN"
            };
            var expectedReturn = new StringBuilder(expected);
            var isVipCust = amendment.Site == "789y" && amendment.IsVip;

            var messageContent = fixture.Create<IEnumerable<Message>>();
            commonFeedback.GetProductName(amendment.ProductId.ToString(), Language.en_US).Returns(productName);
            commonFeedback.GetLeagueNameBySiteName(amendment.Site, amendment.LeagueId, amendment.LeagueGroupId, Language.vi_VN).Returns("leagueNameYN");
            commonFeedback.GetLeagueNameBySiteName(amendment.Site, amendment.LeagueId, amendment.LeagueGroupId, Language.zh_CN).Returns("leagueNameSC");
            commonFeedback.GetSportName(amendment.Sporttype.ToString(), Language.vi_VN).Returns("sportNameYN");
            commonFeedback.GetSportName(amendment.Sporttype.ToString(), Language.zh_CN).Returns("sportNameSC");

            service.Protected("GetAmendmentSolutionTemplateYnScVirtualSport", amendment.affectedName,
                amendmentSolTemplate,
                false,
                "sportNameYN",
                "sportNameSC").Returns(expectedReturn);
            service.Protected("GetAmendmentSolutionTemplateYnScNumberGame",
                amendment.MatchCode,
                amendment.affectedName,
                amendmentSolTemplate,
                false,
                "leagueNameYN",
                amendment.Eventdate.ToString(DefaultValue.Formatddmm),
                "leagueNameSC").Returns(expectedReturn);
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(expectedReturn);

            // Act
            var actual = service.Public_GetAmendmentSolutionTemplateYNSC(amendment, amendmentSolTemplate, messageContent, isVipCust);

            // Assert
            Assert.Equal(stringBuilder.ToString(), actual.ToString());
        }

        [Theory]
        [InlineData("Sportsbook", "789y", true, 0, "test Sportsbook Yn")]
        [InlineData("Sportsbook", "789y", false, 0, "test Sportsbook Sc")]
        [InlineData("Sportsbook", "789y", false, 2, "test Outright Sc")]
        [InlineData("Sportsbook", "789y", false, 9, "test Outright Sc")]
        [InlineData("Sportsbook", "789y", false, 3, "test Sportsbook Sc")]
        [InlineData("Virtual Sports 2", "789y", true, 0, "test VirtualSports2 Yn")]
        [InlineData("Virtual Sports 2", "789y", false, 0, "test VirtualSports2 Sc")]
        [InlineData("Virtual Sports 5", "789y", true, 0, "test VirtualSports2 Yn")]
        [InlineData("Virtual Sports 5", "789y", false, 0, "test VirtualSports2 Sc")]

        public void GetAmendmentSolutionTemplateYNSC_DependProductNameAndIsVip_ReturnExactTemplate(string productName, string site, bool isVip, byte EventType, string expected)
        {
            // Arrange
            var service = MockService();

            var amendment = new AmendmentAutoFeedbackInfor
            {
                LogId = 1,
                TicketEventId = 1,
                HeadlineId = 1,
                HeadlineName = "hl1",
                ProductId = 0,
                affectedId = "1,2",
                affectedName = "1,2",
                MatchId = 1,
                MatchCode = "0",
                EventType = EventType,
                ActualResult = "win",
                WrongResult = "lose",
                Listbettype = "10",
                Currency = "HKK",
                MessageId = 338,
                Site = site,
                IsVip = isVip,
                Sporttype = 0,
                LeagueId = 93816,
                LeagueGroupId = 93816,
                Leaguenametc = "Basketball",
                HomeId = 714227,
                Awayid = 0,
                KO_time = DateTime.Parse("2021-02-23 19:59:00.000"),
                Eventdate = DateTime.Parse("2021-02-24 19:59:00.000"),
                Race_time = DateTime.Parse("2021-02-25 19:59:00.000")
            };
            var isVipCust = amendment.Site == "789y" && amendment.IsVip;

            var amendmentSolTemplate = new AmendmentSolutionTemplate
            {
                SportsbookYN = "test Sportsbook Yn",
                SportsbookSC = "test Sportsbook Sc",
                VirtualSports2SC = "test VirtualSports2 Sc",
                VirtualSports2YN = "test VirtualSports2 Yn",
                OutrightSC = "test Outright Sc",
                VirtualSportsSC = "test Sportsbook sc"
            };

            var sportBook = new StringBuilder(expected);
            var sportBookOrOutright = new StringBuilder(expected);


            var messageContent = fixture.Create<IEnumerable<Message>>();
            service.Protected("GetTeamNameFunction", amendment.EventType).Returns(commonFeedback.GetTeamName);
            commonFeedback.GetProductName(amendment.ProductId.ToString(), Language.en_US).Returns(productName);
            commonFeedback.GetLeagueNameBySiteName(amendment.Site, amendment.LeagueId, amendment.LeagueGroupId, Language.vi_VN).Returns("leagueNameYN");
            commonFeedback.GetLeagueNameBySiteName(amendment.Site, amendment.LeagueId, amendment.LeagueGroupId, Language.zh_CN).Returns("leagueNameSC");
            commonFeedback.GetLeagueName3rd(amendment.LeagueId.ToString(), Language.vi_VN).Returns("leagueNameYN");
            commonFeedback.GetLeagueName3rd(amendment.LeagueId.ToString(), Language.zh_CN).Returns("leagueNameSC");
            commonFeedback.GetSportName(amendment.Sporttype.ToString(), Language.vi_VN).Returns("sportNameYN");
            commonFeedback.GetSportName(amendment.Sporttype.ToString(), Language.zh_CN).Returns("sportNameSC");
            commonFeedback.GetTeamName(amendment.HomeId.ToString(), Language.vi_VN).Returns("homeNameYN");
            commonFeedback.GetTeamName(amendment.Awayid.ToString(), Language.vi_VN).Returns("awayNameYN");
            var Eventdate = amendment.Eventdate.ToString(DefaultValue.Formatddmm);

            service.Protected("GetAmendmentSolutionTemplateYnSportBook", amendment,
                amendmentSolTemplate,
                messageContent,
                "sportNameYN",
                "homeNameYN",
                "awayNameYN",
                "leagueNameYN").Returns(sportBook);
            service.Protected("GetAmendmentSolutionTemplateSportBookOrOutright", amendment, isVipCust, sportBook, messageContent, amendmentSolTemplate, Eventdate).Returns(sportBookOrOutright);
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(sportBookOrOutright);

            // Act
            var actual = service.Public_GetAmendmentSolutionTemplateYNSC(amendment, amendmentSolTemplate, messageContent, isVipCust);

            // Assert
            Assert.Equal(stringBuilder.ToString(), actual.ToString());
        }


        #endregion GetAmendmentSolutionTemplateYNSC

        #region GetAmendmentSolutionTemplateYnScVirtualSport

        [Theory]
        [InlineData(true, "test VirtualSportsYN")]
        [InlineData(false, "test VirtualSportsSC")]
        public void GetAmendmentSolutionTemplateYnScVirtualSport_WhenCalled_ReturnExactTemplate(bool isVipCust, string expected)
        {
            // Arrange
            var amendmentSolTemplate = new AmendmentSolutionTemplate
            {
                VirtualSportsYN = "test VirtualSportsYN",
                VirtualSportsSC = "test VirtualSportsSC"
            };

            var stringBuilder = new StringBuilder();
            stringBuilder.Append(expected);
            var service = MockService();

            // Act
            var actual = service.Public_GetAmendmentSolutionTemplateYnScVirtualSport("affectedName",
                amendmentSolTemplate,
                isVipCust,
                "sportNameYN",
                "sportNameSC");

            // Assert
            Assert.Equal(stringBuilder.ToString(), actual.ToString());
        }

        #endregion GetAmendmentSolutionTemplateYnScVirtualSport

        #region GetAmendmentSolutionTemplateYnScNumberGame

        [Theory]
        [InlineData(true, "test NumberGameYN")]
        [InlineData(false, "test NumberGameSC")]
        public void GetAmendmentSolutionTemplateYnScNumberGame_WhenCalled_ReturnExactTemplate(bool isVipCust, string expected)
        {
            // Arrange
            var amendmentSolTemplate = new AmendmentSolutionTemplate
            {
                NumberGameYN = "test NumberGameYN",
                NumberGameSC = "test NumberGameSC"
            };

            var stringBuilder = new StringBuilder();
            stringBuilder.Append(expected);
            var service = MockService();

            // Act
            var actual = service.Public_GetAmendmentSolutionTemplateYnScNumberGame("MatchCode", "affectedName",
                amendmentSolTemplate,
                isVipCust,
                "leagueNameYN",
                "Eventdate",
                "leagueNameSC");

            // Assert
            Assert.Equal(stringBuilder.ToString(), actual.ToString());
        }

        #endregion GetAmendmentSolutionTemplateYnScNumberGame



        #region Helpers

        private ResultAmendmentServiceTestable MockService()
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
            customerService = Substitute.For<ICustomerService>();
            logService = Substitute.For<ICrmLogService>();
            transactionService = Substitute.For<IFeedbackTransactionService>();
            commonFeedback = Substitute.For<ICommonFeedback>();

            return Substitute.For<ResultAmendmentServiceTestable>(
                appJobSettings,
                baseService,
                settingService,
                customerService,
                logService,
                commonFeedback,
                transactionService
                );
        }

        #endregion Helpers
    }
}