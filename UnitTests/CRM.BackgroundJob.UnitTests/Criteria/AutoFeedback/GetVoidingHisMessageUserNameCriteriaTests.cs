namespace CRM.BackgroundJob.UnitTests.Criteria.AutoFeedback
{
    using AutoFixture;
    using CRM.BackgroundJob.Criteria.AutoFeedback;
    using Xunit;

    public class GetVoidingHisMessageUserNameCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetVoidingHisMessageUserNameCriteria();
            var expected = "Acc_CRM_AbnormalAccount_GetUsernameList";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void IsValid_ParamIsInvalid_ReturnFalse(string messages)
        {
            // Arrange
            var criteria = new GetVoidingHisMessageUserNameCriteria
            {
                Messages = messages
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
            var fixture = new Fixture();
            var criteria = fixture.Create<GetVoidingHisMessageUserNameCriteria>();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}