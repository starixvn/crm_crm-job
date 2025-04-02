namespace CRM.UnitTests.Criteria.ChatBotResultAmendment;

using System;
using CRM.Criteria.ChatBot_ResultAmendment;
using Xunit;

public class GetChatBotResultAmendmentCriteriaTests
{
    [Fact]
    public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
    {
        // Arrange
        var criteria = new GetChatBotResultAmendmentCriteria();
        var expected = "Acc_CRM_Chatbot_ResultAmendment_Sel";

        // Act
        var actual = criteria.GetSettingKey();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsValid_ParamIsValid_ReturnTrue()
    {
        // Arrange
        var criteria = new GetChatBotResultAmendmentCriteria
        {
            BatchSize = 3000,
            SubmitDate = DateTime.Now.ToString("yyyy-MM-dd H:mm:ss.fffffff"),
            Seq = 1
        };

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.True(actual);
    }
}