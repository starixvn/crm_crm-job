namespace CRM.BackgroundJob.UnitTests.Criteria.AbnormalBet
{
    using CRM.BackgroundJob.Criteria.AbnormalBet;
    using Xunit;

    public class GetAbnormalBetMessageCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetAbnormalBetMessageCriteria();
            var expected = "Acc_CRM_AutoFB_AbnormalBets_GetMessage";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_ReturnTrue()
        {
            // Arrange
            var criteria = new GetAbnormalBetMessageCriteria();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
