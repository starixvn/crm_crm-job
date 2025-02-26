namespace CRM.Criteria.ChatBotCWL;

using Fanex.Data.Repository;

public class ImportCWLLogInfoCommand : NonQueryCommand
{
    public string XmlData { get; set; }
    public string Environment { get; set; }
    public int ErrCode { get; set; }
    public string ErrMsg { get; set; }
    public override string GetSettingKey() => "Acc_CRM_Chatbot_CWL_Import";
    public override bool IsValid() => !string.IsNullOrWhiteSpace(XmlData);
}