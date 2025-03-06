namespace CRM.UnitTests.Criteria.PushResultAmendment
{
    using CRM.Criteria.PushResultAmendment;
    using Xunit;

    public class GetMessageResultAmendmentCommandTests
    {
        [Fact]
        public void GetSettingKey_ReturnCorrectSettingKey()
        {
            // Arrange
            var command = new GetMessageResultAmendmentCommand();
            var expected = "MNC_PushNotification_MsgIns";

            // Action
            var actual = command.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_ParamsAreValid_ReturnTrue()
        {
            // Arrange
            var command = new GetMessageResultAmendmentCommand();

            // Act
            var actual = command.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
