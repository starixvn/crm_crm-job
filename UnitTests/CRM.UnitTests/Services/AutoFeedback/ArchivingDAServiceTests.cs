namespace CRM.UnitTests.Services.AutoFeedback;

using AutoFixture;
using CRM.Constants;
using CRM.Criteria.AutoFeedback;
using CRM.Models.ArchivingDA;
using CRM.Models.Common;
using CRM.Services.AutoFeedback;
using CRM.Services.Common;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Testable;
using Xunit;

public class ArchivingDAServiceTests
{
    private AppJobSettings appSettings;
    private IBaseService baseService;
    private ISettingService settingService;
    private ICrmLogService logService;
    private IFeedbackTransactionService feedbackTransactionService;
    private ICommonFeedback commonFeedback;
    private readonly IFixture fixture = new Fixture();

    #region CreateFeedbackForArchivingDAAsync

    [Fact]
    public async Task CreateFeedbackForArchivingDAAsync_IsStageOneAndIsSyncArchivedAccount_ExpectedCallExactFunction()
    {
        // Arrange
        var service = MockService();
        var archivingAutoFbInfos = fixture.Create<IEnumerable<ArchivingAutoFbInfo>>();
        var LastRuntimeEnv = DateTime.Now;
        fixture.Customize<ArchivingDASetting>(x => x.With(b => b.IsSyncArchivedAccount, true));
        var archivingDASetting = fixture.Create<ArchivingDASetting>();
        feedbackTransactionService.GetArchivingDAStatus().Returns(true);
        feedbackTransactionService.ArchivingAutoFeedbackInfoInsAsync(archivingDASetting, "LIVE").Returns(archivingAutoFbInfos);
        // Act
        await service.Public_CreateFeedbackForArchivingDAAsync(archivingDASetting, false, LastRuntimeEnv, "LastRuntimeNameEnv", "JobStatusNameEnv", "LIVE");

        // Assert
        service.Received().Protected("CheckExistAnyArchivingFb", archivingAutoFbInfos,
            Arg.Any<IEnumerable<SettingItem>>(),
            Arg.Any<IEnumerable<SystemSettingMapping>>(),
            Arg.Any<string>(), Arg.Any<string>()
        );
    }

    [Fact]
    public async Task CreateFeedbackForArchivingDAAsync_IsStageOneAndIsNotSyncArchivedAccount_ExpectedCallExactFunction()
    {
        // Arrange
        var service = MockService();
        var archivingAutoFbInfos = fixture.Create<IEnumerable<ArchivingAutoFbInfo>>();
        var LastRuntimeEnv = DateTime.Now;
        fixture.Customize<ArchivingDASetting>(x => x.With(b => b.IsSyncArchivedAccount, false));
        var archivingDASetting = fixture.Create<ArchivingDASetting>();
        var custInfo = fixture.Create<IEnumerable<ArchivingCustInfo>>();
        feedbackTransactionService.GetArchivingDAStatus().Returns(true);
        feedbackTransactionService.ArchivingAutoFeedbackInfoInsAsync(archivingDASetting, "LIVE").Returns(archivingAutoFbInfos);
        feedbackTransactionService.GetArchivingCustInfo().Returns(custInfo);
        // Act
        await service.Public_CreateFeedbackForArchivingDAAsync(archivingDASetting, false, LastRuntimeEnv, "LastRuntimeNameEnv", "JobStatusNameEnv", "LIVE");

        // Assert
        await feedbackTransactionService.ArchivingImportDirectAccountAsync(Arg.Any<string>(), Arg.Any<DateTime>());
        await commonFeedback.EditArchivingDAStatusSettingAsync(BackgroundJobConstants.ServiceName_ArchivingDA, "1");
        await commonFeedback.EditSystemSettingAsyncByEnv(Arg.Any<string>(), Arg.Any<string>(), JobStatus.Completed, BackgroundJobConstants.ServiceName_ChatBot_ArchivedAccounts);
        service.Received().Protected("CheckExistAnyArchivingFb", archivingAutoFbInfos,
            Arg.Any<IEnumerable<SettingItem>>(),
            Arg.Any<IEnumerable<SystemSettingMapping>>(),
            Arg.Any<string>(), Arg.Any<string>()
        );
    }

