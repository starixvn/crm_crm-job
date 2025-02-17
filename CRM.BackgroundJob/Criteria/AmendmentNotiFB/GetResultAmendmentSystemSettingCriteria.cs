namespace CRM.BackgroundJob.Criteria.AmendmentNotiFB
{
    using Fanex.Data.Repository;
    public class GetResultAmendmentSystemSettingCriteria : CriteriaBase
    {
        public override string GetSettingKey() => "Acc_CRM_AmendmentNoti_SystemSettingSel";

        public override bool IsValid() => true;
    }
}
