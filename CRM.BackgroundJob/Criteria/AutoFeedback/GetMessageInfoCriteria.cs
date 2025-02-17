namespace Fanex.CRM.BackgroundJob.Criteria
{
    using Fanex.Data.Repository;

    public class GetMessageInfoCriteria : CriteriaBase
    {
        public string MessageIds { get; set; }

        public override string GetSettingKey() => "Acc_CRM_MessageAbnormalTicketsSel";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(MessageIds);
    }
}