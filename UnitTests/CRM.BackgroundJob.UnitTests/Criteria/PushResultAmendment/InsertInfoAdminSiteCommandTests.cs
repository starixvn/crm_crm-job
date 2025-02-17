namespace CRM.BackgroundJob.UnitTests.Criteria.PushResultAmendment
{
    using CRM.BackgroundJob.Criteria.PushResultAmendment;
    using Xunit;

    public class InsertInfoAdminSiteCommandTests
    {
        [Fact]
        public void GetSettingKey_ReturnCorrectSettingKey()
        {
            // Arrange
            var command = new InsertInfoAdminSiteCommand();
            var expected = "Dot_Admin_message_new_insert";

            // Action
            var actual = command.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_ParamsAreValid_ReturnTrue()
        {
            // Arrange
            var command = new InsertInfoAdminSiteCommand();

            // Act
            var actual = command.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
