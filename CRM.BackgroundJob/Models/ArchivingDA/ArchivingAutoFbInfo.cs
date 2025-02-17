namespace CRM.BackgroundJob.Models.ArchivingDA
{
    using System;

    public class ArchivingAutoFbInfo
    {
        public int BatchSize { get; set; }
        public int LogFeedbackId { get; set; }
        public int HeadlineID { get; set; }
        public string HeadlineName { get; set; }
        public short RetryCount { get; set; }
        public string AffectedId { get; set; }
        public string AffectedName { get; set; }
        public DateTime ArchivedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public string Environment { get; set; }
        public short ServiceID { get; set; }
        public bool IsVip { get; set; }
        public string SiteLabel { get; set; }
    }
}
