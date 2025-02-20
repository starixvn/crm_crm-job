namespace CRM.BackgroundJob.Criteria.ChatBot_ArchivingDA;

using Fanex.Data.Repository;

public class ImportArchivedAccountsCommand : NonQueryCommand
{
    public string XmlData { get; set; }
    public string Environment { get; set; }
    public int ErrCode { get; set; }
    public string ErrMsg { get; set; }

    public override string GetSettingKey() => "Acc_CRM_Chatbot_ArchivedAccounts_Import";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(XmlData);
}