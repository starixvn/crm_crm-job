namespace CRM.BackgroundJob.UnitTests.Criteria.AutoFeedback
{
    using AutoFixture;
    using Fanex.CRM.BackgroundJob.Criteria;
    using Xunit;

    public class GetMessageInfoCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetMessageInfoCriteria();
            var expected = "Acc_CRM_MessageAbnormalTicketsSel";

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
            var criteria = new GetMessageInfoCriteria
            {
                MessageIds = messageIds
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
            var criteria = fixture.Create<GetMessageInfoCriteria>();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}