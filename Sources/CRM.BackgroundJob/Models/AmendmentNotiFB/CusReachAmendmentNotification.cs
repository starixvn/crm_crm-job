using System;

namespace CRM.BackgroundJob.Models.AmendmentNotiFB;

public class CusReachAmendmentNotification
{
    public int TicketId { get; set; }
    public int CustId { get; set; }
    public int TicketEventId { get; set; }
    public short Bettype { get; set; }
    public string Status { get; set; }
    public DateTime SubmitDate { get; set; }
    public int Seq { get; set; }
}