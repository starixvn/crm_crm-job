using Fanex.Data.Repository;

namespace CRM.BackgroundJob.Criteria.TurnoverControl;

public class GetTOControlCustOutstandingCriteria : CriteriaBase
{
    public string xml { get; set; }

    public override string GetSettingKey() => "Acc_CRM_TOControl_CustOutstandingSel";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(xml);
}