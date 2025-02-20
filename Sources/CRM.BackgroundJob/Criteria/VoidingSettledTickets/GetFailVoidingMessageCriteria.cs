namespace CRM.BackgroundJob.Criteria.VoidingSettledTickets;

using Fanex.Data.Repository;

public class GetFailVoidingMessageCriteria : CriteriaBase
{
    public override string GetSettingKey() => "Acc_CRM_FailVoidingMessageSel";

    public override bool IsValid() => true;
}