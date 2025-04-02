namespace CRM.UnitTests.Criteria.ArchivingDA;

using CRM.Criteria.ArchivingDA;
using Xunit;

public class GetArchivingDAStatusCriteriaTests
{
    [Fact]
    public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
    {
        // Arrange
        var criteria = new GetArchivingDAStatusCriteria();
        var expected = "Acc_CRM_ArchivedAccount_CheckStatus";

        // Act
        var actual = criteria.GetSettingKey();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsValid_WhenCalled_ReturnTrue()
    {
        // Arrange
        var criteria = new GetArchivingDAStatusCriteria();

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.True(actual);
    }
}