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
            "{\"s\": [{\"t\": \"points\", \"value\": \"30+\"}], \"r\": [{\"t\": \"over\", \"value\": \"85.5\"}], \"pid\": 2111, \"pname\": \"Lebron\", \"isH\": true, \"tid\":1111 }",
            "{\"s\": [{\"t\": \"points\", \"value\": \"30+\"}], \"r\": [{\"t\": \"handicap\", \"value\": \"-10.5\"}], \"pid\": 2111, \"pname\": \"Lebron\", \"isH\": true, \"tid\":1111 }",
            "{\"s\": [{\"t\": \"points\", \"value\": \"30+\"}], \"r\": [{\"t\": \"winner\", \"value\": \"1111\"}, {\"t\": \"over\", \"value\": \"85.5\"}], \"pid\": 2111, \"pname\": \"Lebron\", \"isH\": true, \"tid\":1111 }",
            "{\"s\": [{\"t\": \"three_points\", \"value\": \"5+\"}], \"r\": [{\"t\": \"winner\", \"value\": \"1111\"}, {\"t\": \"over\", \"value\": \"85.5\"}], \"pid\": 2111, \"pname\": \"Lebron\", \"isH\": true, \"tid\":1111 }",
            "{\"s\": [{\"t\": \"assists\", \"value\": \"4+\"}], \"r\": [{\"t\": \"winner\", \"value\": \"1111\"}], \"pid\": 2111, \"pname\": \"Lebron\", \"isH\": true, \"tid\":1111 }",
            "{\"s\": [{\"t\": \"rebounds\", \"value\": \"9+\"}], \"r\": [{\"t\": \"winner\", \"value\": \"1111\"}], \"pid\": 2111, \"pname\": \"Lebron\", \"isH\": true, \"tid\":1111 }"
        };
        var expected = new[]
        {
            "Lebron James have 25+ points and WAS to Win",
            "Lebron James have 30+ points and WAS Total Points Over 85.5",
            "Lebron James have 30+ points and WAS to Win with Handicap -10.5",
            "Lebron James have 30+ points, WAS to Win and WAS Total Points Over 85.5",
            "Lebron James have 5+ three point field goals, WAS to Win and WAS Total Points Over 85.5",
            "Lebron James have 4+ assists and WAS to Win",
            "Lebron James have 9+ total rebounds and WAS to Win"
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

    [Fact]
    public void BasketballPlayer_676_ReturnCorrectBetType()
    {
        // Arrange
        var oddsInfoJson = new[]
        {
            "{\"s\": [{\"t\": \"points\", \"value\": \"10+\"}, {\"t\": \"assists\", \"value\": \"4+\"}], \"pid\": 2111, \"pname\": \"Lebron\", \"isH\": true, \"tid\":1111 }",
            "{\"s\": [{\"t\": \"points\", \"value\": \"10+\"}, {\"t\": \"assists\", \"value\": \"4+\"}], \"r\": [{\"t\": \"winner\", \"value\": \"1111\"}], \"pid\": 2111, \"pname\": \"Lebron\", \"isH\": true, \"tid\":1111 }",
            "{\"s\": [{\"t\": \"points\", \"value\": \"10+\"}, {\"t\": \"assists\", \"value\": \"4+\"}], \"r\": [{\"t\": \"handicap\", \"value\": \"-10.5\"}], \"pid\": 2111, \"pname\": \"Lebron\", \"isH\": true, \"tid\":1111 }",
            "{\"s\": [{\"t\": \"points\", \"value\": \"10+\"}, {\"t\": \"rebounds\", \"value\": \"4+\"}], \"pid\": 2111, \"pname\": \"Lebron\", \"isH\": true, \"tid\":1111 }",
            "{\"s\": [{\"t\": \"points\", \"value\": \"10+\"}, {\"t\": \"rebounds\", \"value\": \"4+\"}], \"r\": [{\"t\": \"winner\", \"value\": \"1111\"}], \"pid\": 2111, \"pname\": \"Lebron\", \"isH\": true, \"tid\":1111 }",
            "{\"s\": [{\"t\": \"points\", \"value\": \"10+\"}, {\"t\": \"rebounds\", \"value\": \"4+\"}], \"r\": [{\"t\": \"handicap\", \"value\": \"-10.5\"}], \"pid\": 2111, \"pname\": \"Lebron\", \"isH\": true, \"tid\":1111 }"
        };
        var expected = new[]
        {
            "Lebron James have 10+ points and 4+ assists",
            "Lebron James have 10+ points, 4+ assists and WAS to Win",
            "Lebron James have 10+ points, 4+ assists and WAS to Win with Handicap -10.5",
            "Lebron James have 10+ points and 4+ total rebounds",
            "Lebron James have 10+ points, 4+ total rebounds and WAS to Win",
            "Lebron James have 10+ points, 4+ total rebounds and WAS to Win with Handicap -10.5"
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

    [Fact]
    public void BasketballPlayer_678_ReturnCorrectBetType()
    {
        // Arrange
        var oddsInfoJson = new[]
        {
            "{\"s\": [{\"t\": \"points\", \"value\": \"10+\"}, {\"t\": \"assists\", \"value\": \"4+\"}, {\"t\": \"rebounds\", \"value\": \"5+\"}], \"pid\": 2111, \"pname\": \"Lebron\", \"isH\": true, \"tid\":1111 }",
            "{\"s\": [{\"t\": \"points\", \"value\": \"10+\"}, {\"t\": \"assists\", \"value\": \"4+\"}, {\"t\": \"rebounds\", \"value\": \"5+\"}], \"r\": [{\"t\": \"winner\", \"value\": \"1111\"}], \"pid\": 2111, \"pname\": \"Lebron\", \"isH\": true, \"tid\":1111 }",
            "{\"s\": [{\"t\": \"points\", \"value\": \"10+\"}, {\"t\": \"assists\", \"value\": \"4+\"}, {\"t\": \"rebounds\", \"value\": \"5+\"}], \"r\": [{\"t\": \"handicap\", \"value\": \"-10.5\"}], \"pid\": 2111, \"pname\": \"Lebron\", \"isH\": true, \"tid\":1111 }"
        };
        var expected = new[]
        {
            "Lebron James have 10+ points, 4+ assists and 5+ total rebounds",
            "Lebron James have 10+ points, 4+ assists, 5+ total rebounds and WAS to Win",
            "Lebron James have 10+ points, 4+ assists, 5+ total rebounds and WAS to Win with Handicap -10.5"
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