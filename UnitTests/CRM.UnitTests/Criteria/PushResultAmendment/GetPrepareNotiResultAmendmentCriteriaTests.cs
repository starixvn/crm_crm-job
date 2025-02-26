namespace CRM.UnitTests.Criteria.PushResultAmendment
{
    using CRM.Criteria.PushResultAmendment;
    using Xunit;

    public class GetPrepareNotiResultAmendmentCriteriaTests
    {
        [Fact]
        public void GetSettingKey_ReturnCorrectSettingKey()
        {
            // Arrange
            var criteria = new GetPrepareNotiResultAmendmentCriteria();
            var expected = "MNC_PushNotification_PrepareNoti";

            // Action
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_ParamsAreValid_ReturnTrue()
        {
            // Arrange
            var criteria = new GetPrepareNotiResultAmendmentCriteria();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
