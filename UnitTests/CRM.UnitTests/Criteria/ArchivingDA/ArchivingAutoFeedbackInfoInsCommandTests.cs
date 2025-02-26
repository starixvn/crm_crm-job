namespace CRM.UnitTests.Criteria.ArchivingDA
{
    using AutoFixture;
    using CRM.Criteria.ArchivingDA;
    using Xunit;

    public class ArchivingAutoFeedbackInfoInsCommandTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new ArchivingAutoFeedbackInfoInsCommand();
            var expected = "Acc_CRM_ArchivedAccount_AutoFBInfoIns";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_ParamIsValid_ReturnTrue()
        {
            // Arrange
            var fixture = new Fixture();

            var command = fixture.Create<ArchivingAutoFeedbackInfoInsCommand>();

            // Act
            var actual = command.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}