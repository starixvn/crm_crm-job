namespace CRM.UnitTests.Criteria.ChatBotResultAmendment
{
    using CRM.Criteria.ChatBot_ResultAmendment;
    using Xunit;

    public class ImportMessageVoidSettledCommandTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new ImportMessageVoidSettledCommand();
            var expected = "Acc_CRM_VodingMessage_Ins";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_ReturnTrue()
        {
            // Arrange
            var criteria = new ImportMessageVoidSettledCommand
            {
                Message = "Message"
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

        public void IsValid_ParamIsInvalid_ReturnFalse(string message)
        {
            // Arrange
            var criteria = new ImportMessageVoidSettledCommand
            {
                Message = message
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.False(actual);
        }
    }
}
