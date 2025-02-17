namespace CRM.BackgroundJob.UnitTests.Criteria.ArchivingDA
{
    using AutoFixture;
    using CRM.BackgroundJob.Criteria.ArchivingDA;
    using Xunit;

    public class ArchivingAutoFeedbackUpdateCommandTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new ArchivingAutoFeedbackUpdateCommand();
            var expected = "Acc_CRM_ArchivedAccount_AutoFBInfoUpd";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, 1)]
        [InlineData("", 1)]
        [InlineData("Status", 0)]
        public void IsValid_ParamIsInvalid_ReturnFalse(string status, int logFeedbackId)
        {
            // Arrange
            var criteria = new ArchivingAutoFeedbackUpdateCommand
            {
                Status = status,
                LogFeedbackId = logFeedbackId
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

            var command = fixture.Create<ArchivingAutoFeedbackUpdateCommand>();

            // Act
            var actual = command.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}