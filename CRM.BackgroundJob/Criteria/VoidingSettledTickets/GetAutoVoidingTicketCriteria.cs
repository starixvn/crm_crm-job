using Fanex.Data.Repository;

namespace CRM.BackgroundJob.Criteria.VoidingSettledTickets
{
    public class GetAutoVoidingTicketCriteria : CriteriaBase
    {
        public string MessageIDs { get; set; }
        public bool ConfigUAT { get; set; }
        public bool ConfigPRO { get; set; }
        public string Environment { get; set; }
        public override string GetSettingKey() => "Acc_CRM_AutoVoidingTicketSel";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(MessageIDs);
    }
}
