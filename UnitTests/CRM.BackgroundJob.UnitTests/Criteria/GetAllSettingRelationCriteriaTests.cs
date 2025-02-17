namespace CRM.BackgroundJob.UnitTests.Criteria
{
    using Fanex.CRM.BackgroundJob.Criteria;
    using Xunit;

    public class GetAllSettingRelationCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetAllSettingRelationCriteria();
            var expected = "Acc_CRM_SettingRelationSel";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_ReturnTrue()
        {
            // Arrange
            var criteria = new GetAllSettingRelationCriteria();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}