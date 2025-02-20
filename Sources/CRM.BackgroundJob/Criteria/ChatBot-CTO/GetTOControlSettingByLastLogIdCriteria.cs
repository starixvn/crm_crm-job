namespace CRM.BackgroundJob.Criteria.ChatBot_CTO;

using Fanex.Data.Repository;
public class GetTOControlSettingByLastLogIdCriteria : CriteriaBase
{
    public long LastLogId { get; set; }
    public int BatchSize { get; set; }

    public override string GetSettingKey() => "Acc_Chatbot_TOControlSetting_Sel";
    public override bool IsValid() => true;
}