namespace CRM.Criteria.Common;

using CRM.Constants;
using Fanex.Data.Repository;

public class GetAllSettingCriteria : CriteriaBase
{
    public short UserRole => BackgroundJobConstants.UserRole;

    public override string GetSettingKey() => "Acc_CRM_SettingSel";

    public override bool IsValid() => true;
}