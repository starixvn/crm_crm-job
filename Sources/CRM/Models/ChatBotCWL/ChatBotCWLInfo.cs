namespace CRM.Models.ChatBotCWL;

public class ChatBotCWLInfo
{
    public long LogId { get; set; }

    public int CustId { get; set; }

    public string Username { get; set; }

    public int Recommend { get; set; }

    public int Mrecommend { get; set; }

    public int Srecommend { get; set; }

    public double Winloss { get; set; }

    public string CurrencyName { get; set; }

    public int ProductId { get; set; }

    public int LevelId { get; set; }

    public int GroupId { get; set; }
}