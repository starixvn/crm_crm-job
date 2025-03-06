namespace CRM.Criteria.ChatBot_ResultAmendment;

using Fanex.Data.Repository;

public class GetChatBotMsgCriteria : CriteriaBase
{
    public string Messages { get; set; }
    public override string GetSettingKey() => "Acc_CRM_Chatbot_Msg_Sel";
    public override bool IsValid() => true;
}