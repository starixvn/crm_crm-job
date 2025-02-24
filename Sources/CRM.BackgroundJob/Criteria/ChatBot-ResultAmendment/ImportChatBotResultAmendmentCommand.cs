namespace CRM.BackgroundJob.Criteria.ChatBot_ResultAmendment;

using Fanex.Data.Repository;

public class ImportChatBotResultAmendmentCommand : NonQueryCommand
{
    public string TicketData { get; set; }
    public string EventData { get; set; }
    public string MessageData { get; set; }
    public string TargetMessage { get; set; }
    public string InfoMessage { get; set; }
    public string Environment { get; set; }
    public string ServerName { get; set; }
    public int ErrCode { get; set; }
    public string ErrMsg { get; set; }

    public override string GetSettingKey() => "Acc_CRM_Chatbot_ResultAmendment_Import";

    public override bool IsValid() => true;
}