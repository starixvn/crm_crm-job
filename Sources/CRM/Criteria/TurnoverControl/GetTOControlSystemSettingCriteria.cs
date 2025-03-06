namespace CRM.Criteria.TurnoverControl;

using Fanex.Data.Repository;

public class GetTOControlSystemSettingCriteria : CriteriaBase
{
    public override string GetSettingKey() => "Acc_CRM_TOControl_SystemSettingSel";

    public override bool IsValid() => true;
}