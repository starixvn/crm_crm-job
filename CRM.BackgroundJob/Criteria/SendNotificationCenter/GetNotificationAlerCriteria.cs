namespace CRM.BackgroundJob.Criteria.SendNotificationCenter
{
    using Fanex.Data.Repository;

    public class GetNotificationAlerCriteria : CriteriaBase
    {
        public string Environment { get; set; }

        public override string GetSettingKey() => "MNC_Notification_AlertSel";

        public override bool IsValid() => true;
    }
}
