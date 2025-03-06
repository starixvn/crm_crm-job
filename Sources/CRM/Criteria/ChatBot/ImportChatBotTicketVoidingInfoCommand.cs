namespace CRM.Criteria.ChatBot;

using Fanex.Data.Repository;

public class ImportChatBotTicketVoidingInfoCommand : NonQueryCommand
{
    public string XmlData { get; set; }
    public string Environment { get; set; }
    public int MAXVoidingHistoryCollectionId { get; set; }
    public int MAXMsgTargetId { get; set; }
    public int ErrCode { get; set; }
    public string ErrMsg { get; set; }

    public override string GetSettingKey() => "Acc_CRM_Chatbot_AbnormalBets_Import";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(XmlData);
}