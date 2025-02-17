namespace CRM.BackgroundJob.Models.AbnormalBet
{
    public class TransactionVoid
    {
        public int VoidingHistoryCollectionId { get; set; }
        public int MessageId { get; set; }
        public int RequestId { get; set; }
        public int CustId { get; set; }
        public string Username { get; set; }
        public string Site { get; set; }
        public bool IsDeposit { get; set; }
    }
}
