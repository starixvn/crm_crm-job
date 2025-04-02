namespace CRM.UnitTests.Services.AutoFeedback;

using AutoFixture;
using CRM.Constants;
using CRM.Criteria.AutoFeedback;
using CRM.Models.Common;
using CRM.Models.TurnoverControl;
using CRM.Models.WinLossLimit;
using CRM.Services.AutoFeedback;
using CRM.Services.Common;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testable;
using Xunit;

public class TOControlFeedbackServiceTests
{
    private AppJobSettings appSettings;
    private IBaseService baseService;
    private ISettingService settingService;
    private ISystemSettingService systemSettingService;
    private ICustomerService customerService;
    private ICrmLogService logService;
    private IFeedbackTransactionService transactionService;
    private ICommonFeedback commonFeedback;
    private readonly IFixture fixture = new Fixture();

    #region CreateFeedbackForTurnoverControlAsync

    [Fact]
    public async Task CreateFeedbackForTurnoverControlAsync_NotExistedTOFeedbacksInfo_ExpectedUpdateSystemSetting()
    {
        // Arrange
        var service = MockService();
        var turnOverSetting = fixture.Create<WinLossSystemSetting>();
        var custNotifications = fixture.Create<IEnumerable<TONotification>>();
        var custOutStandings = fixture.Create<IEnumerable<CustOutStanding>>();
        customerService.GetListSMANotificationAsync(turnOverSetting).Returns(custNotifications);
        customerService.GetListCustOutStandingAsync(custNotifications).Returns(custOutStandings);

        transactionService.ImportTOControlFeedbackInfoAsync(
                Arg.Any<IEnumerable<GroupHeadline>>(),
                Arg.Any<IEnumerable<CustOutStanding>>(),
                Arg.Any<IEnumerable<TONotification>>())
            .Returns(new List<TOControlFeedbackInfo>());
        // Act
        await service.CreateFeedbackForTurnoverControlAsync(turnOverSetting);

        // Assert
        await commonFeedback.Received().EditFeedbackSystemSettingAsync(Arg.Any<long>(), Arg.Any<string>());
        service.DidNotReceive().Protected(
            "CreateTOControlFeedbackAsync",
            Arg.Any<TOControlFeedbackInfo>(),
            Arg.Any<IEnumerable<SettingItem>>(),
            Arg.Any<IEnumerable<SystemSettingMapping>>());
    }

    [Fact]
    public async Task CreateFeedbackForTurnoverControlAsync_ExistedTOFeedbacksInfo_ExpectedEditTOSystemSettingAsyncAndCallCreateTOControlFeedbackAsync()
    {
        // Arrange
        var service = MockService();
        var turnOverSetting = fixture.Create<WinLossSystemSetting>();
        var custNotifications = fixture.Create<IEnumerable<TONotification>>();
        var custOutStandings = fixture.Create<IEnumerable<CustOutStanding>>();
        var importTOFeedbacks = fixture.Create<IEnumerable<TOControlFeedbackInfo>>();
        customerService.GetListSMANotificationAsync(turnOverSetting).Returns(custNotifications);
        customerService.GetListCustOutStandingAsync(custNotifications).Returns(custOutStandings);

        transactionService.ImportTOControlFeedbackInfoAsync(
                Arg.Any<IEnumerable<GroupHeadline>>(),
                Arg.Any<IEnumerable<CustOutStanding>>(),
                Arg.Any<IEnumerable<TONotification>>())
            .Returns(importTOFeedbacks);
        // Act
        await service.CreateFeedbackForTurnoverControlAsync(turnOverSetting);

        // Assert
        await commonFeedback.Received().EditFeedbackSystemSettingAsync(Arg.Any<long>(), Arg.Any<string>());
        service.Received().Protected(
            "CreateTOControlFeedbackAsync",
            Arg.Any<TOControlFeedbackInfo>(),
            Arg.Any<IEnumerable<SettingItem>>(),
            Arg.Any<IEnumerable<SystemSettingMapping>>());
    }

    #endregion CreateFeedbackForTurnoverControlAsync

    #region CreateTOControlFeedbackAsync