    [Fact]
    public async Task CreateFeedbackForArchivingDAAsync_IsStageNotOne_ExpectedCallExactFunction()
    {
        // Arrange
        var service = MockService();
        var archivingAutoFbInfos = fixture.Create<IEnumerable<ArchivingAutoFbInfo>>();
        var LastRuntimeEnv = DateTime.Now;
        fixture.Customize<ArchivingDASetting>(x => x.With(b => b.IsSyncArchivedAccount, false));
        var archivingDASetting = fixture.Create<ArchivingDASetting>();
        var custInfo = fixture.Create<IEnumerable<ArchivingCustInfo>>();
        feedbackTransactionService.GetArchivingDAStatus().Returns(false);
        feedbackTransactionService.ArchivingAutoFeedbackInfoInsAsync(archivingDASetting, "LIVE").Returns(archivingAutoFbInfos);
        feedbackTransactionService.GetArchivingCustInfo().Returns(custInfo);
        // Act
        await service.Public_CreateFeedbackForArchivingDAAsync(archivingDASetting, false, LastRuntimeEnv, "LastRuntimeNameEnv", "JobStatusNameEnv", "LIVE");

        // Assert
        logService.Received(1).LogInformation(Arg.Any<string>());
        service.Received().Protected("UpdateArchivingStatus", false, "LastRuntimeNameEnv", "JobStatusNameEnv");
        await commonFeedback.EditSystemSettingAsyncByEnv(Arg.Any<string>(), Arg.Any<string>(), JobStatus.Completed, BackgroundJobConstants.ServiceName_ChatBot_ArchivedAccounts);
    }

    #endregion CreateFeedbackForArchivingDAAsync

    #region CheckExistAnyArchivingFb

    [Fact]
    public async Task CheckExistAnyArchivingFb_IsAnyData_ExpectedCallExactFunction()
    {
        // Arrange
        var archivingAutoFbInfos = fixture.Create<IEnumerable<ArchivingAutoFbInfo>>();
        var settings = fixture.Create<IEnumerable<SettingItem>>();
        var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();
        var service = MockService();

        // Act
        await service.Public_CheckExistAnyArchivingFb(archivingAutoFbInfos, settings, settingMappings,
            "jobStatusNameEnv", "lastRuntimeNameEnv");

        // Assert
        service.Received().Protected("CreateFeedbackForArchivingDACreditAsync", Arg.Any<ArchivingAutoFbInfo>(), Arg.Any<IEnumerable<SettingItem>>(), Arg.Any<IEnumerable<SystemSettingMapping>>());
        await commonFeedback.EditArchivingDAMaxLogIdSettingAsync(BackgroundJobConstants.ServiceName_ArchivingDA, Arg.Any<string>());
    }

    #endregion CheckExistAnyArchivingFb

    #region CreateFeedbackForArchivingDACreditAsync

    [Fact]
    public async Task CreateFeedbackForArchivingDACreditAsync_ExpectedCallExactFunction()
    {
        // Arrange
        var archivingAutoFbInfos = fixture.Create<ArchivingAutoFbInfo>();
        var settings = fixture.Create<IEnumerable<SettingItem>>();
        var settingMappings = fixture.Create<IEnumerable<SystemSettingMapping>>();
        var category = fixture.Create<Tuple<int, int, string>>();
        var service = MockService();
        service.Protected("GetCategorySubCategoryArchivingDA", settings, DefaultValue.Special_Notification).Returns(category);

        // Act
        await service.Public_CreateFeedbackForArchivingDACreditAsync(archivingAutoFbInfos, settings, settingMappings);

        // Assert
        service.Received().Protected("CheckValidArchivingParamsAndInsertFeedbackAsync", Arg.Any<ArchivingAutoFbInfo>(), Arg.Any<CreateMultipleFeedbackCommand>());
        service.Received().Protected("GetProductXmlNA");
        service.Received().Protected("GetArchivingRelatedCustomerXml", Arg.Any<ArchivingAutoFbInfo>());
        service.Received().Protected("GetArchivingDADescription", Arg.Any<ArchivingAutoFbInfo>());
        logService.Received(1).LogInformation(Arg.Any<string>());
    }

