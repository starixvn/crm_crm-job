namespace CRM.Criteria.ChatbotProblemAccount;

using Fanex.Data.Repository;

public class ProblemsAccountImportCommand : NonQueryCommand
{
    public string XmlData { get; set; }
    public string Environment { get; set; }
    public int ErrCode { get; set; }
    public string ErrMsg { get; set; }
    public override string GetSettingKey() => "Acc_CRM_Chatbot_ProblemAccounts_Import";
    public override bool IsValid() => !string.IsNullOrWhiteSpace(XmlData);
}