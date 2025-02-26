namespace CRM.Criteria.ProblemsAccount;

using Fanex.Data.Repository;

public class GetProblemsAccountCustCategoryCriteria : CriteriaBase
{
    public long LastLogID { get; set; }
    public int BatchSize { get; set; }
    public bool ConfigUAT { get; set; }
    public bool ConfigPRO { get; set; }
    public string Environment { get; set; }
    public override string GetSettingKey() => "Acc_CRM_ProblemAccount_CustCategorySel";

    public override bool IsValid() => true;
}