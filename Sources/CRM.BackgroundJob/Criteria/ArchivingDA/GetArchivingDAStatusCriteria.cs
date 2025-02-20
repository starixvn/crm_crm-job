namespace CRM.BackgroundJob.Criteria.ArchivingDA;

using Fanex.Data.Repository;

public class GetArchivingDAStatusCriteria : CriteriaBase
{
    public override string GetSettingKey() => "Acc_CRM_ArchivedAccount_CheckStatus";

    public override bool IsValid() => true;
}