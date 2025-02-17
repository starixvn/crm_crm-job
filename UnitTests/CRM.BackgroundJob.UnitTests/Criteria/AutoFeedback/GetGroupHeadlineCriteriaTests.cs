namespace CRM.BackgroundJob.UnitTests.Criteria.AutoFeedback
{
    using AutoFixture;
    using Fanex.CRM.BackgroundJob.Criteria;
    using Xunit;

    public class GetGroupHeadlineCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetGroupHeadlineCriteria();
            var expected = "Acc_CRM_GroupHeadlineSel";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void IsValid_ParamIsInvalid_ReturnFalse(string custIds)
        {
            // Arrange
            var criteria = new GetGroupHeadlineCriteria
            {
                CustIds = custIds
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
            var criteria = fixture.Create<GetGroupHeadlineCriteria>();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}