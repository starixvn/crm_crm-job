namespace Fanex.CRM.BackgroundJob.Criteria
{
    using Fanex.CRM.BackgroundJob.Constants;
    using Fanex.Data.Repository;

    public class GetAllSettingRelationCriteria : CriteriaBase
    {
        public int UserRole { get; } = BackgroundJobConstants.UserRole;

        public override string GetSettingKey() => "Acc_CRM_SettingRelationSel";

        public override bool IsValid() => true;
    }
}