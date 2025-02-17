
namespace CRM.BackgroundJob.Criteria.VoidingSettledTickets
{
    using Fanex.Data.Repository;

    public class AutoVoidingMessageUpdCommand : NonQueryCommand
    {
        public string MessageIds { get; set; }
        public string Environment { get; set; }

        public override string GetSettingKey() => "Acc_CRM_AutoVoidingMessageUpd";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(MessageIds);
    }
}
