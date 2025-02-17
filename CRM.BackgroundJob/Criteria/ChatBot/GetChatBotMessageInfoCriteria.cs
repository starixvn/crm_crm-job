namespace Fanex.CRM.BackgroundJob.Criteria.ChatBot
{
    using Fanex.Data.Repository;

    public class GetChatBotMessageInfoCriteria : CriteriaBase
    {
        public string Messages { get; set; }

        public override string GetSettingKey() => "Acc_CRM_Chatbot_AbnormalBets_Message_Sel";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(Messages);
    }
}