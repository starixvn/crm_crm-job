namespace CRM.UnitTests.Criteria.ChatBot_CTO
{
    using CRM.Criteria.ChatBot_CTO;
    using Xunit;

    public class TOControlImportCommandTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new TOControlImportCommand();
            var expected = "Acc_CRM_Chatbot_TOControl_Import";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void IsValid_ParamIsInvalid_ReturnFalse(string XmlData)
        {
            // Arrange
            var criteria = new TOControlImportCommand
            {
                XmlData = XmlData
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void IsValid_ParamIsValid_ReturnTrue()
        {
            // Arrange
            var criteria = new TOControlImportCommand
            {
                XmlData = "test"
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
