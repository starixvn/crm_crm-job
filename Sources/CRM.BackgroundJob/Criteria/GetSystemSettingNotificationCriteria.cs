namespace CRM.BackgroundJob.Criteria;

using Fanex.Data.Repository;

public class GetSystemSettingNotificationCriteria : CriteriaBase
{
    public string Environment { get; set; }

    public override string GetSettingKey() => "MNC_Notification_SystemSettingSel";

    public override bool IsValid() => true;
}