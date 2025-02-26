namespace CRM.Models.AmendmentNotiFB;

using System;
using WinLossLimit;

public class AmendmentNotiFbSetting : WinLossSystemSetting
{
    public int BatchSizeTicket { get; set; }
    public int LastSeq { get; set; }
    public int LastSeqUAT { get; set; }
    public DateTime LastSubmitDate { get; set; }
    public DateTime LastSubmitDateUAT { get; set; }
}