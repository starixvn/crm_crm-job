namespace CRM.Criteria.VoidingBetID;

using Fanex.Data.Repository;

public class CleanMessageVoidByBetIDCriteria : CriteriaBase
{
    public string Messages { get; set; }

    public int MAXMsgTargetId { get; set; }

    public override string GetSettingKey() => "Acc_CRM_AutoFB_VoidByBetID_CleanMessage";

    public override bool IsValid() => true;
}