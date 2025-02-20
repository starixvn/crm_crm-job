namespace CRM.BackgroundJob.Criteria.PushResultAmendment;

using CRM.Models;

public class InsertCompleteResultAmendmentCommand : BaseNonQueryCommand
{
    public string Environment { get; set; }

    public override string GetSettingKey() => "MNC_PushNotification_Complete";

    public override bool IsValid() => true;
}