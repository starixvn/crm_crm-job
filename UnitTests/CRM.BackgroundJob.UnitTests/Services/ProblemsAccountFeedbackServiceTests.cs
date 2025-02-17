namespace CRM.BackgroundJob.UnitTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using CRM.BackgroundJob.Models.ProblemsAccount;
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

    public class ProblemsAccountFeedbackServiceTests
    {
        private AppJobSettings appSettings;
        private IBaseService baseService;
        private ISettingService settingService;
        private ICustomerService customerService;
        private ICrmLogService logService;
        private IFeedbackTransactionService transactionService;
        private ICommonFeedback commonFeedback;
        private readonly IFixture fixture = new Fixture();

        #region CreateFeedbackForProblemsAccountAsync

        [Fact]
        public async Task CreateFeedbackForProblemsAccountAsync_NotExistedproblemsAccountInfor_ExpectedUpdateSystemSettingAndReturn()
        {
            // Arrange
            var service = MockService();
            var problemsAccountSetting = fixture.Create<ProblemsAccountSetting>();
            var custCategory = fixture.Create<IEnumerable<CustCategoryInfor>>();
            var problemsAccountInfor = new List<ProblemsAccountAutoFeedbackInfor>();
            var LastRuntimeEnv = DateTime.Now;
            customerService.GetCustCategoryInfor(problemsAccountSetting, false).Returns(custCategory);
            transactionService.ImportProblemsAccountFeedbackInfoAsync(Arg.Any<IEnumerable<GroupHeadline>>(), custCategory, "LIVE").Returns(problemsAccountInfor);

            // Act
            await service.Public_CreateFeedbackForProblemsAccountAsync(problemsAccountSetting, false, LastRuntimeEnv, "LastRuntimeNameEnv", "JobStatusNameEnv", "LIVE");

            // Assert
            await commonFeedback.EditSystemSettingAsyncByEnv(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            logService.Received(1).LogInformation(Arg.Any<string>());
            service.DidNotReceive().Protected(
                "CreateProblemsAccountFeedbackCreditAsync",
                Arg.Any<IEnumerable<ProblemsAccountAutoFeedbackInfor>>(),
                Arg.Any<IEnumerable<SettingItem>>(),
                Arg.Any<IEnumerable<SystemSettingMapping>>());
        }

        [Fact]
        public async Task CreateFeedbackForProblemsAccountAsync_ExistedProblemsAccountInforAndUserNamePerCategoryLessThan250_ExpectedCallExactFunction()
        {
            // Arrange
            var service = MockService();
            var problemsAccountSetting = fixture.Create<ProblemsAccountSetting>();
            var custCategory = fixture.Create<IEnumerable<CustCategoryInfor>>();
            var problemsAccountInfor = new List<ProblemsAccountAutoFeedbackInfor>
            {
                new ProblemsAccountAutoFeedbackInfor
                {
                    LogID = 1,
                    CustID = 1,
                    UserName = "test",
                    HeadlineID = 1,
                    HeadlineName = "HeadlineName",
                    IsVip = true,
                    RelatedCustID = 1,
                    CategoryID = 1
                }
            };
            var LastRuntimeEnv = DateTime.Now;
            customerService.GetCustCategoryInfor(problemsAccountSetting, false).Returns(custCategory);
            transactionService.ImportProblemsAccountFeedbackInfoAsync(Arg.Any<IEnumerable<GroupHeadline>>(), custCategory, "LIVE").Returns(problemsAccountInfor);

            // Act
            await service.Public_CreateFeedbackForProblemsAccountAsync(problemsAccountSetting, false, LastRuntimeEnv, "LastRuntimeNameEnv", "JobStatusNameEnv", "LIVE");

            // Assert
            await commonFeedback.EditSystemSettingUpdateLastLogIdAsyncByEnv(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string>());
            service.Received().Protected(
                "CreateProblemsAccountFeedbackCreditAsync",
                Arg.Any<IEnumerable<ProblemsAccountAutoFeedbackInfor>>(),
                Arg.Any<IEnumerable<SettingItem>>(),
                Arg.Any<IEnumerable<SystemSettingMapping>>());
        }

        [Fact]
        public async Task CreateFeedbackForProblemsAccountAsync_ExistedProblemsAccountInforAndUserNamePerCategoryGreaterThan250_ExpectedCallExactFunction()
        {
            // Arrange
            var service = MockService();
            var problemsAccountSetting = fixture.Create<ProblemsAccountSetting>();
            var custCategory = fixture.Create<IEnumerable<CustCategoryInfor>>();
            var problemsAccountInfor = new List<ProblemsAccountAutoFeedbackInfor>();

            for (int i = 0; i < 255; i++)
            {
                var userName = i;
                var problem = new ProblemsAccountAutoFeedbackInfor
                {
                    LogID = 1,
                    CustID = 1,
                    UserName = userName.ToString(),
                    HeadlineID = 1,
                    HeadlineName = "1",
                    IsVip = true,
                    RelatedCustID = 1,
                    CategoryID = 1
                };
                problemsAccountInfor.Add(problem);
            }
            var cateProblems = new ProblemsAccountAutoFeedbackInfor
            {
                LogID = 1,
                CustID = 1,
                UserName = "1",
                HeadlineID = 1,
                HeadlineName = "1",
                IsVip = true,
                RelatedCustID = 1,
                CategoryID = 2
            };
            problemsAccountInfor.Add(cateProblems);

            var LastRuntimeEnv = DateTime.Now;
            customerService.GetCustCategoryInfor(problemsAccountSetting, true).Returns(custCategory);
            transactionService.ImportProblemsAccountFeedbackInfoAsync(Arg.Any<IEnumerable<GroupHeadline>>(), custCategory, "LIVE").Returns(problemsAccountInfor);

            // Act
            await service.Public_CreateFeedbackForProblemsAccountAsync(problemsAccountSetting, true, LastRuntimeEnv, "LastRuntimeNameEnv", "JobStatusNameEnv", "LIVE");

            // Assert
            service.Received().Protected(
                "CreateProblemsAccountFeedbackByBatchAsync",
                Arg.Any<IEnumerable<ProblemsAccountAutoFeedbackInfor>>(),
                Arg.Any<IEnumerable<ProblemsAccountAutoFeedbackInfor>>(),
                Arg.Any<string>(),
                Arg.Any<IEnumerable<SettingItem>>(),
                Arg.Any<IEnumerable<SystemSettingMapping>>());
        }

        #endregion CreateFeedbackForProblemsAccountAsync

        #region CreateProblemsAccountFeedbackByBatchAsync

        [Fact]
        public async Task CreateProblemsAccountFeedbackByBatchAsync_DevideBatchInsertFeedback_ExpectedCallFunctionExactParams()
        {
            // Arrange
            var service = MockService();
            var problemsAccounts = new List<ProblemsAccountAutoFeedbackInfor>();
            for (int i = 0; i < 560; i++)
            {
                var userName = i;
                var problem = new ProblemsAccountAutoFeedbackInfor
                {
                    LogID = 1,
                    CustID = 1,
                    UserName = userName.ToString(),
                    HeadlineID = 1,
                    HeadlineName = "1",
                    IsVip = true,
                    RelatedCustID = 1,
                    CategoryID = 1
                };
                problemsAccounts.Add(problem);
            }

            var categoryProblemAccount = problemsAccounts.Where(x => x.CategoryID == 1);
            // Act
            await service.Public_CreateProblemsAccountFeedbackByBatchAsync(categoryProblemAccount, problemsAccounts, "1", Arg.Any<IEnumerable<SettingItem>>(), Arg.Any<IEnumerable<SystemSettingMapping>>());

            // Assert
            service.Received(2).Protected(
                "CreateProblemsAccountFeedbackCreditAsync",
                Arg.Any<IEnumerable<ProblemsAccountAutoFeedbackInfor>>(),
                Arg.Any<IEnumerable<SettingItem>>(),
                Arg.Any<IEnumerable<SystemSettingMapping>>());
        }

        [Fact]
        public async Task CreateProblemsAccountFeedbackByBatchAsync_DevideBatchInsertFeedbackLastItem_ExpectedCallFunctionExactParams()
        {
            // Arrange
            var service = MockService();
            var problemsAccounts = new List<ProblemsAccountAutoFeedbackInfor>();
            for (int i = 0; i < 560; i++)
            {
                var userName = i;
                var problem = new ProblemsAccountAutoFeedbackInfor
                {
                    LogID = 1,
                    CustID = 1,
                    UserName = userName.ToString(),
                    HeadlineID = 1,
                    HeadlineName = "1",
                    IsVip = true,
                    RelatedCustID = 1,
                    CategoryID = (short)(i < 252 ? Convert.ToInt32(1) : Convert.ToInt32(2))
                };
                problemsAccounts.Add(problem);
            }

            var categoryProblemAccount = problemsAccounts.Where(x => x.CategoryID == 1);
            // Act
            await service.Public_CreateProblemsAccountFeedbackByBatchAsync(categoryProblemAccount, problemsAccounts, "1", Arg.Any<IEnumerable<SettingItem>>(), Arg.Any<IEnumerable<SystemSettingMapping>>());

            // Assert
            service.Received(1).Protected(
                "CreateProblemsAccountFeedbackCreditAsync",
                Arg.Any<IEnumerable<ProblemsAccountAutoFeedbackInfor>>(),
                Arg.Any<IEnumerable<SettingItem>>(),
                Arg.Any<IEnumerable<SystemSettingMapping>>());
        }

        #endregion CreateProblemsAccountFeedbackByBatchAsync

        #region CreateProblemsAccountFeedbackCreditAsync

        [Fact]
        public async Task CreateProblemsAccountFeedbackCreditAsync_CreateNewCriteria_ExpectedToCallFunction()
        {
            // Arrange
            var service = MockService();
            var problemsAccounts = new List<ProblemsAccountAutoFeedbackInfor>
            {
                new ProblemsAccountAutoFeedbackInfor
                {
                    LogID = 1,
                    CustID = 1,
                    UserName = "1",
                    HeadlineID = 1,
                    HeadlineName = "1",
                    IsVip = true,
                    RelatedCustID = 1,
                    CategoryID = 1
                }
            };
            var settings = new List<SettingItem>
            {
                new SettingItem { SettingId = 1, SettingName = "Status", SettingValue = "Closed" },
                new SettingItem { SettingId = 2, SettingName = "Response", SettingValue = "Neutral" },
                new SettingItem { SettingId = 2022, SettingName = "Station" }
            };

            var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();
            var category = fixture.Create<Tuple<int, int, string>>();
            commonFeedback.GetCategorySubCategoryCWL(settings, DefaultValue.ProblemAccount).Returns(category);
            service.Protected("GetCustomerXml", problemsAccounts).Returns("GetCustomerXml");
            service.Protected("GetProductXml", settings).Returns("GetProductXml");
            service.Protected("GetSolution", problemsAccounts).Returns("GetSolution");
            service.Protected("GetDescription").Returns("GetDescription");

            // Act
            await service.Public_CreateProblemsAccountFeedbackCreditAsync(problemsAccounts, settings, settingMappings);

            // Assert
            service.Received().Protected("CheckValidParamsAndInsertFeedbackAsync", problemsAccounts
                , Arg.Any<CreateMutipleFeedbackCommand>());
        }

        #endregion CreateProblemsAccountFeedbackCreditAsync

        #region CheckValidParamsAndInsertFeedbackAsync

        [Fact]
        public async Task CheckValidParamsAndInsertFeedbackAsync_CreateSuccessfull_ExpectedWriteLog()
        {
            // Arrange
            var problemsAccounts = fixture.Create<IEnumerable<ProblemsAccountAutoFeedbackInfor>>();
            var criteria = fixture.Create<CreateMutipleFeedbackCommand>();
            var service = MockService();
            commonFeedback.CheckFeedbackValidParams(criteria).Returns(new ValueTuple<bool, string>(true, ""));

            var commandResult = new CommandResult { Code = 0 };
            baseService.ExecuteNonQueryAsync(criteria).Returns(commandResult);

            // Act
            await service.Public_CheckValidParamsAndInsertFeedbackAsync(
                problemsAccounts,
                criteria
                );

            // Assert
            logService.Received(1).LogInformation(Arg.Any<string>());
        }

        [Fact]
        public async Task CheckValidParamsAndInsertFeedbackAsync_CreateUnSuccessfull_ExpectedWriteLogAndUpdateStatus()
        {
            // Arrange
            var problemsAccounts = fixture.Create<IEnumerable<ProblemsAccountAutoFeedbackInfor>>();
            var criteria = fixture.Create<CreateMutipleFeedbackCommand>();
            var service = MockService();
            commonFeedback.CheckFeedbackValidParams(criteria).Returns(new ValueTuple<bool, string>(true, ""));

            var commandResult = new CommandResult { Code = 1, Message = "" };
            baseService.ExecuteNonQueryAsync(criteria).Returns(commandResult);

            // Act
            await service.Public_CheckValidParamsAndInsertFeedbackAsync(
                problemsAccounts,
                criteria
                );

            // Assert
            logService.Received(1).LogInformation(Arg.Any<string>());
            await transactionService.EditProblemsAccountFeedbackInfoAsync(Arg.Any<string>(), CreateFeedbackJobStatus.Fail, commandResult.Message);
            await logService.ErrorAsync(commandResult, criteria);
        }

        [Fact]
        public async Task CheckValidParamsAndInsertFeedbackAsync_InvalidParams_ExpectedReturnAndUpdateStatus()
        {
            // Arrange
            var problemsAccounts = fixture.Create<IEnumerable<ProblemsAccountAutoFeedbackInfor>>();
            var criteria = fixture.Create<CreateMutipleFeedbackCommand>();
            var service = MockService();
            commonFeedback.CheckFeedbackValidParams(criteria).Returns(new ValueTuple<bool, string>(false, ""));

            var commandResult = new CommandResult { Code = 0 };
            baseService.ExecuteNonQueryAsync(criteria).Returns(commandResult);

            // Act
            await service.Public_CheckValidParamsAndInsertFeedbackAsync(
                problemsAccounts,
                criteria
                );

            // Assert
            await transactionService.EditTOControlFeedbackInfoAsync(Arg.Any<string>(), CreateFeedbackJobStatus.Fail, Arg.Any<string>());
        }

        [Fact]
        public async Task CheckValidParamsAndInsertFeedbackAsync_Exception_ExpectedWriteLogAndUpdateStatus()
        {
            // Arrange
            var problemsAccounts = fixture.Create<IEnumerable<ProblemsAccountAutoFeedbackInfor>>();
            var exception = fixture.Create<Exception>();
            var criteria = fixture.Create<CreateMutipleFeedbackCommand>();
            var service = MockService();
            commonFeedback.CheckFeedbackValidParams(criteria).Returns(new ValueTuple<bool, string>(true, ""));

            baseService.When(x => x.ExecuteNonQueryAsync(criteria)).Throw(exception);

            // Act
            await service.Public_CheckValidParamsAndInsertFeedbackAsync(
                problemsAccounts,
                criteria
                );

            // Assert
            await transactionService.EditProblemsAccountFeedbackInfoAsync(Arg.Any<string>(), CreateFeedbackJobStatus.Fail, exception.Message);
            await logService.ErrorAsync(exception);
        }

        #endregion CheckValidParamsAndInsertFeedbackAsync

        #region GetProductXml

        [Fact]
        public void GetProductXml_WhenCalled_ExpectedReturnExactString()
        {
            // Arrange
            var service = MockService();
            var settings = fixture.Create<IEnumerable<SettingItem>>();
            var product = new SettingItem
            {
                SettingId = 1
            };
            service.Protected("GetProblemsSportsbookProduct", settings).Returns(product);
            var productXml = "<Product ProductID='1' SubProductID='8063' BetTypeID='9144' />";
            var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);
            stringBuilder.Append(productXml);
            stringBuilder.Append(BackgroundJobConstants.EndRootTag);
            // Act
            var actual = service.Public_GetProductXml(settings);

            // Assert
            Assert.Equal(stringBuilder.ToString(), actual);
        }

        #endregion GetProductXml

        #region GetRelatedCustomerXml

        [Fact]
        public void GetRelatedCustomerXml_WhenCalled_ExpectedReturnExactString()
        {
            // Arrange
            var service = MockService();
            var problemsAccounts = new List<ProblemsAccountAutoFeedbackInfor>
            {
                new ProblemsAccountAutoFeedbackInfor
                {
                    LogID = 1,
                    CustID = 1,
                    UserName = "test",
                    HeadlineID = 1,
                    HeadlineName = "HeadlineName",
                    IsVip = true,
                    RelatedCustID = 1,
                    CategoryID = 1
                }
            };
            var problemsAccount = problemsAccounts[0];

            const string XmlTemplate = DefaultValue.RelatedCustomerXml;
            var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);
            stringBuilder.AppendFormat(XmlTemplate, problemsAccount.HeadlineID, problemsAccount.RelatedCustID, problemsAccount.UserName);
            stringBuilder.Append(BackgroundJobConstants.EndRootTag);
            // Act
            var actual = service.Public_GetRelatedCustomerXml(problemsAccounts);

            // Assert
            Assert.Equal(stringBuilder.ToString(), actual);
        }

        #endregion GetRelatedCustomerXml

        #region GetCustomerXml

        [Fact]
        public void GetCustomerXml_WhenCalled_ExpectedReturnExactString()
        {
            // Arrange
            var service = MockService();
            var problemsAccounts = new List<ProblemsAccountAutoFeedbackInfor>
            {
                new ProblemsAccountAutoFeedbackInfor
                {
                    LogID = 1,
                    CustID = 1,
                    UserName = "test",
                    HeadlineID = 1,
                    HeadlineName = "HeadlineName",
                    IsVip = true,
                    RelatedCustID = 1,
                    CategoryID = 1
                }
            };
            var problemsAccount = problemsAccounts[0];

            const string XmlTemplate = DefaultValue.CustomerXml;
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat(XmlTemplate, problemsAccount.HeadlineID, problemsAccount.HeadlineName);
            // Act
            var actual = service.Public_GetCustomerXml(problemsAccounts);

            // Assert
            Assert.Equal(stringBuilder.ToString(), actual);
        }

        #endregion GetCustomerXml

        #region GetProblemsSportsbookProduct

        [Fact]
        public void GetProblemsSportsbookProduct_NotExistSportbook_ReturnDefaultProduct()
        {
            // Arrange
            var setting = new List<SettingItem>
                {
                    new SettingItem { SettingName = "Product", SettingValue = "sportbook1" }
                };

            var service = MockService();

            // Act
            var actual = service.Public_GetProblemsSportsbookProduct(setting);

            // Assert
            Assert.Equal(BackgroundJobConstants.ProductNA, actual.SettingId);
        }

        [Fact]
        public void GetProblemsSportsbookProduct_ExistSportbook_ReturnDefaultProduct()
        {
            // Arrange
            var settings = new List<SettingItem>
                {
                    new SettingItem
                    {
                        SettingName = "Product",
                        SettingValue = "Sportsbook",
                        SettingId = 1
                    }
                };

            var service = MockService();

            // Act
            var actual = service.Public_GetProblemsSportsbookProduct(settings);

            // Assert
            Assert.Equal(1, actual.SettingId);
        }

        #endregion GetProblemsSportsbookProduct

        #region GetDescription

        [Fact]
        public void GetDescription_WhenCalled_ExpectedReturnExactString()
        {
            // Arrange
            var service = MockService();
            var stringBuilder = new StringBuilder(DefaultValue.autoGenerated);
            stringBuilder.Append(BackgroundJobConstants.Break);
            var expectedString = "Announcement sent to Headline and informed our related department marked their member as problem account.";
            stringBuilder.Append(expectedString);
            stringBuilder.Append(BackgroundJobConstants.LineBreak);

            // Act
            var actual = service.Public_GetDescription();

            // Assert
            Assert.Equal(stringBuilder.ToString(), actual);
        }

        #endregion GetDescription

        #region GetSolution

        [Theory]
        [InlineData(true, "SolutionYN")]
        [InlineData(false, "SolutionSC")]
        public void GetSolution_WhenCalled_ExpectedReturnExactString(bool isVip, string expected)
        {
            // Arrange
            var service = MockService();
            var problemsAccounts = new List<ProblemsAccountAutoFeedbackInfor>
            {
                new ProblemsAccountAutoFeedbackInfor
                {
                    LogID = 1,
                    CustID = 1,
                    UserName = "1",
                    HeadlineID = 1,
                    HeadlineName = "1",
                    IsVip = isVip,
                    RelatedCustID = 1,
                    CategoryID = 1
                }
            };
            var solution = new SolutionTemplate
            {
                SolutionENSC = "SolutionSC",
                SolutionENYN = "SolutionYN"
            };
            commonFeedback.GetFeedbackTemplates<SolutionTemplate>(Arg.Any<string>()).Returns(solution);
            commonFeedback.GetCategogyName(Arg.Any<string>(), Language.en_US).Returns("CategogyEN");
            commonFeedback.GetCategogyName(Arg.Any<string>(), Language.vi_VN).Returns("CategogyYN");

            // Act
            var actual = service.Public_GetSolution(problemsAccounts);

            // Assert
            Assert.Equal(expected, actual);
        }

        #endregion GetSolution

        #region Helpers

        private ProblemsAccountFeedbackServiceTestable MockService()
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

            return Substitute.For<ProblemsAccountFeedbackServiceTestable>(
                appJobSettings,
                baseService,
                settingService,
                customerService,
                logService,
                transactionService,
                commonFeedback
                );
        }

        #endregion Helpers
    }
}