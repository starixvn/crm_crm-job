namespace CRM.Criteria.Announcement;

using Fanex.Data.Repository;

public class GetAnnouncementNotificationToSendCriteria : CriteriaBase
{
    public override string GetSettingKey() => "Acc_CRM_Chatbot_Announcement_Notification_Sel";

    public override bool IsValid() => true;
}