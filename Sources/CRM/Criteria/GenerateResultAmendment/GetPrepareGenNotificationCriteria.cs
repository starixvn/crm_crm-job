namespace CRM.Criteria.GenerateResultAmendment;

using Fanex.Data.Repository;

public class GetPrepareGenNotificationCriteria : CriteriaBase
{
    public string Environment {  get; set; }

    public override string GetSettingKey() => "MNC_ResultAmendment_PrepareGen";

    public override bool IsValid() => true;
}