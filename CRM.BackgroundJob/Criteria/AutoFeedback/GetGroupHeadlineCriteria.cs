namespace Fanex.CRM.BackgroundJob.Criteria
{
    using Fanex.Data.Repository;

    public class GetGroupHeadlineCriteria : CriteriaBase
    {
        public string CustIds { get; set; }

        public override string GetSettingKey() => "Acc_CRM_GroupHeadlineSel";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(CustIds);
    }
}