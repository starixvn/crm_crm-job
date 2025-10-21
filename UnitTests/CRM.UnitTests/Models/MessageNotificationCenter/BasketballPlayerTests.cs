namespace CRM.UnitTests.Services.NotificationCenter;

using System;
using System.Text.Json;
using System.Threading.Tasks;
using CRM.Constants;
using CRM.Models.MessageNotificationCenter;
using Xunit;

public class BasketballPlayerTests
{
    [Fact]
    public void BasketballPlayer_675_ReturnCorrectBetType()
    {
        // Arrange
        var oddsInfoJson = new[]
        {
            "{\"s\": [{\"t\": \"points\", \"value\": \"25+\"}], \"r\": [{\"t\": \"winner\", \"value\": \"1111\"}], \"pid\": 2111, \"pname\": \"Lebron\", \"isH\": true, \"tid\":1111 }",
            "{\"s\": [{\"t\": \"points\", \"value\": \"30+\"}], \"r\": [{\"t\": \"over\", \"value\": \"85.5\"}], \"pid\": 2111, \"pname\": \"Lebron\", \"isH\": true, \"tid\":1111 }"
        };
        var expected = new[]
        {
            "Lebron James have 25+ points and WAS to Win",
            "Lebron James have 30+ points and WAS Total Points Over 85.5"
        };
        Func<string, string, string> getTeamNameHandler = (id, lang) =>
        {
            if (id == "2111") return "Lebron James";
            if (id == "1111") return "WAS";
            return "Unknown";
        };

        // Act
        for (var i = 0; i < oddsInfoJson.Length; i++)
        {
            var model = JsonSerializer.Deserialize<BasketballPlayer>(oddsInfoJson[i]);
            var actual = model.BuildBetTeam(getTeamNameHandler, Language.en_US, Resources.Resources.ResourceManager);

            // Assert
            Assert.Equal(expected[i], actual);
        }
    }
}