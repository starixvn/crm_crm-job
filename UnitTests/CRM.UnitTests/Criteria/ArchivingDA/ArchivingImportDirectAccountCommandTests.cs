namespace CRM.UnitTests.Criteria.ArchivingDA
{
    using AutoFixture;
    using CRM.Criteria.ArchivingDA;
    using Xunit;

    public class ArchivingImportDirectAccountCommandTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new ArchivingImportDirectAccountCommand();
            var expected = "Acc_CRM_ArchivedAccount_SyncData";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void IsValid_ParamIsInvalid_ReturnFalse(string archivedList)
        {
            // Arrange
            var criteria = new ArchivingImportDirectAccountCommand
            {
                ArchivedList = archivedList
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

            var criteria = fixture.Create<ArchivingImportDirectAccountCommand>();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}