    #endregion CreateFeedbackForArchivingDACreditAsync

    #region GetArchivingDADescription

    [Fact]
    public void GetArchivingDADescription_ExpectedReturnExactString()
    {
        // Arrange
        var archivingAutoFbInfos = fixture.Create<ArchivingAutoFbInfo>();
        var template = fixture.Create<ArchivingTemplate>();
        var stringBuilder = new StringBuilder(DefaultValue.autoGenerated);
        var expect = stringBuilder.Append(BackgroundJobConstants.Break).Append(template.Description);
        var service = MockService();
        commonFeedback.GetFeedbackTemplates<ArchivingTemplate>("ArchivingDATemplate.xml").Returns(template);

        // Act
        var actual = service.Public_GetArchivingDADescription(archivingAutoFbInfos);

        // Assert
        Assert.Equal(expect.ToString(), actual);
    }

    #endregion GetArchivingDADescription

    #region GetArchivingDASolution

    [Fact]
    public void GetArchivingDASolution_ExpectedReturnExactString()
    {
        // Arrange
        var archivingAutoFbInfos = fixture.Create<ArchivingAutoFbInfo>();
        var template = fixture.Create<ArchivingTemplate>();
        var stringBuilder = new StringBuilder();
        var expect = stringBuilder.Append(BackgroundJobConstants.Break).Append(template.SolutionENSC);
        var service = MockService();
        commonFeedback.GetFeedbackTemplates<ArchivingTemplate>("ArchivingDATemplate.xml").Returns(template);

        // Act
        var actual = service.Public_GetArchivingDASolution(archivingAutoFbInfos);

        // Assert
        Assert.Equal(expect.ToString(), actual);
    }

    #endregion GetArchivingDASolution

    #region GetArchivingRelatedCustomerXml

    [Fact]
    public void GetArchivingRelatedCustomerXml_IsEmptyOrNullAffectedId_ReturnExactDefaultTemplate()
    {
        // Arrange
        var archivingAutoFbInfo = new ArchivingAutoFbInfo();
        var service = MockService();

        // Act
        var actual = service.Public_GetArchivingRelatedCustomerXml(archivingAutoFbInfo);

        // Assert
        Assert.Equal("<ROOT></ROOT>", actual);
    }

    [Fact]
    public void GetAmendmentRelatedCustomerXml_AffectedIdAreNotNull_ReturnExactTemplate()
    {
        // Arrange
        var archivingAutoFbInfo = new ArchivingAutoFbInfo
        {
            AffectedId = "1,2",
            HeadlineID = 1,
            AffectedName = "1,2"
        };
        var service = MockService();
        var expect = "<ROOT><Row CustId='1' RelatedCustId='1' Username='1' /><Row CustId='1' RelatedCustId='2' Username='2' /></ROOT>";

        // Act
        var actual = service.Public_GetArchivingRelatedCustomerXml(archivingAutoFbInfo);

        // Assert
        Assert.Equal(expect, actual);
    }

    #endregion GetArchivingRelatedCustomerXml

    #region CheckValidArchivingParamsAndInsertFeedbackAsync

