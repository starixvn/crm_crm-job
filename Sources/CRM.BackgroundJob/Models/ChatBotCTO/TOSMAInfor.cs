namespace CRM.BackgroundJob.Models.ChatBotCTO;

public class TOSMAInfor
{
    public long LogId { get; set; }
    public int CustId { get; set; }
    public string UserName { get; set; }
    public int Recommend { get; set; }
    public int Mrecommend { get; set; }
    public int Srecommend { get; set; }
    public string CurrencyName { get; set; }
    public short RoleId { get; set; }
    public int Sporttype { get; set; }
    public int LeagueId { get; set; }
    public int HomeId { get; set; }
    public int AwayId { get; set; }
    public double Outstanding { get; set; }
}