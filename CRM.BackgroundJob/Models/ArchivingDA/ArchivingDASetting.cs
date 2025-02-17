namespace CRM.BackgroundJob.Models.ArchivingDA
{
    using CRM.BackgroundJob.Models.WinLossLimit;

    public class ArchivingDASetting : WinLossSystemSetting
    {
        public int ServiceID { get; set; }
        public int BatchSizeAccount { get; set; }
        public bool IsSyncArchivedAccount { get; set; }
        public int MaxLogFBId { get; set; }
    }
}
