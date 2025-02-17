namespace CRM.BackgroundJob.Models.ProblemsAccount
{
    public class ProblemsAccountAutoFeedbackInfor
    {
        public int LogID { get; set; }
        public int CustID { get; set; }
        public string UserName { get; set; }
        public int HeadlineID { get; set; }
        public string HeadlineName { get; set; }
        public bool IsVip { get; set; }
        public int RelatedCustID { get; set; }
        public short CategoryID { get; set; }
    }
}
