namespace CRM.UnitTests.Criteria.VoidingSettledTickets
{
    using CRM.Criteria.VoidingSettledTickets;
    using Xunit;

    public class GetVoidingSettledTicketsSettingCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetVoidingSettledTicketsSettingCriteria();
            var expected = "Acc_CRM_Voiding_SystemSettingSel";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_ReturnTrue()
        {
            // Arrange
            var criteria = new GetVoidingSettledTicketsSettingCriteria();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
