namespace CRM.BackgroundJob.Models.TurnoverControl;

public class CustOutStanding
{
    public int CustID { get; set; }
    public int MatchID { get; set; }
    public double Outstanding { get; set; }
}