namespace CRM.BackgroundJob.Criteria.AutoFeedback;

using Fanex.Data.Repository;

public class GetAbnormalTicketInfoCriteria : CriteriaBase
{
    public string MessageIds { get; set; }
    public bool ConfigUAT { get; set; }
    public bool ConfigPRO { get; set; }
    public string Environment { get; set; }
    public string TranIDs { get; set; }
    public override string GetSettingKey() => "Acc_CRM_AbnormalTicketsSel";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(MessageIds);
}