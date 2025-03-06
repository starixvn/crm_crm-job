namespace CRM.Criteria.VoidingSettledTickets;

using Fanex.Data.Repository;

public class GetMessagesVoidingSettledCriteria : CriteriaBase
{
    public override string GetSettingKey() => "Acc_CRM_VoidingMsg_GetMessage";

    public override bool IsValid() => true;
}