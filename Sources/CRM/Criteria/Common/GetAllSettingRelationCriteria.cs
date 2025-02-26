namespace CRM.Criteria.Common;

using CRM.Constants;
using Fanex.Data.Repository;

public class GetAllSettingRelationCriteria : CriteriaBase
{
    public int UserRole => BackgroundJobConstants.UserRole;

    public override string GetSettingKey() => "Acc_CRM_SettingRelationSel";

    public override bool IsValid() => true;
}