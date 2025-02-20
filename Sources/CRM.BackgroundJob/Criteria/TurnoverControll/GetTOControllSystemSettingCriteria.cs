namespace CRM.BackgroundJob.Criteria.TurnoverControll;

using Fanex.Data.Repository;
public class GetTOControllSystemSettingCriteria : CriteriaBase
{
    public override string GetSettingKey() => "Acc_CRM_TOControl_SystemSettingSel";

    public override bool IsValid() => true;
}