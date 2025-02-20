
namespace CRM.BackgroundJob.Criteria.ChatbotProblemAccount;

using Fanex.Data.Repository;
public class GetProblemsAccountByLogId : CriteriaBase
{
    public long LastLogId { get; set; }
    public int BatchSize { get; set; }

    public override string GetSettingKey() => "Acc_CRM_Chatbot_ProblemAccounts_Sel";
    public override bool IsValid() => true;
}