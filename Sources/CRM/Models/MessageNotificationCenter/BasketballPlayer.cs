using System.Globalization;
using System.Resources;
using System.Text.Json.Serialization;

namespace CRM.Models.MessageNotificationCenter;

public class BasketballPlayer
{
    private IEnumerable<NameValueModel> allItems;

    [JsonPropertyName("s")]
    public NameValueModel[] Statistics { get; set; }

    [JsonPropertyName("r")]
    public NameValueModel[] Results { get; set; }

    [JsonPropertyName("pid")]
    public int PlayerId { get; set; }

    [JsonPropertyName("isH")]
    public bool IsHome { get; set; }

    [JsonPropertyName("tid")]
    public int TeamId { get; set; }

    public string BuildBetTeam(
        Func<string, string, string> getTeamNameHandler,
        string languageCode,
        ResourceManager rm)
    {
        if (allItems == null)
        {
            Statistics ??= Array.Empty<NameValueModel>();
            Results ??= Array.Empty<NameValueModel>();
            allItems = new List<NameValueModel>(Statistics.Concat(Results));
        }

        var playerName = getTeamNameHandler(PlayerId.ToString(), languageCode);
        var teamName = getTeamNameHandler(TeamId.ToString(), languageCode);
        var resourceKey = $"bet_performance_{string.Join("_and_", allItems.Select(i => i.Name))}";
        var betTeam = rm.GetString(resourceKey, new CultureInfo(languageCode));
        betTeam = string.IsNullOrWhiteSpace(betTeam) ? resourceKey : BuildBetTeam(betTeam, teamName);

        return $"{playerName} {betTeam}";
    }

    private string BuildBetTeam(string betTeam, string teamName)
    {
        var betTeamValue = betTeam.Replace("#winner#", teamName);

        foreach (var item in allItems)
        {
            betTeamValue = betTeamValue.Replace($"#{item.Name}#", item.Value);
        }

        return betTeamValue;
    }
}

public class NameValueModel
{
    [JsonPropertyName("t")]
    public string Name { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }
}