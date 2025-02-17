namespace CRM.BackgroundJob.UnitTests.Criteria.PushResultAmendment
{
    using CRM.BackgroundJob.Criteria.PushResultAmendment;
    using Xunit;

    public class EditMessageResultAmendmentCriteriaTests
    {
        [Fact]
        public void GetSettingKey_ReturnCorrectSettingKey()
        {
            // Arrange
            var command = new EditMessageResultAmendmentCriteria();
            var expected = "MNC_PushNotification_MsgUpd";

            // Action
            var actual = command.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_ParamsAreValid_ReturnTrue()
        {
            // Arrange
            var command = new EditMessageResultAmendmentCriteria();

            // Act
            var actual = command.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
