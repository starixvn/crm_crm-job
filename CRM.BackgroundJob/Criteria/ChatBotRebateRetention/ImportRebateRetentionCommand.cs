namespace CRM.BackgroundJob.Criteria.ChatBotRebateRetention
{
    using CRM.BackgroundJob.Core.Models;

    public class ImportRebateRetentionCommand : BaseNonQueryCommand
    {
        public string XmlData { get; set; }

        public string NewLogSID { get; set; }

        public string Environment { get; set; }

        public override string GetSettingKey() => "Acc_CRM_Chatbot_RebateRetention_Import";

        public override bool IsValid() => true;
    }
}
