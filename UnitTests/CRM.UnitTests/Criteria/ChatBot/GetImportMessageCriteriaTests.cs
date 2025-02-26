namespace CRM.UnitTests.Criteria.ChatBot
{
    using CRM.Criteria.ChatBot;
    using Xunit;

    public class GetImportMessageCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetImportMessageCriteria();
            var expected = "Acc_CRM_ChatBot_crmMessageImport_Sel";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_ReturnTrue()
        {
            // Arrange
            var criteria = new GetImportMessageCriteria
            {
                BatchSize = 10
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}