    [Fact]
    public async Task CheckValidArchivingParamsAndInsertFeedbackAsync_CreateSuccessful_ExpectedWriteLog()
    {
        // Arrange
        var archivingAutoFbInfo = new ArchivingAutoFbInfo
        {
            LogFeedbackId = 1,
            HeadlineName = "1",
            HeadlineID = 1,
            AffectedId = "1",
            AffectedName = "1"
        };
        var criteria = fixture.Create<CreateMultipleFeedbackCommand>();
        var service = MockService();
        commonFeedback.CheckFeedbackValidParams(criteria).Returns(new ValueTuple<bool, string>(true, ""));

        var commandResult = new CRM.Models.Common.CommandResult { Code = 0 };
        baseService.ExecuteNonQueryAsync(criteria).Returns(commandResult);

        // Act
        await service.Public_CheckValidArchivingParamsAndInsertFeedbackAsync(
            archivingAutoFbInfo,
            criteria
        );

        // Assert
        logService.Received(2).LogInformation(Arg.Any<string>());
    }

    [Fact]
    public async Task CheckValidArchivingParamsAndInsertFeedbackAsync_CreateUnSuccessful_ExpectedWriteLogAndUpdateStatus()
    {
        var archivingAutoFbInfo = new ArchivingAutoFbInfo
        {
            LogFeedbackId = 1,
            HeadlineName = "1",
            HeadlineID = 1,
            AffectedId = "1",
            AffectedName = "1"
        };
        var criteria = fixture.Create<CreateMultipleFeedbackCommand>();
        var service = MockService();
        commonFeedback.CheckFeedbackValidParams(criteria).Returns(new ValueTuple<bool, string>(true, ""));

        var commandResult = new CRM.Models.Common.CommandResult { Code = 1, Message = "" };
        baseService.ExecuteNonQueryAsync(criteria).Returns(commandResult);

        // Act
        await service.Public_CheckValidArchivingParamsAndInsertFeedbackAsync(
            archivingAutoFbInfo,
            criteria
        );

        // Assert
        logService.Received(2).LogInformation(Arg.Any<string>());
        await feedbackTransactionService.EditArchivingDAAutoFeedback(1, CreateFeedbackJobStatus.Fail, commandResult.Message);
        await logService.ErrorAsync(commandResult, criteria);
    }

    [Fact]
    public async Task CheckValidArchivingParamsAndInsertFeedbackAsync_InvalidParams_ExpectedReturnAndUpdateStatus()
    {
        // Arrange
        var archivingAutoFbInfo = new ArchivingAutoFbInfo
        {
            LogFeedbackId = 1,
            HeadlineName = "1",
            HeadlineID = 1,
            AffectedId = "1",
            AffectedName = "1"
        };
        var criteria = fixture.Create<CreateMultipleFeedbackCommand>();
        var service = MockService();
        commonFeedback.CheckFeedbackValidParams(criteria).Returns(new ValueTuple<bool, string>(false, ""));

        var commandResult = new CRM.Models.Common.CommandResult { Code = 0 };
        baseService.ExecuteNonQueryAsync(criteria).Returns(commandResult);

        // Act
        await service.Public_CheckValidArchivingParamsAndInsertFeedbackAsync(
            archivingAutoFbInfo,
            criteria
        );

        // Assert
        logService.Received(1).LogInformation(Arg.Any<string>());
        await feedbackTransactionService.EditArchivingDAAutoFeedback(1, CreateFeedbackJobStatus.Fail, Arg.Any<string>());
    }

