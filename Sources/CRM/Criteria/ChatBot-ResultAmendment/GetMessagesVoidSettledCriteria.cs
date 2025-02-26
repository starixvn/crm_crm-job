namespace CRM.Criteria.ChatBot_ResultAmendment;

using Fanex.Data.Repository;

public class GetMessagesVoidSettledCriteria : CriteriaBase
{
    public int LastMsgTargetId { get; set; }

    public int BatchSizeGetMsg { get; set; }

    public override string GetSettingKey() => "Acc_CRM_ChatBot_VoidingMsg_GetMessage";

    public override bool IsValid() => true;
}