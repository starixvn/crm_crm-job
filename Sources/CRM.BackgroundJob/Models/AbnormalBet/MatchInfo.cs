namespace CRM.BackgroundJob.Models.AbnormalBet;

using System;

public class MatchInfo
{
    public int VoidingHistoryCollectionId { get; set; }
    public string MessageId { get; set; }
    public DateTime EventDate { get; set; }
    public string KickOffTime { get; set; }
    public string LeagueName { get; set; }
    public string HomeName { get; set; }
    public string AwayName { get; set; }
    public string SportName { get; set; }
    public int IssueTypeId { get; set; }
    public string RequestIDList { get; set; }
    public string DecidedBy { get; set; }
    public string Requestby { get; set; }
    public string VoidBy { get; set; }
    public DateTime MinWinlostdate { get; set; }
    public DateTime MaxWinlostdate { get; set; }
    public byte FilterBy { get; set; }
}