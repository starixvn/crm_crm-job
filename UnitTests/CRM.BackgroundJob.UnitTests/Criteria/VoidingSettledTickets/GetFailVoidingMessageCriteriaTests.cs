namespace CRM.BackgroundJob.UnitTests.Criteria.VoidingSettledTickets
{

    using CRM.BackgroundJob.Criteria.VoidingSettledTickets;
    using Xunit;

    public class GetFailVoidingMessageCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetFailVoidingMessageCriteria();
            var expected = "Acc_CRM_FailVoidingMessageSel";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_ReturnTrue()
        {
            // Arrange
            var criteria = new GetFailVoidingMessageCriteria();
           
            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
