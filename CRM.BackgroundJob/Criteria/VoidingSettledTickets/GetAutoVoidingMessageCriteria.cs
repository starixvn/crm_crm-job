namespace CRM.BackgroundJob.Criteria.VoidingSettledTickets
{
    using System;
    using Fanex.Data.Repository;

    public class GetAutoVoidingMessageCriteria : CriteriaBase
    {
        public int BatchSize { get; set; }
        public string Environment { get; set; }
        public DateTime LastDateUAT { get; set; }
        public override string GetSettingKey() => "Acc_CRM_AutoVoidingMessageSel";

        public override bool IsValid() => true;
    }
}
