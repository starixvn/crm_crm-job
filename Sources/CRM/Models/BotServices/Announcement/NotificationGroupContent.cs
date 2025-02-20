namespace CRM.Models.BotServices.Announcement;

using System.Collections.Generic;
using Enumerables;

public class NotificationGroupContent
{
    public int AnnouncementId { get; set; }
    public long BotId { get; set; }
    public ChatType ChatType { get; set; }
    public string Content { get; set; }
    public bool IsOnlyRetryImages { get; set; }
    public List<string> ConversationIds { get; set; }
    public List<string> Attachments { get; set; }
}