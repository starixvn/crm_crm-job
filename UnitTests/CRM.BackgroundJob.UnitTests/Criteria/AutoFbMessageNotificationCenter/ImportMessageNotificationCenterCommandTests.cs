namespace CRM.BackgroundJob.UnitTests.Criteria.AutoFbMessageNotificationCenter
{
    using CRM.BackgroundJob.Criteria.AutoFbMessageNotificationCenter;
    using Xunit;

    public class ImportMessageNotificationCenterCommandTests
    {
        [Fact]
        public void GetSettingKey_ReturnCorrectSettingKey()
        {
            // Arrange
            var command = new ImportMessageNotificationCenterCommand();
            var expected = "Acc_CRM_AutoFB_MessageNotificationCenter_Import";

            // Action
            var actual = command.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_ParamsAreValid_ReturnTrue()
        {
            // Arrange
            var command = new ImportMessageNotificationCenterCommand();

            // Act
            var actual = command.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
