namespace CRM.BackgroundJob.Criteria.RefundSettledTicket
{
    using Fanex.Data.Repository;

    public class GetMessageRefundSettledTicketCriteria : CriteriaBase
    {
        public int LastMsgTargetId { get; set; }

        public int BatchSize { get; set; }

        public override string GetSettingKey() => "Acc_CRM_AutoFB_RefundSettledTickets_GetMessage";

        public override bool IsValid() => true;
    }
}
