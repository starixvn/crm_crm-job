namespace CRM.UnitTests.Criteria.WinLossLimit
{
    using AutoFixture;
    using CRM.Criteria.WinLossLimit;
    using Xunit;

    public class ImportWinLossFeedbackInfoCriteriaTest
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new ImportWinLossFeedbackInfoCriteria();
            var expected = "Acc_CRM_WinLimit_AutoFeedbackInfoIns";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_ReturnTrue()
        {
            // Arrange
            var fixture = new Fixture();
            var criteria = fixture.Create<ImportWinLossFeedbackInfoCriteria>();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