    [Fact]
    public async Task CreateTOControlFeedbackAsync_CreateNewCriteria_ExpectedToCallFunction()
    {
        // Arrange
        var ToInfo = new TOControlFeedbackInfo
        {
            SportType = 2,
            BettypeList = "1,20",
            LogID = 1,
            HeadlineID = 1,
            HeadlineName = "1",
            ProfileID = 1,
            ProfileName = "1",
            CustID = 1
        };

        var settings = new List<SettingItem>
        {
            new() { SettingId = 1, SettingName = "Status", SettingValue = "Closed" },
            new() { SettingId = 2, SettingName = "Response", SettingValue = "Neutral" },
            new() { SettingId = 2022, SettingName = "Station" }
        };
        var setting = new SettingItem
        {
            SettingId = 1
        };

        var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();
        var category = fixture.Create<Tuple<int, int, string>>();
        var service = MockService();
        commonFeedback.GetCategorySubCategoryCWL(settings, DefaultValue.MANotification).Returns(category);
        service.Protected("GetTOSportsbookProduct", settings).Returns(setting);
        service.Protected("GetTOProductXml", Arg.Any<string>(), setting.SettingId, Arg.Any<List<string>>(), settings, settingMappings).Returns("product xml");
        service.Protected("GetTOSolution", ToInfo).Returns("solution");
        service.Protected("GetTORelatedCustomerXml", ToInfo).Returns("related customer xml");
        service.Protected("GetTODescription", ToInfo).Returns("description");

        // Act
        await service.Public_CreateTOControlFeedbackAsync(
            ToInfo,
            settings,
            settingMappings
        );

        // Assert
        service.Received().Protected("CheckValidTOParamsAndInsertFeedbackAsync", Arg.Any<TOControlFeedbackInfo>()
            , Arg.Any<CreateMultipleFeedbackCommand>());
    }

    #endregion CreateTOControlFeedbackAsync

    #region CheckValidTOParamsAndInsertFeedbackAsync

    [Fact]
    public async Task CheckValidTOParamsAndInsertFeedbackAsync_CreateSuccessful_ExpectedWriteLog()
    {
        // Arrange
        var ToInfo = new TOControlFeedbackInfo
        {
            LogID = 1,
            HeadlineID = 1,
            HeadlineName = "1",
            ProfileID = 1,
            ProfileName = "1",
            CustID = 1
        };
        var criteria = fixture.Create<CreateMultipleFeedbackCommand>();
        var service = MockService();
        commonFeedback.CheckFeedbackValidParams(criteria).Returns(new ValueTuple<bool, string>(true, ""));

        var commandResult = new CRM.Models.Common.CommandResult { Code = 0 };
        baseService.ExecuteNonQueryAsync(criteria).Returns(commandResult);

        // Act
        await service.Public_CheckValidTOParamsAndInsertFeedbackAsync(
            ToInfo,
            criteria
        );

        // Assert
        logService.Received(1).LogInformation(Arg.Any<string>());
    }

    [Fact]
    public async Task CheckValidTOParamsAndInsertFeedbackAsync_CreateUnSuccessful_ExpectedWriteLogAndUpdateStatus()
    {
        // Arrange
        var ToInfo = new TOControlFeedbackInfo
        {
            LogID = 1,
            HeadlineID = 1,
            HeadlineName = "1",
            ProfileID = 1,
            ProfileName = "1",
            CustID = 1
        };
        var criteria = fixture.Create<CreateMultipleFeedbackCommand>();
        var service = MockService();
        commonFeedback.CheckFeedbackValidParams(criteria).Returns(new ValueTuple<bool, string>(true, ""));

        var commandResult = new CRM.Models.Common.CommandResult { Code = 1, Message = "" };
        baseService.ExecuteNonQueryAsync(criteria).Returns(commandResult);

        // Act
        await service.Public_CheckValidTOParamsAndInsertFeedbackAsync(
            ToInfo,
            criteria
        );

        // Assert
        logService.Received(1).LogInformation(Arg.Any<string>());
        await transactionService.EditTOControlFeedbackInfoAsync("1", CreateFeedbackJobStatus.Fail, commandResult.Message);
        await logService.ErrorAsync(commandResult, criteria);
    }

