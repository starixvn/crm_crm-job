namespace CRM.Criteria.ProblemsAccount;

using Fanex.Data.Repository;

public class GetProblemsAccountSystemSettingCriteria : CriteriaBase
{
    public override string GetSettingKey() => "Acc_CRM_ProblemAccount_SystemSettingSel";

    public override bool IsValid() => true;
}