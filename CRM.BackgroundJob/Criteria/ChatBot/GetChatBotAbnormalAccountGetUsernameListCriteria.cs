namespace CRM.BackgroundJob.Criteria.ChatBot
{
    using CRM.BackgroundJob.Core.Criteria;

    public class GetChatBotAbnormalAccountGetUsernameListCriteria : SiteDbBaseCriteria
    {
        public string Messages { get; set; }

        public override string GetSettingKey() => "Acc_CRM_ChatBot_AbnormalAccount_GetUsernameList";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(Messages) && SiteDBGroupID >= 0;
    }
}