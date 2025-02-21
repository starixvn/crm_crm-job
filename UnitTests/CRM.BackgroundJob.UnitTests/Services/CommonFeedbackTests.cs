namespace CRM.BackgroundJob.UnitTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CRM.BackgroundJob.Constants;
    using CRM.BackgroundJob.Criteria.AutoFeedback;
    using CRM.BackgroundJob.Models;
    using CRM.BackgroundJob.Services;
    using Fanex.RemotingRef;
    using Microsoft.AspNetCore.Hosting;
    using NSubstitute;
    using Xunit;

    public class CommonFeedbackTests
    {
        private IRemotingRefService remotingRefService;
        private ISystemSettingService systemSetting;
        private IWebHostEnvironment hostingEnvironment;

        #region CheckFeedbackValidParams

        [Fact]
        public void CheckFeedbackValidParams__ValidParamsCriteria_ExpectedReturnTrue()
        {
            // Arrange
            var criteria = new CreateMutipleFeedbackCommand
            {
                CategoryIds = "1",
                CategoryXml = "1",
                ProductXml = "1",
                SubCategoryIds = "1",
                ResponseId = 1,
                StationId = "1",
                StatusId = 1,
            };
            var expect = new ValueTuple<bool, string>(true, "");
            var service = GetService();

            // Act
            var result = service.CheckFeedbackValidParams(criteria);

            // Assert
            Assert.Equal(expect, result);
        }

        [Fact]
        public void CheckFeedbackValidParams__InValidParamsResponseIdCriteria_ExpectedReturnFalseAndMessage()
        {
            // Arrange
            var criteria = new CreateMutipleFeedbackCommand
            {
                CategoryIds = "1",
                CategoryXml = "1",
                ProductXml = "1",
                SubCategoryIds = "1",
                ResponseId = short.Parse("0"),
                StationId = "1",
                StatusId = 1
            };
            var expect = new ValueTuple<bool, string>(false, "ResponseId is null.");
            var service = GetService();

            // Act
            var result = service.CheckFeedbackValidParams(criteria);

            // Assert
            Assert.Equal(expect, result);
        }

        [Fact]
        public void CheckFeedbackValidParams__InValidParamsStatusIdCriteria_ExpectedReturnFalseAndMessage()
        {
            // Arrange
            var criteria = new CreateMutipleFeedbackCommand
            {
                CategoryIds = "1",
                CategoryXml = "1",
                ProductXml = "1",
                SubCategoryIds = "1",
                ResponseId = 1,
                StationId = "1",
                StatusId = short.Parse("0")
            };
            var expect = new ValueTuple<bool, string>(false, "StatusId is null.");
            var service = GetService();

            // Act
            var result = service.CheckFeedbackValidParams(criteria);

            // Assert
            Assert.Equal(expect, result);
        }

        [Fact]
        public void CheckFeedbackValidParams__InValidParamsCategoryIdsCriteria_ExpectedReturnFalseAndMessage()
        {
            // Arrange
            var criteria = new CreateMutipleFeedbackCommand
            {
                CategoryIds = "",
                CategoryXml = "1",
                ProductXml = "1",
                SubCategoryIds = "1",
                ResponseId = 1,
                StationId = "1",
                StatusId = 1
            };
            var expect = new ValueTuple<bool, string>(false, "CategoryIds is null.");
            var service = GetService();

            // Act
            var result = service.CheckFeedbackValidParams(criteria);

            // Assert
            Assert.Equal(expect, result);
        }

        [Fact]
        public void CheckFeedbackValidParams__InValidParamsCategoryXmlCriteria_ExpectedReturnFalseAndMessage()
        {
            // Arrange
            var criteria = new CreateMutipleFeedbackCommand
            {
                CategoryIds = "1",
                CategoryXml = "",
                ProductXml = "1",
                SubCategoryIds = "1",
                ResponseId = 1,
                StationId = "1",
                StatusId = 1
            };
            var expect = new ValueTuple<bool, string>(false, "CategoryXml is null.");
            var service = GetService();

            // Act
            var result = service.CheckFeedbackValidParams(criteria);

            // Assert
            Assert.Equal(expect, result);
        }

        [Fact]
        public void CheckFeedbackValidParams__InValidParamsProductXmlCriteria_ExpectedReturnFalseAndMessage()
        {
            // Arrange
            var criteria = new CreateMutipleFeedbackCommand
            {
                CategoryIds = "1",
                CategoryXml = "1",
                ProductXml = "",
                SubCategoryIds = "1",
                ResponseId = 1,
                StationId = "1",
                StatusId = 1
            };
            var expect = new ValueTuple<bool, string>(false, "Products is null.");
            var service = GetService();

            // Act
            var result = service.CheckFeedbackValidParams(criteria);

            // Assert
            Assert.Equal(expect, result);
        }

        [Fact]
        public void CheckFeedbackValidParams__InValidParamsSubCategoryIdsCriteria_ExpectedReturnFalseAndMessage()
        {
            // Arrange
            var criteria = new CreateMutipleFeedbackCommand
            {
                CategoryIds = "1",
                CategoryXml = "1",
                ProductXml = "1",
                SubCategoryIds = "",
                ResponseId = 1,
                StationId = "1",
                StatusId = 1
            };
            var expect = new ValueTuple<bool, string>(false, "SubCategoryIds is null.");
            var service = GetService();

            // Act
            var result = service.CheckFeedbackValidParams(criteria);

            // Assert
            Assert.Equal(expect, result);
        }

        [Fact]
        public void CheckFeedbackValidParams__InValidParamsStationIdCriteria_ExpectedReturnFalseAndMessage()
        {
            // Arrange
            var criteria = new CreateMutipleFeedbackCommand
            {
                CategoryIds = "1",
                CategoryXml = "1",
                ProductXml = "1",
                SubCategoryIds = "1",
                ResponseId = 1,
                StationId = "",
                StatusId = 1
            };
            var expect = new ValueTuple<bool, string>(false, "StationId is null.");
            var service = GetService();

            // Act
            var result = service.CheckFeedbackValidParams(criteria);

            // Assert
            Assert.Equal(expect, result);
        }

        #endregion CheckFeedbackValidParams

        #region GetCategorySubCategoryCWL

        [Fact]
        public void GetCategorySubCategoryCWL_NotExistMainCategory_ReturnDefaultMainCategoryXML()
        {
            // Arrange
            var settings = new List<SettingItem>
                {
                    new SettingItem { SettingName = "MainCategory", SettingValue = "Special" }
                };
            var service = GetService();

            // Act
            var actual = service.GetCategorySubCategoryCWL(settings, DefaultValue.MANotification);

            // Assert
            Assert.Equal(0, actual.Item1);
            Assert.Equal(0, actual.Item2);
            Assert.Equal("<ROOT><Category MainCategoryID='0' SubCategoryID='0' /></ROOT>", actual.Item3);
        }

        [Fact]
        public void GetCategorySubCategoryCWL_ExistMainCategoryAndNotExistSubCategory_ReturnExpectedTuple()
        {
            // Arrange
            var settings = new List<SettingItem>
                {
                    new SettingItem { SettingId = 1, SettingName = "MainCategory", SettingValue = DefaultValue.Special_Notification },
                    new SettingItem { SettingName = "SubCategory", SettingValue = "Company" }
                };

            var service = GetService();

            // Act
            var actual = service.GetCategorySubCategoryCWL(settings, DefaultValue.MANotification);

            // Assert
            Assert.Equal(1, actual.Item1);
            Assert.Equal(0, actual.Item2);
            Assert.Equal("<ROOT><Category MainCategoryID='1' SubCategoryID='0' /></ROOT>", actual.Item3);
        }

        [Fact]
        public void GetCategorySubCategoryCWL_ExistMainCategoryAndExistSubCategory_ReturnExpectedTuple()
        {
            // Arrange
            var settings = new List<SettingItem>
                {
                    new SettingItem { SettingId = 1, SettingName = "MainCategory", SettingValue = DefaultValue.Special_Notification },
                    new SettingItem { SettingId = 2, SettingName = "SubCategory", SettingValue = DefaultValue.MANotification , ParentIds = new List<int>{ 1 }}
                };
            var service = GetService();

            // Act
            var actual = service.GetCategorySubCategoryCWL(settings, DefaultValue.MANotification);

            // Assert
            Assert.Equal(1, actual.Item1);
            Assert.Equal(2, actual.Item2);
            Assert.Equal("<ROOT><Category MainCategoryID='1' SubCategoryID='2' /></ROOT>", actual.Item3);
        }

        #endregion GetCategorySubCategoryCWL

        #region Get Remoting Ref

        [Fact]
        public void GetProductName_CallFunction_ReturnExpectedProductName()
        {
            // Arrange
            var expect = "Sportsbook";
            var service = GetService();
            remotingRefService.GetResourceDataByRefIdAndResourceId("Product", Arg.Any<string>(), Arg.Any<string>(), string.Empty).Returns("Sportsbook");

            // Act
            var actual = service.GetProductName("2", Language.en_US);

            // Assert
            Assert.Equal(expect, actual);
        }

        [Fact]
        public void GetSportName_CallFunction_ReturnExpectedSportName()
        {
            // Arrange
            var expect = "Soccer";
            var service = GetService();
            remotingRefService.GetSportName(Arg.Any<string>(), Arg.Any<string>(), string.Empty).Returns("Soccer");

            // Act
            var actual = service.GetSportName("2", Language.en_US);

            // Assert
            Assert.Equal(expect, actual);
        }

        [Fact]
        public void GetLeagueName_CallFunction_ReturnExpectedLeagueName()
        {
            // Arrange
            var expect = "LeagueA";
            var service = GetService();
            remotingRefService.GetLeagueName(Arg.Any<string>(), Arg.Any<string>(), string.Empty).Returns("LeagueA");

            // Act
            var actual = service.GetLeagueName("2", Language.en_US);

            // Assert
            Assert.Equal(expect, actual);
        }

        [Fact]
        public void GetTeamName_CallFunction_ReturnExpectedTeamName()
        {
            // Arrange
            var expect = "TeamA";
            var service = GetService();
            remotingRefService.GetTeamName(Arg.Any<string>(), Arg.Any<string>(), string.Empty).Returns("TeamA");

            // Act
            var actual = service.GetTeamName("2", Language.en_US);

            // Assert
            Assert.Equal(expect, actual);
        }

        [Fact]
        public void GetBetTypeName_CallFunction_ReturnExpectedBetTypeName()
        {
            // Arrange
            var expect = "BetType";
            var service = GetService();
            remotingRefService.GetBetTypeName(Arg.Any<string>(), Arg.Any<string>()).Returns("BetType");

            // Act
            var actual = service.GetBetTypeName("2", Language.en_US);

            // Assert
            Assert.Equal(expect, actual);
        }

        #endregion Get Remoting Ref

        [Fact]
        public async Task EditFeedbackSystemSettingAsync_CallFunction_ExpectCallExactFunction()
        {
            // Arrange
            var service = GetService();

            // Act
            await service.EditFeedbackSystemSettingAsync(1, BackgroundJobConstants.ServiceName_TOControlFB);

            // Assert
            await systemSetting.Received().EditSystemSettingAsync(Arg.Any<IEnumerable<SystemSettingItem>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task EditSystemSettingAsyncByEnv_WhenCalled_CallExactFunctionAndParams()
        {
            // Arrange
            var service = GetService();

            // Act
            await service.EditSystemSettingAsyncByEnv("JobStatusNameEnv", "LastRuntimeNameEnv", "JobStatusValue", BackgroundJobConstants.ServiceName_TOControlFB);

            // Assert
            await systemSetting.Received().EditSystemSettingAsync(Arg.Any<IEnumerable<SystemSettingItem>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task EditSystemSettingUpdateLastLogIdAsyncByEnv_WhenCalled_CallExactFunctionAndParams()
        {
            // Arrange
            var service = GetService();

            // Act
            await service.EditSystemSettingUpdateLastLogIdAsyncByEnv("JobStatusNameEnv", "LastRuntimeNameEnv", "lastLogIdName", 1, BackgroundJobConstants.ServiceName_TOControlFB);

            // Assert
            await systemSetting.Received().EditSystemSettingAsync(Arg.Any<IEnumerable<SystemSettingItem>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task EditSystemSettingUpdateLastSeqAndLastSubmitDateAsync_WhenCalled_CallExactFunctionAndParams()
        {
            // Arrange
            var service = GetService();

            // Act
            await service.EditSystemSettingUpdateLastSeqAndLastSubmitDateAsync("JobStatusNameEnv", "lastSeqEnvName", 1, "lastSubmitTimeEnvName", "maxLastSubmitTime", BackgroundJobConstants.ServiceName_AmendmentNotiFB);

            // Assert
            await systemSetting.Received().EditSystemSettingAsync(Arg.Any<IEnumerable<SystemSettingItem>>(), Arg.Any<string>());
        }

        #region GetMatchCodeFormat
        [Theory]
        [InlineData("", "")]
        [InlineData("{2} 23, {0} 6666", "match code 2 23, match code 0 6666")]
        public void GetMatchCoddFormat_WhenCalled_ReturnExpectString(string matchCode, string expected)
        {
            // Arrange
            var service = GetService();
            service.GetMatchCode("{2}", Language.en_US).Returns("match code 2");
            service.GetMatchCode("{0}", Language.en_US).Returns("match code 0");

            // Act
            var actual = service.GetMatchCodeFormat(matchCode, Language.en_US);

            // Assert
            Assert.Equal(expected, actual);
        }
        #endregion GetMatchCodeFormat

        #region Helpers
        private CommonFeedback GetService()
        {
            remotingRefService = Substitute.For<IRemotingRefService>();
            systemSetting = Substitute.For<ISystemSettingService>();
            hostingEnvironment = Substitute.For<IWebHostEnvironment>();
            return new CommonFeedback(
                remotingRefService,
                systemSetting,
                hostingEnvironment
            );
        }

        #endregion Helpers
    }
}