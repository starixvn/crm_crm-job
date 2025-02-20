namespace CRM.BackgroundJob.Criteria.ChatBot_CTO;

using Fanex.Data.Repository;
public class GetTOControlSMAInfroCriteria : CriteriaBase
{
    public string XML { get; set; }
    public override string GetSettingKey() => "Acc_Chatbot_TOControlSMAInfo_Sel";
    public override bool IsValid() => !string.IsNullOrWhiteSpace(XML);
}