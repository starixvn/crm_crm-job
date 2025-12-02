namespace CRM.Criteria.ChatBotRebateRetention;

using Fanex.Data.Repository;

public class GetRebateRetentionInfoCriteria : CriteriaBase
{
    public string LastLogSID { get; set; }

    public int BatchSize { get; set; }

    public string NewLogSID { get; set; }

    public override string GetSettingKey() => "Acc_CRM_Chatbot_RebateRetention_Sel";

    public override bool IsValid() => true;
}