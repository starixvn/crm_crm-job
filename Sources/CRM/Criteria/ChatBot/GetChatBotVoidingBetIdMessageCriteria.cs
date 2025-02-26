namespace CRM.Criteria.ChatBot;

using Fanex.Data.Repository;

public class GetChatBotVoidingBetIdMessageCriteria : CriteriaBase
{
    public int LastMsgTargetId { get; set; }

    public int BatchSize { get; set; }

    public override string GetSettingKey() => "Acc_CRM_Chatbot_AbnormalBets_VoidByBetID_GetMessage";

    public override bool IsValid() => BatchSize > 0;
}