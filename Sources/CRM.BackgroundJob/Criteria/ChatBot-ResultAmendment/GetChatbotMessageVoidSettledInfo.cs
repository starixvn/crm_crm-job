namespace CRM.BackgroundJob.Criteria.ChatBot_ResultAmendment;

using Fanex.Data.Repository;

public class GetChatbotMessageVoidSettledInfo : CriteriaBase
{
    public string Message { get; set; }
    public override string GetSettingKey() => "Acc_CRM_Chatbot_VodingMessage_Sel";
    public override bool IsValid() => true;
}