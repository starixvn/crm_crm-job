namespace CRM.Criteria.AutoFeedback;

using CRM.Criteria.Common;

public class GetVoidingHisMessageUserNameCriteria : SiteDbBaseCriteria
{
    public string Messages { get; set; }

    public override string GetSettingKey() => "Acc_CRM_AbnormalAccount_GetUsernameList";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(Messages) && SiteDBGroupID > 0;
}