namespace CRM.UnitTests.Criteria.CaptureSettlementData
{
    using CRM.Criteria.CaptureSettlementData;
    using Xunit;

    public class InsertDataLoadCommandTests
    {
        [Fact]
        public void GetSettingKey_ReturnCorrectSettingKey()
        {
            // Arrange
            var command = new InsertDataLoadCommand();
            var expected = "MNC_SettlResultHist_DataLoad";

            // Action
            var actual = command.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_ParamsAreValid_ReturnTrue()
        {
            // Arrange
            var command = new InsertDataLoadCommand();

            // Act
            var actual = command.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
