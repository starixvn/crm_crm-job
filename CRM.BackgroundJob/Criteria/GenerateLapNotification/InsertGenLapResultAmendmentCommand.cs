namespace CRM.BackgroundJob.Criteria.GenerateLapNotification
{
    using CRM.BackgroundJob.Core.Models;

    public class InsertGenLapResultAmendmentCommand : BaseNonQueryCommand
    {
        public string Environment { get; set; }

        public string JSRemotingRef { get; set; }

        public override string GetSettingKey() => "MNC_ResultAmendment_GenNotificationLAP";

        public override bool IsValid() => true;
    }
}