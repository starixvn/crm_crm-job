namespace CRM.BackgroundJob.UnitTests.Criteria.TurnoverControll
{
    using CRM.BackgroundJob.Criteria.TurnoverControll;
    using Xunit;

    public class GetTOControllSystemSettingCriteriaTest
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetTOControllSystemSettingCriteria();
            var expected = "Acc_CRM_TOControl_SystemSettingSel";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_ReturnTrue()
        {
            // Arrange
            var criteria = new GetTOControllSystemSettingCriteria();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
