namespace CRM.Models.BotServices;

using System;
using Extensions.Xml;

[XmlName("r")]
public class NotificationMessageUpdateResult
{
    public int NotificationId { get; set; }
    public int ConversationGroupId { get; set; }
    public DateTime SentDate { get; set; }
    public string Status { get; set; }
    public string ErrMsg { get; set; }
}