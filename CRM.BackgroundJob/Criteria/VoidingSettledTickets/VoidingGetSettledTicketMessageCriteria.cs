namespace CRM.BackgroundJob.Criteria.VoidingSettledTickets
{
    using Fanex.Data.Repository;

    public class VoidingGetSettledTicketMessageCriteria : CriteriaBase
    {
        public string Message { get; set; }

        public override string GetSettingKey() => "Acc_CRM_VodingMessage_GetSettledTickets";

        public override bool IsValid() => !string.IsNullOrEmpty(Message);
    }
}
