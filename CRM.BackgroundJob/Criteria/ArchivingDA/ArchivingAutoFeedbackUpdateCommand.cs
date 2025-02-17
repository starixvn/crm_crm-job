namespace CRM.BackgroundJob.Criteria.ArchivingDA
{
    using Fanex.Data.Repository;
    public class ArchivingAutoFeedbackUpdateCommand : NonQueryCommand
    {
        public int LogFeedbackId { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }

        public override string GetSettingKey() => "Acc_CRM_ArchivedAccount_AutoFBInfoUpd";

        public override bool IsValid() => LogFeedbackId > 0 && !string.IsNullOrWhiteSpace(Status);
    }
}
