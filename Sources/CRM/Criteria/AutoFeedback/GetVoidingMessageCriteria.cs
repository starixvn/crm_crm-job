namespace CRM.Criteria.AutoFeedback;

using Fanex.Data.Repository;

public class GetVoidingMessageCriteria : CriteriaBase
{
    public int MaxVoidingHistoryCollectionId { get; set; }
    public override string GetSettingKey() => "Acc_CRM_VoidingMessageSel";

    public override bool IsValid() => true;
}