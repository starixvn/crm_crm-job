namespace CRM.BackgroundJob.UnitTests.Criteria.AutoFeedback
{
    using AutoFixture;
    using CRM.BackgroundJob.Constants;
    using CRM.BackgroundJob.Criteria.AutoFeedback;

    using Xunit;

    public class EditSystemSettingCommandTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new EditSystemSettingCommand();
            var expected = "Acc_CRM_SystemSettingByServiceUpd";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void IsValid_ParamIsInvalid_ReturnFalse(string settings)
        {
            // Arrange
            var criteria = new EditSystemSettingCommand
            {
                Settings = settings,
                ServiceName = BackgroundJobConstants.ServiceName_AutoFB
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void IsValid_ParamIsValid_ReturnTrue()
        {
            // Arrange
            var fixture = new Fixture();
            var criteria = fixture.Create<EditSystemSettingCommand>();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}