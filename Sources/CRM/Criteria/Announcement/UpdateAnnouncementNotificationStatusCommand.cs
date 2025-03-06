namespace CRM.Criteria.Announcement;

public class UpdateAnnouncementNotificationStatusCommand : CRM.Models.Common.BaseNonQueryCommand
{
    public string XmlData { get; set; }

    public override string GetSettingKey() => "Acc_CRM_ChatBot_Announcement_Notification_UpdStatus";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(XmlData);
}