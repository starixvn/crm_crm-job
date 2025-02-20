namespace CRM.BackgroundJob.Criteria.ChatBot_ArchivingDA;

using Fanex.Data.Repository;

public class GETChatbotArchivedAccountsCriteria : CriteriaBase
{
    public long LastLogId { get; set; }
    public int BatchSize { get; set; }

    public override string GetSettingKey() => "Acc_CRM_Chatbot_ArchivedAccounts_Sel";
    public override bool IsValid() => true;
}