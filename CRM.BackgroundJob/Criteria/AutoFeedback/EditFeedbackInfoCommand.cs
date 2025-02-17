namespace Fanex.CRM.BackgroundJob.Criteria
{
    using Fanex.Data.Repository;

    public class EditFeedbackInfoCommand : NonQueryCommand
    {
        public string MessageId { get; set; }
        public string RequestIds { get; set; }
        public string CustIds { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public string Environment { get; set; }

        public override string GetSettingKey() => "Acc_CRM_AutoFeedbackInfoUpd";

        public override bool IsValid() => true;
    }
}