namespace CRM.BackgroundJob.Criteria.VoidingSettledTickets;

using Fanex.Data.Repository;

public class GetMesssagesVoidingSettledCriteria : CriteriaBase
{
    public override string GetSettingKey() => "Acc_CRM_VoidingMsg_GetMessage";

    public override bool IsValid() => true;
}