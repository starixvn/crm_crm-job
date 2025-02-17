namespace CRM.BackgroundJob.UnitTests.Criteria
{
    using Fanex.CRM.BackgroundJob.Criteria;
    using Xunit;

    public class GetCustomerInfoByUsernameCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetCustomerInfoByUsernameCriteria();
            var expected = "Acc_CRM_CustomerSearch";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_ReturnTrue()
        {
            // Arrange
            var criteria = new GetCustomerInfoByUsernameCriteria();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}