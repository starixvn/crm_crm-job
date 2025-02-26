namespace CRM.Criteria.SendNotificationCenter;

using Fanex.Data.Repository;

public class GetNotificationAlertCriteria : CriteriaBase
{
    public string Environment { get; set; }

    public override string GetSettingKey() => "MNC_Notification_AlertSel";

    public override bool IsValid() => true;
}