namespace CRM.UnitTests.Criteria.PushResultAmendment
{
    using CRM.Criteria.PushResultAmendment;
    using Xunit;

    public class InsertCompleteResultAmendmentCommandTests
    {
        [Fact]
        public void GetSettingKey_ReturnCorrectSettingKey()
        {
            // Arrange
            var criteria = new InsertCompleteResultAmendmentCommand();
            var expected = "MNC_PushNotification_Complete";

            // Action
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_ParamsAreValid_ReturnTrue()
        {
            // Arrange
            var criteria = new InsertCompleteResultAmendmentCommand();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
