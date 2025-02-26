namespace CRM.UnitTests.Criteria.AutoFeedback
{
    using AutoFixture;
    using CRM.Constants;
    using CRM.Criteria.AutoFeedback;
    using Xunit;

    public class GetAbnormalTicketInfoCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetAbnormalTicketInfoCriteria();
            var expected = "Acc_CRM_AbnormalTicketsSel";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void IsValid_ParamIsInvalid_ReturnFalse(string messageIds)
        {
            // Arrange
            var criteria = new GetAbnormalTicketInfoCriteria
            {
                MessageIds = messageIds,
                ConfigUAT = true,
                ConfigPRO = false,
                Environment = BackgroundJobConstants.DefaultEnvironment
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
            var criteria = fixture.Create<GetAbnormalTicketInfoCriteria>();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}