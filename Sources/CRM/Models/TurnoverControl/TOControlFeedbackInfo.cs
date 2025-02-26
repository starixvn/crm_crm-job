namespace CRM.Models.TurnoverControl;

public class TOControlFeedbackInfo : CustOutStanding
{
    public int LogID { get; set; }
    public int ProfileID { get; set; }
    public int HeadlineID { get; set; }
    public string HeadlineName { get; set; }
    public int RelatedCustID { get; set; }
    public string ProfileName { get; set; }
    public string BettypeList { get; set; }
    public byte Level { get; set; }
    public double TotalTurnover { get; set; }
    public double MaxTurnover { get; set; }
    public double HouseTurnover { get; set; }
    public double Usage { get; set; }
    public string UserName { get; set; }
    public byte RoleID { get; set; }
    public string Currency { get; set; }
    public int HomeID { get; set; }
    public int AwayID { get; set; }
    public int LeagueID { get; set; }
    public int SportType { get; set; }
    public bool IsVip { get; set; }
    public string Site { get; set; }
    public string SiteLabel { get; set; }
}