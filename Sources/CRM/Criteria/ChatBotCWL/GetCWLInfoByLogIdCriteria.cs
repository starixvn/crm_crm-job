namespace CRM.Criteria.ChatBotCWL;

using Fanex.Data.Repository;

public class GetCWLInfoByLogIdCriteria : CriteriaBase
{
    public long LastLogId { get; set; }
    public int BatchSize { get; set; }

    public override string GetSettingKey() => "Acc_CRM_Chatbot_CWL_Log_Sel";
    public override bool IsValid() => true;
}