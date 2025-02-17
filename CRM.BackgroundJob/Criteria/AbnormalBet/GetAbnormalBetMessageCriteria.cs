namespace CRM.BackgroundJob.Criteria.AbnormalBet
{
    using Fanex.Data.Repository;

    public class GetAbnormalBetMessageCriteria : CriteriaBase
    {
        public string Message { get; set; }

        public override string GetSettingKey() => "Acc_CRM_AutoFB_AbnormalBets_GetMessage";

        public override bool IsValid() => true;
    }
}
