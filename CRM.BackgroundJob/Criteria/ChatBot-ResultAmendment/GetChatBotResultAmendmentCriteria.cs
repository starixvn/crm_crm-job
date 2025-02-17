namespace CRM.BackgroundJob.Criteria.ChatBot_ResultAmendment
{
    using Fanex.Data.Repository;
    public class GetChatBotResultAmendmentCriteria : CriteriaBase
    {
        public int BatchSize { get; set; }
        public string SubmitDate { get; set; }
        public int Seq { get; set; }
        public string ServerName { get; set; }
        public override string GetSettingKey() => "Acc_CRM_Chatbot_ResultAmendment_Sel";
        public override bool IsValid() => true;
    }
}
