namespace CRM.BackgroundJob.Criteria.GenerateResultAmendment
{
    using CRM.BackgroundJob.Core.Models;

    public class InsertGenResultAmendmentCommand : BaseNonQueryCommand
    {
        public string Environment { get; set; }

        public string JSRemotingRef { get; set; }

        public override string GetSettingKey() => "MNC_ResultAmendment_GenNotification";

        public override bool IsValid() => true;
    }
}