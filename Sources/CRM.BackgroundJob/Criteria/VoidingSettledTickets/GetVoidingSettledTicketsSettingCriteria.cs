namespace CRM.BackgroundJob.Criteria.VoidingSettledTickets;

using Fanex.Data.Repository;

public class GetVoidingSettledTicketsSettingCriteria : CriteriaBase
{
    public override string GetSettingKey() => "Acc_CRM_Voiding_SystemSettingSel";

    public override bool IsValid() => true;
}