    [Fact]
    public async Task CheckValidTOParamsAndInsertFeedbackAsync_InvalidParams_ExpectedReturnAndUpdateStatus()
    {
        // Arrange
        var ToInfo = new TOControlFeedbackInfo
        {
            LogID = 1,
            HeadlineID = 1,
            HeadlineName = "1",
            ProfileID = 1,
            ProfileName = "1",
            CustID = 1
        };
        var criteria = fixture.Create<CreateMultipleFeedbackCommand>();
        var service = MockService();
        commonFeedback.CheckFeedbackValidParams(criteria).Returns(new ValueTuple<bool, string>(false, ""));

        var commandResult = new CRM.Models.Common.CommandResult { Code = 0 };
        baseService.ExecuteNonQueryAsync(criteria).Returns(commandResult);

        // Act
        await service.Public_CheckValidTOParamsAndInsertFeedbackAsync(
            ToInfo,
            criteria
        );

        // Assert
        logService.Received(1).LogInformation(Arg.Any<string>());
        await transactionService.EditTOControlFeedbackInfoAsync("1", CreateFeedbackJobStatus.Fail, Arg.Any<string>());
    }

    [Fact]
    public async Task CheckValidTOParamsAndInsertFeedbackAsync_Exception_ExpectedWriteLogAndUpdateStatus()
    {
        // Arrange
        var ToInfo = new TOControlFeedbackInfo
        {
            LogID = 1,
            HeadlineID = 1,
            HeadlineName = "1",
            ProfileID = 1,
            ProfileName = "1",
            CustID = 1
        };
        var exception = fixture.Create<Exception>();
        var criteria = fixture.Create<CreateMultipleFeedbackCommand>();
        var service = MockService();
        commonFeedback.CheckFeedbackValidParams(criteria).Returns(new ValueTuple<bool, string>(true, ""));

        baseService.When(x => x.ExecuteNonQueryAsync(criteria)).Throw(exception);

        // Act
        await service.Public_CheckValidTOParamsAndInsertFeedbackAsync(
            ToInfo,
            criteria
        );

        // Assert
        await transactionService.EditTOControlFeedbackInfoAsync("1", CreateFeedbackJobStatus.Fail, exception.Message);
        await logService.ErrorAsync(exception);
    }

    #endregion CheckValidTOParamsAndInsertFeedbackAsync

    #region GetTOProductXml

    [Fact]
    public void GetProductXml_NotExistedBetType_ReturnExpectedXml()
    {
        // Arrange
        var betTypeMappings = fixture.Create<List<string>>();
        var sportName = fixture.Create<string>();
        var settings = fixture.Create<IEnumerable<SettingItem>>();
        var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();
        var subProduct = new List<SettingItem>
        {
            new() { SettingId = 1 }
        };
        var product = new SettingItem
        {
            SettingId = 1,
            SettingName = "test"
        };
        var tempBetTypes = new List<SystemSettingMapping>();
        var service = MockService();
        service.Protected("GetTOSportsbookProduct", settings).Returns(product);
        service.Protected("GetTOSubProduct", product.SettingId, sportName, settings, settingMappings).Returns(subProduct);
        service.Protected("GetTOBetType", betTypeMappings, settingMappings).Returns(tempBetTypes);

        // Act
        var actual = service.Public_GetTOProductXml(sportName, product.SettingId, betTypeMappings, settings, settingMappings);

        // Assert
        Assert.Equal("<ROOT><Product ProductID='1' SubProductID='1' BetTypeID='9144' /></ROOT>", actual);
    }

    [Fact]
    public void GetProductXml_ExistedBetType_ReturnExpectedXml()
    {
        // Arrange
        var betTypeMappings = fixture.Create<List<string>>();
        var sportName = fixture.Create<string>();
        var settings = fixture.Create<IEnumerable<SettingItem>>();
        var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();
        var subProduct = new List<SettingItem>
        {
            new() { SettingId = 1 }
        };
        var product = new SettingItem
        {
            SettingId = 1,
            SettingName = "test"
        };
        var tempBetTypes = new List<SystemSettingMapping>
        {
            new() { SettingId = 1 }
        };
        var service = MockService();
        service.Protected("GetTOSportsbookProduct", settings).Returns(product);
        service.Protected("GetTOSubProduct", product.SettingId, sportName, settings, settingMappings).Returns(subProduct);
        service.Protected("GetTOBetType", betTypeMappings, settingMappings).Returns(tempBetTypes);

        // Act
        var actual = service.Public_GetTOProductXml(sportName, product.SettingId, betTypeMappings, settings, settingMappings);

        // Assert
        Assert.Equal("<ROOT><Product ProductID='1' SubProductID='1' BetTypeID='1' /></ROOT>", actual);
    }

