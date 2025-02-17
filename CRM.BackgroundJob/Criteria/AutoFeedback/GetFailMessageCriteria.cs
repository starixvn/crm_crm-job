namespace Fanex.CRM.BackgroundJob.Criteria
{
    using Fanex.Data.Repository;

    public class GetFailMessageCriteria : CriteriaBase
    {
        public override string GetSettingKey() => "Acc_CRM_FailMessageSel";

        public override bool IsValid() => true;
    }
}