namespace CRM.UnitTests.Criteria.AutoFbMessageNotificationCenter
{
    using CRM.Criteria.AutoFbMessageNotificationCenter;
    using Xunit;

    public class GetMsgInfoMessageNotificationCriteriaTests
    {
        [Fact]
        public void GetSettingKey_ReturnCorrectSettingKey()
        {
            // Arrange
            var criteria = new GetMsgInfoMessageNotificationCriteria();
            var expected = "Acc_CRM_AutoFB_MessageNotificationCenter_GetMsgInfo";

            // Action
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_ParamsAreValid_ReturnTrue()
        {
            // Arrange
            var criteria = new GetMsgInfoMessageNotificationCriteria();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
