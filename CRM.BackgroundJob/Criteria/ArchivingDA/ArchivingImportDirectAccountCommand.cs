namespace CRM.BackgroundJob.Criteria.ArchivingDA
{
    using System;
    using Fanex.Data.Repository;

    public class ArchivingImportDirectAccountCommand : NonQueryCommand
    {
        public string ArchivedList { get; set; }
        public DateTime ArchivedDate { get; set; }
        public override string GetSettingKey() => "Acc_CRM_ArchivedAccount_SyncData";

        public override bool IsValid() => !string.IsNullOrEmpty(ArchivedList);
    }
}
