namespace CRM.BackgroundJob.Criteria;

using Constants;
using Fanex.Data.Repository;

public class GetAllSettingCriteria : CriteriaBase
{
    public short UserRole { get; } = BackgroundJobConstants.UserRole;

    public override string GetSettingKey() => "Acc_CRM_SettingSel";

    public override bool IsValid() => true;
}