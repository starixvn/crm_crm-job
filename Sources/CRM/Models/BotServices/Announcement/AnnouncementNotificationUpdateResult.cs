namespace CRM.Models.BotServices.Announcement;

using Extensions.Xml;

[XmlName("r")]
public class AnnouncementNotificationUpdateResult
{
    public int NotificationId { get; set; }
    public int ConversationGroupId { get; set; }
    public string Status { get; set; }
    public string ErrMsg { get; set; }
    public bool IsOnlyRetryImage { get; set; }
    public string ErrImages { get; set; }
}