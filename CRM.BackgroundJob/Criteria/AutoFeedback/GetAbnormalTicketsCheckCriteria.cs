namespace CRM.BackgroundJob.Criteria.AutoFeedback
{
    using CRM.BackgroundJob.Core.Criteria;

    public class GetAbnormalTicketsCheckCriteria : SiteDbBaseCriteria
    {
        public string TranIDs { get; set; }
        public bool ConfigUAT { get; set; }
        public bool ConfigPRO { get; set; }
        public string Environment { get; set; }

        public override string GetSettingKey() => "Acc_CRM_AbnormalTicketsCheck";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(TranIDs) && SiteDBGroupID >= 0;
    }
}