namespace CRM.UnitTests.Criteria.SynchronizeLicensee
{
    using CRM.Criteria.SynchronizeLicensee;
    using Xunit;

    public class ImportLicenseeStatusCommandTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new ImportLicenseeStatusCommand();
            var expected = "Acc_CRM_OICLicenseeIns";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_ParamIsInvalid_ReturnTrue()
        {
            // Arrange
            var criteria = new ImportLicenseeStatusCommand
            {
                Licensees = "1"
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]

        public void IsValid_ParamIsInvalid_ReturnFalse(string xml)
        {
            // Arrange
            var criteria = new ImportLicenseeStatusCommand
            {
                Licensees = xml
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.False(actual);
        }
    }
}
