namespace CRM.BackgroundJob.Models.AbnormalBet;

public class DuplicateTicketDetail
{
    public int VoidingHistoryCollectionId { get; set; }
    public int MessageId { get; set; }
    public int RequestID { get; set; }
    public string OrgTransIds { get; set; }
    public long VoidedTransId { get; set; }
    public string Username { get; set; }
    public int CustId { get; set; }
    public string Site { get; set; }
    public bool IsDeposit { get; set; }
    public bool IsTest { get; set; }
    public string Bettype { get; set; }
    public int ProductId { get; set; }
    public int BetTypeId { get; set; }
    public short SportTypeId { get; set; }
}