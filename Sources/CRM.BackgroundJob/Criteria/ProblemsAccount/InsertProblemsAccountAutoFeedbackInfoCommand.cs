namespace CRM.BackgroundJob.Criteria.ProblemsAccount;

using Fanex.Data.Repository;

public class InsertProblemsAccountAutoFeedbackInfoCommand : NonQueryCommand
{
    public string xml { get; set; }

    public string Enviroment { get; set; }

    public override string GetSettingKey() => "Acc_CRM_ProblemAccount_AutoFeedbackInfoIns";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(xml);
}