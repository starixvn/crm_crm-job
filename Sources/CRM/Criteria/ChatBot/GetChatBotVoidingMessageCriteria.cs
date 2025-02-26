namespace CRM.Criteria.ChatBot;

using Fanex.Data.Repository;

public class GetChatBotVoidingMessageCriteria : CriteriaBase
{
    public int LastVoidingHistoryCollectionId { get; set; }
    public int BatchSize { get; set; }
    public override string GetSettingKey() => "Acc_CRM_Chatbot_AbnormalBets_GetMessage";

    public override bool IsValid() => BatchSize > 0;
}