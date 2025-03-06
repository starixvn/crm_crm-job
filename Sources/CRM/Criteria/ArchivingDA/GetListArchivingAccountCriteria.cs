namespace CRM.Criteria.ArchivingDA;

using Fanex.Data.Repository;

public class GetListArchivingAccountCriteria : CriteriaBase
{
    public override string GetSettingKey() => "Acc_CRM_ArchivedAccount_GetListAccount";

    public override bool IsValid() => true;
}