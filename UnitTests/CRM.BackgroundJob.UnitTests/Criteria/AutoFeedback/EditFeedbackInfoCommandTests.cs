namespace CRM.BackgroundJob.UnitTests.Criteria.AutoFeedback
{
    using AutoFixture;
    using Fanex.CRM.BackgroundJob.Criteria;
    using Xunit;

    public class EditFeedbackInfoCommandTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new EditFeedbackInfoCommand();
            var expected = "Acc_CRM_AutoFeedbackInfoUpd";

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
            var criteria = fixture.Create<EditFeedbackInfoCommand>();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}