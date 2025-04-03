namespace CRM.UnitTests.Criteria.ChatBot_ArchivingDA;

using CRM.Criteria.ChatBot_ArchivingDA;
using Xunit;

public class ImportArchivedAccountsCommandTests
{
    [Fact]
    public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
    {
        // Arrange
        var criteria = new ImportArchivedAccountsCommand();
        var expected = "Acc_CRM_Chatbot_ArchivedAccounts_Import";

        // Act
        var actual = criteria.GetSettingKey();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void IsValid_ParamIsInvalid_ReturnFalse(string XmlData)
    {
        // Arrange
        var criteria = new ImportArchivedAccountsCommand
        {
            XmlData = XmlData
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
        var criteria = new ImportArchivedAccountsCommand
        {
            XmlData = "test"
        };

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.True(actual);
    }
}