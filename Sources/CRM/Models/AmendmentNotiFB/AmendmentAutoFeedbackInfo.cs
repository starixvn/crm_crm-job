namespace CRM.Models.AmendmentNotiFB;

using System;

public class AmendmentAutoFeedbackInfo
{
    public int LogId { get; set; }
    public int TicketEventId { get; set; }
    public int HeadlineId { get; set; }
    public string HeadlineName { get; set; }
    public int ProductId { get; set; }
    public string affectedId { get; set; }
    public string affectedName { get; set; }
    public int MatchId { get; set; }
    public string MatchCode { get; set; }
    public byte EventType { get; set; }
    public int MessageId { get; set; }
    public string ActualResult { get; set; }
    public string WrongResult { get; set; }
    public string ListBettype { get; set; }
    public string Currency { get; set; }
    public string Site { get; set; }
    public string SiteLabel { get; set; }
    public byte SportType { get; set; }
    public int LeagueId { get; set; }
    public int LeagueGroupId { get; set; }
    public string LeagueNameTc { get; set; }
    public int HomeId { get; set; }
    public int AwayId { get; set; }
    public bool IsVip { get; set; }
    public DateTime KO_time { get; set; }
    public DateTime EventDate { get; set; }
    public DateTime Race_time { get; set; }
}