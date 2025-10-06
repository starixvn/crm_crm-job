namespace CRM.UnitTests.Services.AutoFeedback;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using CRM.Constants;
using CRM.Criteria.AutoFeedback;
using CRM.Criteria.Common;
using CRM.Criteria.TurnoverControl;
using CRM.Criteria.WinLossLimit;
using CRM.Models.Common;
using CRM.Models.TurnoverControl;
using CRM.Models.WinLossLimit;
using CRM.Services.AutoFeedback;
using CRM.Services.Common;
using Fanex.Data.Repository;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit;

public class CustomerServiceTests
{
    private IDynamicRepository dynamicRepository;
    private IBaseService baseService;
    private IConfiguration configuration;

    private readonly IFixture fixture = new Fixture();

    [Fact]
    public async Task GetCustomerInfoAsync_WhenCalled_ReturnExpectedCustomer()
    {
        // Arrange
        var expected = fixture.Create<(IEnumerable<CustomerInfo>, IEnumerable<int>)>();
        var username = fixture.Create<string>();
        var isGuest = fixture.Create<bool>();
        var criteria = new GetCustomerInfoByUsernameCriteria();
        var service = GetService();
        dynamicRepository.FetchMultipleAsync<CustomerInfo, int>(Arg.Do<GetCustomerInfoByUsernameCriteria>(x => criteria = x)).Returns(expected);

        // Act
        var actual = await service.GetCustomerInfoAsync(username, isGuest);

        // Assert
        Assert.Equal(expected.Item1.FirstOrDefault(), actual);
        Assert.Equal(username, criteria.Username);
        Assert.Equal(isGuest, criteria.IsGuest);
        Assert.Equal(BackgroundJobConstants.UserRole, criteria.UserRole);
    }

    [Fact]
    public async Task GetGroupHeadlineAsync_WhenCalled_ReturnExpectedHeadlines()
    {
        // Arrange
        var expected = fixture.Create<IEnumerable<GroupHeadline>>();
        var custIds = fixture.Create<IEnumerable<int>>();
        var criteria = new GetGroupHeadlineCriteria();
        var service = GetService();
        baseService.GetDataAsync<GroupHeadline>(Arg.Do<GetGroupHeadlineCriteria>(x => criteria = x)).Returns(expected);

        // Act
        var actual = await service.GetGroupHeadlineAsync(custIds);

        // Assert
        Assert.Equal(expected, actual);
        Assert.Equal(string.Join(",", custIds), criteria.CustIds);
    }

    [Fact]
    public async Task GetListSMANotificationAsync_NotExistEnvironment_ReturnDefault()
    {
        // Arrange
        var expected = fixture.Create<IEnumerable<TONotification>>();
        var criteria = new GetTOControlCustNotificationSelCriteria();
        var setting = fixture.Create<WinLossSystemSetting>();
        var service = GetService();
        baseService.GetDataAsync<TONotification>(Arg.Do<GetTOControlCustNotificationSelCriteria>(x => criteria = x)).Returns(expected);

        // Act
        var actual = await service.GetListSMANotificationAsync(setting);

        // Assert
        Assert.Equal(expected, actual);
        Assert.Equal(setting.BatchSize, criteria.BatchSize);
        Assert.Equal(setting.ConfigUAT, criteria.ConfigUAT);
        Assert.Equal(setting.ConfigPRO, criteria.ConfigPRO);
        Assert.Equal(setting.LastLogId, criteria.LastLogID);
        Assert.Equal(setting.IsCredit, criteria.IsCredit);
        Assert.Equal(setting.IsDeposit, criteria.IsDeposit);
        Assert.Equal(BackgroundJobConstants.DefaultEnvironment, criteria.Environment);
    }

    [Fact]
    public async Task GetListSMANotificationAsync_ExistEnvironment_ReturnExpectedEnvironment()
    {
        // Arrange
        var expected = fixture.Create<IEnumerable<TONotification>>();
        var criteria = new GetTOControlCustNotificationSelCriteria();
        var setting = fixture.Create<WinLossSystemSetting>();
        var service = GetService();
        baseService.GetDataAsync<TONotification>(Arg.Do<GetTOControlCustNotificationSelCriteria>(x => criteria = x)).Returns(expected);
        configuration.GetValue<string>("SiteEnvironment").Returns("LIVE");
        // Act
        var actual = await service.GetListSMANotificationAsync(setting);

        // Assert
        Assert.Equal(expected, actual);
        Assert.Equal(setting.BatchSize, criteria.BatchSize);
        Assert.Equal(setting.ConfigUAT, criteria.ConfigUAT);
        Assert.Equal(setting.ConfigPRO, criteria.ConfigPRO);
        Assert.Equal(setting.LastLogId, criteria.LastLogID);
        Assert.Equal(setting.IsCredit, criteria.IsCredit);
        Assert.Equal(setting.IsDeposit, criteria.IsDeposit);
    }

