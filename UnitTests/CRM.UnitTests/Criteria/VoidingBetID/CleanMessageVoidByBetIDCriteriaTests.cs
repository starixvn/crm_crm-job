namespace CRM.UnitTests.Criteria.VoidingBetID
{
    using CRM.Criteria.VoidingBetID;
    using Xunit;

    public class CleanMessageVoidByBetIDCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new CleanMessageVoidByBetIDCriteria();
            var expected = "Acc_CRM_AutoFB_VoidByBetID_CleanMessage";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_ReturnTrue()
        {
            // Arrange
            var criteria = new CleanMessageVoidByBetIDCriteria();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
