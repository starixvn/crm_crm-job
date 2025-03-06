namespace CRM.Models.VoidingSettledTickets;

using System;

public class AutoVoidingEventDetails
{
    public int LogVoidingID { get; set; }
    public int MatchId { get; set; }
    public string MatchCode { get; set; }
    public short EventType { get; set; }
    public short SportType { get; set; }
    public int LeagueId { get; set; }
    public int LeagueGroupId { get; set; }
    public string LeagueNameTc { get; set; }
    public int HomeId { get; set; }
    public int AwayId { get; set; }
    public DateTime KO_time { get; set; }
    public DateTime EventDate { get; set; }
    public DateTime Race_time { get; set; }
    public int ProductId { get; set; }
    public string VendorMatchId { get; set; }
}