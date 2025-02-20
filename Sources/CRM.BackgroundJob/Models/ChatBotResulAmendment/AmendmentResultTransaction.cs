using System;

namespace CRM.BackgroundJob.Models.ChatBotResulAmendment;

public class AmendmentResultTransaction
{
    public int TicketEventId { get; set; }
    public int ProductId { get; set; }
    public int MatchId { get; set; }
    public string MatchCode { get; set; }
    public int EventType { get; set; }
    public DateTime SubmitDate { get; set; }
    public DateTime EventDate { get; set; }
    public string ActualResult { get; set; }
    public string WrongResult { get; set; }
    public string Status { get; set; }
    public int MessageId { get; set; }
    public int Sporttype { get; set; }
    public int HomeId { get; set; }
    public int AwayId { get; set; }
    public int LeagueId { get; set; }
    public int LeagueGroupID { get; set; }
    public int Type { get; set; }
}