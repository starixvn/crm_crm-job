namespace CRM.Criteria.PushResultAmendment;

public class InsertCompleteResultAmendmentCommand : CRM.Models.Common.BaseNonQueryCommand
{
    public string Environment { get; set; }

    public override string GetSettingKey() => "MNC_PushNotification_Complete";

    public override bool IsValid() => true;
}