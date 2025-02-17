namespace CRM.BackgroundJob.UnitTests.Criteria.VoidingSettledTickets
{
    using CRM.BackgroundJob.Criteria.VoidingSettledTickets;
    using Xunit;
    public class VoidingAutoFeedbackInfoInsCommandTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new VoidingAutoFeedbackInfoInsCommand
            {
                Message = "Message",
                ErrCode = 0,
                ErrMsg = "ErrMsg",
                Environment = "LIVE"
            };
            var expected = "Acc_CRM_VoidingAutoFeedbackInfo_Ins";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_ReturnTrue()
        {
            // Arrange
            var criteria = new VoidingAutoFeedbackInfoInsCommand
            {
                Message = "Message",
                ErrCode = 0,
                ErrMsg = "ErrMsg",
                Environment = "LIVE"
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
        public void IsValid_ParamIsInvalid_ReturnFalse(string messsage)
        {
            // Arrange
            var criteria = new VoidingAutoFeedbackInfoInsCommand
            {
                Message = messsage
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.False(actual);
        }
    }
}
