namespace CRM.BackgroundJob.Core.Criteria.Announcement
{
    using CRM.BackgroundJob.Core.Models;

    public class UpdateAnnouncementNotificationStatusCommand : BaseNonQueryCommand
    {
        public string XmlData { get; set; }

        public override string GetSettingKey() => "Acc_CRM_ChatBot_Announcement_Notification_UpdStatus";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(XmlData);
    }
}