    [Fact]
    public async Task CheckValidArchivingParamsAndInsertFeedbackAsync_Exception_ExpectedWriteLogAndUpdateStatus()
    {
        // Arrange
        var archivingAutoFbInfo = new ArchivingAutoFbInfo
        {
            LogFeedbackId = 1,
            HeadlineName = "1",
            HeadlineID = 1,
            AffectedId = "1",
            AffectedName = "1"
        };
        var exception = fixture.Create<Exception>();
        var criteria = fixture.Create<CreateMultipleFeedbackCommand>();
        var service = MockService();
        commonFeedback.CheckFeedbackValidParams(criteria).Returns(new ValueTuple<bool, string>(true, ""));

        baseService.When(x => x.ExecuteNonQueryAsync(criteria)).Throw(exception);

        // Act
        await service.Public_CheckValidArchivingParamsAndInsertFeedbackAsync(
            archivingAutoFbInfo,
            criteria
        );

        // Assert
        logService.Received(1).LogInformation(Arg.Any<string>());
        await feedbackTransactionService.EditArchivingDAAutoFeedback(1, CreateFeedbackJobStatus.Fail, exception.Message);
        await logService.ErrorAsync(exception);
    }

    #endregion CheckValidArchivingParamsAndInsertFeedbackAsync

    #region GetProductXmlNA

    [Fact]
    public void GetProductXmlNA_ReturnExactDefaultTemplate()
    {
        // Arrange
        var service = MockService();

        // Act
        var actual = service.Public_GetProductXmlNA();

        // Assert
        Assert.Equal("<ROOT><Product ProductID='46004' SubProductID='8063' BetTypeID='9144' /></ROOT>", actual);
    }

    #endregion GetProductXmlNA

    #region GetCategorySubCategoryArchivingDA

    [Fact]
    public void GetCategorySubCategoryArchivingDA_NotExistSettingNameMainCategory_ReturnDefaultMainCategoryXML()
    {
        // Arrange
        var settings = new List<SettingItem>
        {
            new() { SettingName = "MainCategory1", SettingValue = DefaultValue.Special_Notification }
        };
        var service = MockService();

        // Act
        var actual = service.Public_GetCategorySubCategoryArchivingDA(settings, DefaultValue.Special_Notification);

        // Assert
        Assert.Equal(0, actual.Item1);
        Assert.Equal(0, actual.Item2);
        Assert.Equal("<ROOT><Category MainCategoryID='0' SubCategoryID='0' /></ROOT>", actual.Item3);
    }

    [Fact]
    public void GetCategorySubCategoryArchivingDA_ExistSettingNameMainCategory_ReturnXML()
    {
        // Arrange
        var settings = new List<SettingItem>
        {
            new() { SettingId = 1, SettingName = "MainCategory", SettingValue = DefaultValue.Special_Notification },
            new() { SettingId = 2, SettingName = "SubCategory", SettingValue = "Archive Dormant Accounts" }
        };
        var service = MockService();

        // Act
        var actual = service.Public_GetCategorySubCategoryArchivingDA(settings, DefaultValue.Special_Notification);

        // Assert
        Assert.Equal(1, actual.Item1);
        Assert.Equal(2, actual.Item2);
        Assert.Equal("<ROOT><Category MainCategoryID='1' SubCategoryID='2' /></ROOT>", actual.Item3);
    }

    #endregion GetCategorySubCategoryArchivingDA

    #region UpdateArchivingStatus

    [Fact]
    public async Task UpdateArchivingStatus_IsSyncArchivedAccount_ExpectedCallExactFunction()
    {
        // Arrange
        var service = MockService();

        // Act
        await service.Public_UpdateArchivingStatus(true, "lastRuntimeNameEnv", "jobStatusNameEnv");

        // Assert
        await commonFeedback.EditArchivingDAStatusSettingAsync(BackgroundJobConstants.ServiceName_ArchivingDA, "0");
    }

    #endregion UpdateArchivingStatus

    #region Helpers

    private ArchivingDAServiceTestable MockService()
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
        feedbackTransactionService = Substitute.For<IFeedbackTransactionService>();
        commonFeedback = Substitute.For<ICommonFeedback>();

        return Substitute.For<ArchivingDAServiceTestable>(
            appJobSettings,
            baseService,
            settingService,
            logService,
            feedbackTransactionService,
            commonFeedback
        );
    }

    #endregion Helpers
}