using CRM.BackgroundJob.Criteria.TurnoverControll;
using Xunit;

namespace CRM.BackgroundJob.UnitTests.Criteria.TurnoverControll
{
    public class GetTOControlCustNotificationSelCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetTOControlCustNotificationSelCriteria();
            var expected = "Acc_CRM_TOControl_CustNotificationSel";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_ReturnTrue( )
        {
            // Arrange
            var criteria = new GetTOControlCustNotificationSelCriteria
            {
                BatchSize = 10
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
            var criteria = new GetTOControlCustNotificationSelCriteria
            {
                BatchSize = 0
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.False(actual);
        }
    }
}
