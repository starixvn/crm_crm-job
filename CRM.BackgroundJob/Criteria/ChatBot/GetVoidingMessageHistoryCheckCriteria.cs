
namespace CRM.BackgroundJob.Criteria.ChatBot
{
     using Fanex.Data.Repository;

    public class GetVoidingMessageHistoryCheckCriteria : CriteriaBase
    {
        public string Messages { get; set; }

        public override string GetSettingKey() => "Acc_CRM_crmMessageImport_Check";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(Messages);
    }
}

