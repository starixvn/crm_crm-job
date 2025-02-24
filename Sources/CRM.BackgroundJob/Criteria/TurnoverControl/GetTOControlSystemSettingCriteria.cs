using Fanex.Data.Repository;

namespace CRM.BackgroundJob.Criteria.TurnoverControl;

public class GetTOControlSystemSettingCriteria : CriteriaBase
{
    public override string GetSettingKey() => "Acc_CRM_TOControl_SystemSettingSel";

    public override bool IsValid() => true;
}