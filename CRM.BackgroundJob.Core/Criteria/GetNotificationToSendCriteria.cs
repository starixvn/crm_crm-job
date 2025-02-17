namespace CRM.BackgroundJob.Core.Criteria
{
    using Fanex.Data.Repository;

    public class GetNotificationToSendCriteria : CriteriaBase
    {
        public override string GetSettingKey() => "Acc_CRM_Chatbot_SendNotification";

        public override bool IsValid() => true;
    }
}