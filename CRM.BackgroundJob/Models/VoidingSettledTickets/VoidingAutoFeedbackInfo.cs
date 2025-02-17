namespace CRM.BackgroundJob.Models.VoidingSettledTickets
{
    using System;

    public class VoidingAutoFeedbackInfo
    {
        public int LogVoidingID { get; set; }
        public int MessageId { get; set; }
        public int HeadlineId { get; set; }
        public string HeadlineName { get; set; }
        public string ProductId { get; set; }
        public short RetryCount { get; set; }
        public string AffectedId { get; set; }
        public string AffectedName { get; set; }
        public bool IsVip { get; set; }
        public string Site { get; set; }
        public string SiteLabel { get; set; }
        public string ListMsgTargetId { get; set; }
        public string Listbettype { get; set; }
        public int IssueTypeId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}