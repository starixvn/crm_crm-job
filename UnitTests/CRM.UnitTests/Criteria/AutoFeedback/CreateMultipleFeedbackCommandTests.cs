namespace CRM.UnitTests.Criteria.AutoFeedback
{
    using AutoFixture;
    using CRM.Criteria.AutoFeedback;
    using Xunit;

    public class CreateMultipleFeedbackCommandTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new CreateMultipleFeedbackCommand();
            var expected = "Acc_CRM_NewAutoFeedback_GuestFeedbackIns_Multiple";

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
            var criteria = fixture.Create<CreateMultipleFeedbackCommand>();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}