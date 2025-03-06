namespace CRM.Criteria.ChatBot;

using Fanex.Data.Repository;

public class GetChatBotSystemSettingCriteria : CriteriaBase
{
    public string ServiceName { get; set; }
    public string Environment { get; set; }

    public override string GetSettingKey() => "Acc_CRM_Chatbot_SystemSetting_Sel";

    public override bool IsValid() => true;
}