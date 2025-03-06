namespace CRM.Criteria.AutoFeedback;

using Fanex.Data.Repository;

public class GetGroupHeadlineVipCriteria : CriteriaBase
{
    public string CustIds { get; set; }

    public override string GetSettingKey() => "Acc_CRM_GroupHeadlineVipSel";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(CustIds);
}