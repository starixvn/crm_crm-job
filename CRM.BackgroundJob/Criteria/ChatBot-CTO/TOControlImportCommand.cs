namespace CRM.BackgroundJob.Criteria.ChatBot_CTO
{
    using Fanex.Data.Repository;
    public class TOControlImportCommand : NonQueryCommand
    {
        public string XmlData { get; set; }
        public string Environment { get; set; }
        public int ErrCode { get; set; }
        public string ErrMsg { get; set; }
        public override string GetSettingKey() => "Acc_CRM_Chatbot_TOControl_Import";
        public override bool IsValid() => !string.IsNullOrWhiteSpace(XmlData);
    }
}
