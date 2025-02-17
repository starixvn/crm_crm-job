namespace CRM.BackgroundJob.UnitTests.Criteria.TurnoverControll
{
    using CRM.BackgroundJob.Criteria.TurnoverControll;
    using Xunit;

    public class GetTOControllCustOutstandingCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetTOControllCustOutstandingCriteria();
            var expected = "Acc_CRM_TOControl_CustOutstandingSel";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_ReturnTrue()
        {
            // Arrange
            var criteria = new GetTOControllCustOutstandingCriteria
            {
                xml = "test"
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public void IsValid_WhenCalled_ReturnFalse()
        {
            // Arrange
            var criteria = new GetTOControlCustNotificationSelCriteria();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.False(actual);
        }
    }
}
