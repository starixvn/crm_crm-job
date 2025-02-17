namespace CRM.BackgroundJob.UnitTests.Criteria.WinLossLimit
{
    using AutoFixture;
    using CRM.BackgroundJob.Criteria.WinLossLimit;
    using Xunit;
    public class EditFeedbackWinLossInfoCommandTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new EditFeedbackWinLossInfoCommand();
            var expected = "Acc_CRM_WinLimit_AutoFeedbackInfoUpd";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_ReturnTrue()
        {
            // Arrange
            var fixture = new Fixture();
            var criteria = fixture.Create<EditFeedbackWinLossInfoCommand>();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
