namespace CRM.BackgroundJob.Core.Criteria
{
    using CRM.BackgroundJob.Core.Models;

    public class UpdateNotificationStatusCommand : BaseNonQueryCommand
    {
        public string XmlData { get; set; }

        public override string GetSettingKey() => "Acc_CRM_ChatBot_SendNotification_Status_Upd";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(XmlData);
    }
}