namespace CRM.UnitTests.Criteria.ChatBot_CTO
{
    using CRM.Criteria.ChatBot_CTO;
    using Xunit;

    public class GetTOControlSMAInfoCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetTOControlSMAInfoCriteria();
            var expected = "Acc_Chatbot_TOControlSMAInfo_Sel";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void IsValid_ParamIsInvalid_ReturnFalse(string XML)
        {
            // Arrange
            var criteria = new GetTOControlSMAInfoCriteria
            {
                XML = XML
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
            var criteria = new GetTOControlSMAInfoCriteria
            {
                XML = "test"
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}