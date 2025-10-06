namespace CRM.Criteria.WinLossLimit;

using Fanex.Data.Repository;

public class ImportWinLossFeedbackInfoCriteria : CriteriaBase
{
    public string xml { get; set; }

    public string Environment { get; set; }

    public override string GetSettingKey() => "Acc_CRM_WinLimit_AutoFeedbackInfoIns";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(xml);
}