    #endregion GetTOProductXml

    #region GetTOSportsbookProduct

    [Fact]
    public void GetTOSportsbookProduct_NotExistSportsbook_ReturnDefaultProduct()
    {
        // Arrange
        var setting = new List<SettingItem>
        {
            new() { SettingName = "Product", SettingValue = "sportsbook1" }
        };

        var service = MockService();

        // Act
        var actual = service.Public_GetTOSportsbookProduct(setting);

        // Assert
        Assert.Equal(BackgroundJobConstants.ProductNA, actual.SettingId);
    }

    [Fact]
    public void GetTOSportsbookProduct_ExistSportsbook_ReturnDefaultProduct()
    {
        // Arrange
        var settings = new List<SettingItem>
        {
            new()
            {
                SettingName = "Product",
                SettingValue = "Sportsbook",
                SettingId = 1
            }
        };

        var service = MockService();

        // Act
        var actual = service.Public_GetTOSportsbookProduct(settings);

        // Assert
        Assert.Equal(1, actual.SettingId);
    }

    #endregion GetTOSportsbookProduct

    #region GetTOSubProduct

    [Fact]
    public void GetTOSubProduct_NotExistedSportNames_ReturnDefaultSubProduct()
    {
        // Arrange
        var sportName = fixture.Create<string>();
        var settings = fixture.Create<IEnumerable<SettingItem>>();
        var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();

        var service = MockService();

        // Act
        var actual = service.Public_GetTOSubProduct(1, sportName, settings, settingMappings);

        // Assert
        Assert.Single(actual);
        Assert.Equal(BackgroundJobConstants.SubProductNA, actual.First().SettingId);
    }

    [Fact]
    public void GetTOSubProduct_ExistedSportNames_ReturnExpectedSubProduct()
    {
        // Arrange
        var sportName = "1";
        var settings = new List<SettingItem>
        {
            new()
            {
                SettingName = "SubProduct",
                ParentIds = new List<int>{1} ,
                SettingId = 1
            }
        };
        var settingMappings = new List<SystemSettingMapping>
        {
            new()
            {
                SettingName = "SubProduct",
                SettingId = 1,
                SettingSource = "1"
            }
        };

        var service = MockService();

        // Act
        var actual = service.Public_GetTOSubProduct(1, sportName, settings, settingMappings);

        // Assert
        Assert.Single(actual);
        Assert.Equal(1, actual.First().SettingId);
    }

    #endregion GetTOSubProduct

    #region GetTOBettype

    [Fact]
    public void GetTOBetType_NotExistedBetTypeMappings_ReturnDefaultBetType()
    {
        // Arrange
        var betTypeMappings = fixture.Create<List<string>>();
        var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();

        var service = MockService();

        // Act
        var actual = service.Public_GetTOBetType(betTypeMappings, settingMappings);

        // Assert
        Assert.Single(actual);
        Assert.Equal(BackgroundJobConstants.BettypeNA, actual.First().SettingId);
    }

    [Fact]
    public void GetTOBetType_ExistedBetTypeMappings_ReturnExpectedBetType()
    {
        // Arrange
        var betTypeMappings = new List<string>
        {
            "1"
        };
        var settingMappings = new List<SystemSettingMapping>
        {
            new()
            {
                SettingName = "BetType",
                SettingSource = "1",
                SettingId = 1
            }
        };
        var service = MockService();

        // Act
        var actual = service.Public_GetTOBetType(betTypeMappings, settingMappings);

        // Assert
        Assert.Single(actual);
        Assert.Equal(1, actual.First().SettingId);
    }

    #endregion GetTOBettype

    #region GetTORelatedCustomerXml

