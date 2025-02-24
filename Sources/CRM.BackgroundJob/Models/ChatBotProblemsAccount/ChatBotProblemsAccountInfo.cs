namespace CRM.BackgroundJob.Models.ChatBotProblemsAccount;

public class ChatBotProblemsAccountInfo
{
    public long LogId { get; set; }
    public string UserName { get; set; }
    public int CustId { get; set; }
    public int Recommend { get; set; }
    public int Mrecommend { get; set; }
    public int Srecommend { get; set; }
    public short CategoryId { get; set; }
    public int IsTest { get; set; }
}