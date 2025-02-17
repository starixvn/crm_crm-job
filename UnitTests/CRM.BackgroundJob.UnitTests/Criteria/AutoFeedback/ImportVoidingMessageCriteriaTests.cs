namespace CRM.BackgroundJob.UnitTests.Criteria.AutoFeedback
{
    using AutoFixture;
    using Fanex.CRM.BackgroundJob.Criteria;
    using Xunit;

    public class ImportVoidingMessageCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new ImportVoidingMessageCriteria();
            var expected = "Acc_CRM_VoidingMessageIns";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, 0)]
        [InlineData("", 1)]
        [InlineData(" ", 1)]
        public void IsValid_ParamIsInvalid_ReturnFalse(string messageIds, int batchSize)
        {
            // Arrange
            var criteria = new ImportVoidingMessageCriteria
            {
                MessageIds = messageIds,
                BatchSize = batchSize
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
            var criteria = fixture.Create<ImportVoidingMessageCriteria>();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}