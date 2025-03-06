namespace CRM.Criteria.TurnoverControl;

using Fanex.Data.Repository;

public class GetTOControlCustOutstandingCriteria : CriteriaBase
{
    public string xml { get; set; }

    public override string GetSettingKey() => "Acc_CRM_TOControl_CustOutstandingSel";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(xml);
}