namespace CRM.BackgroundJob.UnitTests.Criteria.ChatBotCWL
{
    using CRM.BackgroundJob.Criteria.ChatBotCWL;
    using Xunit;
    public class ImportCWLLogInfoCommandTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new ImportCWLLogInfoCommand();
            var expected = "Acc_CRM_Chatbot_CWL_Import";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_ReturnTrue()
        {
            // Arrange
            var criteria = new ImportCWLLogInfoCommand
            {
                XmlData = "test",
                Environment = "LIVE",
                ErrCode = 0,
                ErrMsg = ""
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void IsValid_ParamIsInvalid_ReturnFalse(string XmlData)
        {
            // Arrange
            var criteria = new ImportCWLLogInfoCommand
            {
                XmlData = XmlData
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.False(actual);
        }
    }
}
