namespace CRM.Criteria.GenerateLapNotification;

using Fanex.Data.Repository;

public class GetPrepareGenLapNotificationCriteria : CriteriaBase
{
    public string Environment {  get; set; }

    public override string GetSettingKey() => "MNC_ResultAmendment_PrepareGenLAP";

    public override bool IsValid() => true;
}