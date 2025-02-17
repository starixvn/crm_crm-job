
namespace CRM.BackgroundJob.UnitTests.Criteria.SynchronizeLicensee
{

    using CRM.BackgroundJob.Criteria.SynchronizeLicensee;
    using Xunit;

    public class GetLicenseeStatusTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetLicenseeStatus();
            var expected = "Acc_CRM_OICLicenseeSel";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_ParamIsValid_ReturnTrue()
        {
            // Arrange
            var criteria = new GetLicenseeStatus();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
