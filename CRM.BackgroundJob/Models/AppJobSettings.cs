namespace Fanex.CRM.BackgroundJob.Models
{
    public class AppJobSettings
    {
        public short TimeSpan { get; set; }
        public byte RetryRuntime { get; set; }
        public string LicenseeUsername { get; set; }
        public int CreatorId { get; set; }
        public int NoOfRelatedCustomerPerFeedback { get; set; }
    }
}