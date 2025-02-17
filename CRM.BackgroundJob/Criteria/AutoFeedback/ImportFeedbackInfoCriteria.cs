namespace Fanex.CRM.BackgroundJob.Criteria
{
    using Fanex.Data.Repository;

    public class ImportFeedbackInfoCriteria : CriteriaBase
    {
        public string Xml { get; set; }
        public string Environment { get; set; }

        public override string GetSettingKey() => "Acc_CRM_AutoFeedbackInfoIns";

        public override bool IsValid() => true;
    }
}