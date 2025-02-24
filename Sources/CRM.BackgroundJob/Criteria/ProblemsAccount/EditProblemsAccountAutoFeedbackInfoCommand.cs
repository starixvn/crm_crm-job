namespace CRM.BackgroundJob.Criteria.ProblemsAccount;

using Fanex.Data.Repository;

public class EditProblemsAccountAutoFeedbackInfoCommand : NonQueryCommand
{
    public string LogID { get; set; }
    public string Status { get; set; }
    public string Description { get; set; }

    public override string GetSettingKey() => "Acc_CRM_ProblemAccount_AutoFeedbackInfoUpd";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(LogID) && !string.IsNullOrWhiteSpace(Status);
}