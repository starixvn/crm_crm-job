namespace CRM.BackgroundJob.Models.AmendmentNotiFB
{
    using CRM.BackgroundJob.Models.WinLossLimit;
    using System;

    public class AmendmentNotiFbSetting : WinLossSystemSetting
    {
        public int BatchSizeTicket { get; set; }
        public int LastSeq { get; set; }
        public int LastSeqUAT { get; set; }
        public DateTime LastSubmitDate { get; set; }
        public DateTime LastSubmitDateUAT { get; set; }
    }
}
