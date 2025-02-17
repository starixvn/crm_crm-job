namespace CRM.BackgroundJob.Models.AbnormalBet
{
    public class AutoFeedbackInfo
    {
        public int VoidingHistoryCollectionId { get; set; }
        public int MessageId { get; set; }
        public int CustId { get; set; }
        public int RequestID { get; set; }
        public int HeadlineId { get; set; }
        public int RelatedCustId { get; set; }
        public string EventInfo { get; set; }
    }
}