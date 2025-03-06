namespace CRM.Models.Common;

using System.Collections.Generic;

public class BotServiceSetting
{
    public int AppId { get; set; }
    public string ServiceUrl { get; set; }
    public List<ConversationConfig> ConversationConfigs { get; set; }
}

public class ConversationConfig
{
    public string DisplayName { get; set; }
    public string ConversationId { get; set; }
    public bool IsOnlySendTest { get; set; }
}