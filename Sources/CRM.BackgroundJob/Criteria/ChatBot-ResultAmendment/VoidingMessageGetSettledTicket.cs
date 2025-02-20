namespace CRM.BackgroundJob.Criteria.ChatBot_ResultAmendment;

using Fanex.Data.Repository;
public class VoidingMessageGetSettledTicket : CriteriaBase
{
    public string Message { get; set; }

    public int SiteDBGroupId { get; set; }

    public override string GetSettingKey() => "Acc_CRM_ChatBot_VodingMessage_GetSettledTickets";

    public override bool IsValid() => !string.IsNullOrEmpty(Message);
}