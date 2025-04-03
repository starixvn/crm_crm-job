namespace CRM.UnitTests.Criteria.GenerateResultAmendment;

using CRM.Criteria.GenerateResultAmendment;
using Xunit;

public class InsertGenResultAmendmentCommandTests
{
    [Fact]
    public void GetSettingKey_ReturnCorrectSettingKey()
    {
        // Arrange
        var command = new InsertGenResultAmendmentCommand();
        var expected = "MNC_ResultAmendment_GenNotification";

        // Action
        var actual = command.GetSettingKey();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsValid_ParamsAreValid_ReturnTrue()
    {
        // Arrange
        var command = new InsertGenResultAmendmentCommand();

        // Act
        var actual = command.IsValid();

        // Assert
        Assert.True(actual);
    }
}