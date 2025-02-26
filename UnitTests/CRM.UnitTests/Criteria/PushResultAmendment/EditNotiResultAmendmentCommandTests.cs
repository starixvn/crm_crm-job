namespace CRM.UnitTests.Criteria.PushResultAmendment
{
    using CRM.Criteria.PushResultAmendment;
    using Xunit;

    public class EditNotiResultAmendmentCommandTests
    {
        [Fact]
        public void GetSettingKey_ReturnCorrectSettingKey()
        {
            // Arrange
            var command = new EditNotiResultAmendmentCommand();
            var expected = "MNC_PushNotification_NotiUpd";

            // Action
            var actual = command.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_ParamsAreValid_ReturnTrue()
        {
            // Arrange
            var command = new EditNotiResultAmendmentCommand();

            // Act
            var actual = command.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
