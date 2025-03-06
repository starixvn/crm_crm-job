namespace CRM.UnitTests.Criteria.WinLossLimit
{
    using CRM.Criteria.WinLossLimit;
    using Xunit;

    public class GetWinLossSystemSettingCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetWinLossSystemSettingCriteria();
            var expected = "Acc_CRM_WinLimit_SystemSettingSel";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_ReturnTrue()
        {
            // Arrange
            var criteria = new GetWinLossSystemSettingCriteria();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