    [Fact]
    public void GetTORelatedCustomerXml_CallFunction_ReturnExpectTemplate()
    {
        // Arrange
        var toInfo = new TOControlFeedbackInfo
        {
            HeadlineID = 1,
            RelatedCustID = 1,
            UserName = "test"
        };

        var service = MockService();

        // Act
        var actual = service.Public_GetTORelatedCustomerXml(toInfo);

        // Assert
        Assert.Equal("<ROOT><Row CustId='1' RelatedCustId='1' Username='test' /></ROOT>", actual);
    }

    #endregion GetTORelatedCustomerXml

    #region GetTODescription

    [Theory]
    [InlineData(4)]
    [InlineData(3)]
    [InlineData(2)]
    public void GetTODescription_BaseOnSmaLevel_ReturnExpectedTemplate(byte RoleID)
    {
        // Arrange
        var toInfo = new TOControlFeedbackInfo
        {
            RoleID = RoleID,
            ProfileID = 1,
            SportType = 1,
            LeagueID = 1,
            HomeID = 1,
            AwayID = 1,
            Level = 1,
            Currency = "usa",
            TotalTurnover = 1,
            ProfileName = "1",
            HouseTurnover = 1,
            Usage = 1,
            Outstanding = 1,
            UserName = "test"
        };
        var service = MockService();

        commonFeedback.GetSportName(toInfo.SportType.ToString(), "en-US").Returns("sportName");
        commonFeedback.GetLeagueName(toInfo.LeagueID.ToString(), "en-US").Returns("leagueName");
        commonFeedback.GetTeamName(toInfo.HomeID.ToString(), "en-US").Returns("homeName");
        commonFeedback.GetTeamName(toInfo.AwayID.ToString(), "en-US").Returns("awayName");
        var desc = new TODescriptionTemplate
        {
            Desc = "test"
        };
        commonFeedback.GetFeedbackTemplates<TODescriptionTemplate>(Arg.Any<string>()).Returns(desc);
        var stringBuilder = new StringBuilder(DefaultValue.autoGenerated);
        stringBuilder.Append(BackgroundJobConstants.Break);
        stringBuilder.Append("test");
        stringBuilder.Append(BackgroundJobConstants.Break);
        var expected = stringBuilder.ToString();

        // Act
        var actual = service.Public_GetTODescription(toInfo);

        // Assert
        Assert.Equal(expected, actual);
    }

    #endregion GetTODescription

    #region GetTOSolution

    [Theory]
    [InlineData(4, SMALevel.Super, "789y", false, false, SMALevel.SuperSC)]
    [InlineData(3, SMALevel.Master, "123", true, false, SMALevel.MasterSC)]
    [InlineData(2, SMALevel.Agent, "789y", true, true, SMALevel.AgentSC)]
    public void GetTOSolution_BaseOnSmaLevelAndIsVipCustomer_ReturnExpectedTemplate(
        byte RoleID,
        string smsLevel,
        string Site,
        bool IsVip,
        bool IsVipCust,
        string smsLevelSc)
    {
        // Arrange
        var ToInfo = new TOControlFeedbackInfo
        {
            RoleID = RoleID,
            ProfileID = 1,
            SportType = 1,
            LeagueID = 1,
            HomeID = 1,
            AwayID = 1,
            Level = 1,
            Currency = "usa",
            TotalTurnover = 1,
            ProfileName = "1",
            HouseTurnover = 1,
            Usage = 1,
            Outstanding = 1,
            UserName = "test",
            SiteLabel = Site,
            IsVip = IsVip,
            MatchID = 1
        };
        var service = MockService();
        var sol = new TOSolutionTemplate
        {
            SolutionEN = "test",
            SolutionSC = "test",
            SolutionYN = "test"
        };
        var stringBuilder = new StringBuilder();

        commonFeedback.GetFeedbackTemplates<TOSolutionTemplate>(Arg.Any<string>()).Returns(sol);
        var templateENReturn = new StringBuilder("test1");
        var templateSCReturn = new StringBuilder("test2");

        service.Protected("GetTOSolutionTemplateEN", ToInfo, sol, smsLevel).Returns(templateENReturn);
        service.Protected("GetTOSolutionTemplateYNSC", ToInfo, IsVipCust, smsLevel, smsLevelSc, sol).Returns(templateSCReturn);
        var expected = stringBuilder.Append(templateENReturn).Append(BackgroundJobConstants.Break).Append(templateSCReturn).Append(BackgroundJobConstants.Break).ToString();

        // Act
        var actual = service.Public_GetTOSolution(ToInfo);

        // Assert
        Assert.Equal(expected, actual);
    }

