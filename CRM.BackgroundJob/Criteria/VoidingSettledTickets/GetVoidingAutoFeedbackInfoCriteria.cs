namespace CRM.BackgroundJob.Criteria.VoidingSettledTickets
{
    using Fanex.Data.Repository;

    public class GetVoidingAutoFeedbackInfoCriteria : CriteriaBase
    {
        public int BatchSize { get; set; }
        public override string GetSettingKey() => "Acc_CRM_VoidingAutoFeedbackInfo_Sel";

        public override bool IsValid() => true;
    }
}
