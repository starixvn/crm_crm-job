namespace CRM.BackgroundJob.Criteria.SendNotificationCenter;

using CRM.Models;

public class InsertNotificationAlertCommand : BaseNonQueryCommand
{
    public string Environment {  get; set; }

    public override string GetSettingKey() => "MNC_Notification_AlertIns";

    public override bool IsValid() => true;
}