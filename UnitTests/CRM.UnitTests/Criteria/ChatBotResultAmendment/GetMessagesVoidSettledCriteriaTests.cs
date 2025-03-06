namespace CRM.UnitTests.Criteria.ChatBotResultAmendment
{
    using CRM.Criteria.ChatBot_ResultAmendment;
    using Xunit;

    public class GetMessagesVoidSettledCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetMessagesVoidSettledCriteria();
            var expected = "Acc_CRM_ChatBot_VoidingMsg_GetMessage";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_ParamIsValid_ReturnTrue()
        {
            // Arrange
            var criteria = new GetMessagesVoidSettledCriteria();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}