namespace CRM.BackgroundJob.UnitTests.Criteria.VoidingSettledTickets
{
    using CRM.BackgroundJob.Criteria.VoidingSettledTickets;
    using Xunit;

    public class GetMesssagesVoidingSettledCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetMesssagesVoidingSettledCriteria();
            var expected = "Acc_CRM_VoidingMsg_GetMessage";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_ReturnTrue()
        {
            // Arrange
            var criteria = new GetMesssagesVoidingSettledCriteria();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
