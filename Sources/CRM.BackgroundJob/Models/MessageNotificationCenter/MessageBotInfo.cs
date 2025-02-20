namespace CRM.BackgroundJob.Models.MessageNotificationCenter;

public class MessageBotInfo
{
    public string ChatGroupID { get; set; }

    public byte IsTest { get; set; }

    public string Content { get; set; }
}