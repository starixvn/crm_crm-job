namespace CRM.Models.WinLossLimit;

using System;

public class CustomerWinLimit
{
    public int LogID { get; set; }
    public int CustId { get; set; }
    public int ProductID { get; set; }
    public string ProductGroup { get; set; }
    public string Level { get; set; }
    public byte LevelID { get; set; }
    public string MaxWinLimitSetting { get; set; }
    public double TotalWinAmt { get; set; }
    public double TotalWinAmt_RM { get; set; }
    public DateTime ReachedTime { get; set; }
    public string BettypeIDList { get; set; }
    public byte CasinoType { get; set; }
    public bool IsNormalGame { get; set; }
}