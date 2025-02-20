namespace CRM.BackgroundJob.Criteria.AutoFeedback;

using Fanex.Data.Repository;

public class EditStatusVoidingMessageCommand : NonQueryCommand
{
    public string MessageIds { get; set; }

    public string Environment { get; set; }

    public override string GetSettingKey() => "Acc_CRM_VoidingMessageBatchUpd";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(MessageIds);
}