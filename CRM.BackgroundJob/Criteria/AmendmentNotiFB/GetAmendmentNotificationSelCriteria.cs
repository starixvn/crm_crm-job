namespace CRM.BackgroundJob.Criteria.AmendmentNotiFB
{
    using Fanex.Data.Repository;
    using System;

    public class GetAmendmentNotificationSelCriteria : CriteriaBase
    {
        public int LastSeq { get; set; }
        public DateTime LastSubmitDate { get; set; }
        public int BatchSizeTicket { get; set; }
        public bool ConfigUAT { get; set; }
        public bool ConfigPRO { get; set; }
        public bool IsCredit { get; set; }
        public bool IsDeposit { get; set; }
        public string Environment { get; set; }
        public override string GetSettingKey() => "Acc_CRM_AmendmentNotificationSel";

        public override bool IsValid() => true;
    }
}
