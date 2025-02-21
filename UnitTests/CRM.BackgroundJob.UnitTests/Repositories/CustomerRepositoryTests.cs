namespace CRM.BackgroundJob.UnitTests.Repositories
{
    using System.Threading.Tasks;
    using AutoFixture;
    using CRM.BackgroundJob.Models;
    using Fanex.Data;
    using Fanex.Data.Repository;
    using NSubstitute;
    using Xunit;

    //public class CustomerRepositoryTests
    //{
    //    private readonly IObjectDb objectDb;
    //    private readonly IFixture fixture = new Fixture();

    //    public CustomerRepositoryTests()
    //    {
    //        DbSettingProviderManager
    //            .StartTestingSession()
    //            .Run();

    //        objectDb = Substitute.For<IObjectDb>();
    //        ObjectDbFactory.Mock((key, time) => objectDb);
    //    }

    //    [Fact]
    //    public async Task GetCustomerInfoByUsernameAsync_CustomerIsNotExist_ReturnNull()
    //    {
    //        // Arrange
    //        var repository = new CustomerRepository();
    //        var criteria = Substitute.For<ICriteria>();
    //        CustomerInfo customer = null;
    //        var multi = Substitute.For<IMultiResultReader>();
    //        multi.ReadFirstOrDefaultAsync<CustomerInfo>().Returns(customer);
    //        multi.ReadFirstAsync<int>().Returns(1);
    //        objectDb.QueryMultipleAsync(criteria).Returns(multi);

    //        // Act
    //        var actual = await repository.GetCustomerInfoByUsernameAsync(criteria);

    //        // Assert
    //        Assert.Null(actual);
    //    }

    //    [Fact]
    //    public async Task GetCustomerInfoByUsernameAsync_CustomerIsExist_ReturnExpectedCustomer()
    //    {
    //        // Arrange
    //        var repository = new CustomerRepository();
    //        var criteria = Substitute.For<ICriteria>();
    //        var customer = fixture.Create<CustomerInfo>();
    //        var multi = Substitute.For<IMultiResultReader>();
    //        multi.ReadFirstOrDefaultAsync<CustomerInfo>().Returns(customer);
    //        multi.ReadFirstAsync<int>().Returns(1);
    //        objectDb.QueryMultipleAsync(criteria).Returns(multi);

    //        // Act
    //        var actual = await repository.GetCustomerInfoByUsernameAsync(criteria);

    //        // Assert
    //        Assert.Equal(customer, actual);
    //    }
    //}
}