    [Fact]
    public async Task GetListCustReachLimitAsync_WhenCalled_ReturnExpectedLimitInfo()
    {
        // Arrange
        var expected = fixture.Create<IEnumerable<CustomerWinLimit>>();
        var criteria = new GetListCustomerReachLimitCriteria();
        var setting = fixture.Create<WinLossSystemSetting>();
        var service = GetService();
        baseService.GetDataAsync<CustomerWinLimit>(Arg.Do<GetListCustomerReachLimitCriteria>(x => criteria = x)).Returns(expected);

        // Act
        var actual = await service.GetListCustReachLimitAsync(setting, true);

        // Assert
        Assert.Equal(expected, actual);
        Assert.Equal(setting.BatchSize, criteria.BatchSize);
        Assert.Equal(setting.ConfigUAT, criteria.ConfigUAT);
        Assert.Equal(setting.ConfigPRO, criteria.ConfigPRO);
        Assert.Equal(setting.LastLogId, criteria.LastLogID);
        Assert.Equal(setting.IsCredit, criteria.IsCredit);
        Assert.Equal(setting.IsDeposit, criteria.IsDeposit);
        Assert.Equal(BackgroundJobConstants.DefaultEnvironment, criteria.Environment);
    }

    [Fact]
    public async Task GetListCustOutStandingAsync_WhenCalled_ReturnExpectedOutStanding()
    {
        // Arrange
        var expected = fixture.Create<IEnumerable<CustOutStanding>>();
        var custNotifications = fixture.Create<IEnumerable<TONotification>>();
        var service = GetService();
        baseService.GetDataAsync<CustOutStanding>(Arg.Do<GetTOControlCustOutstandingCriteria>(x => _ = x)).Returns(expected);

        // Act
        var actual = await service.GetListCustOutStandingAsync(custNotifications);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetListCustReachLimitAsync_WhenIsLiveTrue_AssignCorrectLastLogID()
    {
        // Arrange
        var expected = fixture.Create<IEnumerable<CustomerWinLimit>>();
        var criteria = new GetListCustomerReachLimitCriteria
        {
            LastLogID = 101
        };
        var setting = fixture.Create<WinLossSystemSetting>();
        setting.LastLogId = 101;
        var service = GetService();
        baseService.GetDataAsync<CustomerWinLimit>(Arg.Do<GetListCustomerReachLimitCriteria>(x => criteria = x)).Returns(expected);

        // Act
        var actual = await service.GetListCustReachLimitAsync(setting, true);

        // Assert
        Assert.Equal(setting.LastLogId, criteria.LastLogID);
    }

    [Fact]
    public async Task GetListCustReachLimitAsync_WhenIsLiveFalse_AssignCorrectLastLogID()
    {
        // Arrange
        var expected = fixture.Create<IEnumerable<CustomerWinLimit>>();
        var criteria = new GetListCustomerReachLimitCriteria
        {
            LastLogID = 1002
        };
        var setting = fixture.Create<WinLossSystemSetting>();
        setting.LastLogIdUAT = 1002;
        var service = GetService();
        baseService.GetDataAsync<CustomerWinLimit>(Arg.Do<GetListCustomerReachLimitCriteria>(x => criteria = x)).Returns(expected);

        // Act
        var actual = await service.GetListCustReachLimitAsync(setting, false);

        // Assert
        Assert.Equal(setting.LastLogIdUAT, criteria.LastLogID);
    }

    #region Helpers

    private CustomerService GetService()
    {
        dynamicRepository = Substitute.For<IDynamicRepository>();
        baseService = Substitute.For<IBaseService>();
        configuration = Substitute.For<IConfiguration>();

        return new CustomerService(dynamicRepository, baseService, configuration);
    }

    #endregion Helpers
}