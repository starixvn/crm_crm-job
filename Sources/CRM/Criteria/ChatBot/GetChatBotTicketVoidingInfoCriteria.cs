namespace CRM.Criteria.ChatBot;

using Fanex.Data.Repository;

public class GetChatBotTicketVoidingInfoCriteria : CriteriaBase
{
    public string Messages { get; set; }
    public string Environment { get; set; }
    public bool IncludeUUSCurrency { get; set; }
    public int MAXVoidingHistoryCollectionId { get; set; }
    public string Messages_VoidByBetID { get; set; }
    public int MAXMsgTargetId { get; set; }

    public override string GetSettingKey() => "Acc_CRM_Chatbot_AbnormalBets_VoidingInfo_Sel";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(Messages);
}