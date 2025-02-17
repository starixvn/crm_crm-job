namespace Fanex.CRM.BackgroundJob.Criteria.ChatBot
{
    using Fanex.Data.Repository;

    public class EditChatBotVoidingMessageCommand : NonQueryCommand
    {
        public string Messages { get; set; }

        public override string GetSettingKey() => "Acc_CRM_ChatBot_crmMessageImport_Upd";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(Messages);
    }
}