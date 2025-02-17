
namespace CRM.BackgroundJob.Criteria.ChatBot_ResultAmendment
{
    using Fanex.Data.Repository;
    public class GetChatbotVoidingMessageCheck : CriteriaBase
    {
        public string Message { get; set; }
        public string Environment { get; set; }
        public override string GetSettingKey() => "Acc_CRM_VodingMessage_Check";
        public override bool IsValid() => !string.IsNullOrEmpty(Message);
    }
}
