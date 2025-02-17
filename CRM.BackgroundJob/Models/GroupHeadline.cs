namespace Fanex.CRM.BackgroundJob.Models
{
    public class GroupHeadline
    {
        public int CustId { get; set; }
        public int HeadlineId { get; set; }
        public string HeadlineName { get; set; }
        public bool IsVip { get; set; }
        public string Site { get; set; }
        public string SiteLabel { get; set; }
    }
}