namespace CRM.UnitTests.Criteria.ArchivingDA
{
    using CRM.Criteria.ArchivingDA;
    using Xunit;

    public class GetSystemSettingByServiceNameCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetSystemSettingByServiceNameCriteria();
            var expected = "Acc_CRM_Service_SystemSettingSel";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_ReturnTrue()
        {
            // Arrange
            var criteria = new GetSystemSettingByServiceNameCriteria();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}