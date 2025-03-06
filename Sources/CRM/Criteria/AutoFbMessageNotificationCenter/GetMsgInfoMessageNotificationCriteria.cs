namespace CRM.Criteria.AutoFbMessageNotificationCenter;

using Fanex.Data.Repository;

public class GetMsgInfoMessageNotificationCriteria : CriteriaBase
{
    public int LastMsgTargetId { get; set; }

    public int BatchSize { get; set; }

    public override string GetSettingKey() => "Acc_CRM_AutoFB_MessageNotificationCenter_GetMsgInfo";

    public override bool IsValid() => true;
}