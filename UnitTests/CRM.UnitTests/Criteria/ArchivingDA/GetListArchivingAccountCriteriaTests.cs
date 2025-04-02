namespace CRM.UnitTests.Criteria.ArchivingDA;

using CRM.Criteria.ArchivingDA;
using Xunit;

public class GetListArchivingAccountCriteriaTests
{
    [Fact]
    public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
    {
        // Arrange
        var criteria = new GetListArchivingAccountCriteria();
        var expected = "Acc_CRM_ArchivedAccount_GetListAccount";

        // Act
        var actual = criteria.GetSettingKey();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsValid_WhenCalled_ReturnTrue()
    {
        // Arrange
        var criteria = new GetListArchivingAccountCriteria();

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.True(actual);
    }
}