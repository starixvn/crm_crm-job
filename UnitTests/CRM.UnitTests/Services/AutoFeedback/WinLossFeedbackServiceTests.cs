namespace CRM.UnitTests.Services.AutoFeedback;

using AutoFixture;
using CRM.Constants;
using CRM.Criteria.AutoFeedback;
using CRM.Models.Common;
using CRM.Models.WinLossLimit;
using CRM.Services.AutoFeedback;
using CRM.Services.Common;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Testable;
using Xunit;

public class WinLossFeedbackServiceTests
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

    #region CreateFeedbackForWinLossLimitAsync

    [Fact]
    public async Task CreateFeedbackForWinLossLimitAsync_NotExistedWinlossInfo_ExpectedUpdateSystemSetting()
    {
        // Arrange
        var service = MockService();
        var setting = fixture.Create<WinLossSystemSetting>();
        var cusWinLimit = new List<CustomerWinLimit>
       {
           new() { LogID = 1, CustId = 1},
           new() { LogID = 2, CustId = 1}
       };
        var betInfo = fixture.Create<IEnumerable<BettypeInfo>>();
        var headlines = new List<GroupHeadline>
       {
           new() {CustId = 1, HeadlineId = 1, HeadlineName = "test"}
       };
        var environment = "LIVE";
        var sport = new List<SportInfo>
       {
           new() {LogID = 1, SportType = 1, SportName = "test"}
       };
        var winlossInfo = new ValueTuple<IEnumerable<WinLimitFeedBackInfo>, IEnumerable<BettypeInfo>, IEnumerable<SportInfo>>(new List<WinLimitFeedBackInfo>(), betInfo, sport);
        customerService.GetListCustReachLimitAsync(setting, Arg.Any<bool>()).Returns(cusWinLimit);
        customerService.GetGroupHeadlineAsync(Arg.Any<IEnumerable<int>>()).Returns(headlines);
        transactionService.ImportWinLimitFeedbackInfoAsync(Arg.Any<IEnumerable<CustomerWinLimit>>(), headlines, environment).Returns(winlossInfo);
        // Act
        await service.CreateFeedbackForWinLossLimitAsync(setting, environment);

        // Assert

        await systemSettingService.Received().EditSystemSettingAsync(Arg.Any<IEnumerable<SystemSettingItem>>(), Arg.Any<string>());
        service.DidNotReceive().Protected(
            "CreateWinLossFeedbackCreditAsync",
            Arg.Any<bool>(),
            Arg.Any<WinLimitFeedBackInfo>(),
            Arg.Any<IEnumerable<BettypeInfo>>(),
            Arg.Any<IEnumerable<SportInfo>>(),
            Arg.Any<IEnumerable<SettingItem>>(),
            Arg.Any<IEnumerable<SystemSettingMapping>>());
    }

    [Fact]
    public async Task CreateFeedbackForWinLossLimitAsync_ExistedWinlossInfo_ExpectedEditWinLossSystemSettingAsyncAndCallCreateWinLossFeedbackCreditAsync()
    {
        // Arrange
        var service = MockService();
        var setting = fixture.Create<WinLossSystemSetting>();
        var cusWinLimit = fixture.Create<IEnumerable<CustomerWinLimit>>();
        var winLimit = new List<WinLimitFeedBackInfo>
        {
            new() { LogID = 1, CustId = 1}
        };
        var betInfo = new List<BettypeInfo>
        {
            new() {Name = "Gaming", Bettype = "1"}
        };
        var sport = new List<SportInfo>
        {
            new() {LogID = 1, SportType =1, SportName = "test"}
        };
        var winlossInfo = new ValueTuple<IEnumerable<WinLimitFeedBackInfo>, IEnumerable<BettypeInfo>, IEnumerable<SportInfo>>(winLimit, betInfo, sport);
        var environment = "LIVE";
        customerService.GetListCustReachLimitAsync(setting, Arg.Any<bool>()).Returns(cusWinLimit);
        transactionService.ImportWinLimitFeedbackInfoAsync(Arg.Any<IEnumerable<CustomerWinLimit>>(), Arg.Any<IEnumerable<GroupHeadline>>(), environment).Returns(winlossInfo);

        // Act
        await service.CreateFeedbackForWinLossLimitAsync(setting, environment);

        // Assert
        await systemSettingService.Received().EditSystemSettingAsync(Arg.Any<IEnumerable<SystemSettingItem>>(), Arg.Any<string>());
        service.Received().Protected(
            "CreateWinLossFeedbackCreditAsync",
            Arg.Any<bool>(),
            Arg.Any<WinLimitFeedBackInfo>(),
            Arg.Any<IEnumerable<BettypeInfo>>(),
            Arg.Any<IEnumerable<SportInfo>>(),
            Arg.Any<IEnumerable<SettingItem>>(),
            Arg.Any<IEnumerable<SystemSettingMapping>>());
    }

    #endregion CreateFeedbackForWinLossLimitAsync

    #region CreateWinLossFeedbackCreditAsync

    [Fact]
    public async Task CreateWinLossFeedbackCreditAsync__CreateNewCriteria_ExpectedToCallFunction()
    {
        // Arrange
        var winLimit = new WinLimitFeedBackInfo
        {
            LogID = 1,
            CustId = 1,
            BettypeIDList = "1,2",
            HeadlineId = 1,
            HeadlineName = "headline",
            RelatedCustId = "1",
            UserName = "test"
        };
        var betInfo = new List<BettypeInfo>
        {
            new() {Name = "Gaming", Bettype = "1"},
            new() {Name = "Bet", Bettype = "2" }
        };
        var sport = new List<SportInfo>
        {
            new() {LogID = 1, SportType = 1, SportName = "test"}
        };

        var settings = new List<SettingItem>
        {
            new() { SettingId = 1, SettingName = "Status", SettingValue = "Closed" },
            new() { SettingId = 2, SettingName = "Response", SettingValue = "Neutral" },
            new() { SettingId = 2022, SettingName = "Station" }
        };
        var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();
        var category = fixture.Create<Tuple<int, int, string>>();
        var service = MockService();
        commonFeedback.GetCategorySubCategoryCWL(settings, DefaultValue.CompanyWLLimit).Returns(category);
        service.Protected("GetProductXml", winLimit, Arg.Any<List<BettypeInfo>>(), Arg.Any<IEnumerable<SportInfo>>(), settings, settingMappings).Returns("product xml");
        service.Protected("GetSolution", winLimit, true).Returns("solution");
        service.Protected("GetRelatedCustomerXml", winLimit).Returns("related customer xml");
        service.Protected("GetDescription", winLimit, Arg.Any<List<BettypeInfo>>()).Returns("description");

        // Act
        await service.Public_CreateWinLossFeedbackCreditAsync(
            false,
            winLimit,
            betInfo,
            sport,
            settings,
            settingMappings
        );

        // Assert
        service.Received().Protected("CheckValidParamsAndInsertFeedbackAsync", Arg.Any<WinLimitFeedBackInfo>()
            , Arg.Any<CreateMultipleFeedbackCommand>());
    }

    #endregion CreateWinLossFeedbackCreditAsync

    #region CheckValidParamsAndInsertFeedbackAsync

    [Fact]
    public async Task CheckValidParamsAndInsertFeedbackAsync__CreateSuccessful_ExpectedWriteLog()
    {
        // Arrange
        var winLimit = new WinLimitFeedBackInfo
        {
            LogID = 1,
            CustId = 1,
            BettypeIDList = "1,2",
            HeadlineId = 1,
            HeadlineName = "headline",
            RelatedCustId = "1",
            UserName = "test"
        };
        var criteria = fixture.Create<CreateMultipleFeedbackCommand>();
        var service = MockService();
        commonFeedback.CheckFeedbackValidParams(criteria).Returns(new ValueTuple<bool, string>(true, ""));

        var commandResult = new CRM.Models.Common.CommandResult { Code = 0 };
        baseService.ExecuteNonQueryAsync(criteria).Returns(commandResult);

        // Act
        await service.Public_CheckValidParamsAndInsertFeedbackAsync(
            winLimit,
            criteria
        );

        // Assert
        logService.Received(1).LogInformation(Arg.Any<string>());
    }

    [Fact]
    public async Task CheckValidParamsAndInsertFeedbackAsync__CreateUnSuccessful_ExpectedWriteLogAndUpdateStatus()
    {
        // Arrange
        var winLimit = new WinLimitFeedBackInfo
        {
            LogID = 1,
            CustId = 1,
            BettypeIDList = "1,2",
            HeadlineId = 1,
            HeadlineName = "headline",
            RelatedCustId = "1",
            UserName = "test"
        };
        var criteria = fixture.Create<CreateMultipleFeedbackCommand>();
        var service = MockService();
        commonFeedback.CheckFeedbackValidParams(criteria).Returns(new ValueTuple<bool, string>(true, ""));

        var commandResult = new CRM.Models.Common.CommandResult { Code = 1, Message = "" };
        baseService.ExecuteNonQueryAsync(criteria).Returns(commandResult);

        // Act
        await service.Public_CheckValidParamsAndInsertFeedbackAsync(
            winLimit,
            criteria
        );

        // Assert
        logService.Received(1).LogInformation(Arg.Any<string>());
        await transactionService.EditFeedbackWinLossInfoAsync(winLimit.LogID, CreateFeedbackJobStatus.Fail, commandResult.Message);
        await logService.ErrorAsync(commandResult, criteria);
    }

    [Fact]
    public async Task CheckValidParamsAndInsertFeedbackAsync__InvalidParams_ExpectedReturnAndUpdateStatus()
    {
        // Arrange
        var winLimit = new WinLimitFeedBackInfo
        {
            LogID = 1,
            CustId = 1,
            BettypeIDList = "1,2",
            HeadlineId = 1,
            HeadlineName = "headline",
            RelatedCustId = "1",
            UserName = "test"
        };
        var criteria = fixture.Create<CreateMultipleFeedbackCommand>();
        var service = MockService();
        commonFeedback.CheckFeedbackValidParams(criteria).Returns(new ValueTuple<bool, string>(false, ""));

        var commandResult = new CRM.Models.Common.CommandResult { Code = 0 };
        baseService.ExecuteNonQueryAsync(criteria).Returns(commandResult);

        // Act
        await service.Public_CheckValidParamsAndInsertFeedbackAsync(
            winLimit,
            criteria
        );

        // Assert
        logService.Received(1).LogInformation(Arg.Any<string>());
        await transactionService.EditFeedbackWinLossInfoAsync(winLimit.LogID, CreateFeedbackJobStatus.Fail, Arg.Any<string>());
    }

    [Fact]
    public async Task CheckValidParamsAndInsertFeedbackAsync__Exception_ExpectedWriteLogAndUpdateStatus()
    {
        // Arrange
        var winLimit = new WinLimitFeedBackInfo
        {
            LogID = 1,
            CustId = 1,
            BettypeIDList = "1,2",
            HeadlineId = 1,
            HeadlineName = "headline",
            RelatedCustId = "1",
            UserName = "test"
        };
        var exception = fixture.Create<Exception>();
        var criteria = fixture.Create<CreateMultipleFeedbackCommand>();
        var service = MockService();
        commonFeedback.CheckFeedbackValidParams(criteria).Returns(new ValueTuple<bool, string>(true, ""));

        baseService.When(x => x.ExecuteNonQueryAsync(criteria)).Throw(exception);

        // Act
        await service.Public_CheckValidParamsAndInsertFeedbackAsync(
            winLimit,
            criteria
        );

        // Assert
        logService.Received(1).LogInformation(Arg.Any<string>());
        await transactionService.EditFeedbackWinLossInfoAsync(winLimit.LogID, CreateFeedbackJobStatus.Fail, exception.Message);
        await logService.ErrorAsync(exception);
    }

    #endregion CheckValidParamsAndInsertFeedbackAsync

    #region GetProductXml

    [Fact]
    public void GetProductXml_NotExistedBetType_ReturnExpectedXml()
    {
        // Arrange
        var winLimit = fixture.Create<WinLimitFeedBackInfo>();
        var betTypeMappings = fixture.Create<List<BettypeInfo>>();
        var settings = fixture.Create<IEnumerable<SettingItem>>();
        var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();
        var subProducts = new List<SettingItem>
        {
            new()
            {
                SettingId = 1
            }
        };
        var product = new SettingItem
        {
            SettingId = 1
        };

        var sport = new List<SportInfo>
        {
            new() {LogID = 1, SportType = 1, SportName = "test"}
        };
        var betTypes = new List<SettingItem>();
        var service = MockService();
        service.Protected("GetProduct", settings, settingMappings, winLimit.ProductName).Returns(product);
        service.Protected("GetSubProduct", winLimit, sport, product.SettingId, settings, settingMappings).Returns(subProducts);
        service.Protected("GetBetType", subProducts.Select(x => x.SettingId), betTypeMappings, settingMappings, settings).Returns(betTypes);

        // Act
        var actual = service.Public_GetProductXml(winLimit, betTypeMappings, sport, settings, settingMappings);

        // Assert
        Assert.Equal("<ROOT><Product ProductID='1' SubProductID='1' BetTypeID='9144' /></ROOT>", actual);
    }

    [Fact]
    public void GetProductXml_ExistedBetType_ReturnExpectedXml()
    {
        // Arrange
        var winLimit = fixture.Create<WinLimitFeedBackInfo>();
        var betTypeMappings = fixture.Create<List<BettypeInfo>>();
        var settings = new List<SettingItem>
        {
            new() { SettingId = 1, SettingName = "BetType", ParentIds = new List<int> { 1 } }
        };
        var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();
        var subProducts = new List<SettingItem>
        {
            new()
            {
                SettingId = 1
            }
        };

        var product = new SettingItem
        {
            SettingId = 1
        };
        var betTypes = new List<SettingItem>
        {
            new() { SettingId = 1, ParentIds = new List<int> { 1 } }
        };
        var sport = new List<SportInfo>
        {
            new() {LogID = 1, SportType = 1, SportName = "test"}
        };
        var service = MockService();
        service.Protected("GetProduct", settings, settingMappings, winLimit.ProductName).Returns(product);
        service.Protected("GetSubProduct", winLimit, sport, product.SettingId, settings, settingMappings).Returns(subProducts);
        service.Protected("GetBetType", Arg.Any<IEnumerable<int>>(), betTypeMappings, settingMappings, settings).Returns(betTypes);

        // Act
        var actual = service.Public_GetProductXml(winLimit, betTypeMappings, sport, settings, settingMappings);

        // Assert
        Assert.Equal("<ROOT><Product ProductID='1' SubProductID='1' BetTypeID='1' /></ROOT>", actual);
    }

    #endregion GetProductXml

    #region GetProduct

    [Fact]
    public void GetProduct_SettingSourceIsNull_ReturnDefaultProduct()
    {
        // Arrange
        var productName = "1";
        var settingMappings = new List<SystemSettingMapping>
        {
            new() { SettingName = "Product" }
        };
        var settings = new List<SettingItem>
        {
            new() { SettingId = 1 , SettingName = "Product"}
        };

        var service = MockService();

        // Act
        var actual = service.Public_GetProduct(settings, settingMappings, productName);

        // Assert
        Assert.Equal(BackgroundJobConstants.ProductNA, actual.SettingId);
    }

    [Fact]
    public void GetProduct_SettingSourceNotEqualProductName_ReturnDefaultProduct()
    {
        // Arrange
        var productName = "1";
        var settingMappings = new List<SystemSettingMapping>
        {
            new() {SettingId = 1, SettingName = "Product", SettingSource = "2" }
        };
        var settings = new List<SettingItem>
        {
            new() { SettingId = 1 , SettingName = "Product"}
        };
        var service = MockService();

        // Act
        var actual = service.Public_GetProduct(settings, settingMappings, productName);

        // Assert
        Assert.Equal(BackgroundJobConstants.ProductNA, actual.SettingId);
    }

    [Fact]
    public void GetProduct_SourceMappingIdNotEqualSettingItem_ReturnDefaultProduct()
    {
        // Arrange
        var productName = "1";
        var settingMappings = new List<SystemSettingMapping>
        {
            new() {SettingId = 1, SettingName = "Product", SettingSource = "1" }
        };
        var settings = new List<SettingItem>
        {
            new() { SettingId = 2 , SettingName = "Product"}
        };
        var service = MockService();

        // Act
        var actual = service.Public_GetProduct(settings, settingMappings, productName);

        // Assert
        Assert.Equal(BackgroundJobConstants.ProductNA, actual.SettingId);
    }

    [Fact]
    public void GetProduct_ExistProductNameMappingAndSetting_ReturnExpectedProduct()
    {
        // Arrange
        var productName = "RNG Casino";
        var settingMappings = new List<SystemSettingMapping>
        {
            new() { SettingId = 1, SettingName = "Product", SettingSource = "RNG Casino" }
        };
        var settings = new List<SettingItem>
        {
            new() { SettingId = 1 , SettingName = "Product"}
        };

        var service = MockService();

        // Act
        var actual = service.Public_GetProduct(settings, settingMappings, productName);

        // Assert
        Assert.Equal(settings.First(), actual);
    }

    #endregion GetProduct

    #region GetSubProduct

    [Fact]
    public void GetSubProduct_NotExistProductName_ReturnDefaultSubProduct()
    {
        // Arrange
        var winLimit = fixture.Create<WinLimitFeedBackInfo>();
        var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();
        var sport = fixture.Create<IEnumerable<SportInfo>>();
        var settings = fixture.Create<IEnumerable<SettingItem>>();

        var service = MockService();

        // Act
        var actual = service.Public_GetSubProduct(winLimit, sport, 2, settings, settingMappings);

        // Assert
        Assert.Single(actual);
        Assert.Equal(BackgroundJobConstants.SubProductNA, actual.First().SettingId);

        Assert.Single(actual.First().ParentIds);
        Assert.Equal(BackgroundJobConstants.ProductNA, actual.First().ParentIds.First());
    }

    [Fact]
    public void GetSubProduct_ExistProductName_ReturnExpectedSubProduct()
    {
        // Arrange
        var winLimit = new WinLimitFeedBackInfo
        {
            ProductName = "game"
        };
        var settingMappings = new List<SystemSettingMapping>
        {
            new() { SettingName = "SubProduct", SettingSource = "game" }
        };
        var expected = new List<SystemSettingMapping>
        {
            new() { SettingId = 8063}
        };
        var sport = new List<SportInfo>
        {
            new() {LogID = 1, SportType = 1, SportName = "test"}
        };
        var settings = new List<SettingItem>
        {
            new() { SettingId = 1, SettingName = "SubProduct", ParentIds = new List<int> { 1 } }
        };
        var service = MockService();

        // Act
        var actual = service.Public_GetSubProduct(winLimit, sport, 1, settings, settingMappings);

        // Assert
        Assert.Equal(expected.First().SettingId, actual.First().SettingId);
    }

    #endregion GetSubProduct

    #region GetBetType

    [Fact]
    public void GetBetType_NotExistBetTypeMappings_ReturnDefaultBetType()
    {
        // Arrange
        var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();
        var betTypeMappings = fixture.Create<List<BettypeInfo>>();
        var settings = fixture.Create<IEnumerable<SettingItem>>();
        var subProducts = fixture.Create<IEnumerable<int>>();

        var service = MockService();

        // Act
        var actual = service.Public_GetBetType(subProducts, betTypeMappings, settingMappings, settings);

        // Assert
        Assert.Single(actual);
        Assert.Equal(BackgroundJobConstants.BettypeNA, actual.First().SettingId);

        Assert.Single(actual.First().ParentIds);
        Assert.Equal(BackgroundJobConstants.SubProductNA, actual.First().ParentIds.First());
    }

    [Fact]
    public void GetBetType_ExistBetTypeMappings_ReturnExpectedBetType()
    {
        // Arrange
        var settingMappings = new List<SystemSettingMapping>
        {
            new() { SettingId = 1, SettingName = "BetType", SettingSource = "a" }
        };
        var betTypeMappings = new List<BettypeInfo>
        {
            new() { Name = "a" }
        };

        var settings = new List<SettingItem>
        {
            new() { SettingId = 1, SettingName = "BetType", ParentIds = new List<int> { 1 } }
        };

        var subProducts = new List<int>
        {
            1
        };

        var service = MockService();

        // Act
        var actual = service.Public_GetBetType(subProducts, betTypeMappings, settingMappings, settings);

        // Assert
        Assert.Single(actual);
        Assert.Equal(settings.First(), actual.First());
    }

    #endregion GetBetType

    #region GetSolution

    [Theory]
    [InlineData("RNG Casino - Minor Jackpot")]
    [InlineData("RNG Casino - Mega Jackpot")]
    public void GetSolution_ProductNameRNGCasinoNotVipCustomer_ReturnExactTemplate(string ProductGroup)
    {
        // Arrange
        var winLimit = new WinLimitFeedBackInfo
        {
            UserName = "test",
            TotalWinAmt = 1000,
            Currency = "USD",
            ProductID = 1,
            ProductName = "RNG Casino",
            ProductGroup = ProductGroup
        };

        var service = MockService();
        var template = new FeedbackTemplate
        {
            Templates = new Template { RNGTemplateENSC = "{0} {1} {2}" }
        };

        commonFeedback.GetFeedbackTemplates<FeedbackTemplate>(Arg.Any<string>()).Returns(template);
        var expect = "test 1,000 USD";
        // Act
        var result = service.Public_GetSolution(winLimit, false);

        // Assert
        var expected = Regex.Replace(expect, @"\s", "");
        var resulted = Regex.Replace(result, @"\s", "");
        Assert.Equal(expected, resulted);
    }

    [Theory]
    [InlineData("Orange", false, "ProductNameSC")]
    [InlineData("Yellow", false, "ProductNameSC")]
    [InlineData("Grey", false, "ProductNameSC")]
    [InlineData("Red", false, "ProductNameSC")]
    [InlineData("Black", false, "ProductNameSC")]
    [InlineData("Orange", true, "ProductNameYN")]
    [InlineData("Yellow", true, "ProductNameYN")]
    [InlineData("Grey", true, "ProductNameYN")]
    [InlineData("Red", true, "ProductNameYN")]
    [InlineData("Black", true, "ProductNameYN")]
    public void GetSolution_DependOnLevelAndIsVIPCustomer_ReturnExactTemplate(string level, bool isVip, string productName)
    {
        // Arrange
        var winLimit = new WinLimitFeedBackInfo
        {
            Level = level,
            UserName = "test",
            TotalWinAmt = 1000,
            Currency = "USD",
            ProductID = 1
        };

        var service = MockService();
        var template = new FeedbackTemplate
        {
            Templates = new Template
            {
                YellowTemplateENSC = "{0} {1} {2} {3} {4} {5}",
                RedTemplateENSC = "{0} {1} {2} {3} {4} {5}",
                BlackTemplateSC = "{0} {1} {2} {3} {4} {5}",
                GreyTemplateENSC = "{0} {1} {2} {3} {4} {5}",
                YellowTemplateENYN = "{0} {1} {2} {3} {4} {5}",
                RedTemplateENYN = "{0} {1} {2} {3} {4} {5}",
                BlackTemplateYN = "{0} {1} {2} {3} {4} {5}",
                GreyTemplateENYN = "{0} {1} {2} {3} {4} {5}"
            }
        };
        commonFeedback.GetFeedbackTemplates<FeedbackTemplate>(Arg.Any<string>()).Returns(template);
        service.Protected("GetTransformedProductName", winLimit, "en-US").Returns("ProductNameUS");
        service.Protected("GetTransformedProductName", winLimit, "zh-CN").Returns("ProductNameSC");
        service.Protected("GetTransformedProductName", winLimit, "vi-VN").Returns("ProductNameYN");
        commonFeedback.GetProductName(winLimit.ProductID.ToString(), "en-US").Returns("ProductNameUS");
        commonFeedback.GetProductName(winLimit.ProductID.ToString(), "zh-CN").Returns("ProductNameSC");
        commonFeedback.GetProductName(winLimit.ProductID.ToString(), "vi-VN").Returns("ProductNameYN");
        var expect = "test 1,000 USD ProductNameUS " + productName + BackgroundJobConstants.LineBreak;
        // Act
        var result = service.Public_GetSolution(winLimit, isVip);

        // Assert
        var expected = Regex.Replace(expect, @"\s", "");
        var resulted = Regex.Replace(result, @"\s", "");
        Assert.Equal(expected, resulted);
    }

    [Fact]
    public void GetSolution_DoesNotHaveLevelID_ReturnDefault()
    {
        // Arrange
        var winLimit = new WinLimitFeedBackInfo
        {
            Level = "Default",
            UserName = "test",
            TotalWinAmt = 1000,
            Currency = "USD",
            ProductID = 1,
            ProductName = "Test"
        };

        var service = MockService();
        var template = new FeedbackTemplate
        {
            Templates = new Template
            {
                YellowTemplateENSC = "{0} {1} {2} {3} {4} {5}",
                RedTemplateENSC = "{0} {1} {2} {3} {4} {5}",
                BlackTemplateSC = "{0} {1} {2} {3} {4} {5}",
                GreyTemplateENSC = "{0} {1} {2} {3} {4} {5}",
                YellowTemplateENYN = "{0} {1} {2} {3} {4} {5}",
                RedTemplateENYN = "{0} {1} {2} {3} {4} {5}",
                BlackTemplateYN = "{0} {1} {2} {3} {4} {5}",
                GreyTemplateENYN = "{0} {1} {2} {3} {4} {5}"
            }
        };
        commonFeedback.GetFeedbackTemplates<FeedbackTemplate>(Arg.Any<string>()).Returns(template);
        commonFeedback.GetProductName(winLimit.ProductID.ToString(), "en-US").Returns("ProductNameUS");
        commonFeedback.GetProductName(winLimit.ProductID.ToString(), "zh-CN").Returns("ProductNameSC");
        commonFeedback.GetProductName(winLimit.ProductID.ToString(), "vi-VN").Returns("ProductNameYN");

        // Act
        var result = service.Public_GetSolution(winLimit, false);

        // Assert
        Assert.Equal("", result);
    }

    #endregion GetSolution

    #region GetDescription

    public static IEnumerable<object[]> TestBetTypesCount()
    {
        return new List<object[]>
        {
            new object[] { new List<BettypeInfo>
            {
                new() {Name = "a"},
                new() {Name = "b"},
                new() {Name = "c"},
                new() {Name = "d"}
            }, "a, b, c"},
            new object[] { new List<BettypeInfo>
            {
                new() {Name = "a"},
                new() {Name = "b"},
            }, "a, b"},
            new object[] { new List<BettypeInfo>(), ""}
        };
    }

    [Theory]
    [MemberData(nameof(TestBetTypesCount))]
    public void GetDescription_WhenCalled_ReturnExpectTemplate(
        List<BettypeInfo> betTypes,
        string numGamesFormat)
    {
        // Arrange
        var winLimit = new WinLimitFeedBackInfo
        {
            MaxWinLimitSetting = "1000",
            TotalWinAmt_RM = 1000,
            TotalWinAmt = 1000,
            UserName = "Test",
            Site = "123",
            Currency = "USD",
            ReachedTime = new DateTime(2020, 11, 13),
            Level = "Red"
        };

        var service = MockService();
        var description = new StringBuilder("<span style=\"color: rgb(255, 0, 0);\">This is an auto-generated entry.</span>");
        description.Append("<br />Informed Headline Win/Loss Alert for below member:");
        description.Append($"<br />Product: {winLimit.ProductName}");
        description.Append("<br />Game Type: ");
        description.AppendFormat(betTypes.Count > 3 ? "{0} ..." : "{0}", numGamesFormat);
        description.Append($"<br /> UserName: {winLimit.UserName}, Site: {winLimit.Site} ({winLimit.SiteLabel})");
        description.Append(
            $"<br /> Winning/Losing Limit Amount: 1,000 (RM), Winning/Losing Amount in RM: 1,000 (RM), Winloss Amount in player currency: 1,000 ({winLimit.Currency})");
        description.Append(
            $"<br />Reached Limit Time: {winLimit.ReachedTime}, Reached Limit Level: {winLimit.Level}");
        description.Append(BackgroundJobConstants.LineBreak);

        // Act
        var actual = service.Public_GetDescription(winLimit, betTypes);

        // Assert
        Assert.Equal(description.ToString(), actual);
    }

    #endregion GetDescription

    #region GetRelatedCustomerXml

    [Fact]
    public void GetRelatedCustomerXml_CallFunction_ReturnExpectTemplate()
    {
        // Arrange
        var winLimit = new WinLimitFeedBackInfo
        {
            HeadlineId = 1,
            RelatedCustId = "1",
            UserName = "test"
        };

        var service = MockService();

        // Act
        var actual = service.Public_GetRelatedCustomerXml(winLimit);

        // Assert
        Assert.Equal("<ROOT><Row CustId='1' RelatedCustId='1' Username='test' /></ROOT>", actual);
    }

    #endregion GetRelatedCustomerXml

    #region Helpers

    private WinLossFeedbackServiceTestable MockService()
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

        return Substitute.For<WinLossFeedbackServiceTestable>(
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