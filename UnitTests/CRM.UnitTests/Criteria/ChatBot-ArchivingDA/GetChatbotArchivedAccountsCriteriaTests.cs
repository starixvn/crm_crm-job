namespace CRM.UnitTests.Criteria.ChatBot_ArchivingDA;

using CRM.Criteria.ChatBot_ArchivingDA;
using Xunit;

public class GetChatbotArchivedAccountsCriteriaTests
{
    [Fact]
    public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
    {
        // Arrange
        var criteria = new GETChatbotArchivedAccountsCriteria();
        var expected = "Acc_CRM_Chatbot_ArchivedAccounts_Sel";

        // Act
        var actual = criteria.GetSettingKey();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsValid_ParamIsValid_ReturnTrue()
    {
        // Arrange
        var criteria = new GETChatbotArchivedAccountsCriteria
        {
            LastLogId = 1,
            BatchSize = 10
        };

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.True(actual);
    }
}