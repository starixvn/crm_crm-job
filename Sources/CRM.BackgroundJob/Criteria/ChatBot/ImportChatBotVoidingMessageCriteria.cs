namespace CRM.BackgroundJob.Criteria.ChatBot;

using Fanex.Data.Repository;

public class ImportChatBotVoidingMessageCriteria : NonQueryCommand
{
    public string Messages { get; set; }

    public override string GetSettingKey() => "Acc_CRM_ChatBot_crmMessageImport_Ins";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(Messages);
}