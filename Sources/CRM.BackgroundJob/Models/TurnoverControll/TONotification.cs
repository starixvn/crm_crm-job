namespace CRM.BackgroundJob.Models.TurnoverControll;

public class TONotification
{
    public int LogID { get; set; }
    public int CustID { get; set; }
    public int MatchID { get; set; }
    public int ProfileID { get; set; }
    public double MaxTurnOver { get; set; }
    public double TotalTurnover { get; set; }
    public double HouseTurnover { get; set; }
    public byte ReachLevel { get; set; }
    public string ProfileName { get; set; }
    public string Bettype { get; set; }
}