    #endregion GetTOSolution

    [Fact]
    public void GetTOSolutionTemplateEN_CallFunction_ReturnExpectedTemplate()
    {
        // Arrange
        var turnoverInfo = new TOControlFeedbackInfo
        {
            ProfileID = 1,
            SportType = 1,
            LeagueID = 1,
            HomeID = 1,
            AwayID = 1,
            Level = 1,
            Currency = "usa",
            TotalTurnover = 1,
            ProfileName = "1",
            HouseTurnover = 1,
            Usage = 1,
            Outstanding = 1
        };
        var service = MockService();

        commonFeedback.GetSportName(turnoverInfo.SportType.ToString(), "en-US").Returns("sportName");
        commonFeedback.GetLeagueName(turnoverInfo.LeagueID.ToString(), "en-US").Returns("leagueName");
        commonFeedback.GetTeamName(turnoverInfo.HomeID.ToString(), "en-US").Returns("homeName");
        commonFeedback.GetTeamName(turnoverInfo.AwayID.ToString(), "en-US").Returns("awayName");
        var sol = new TOSolutionTemplate
        {
            SolutionEN = "test",
            SolutionSC = "test",
            SolutionYN = "test"
        };
        var stringBuilder = new StringBuilder("test");
        var expected = stringBuilder.ToString();

        // Act
        var actual = service.Public_GetTOSolutionTemplateEN(turnoverInfo, sol, SMALevel.Master);

        // Assert
        Assert.Equal(expected, actual.ToString());
    }

    [Theory]
    [InlineData(true, Language.vi_VN)]
    [InlineData(false, Language.zh_CN)]
    public void GetTOSolutionTemplateYNSC_CallFunction_ReturnExpectedTemplate(bool isVipCust, string language)
    {
        // Arrange
        var toInfo = new TOControlFeedbackInfo
        {
            ProfileID = 1,
            SportType = 1,
            LeagueID = 1,
            HomeID = 1,
            AwayID = 1,
            Level = 1,
            Currency = "usa",
            TotalTurnover = 1,
            ProfileName = "1",
            HouseTurnover = 1,
            Usage = 1,
            Outstanding = 1
        };
        var service = MockService();

        commonFeedback.GetSportName(toInfo.SportType.ToString(), language).Returns("sportName");
        commonFeedback.GetLeagueName(toInfo.LeagueID.ToString(), language).Returns("leagueName");
        commonFeedback.GetTeamName(toInfo.HomeID.ToString(), language).Returns("homeName");
        commonFeedback.GetTeamName(toInfo.AwayID.ToString(), language).Returns("awayName");
        var sol = new TOSolutionTemplate
        {
            SolutionEN = "test",
            SolutionSC = "test",
            SolutionYN = "test"
        };
        var stringBuilder = new StringBuilder("test");
        var expected = stringBuilder.ToString();

        // Act
        var actual = service.Public_GetTOSolutionTemplateYNSC(toInfo, isVipCust, SMALevel.Master, SMALevel.MasterSC, sol);

        // Assert
        Assert.Equal(expected, actual.ToString());
    }

    #region Helpers

    private TOControlFeedbackServiceTestable MockService()
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
        systemSettingService = Substitute.For<ISystemSettingService>();
        customerService = Substitute.For<ICustomerService>();
        logService = Substitute.For<ICrmLogService>();
        transactionService = Substitute.For<IFeedbackTransactionService>();
        commonFeedback = Substitute.For<ICommonFeedback>();

        return Substitute.For<TOControlFeedbackServiceTestable>(
            appJobSettings,
            baseService,
            settingService,
            systemSettingService,
            customerService,
            logService,
            transactionService,
            commonFeedback
        );
    }

    #endregion Helpers
}