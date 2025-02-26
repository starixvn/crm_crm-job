namespace CRM.Criteria.GenerateLapNotification;

public class InsertGenLapResultAmendmentCommand : CRM.Models.Common.BaseNonQueryCommand
{
    public string Environment { get; set; }

    public string JSRemotingRef { get; set; }

    public override string GetSettingKey() => "MNC_ResultAmendment_GenNotificationLAP";

    public override bool IsValid() => true;
}