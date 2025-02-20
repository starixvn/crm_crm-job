namespace CRM.BackgroundJob.Criteria.VoidingBetID;

using Fanex.Data.Repository;

public class GetMessageVoidByBetIDCriteria : CriteriaBase
{
    public int LastMsgTargetId { get; set; }

    public int BatchSize { get; set; }

    public override string GetSettingKey() => "Acc_CRM_AutoFB_VoidByBetID_GetMessage";

    public override bool IsValid() => true;
}