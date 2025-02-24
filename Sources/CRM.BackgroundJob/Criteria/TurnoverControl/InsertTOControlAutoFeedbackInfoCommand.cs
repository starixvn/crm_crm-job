using Fanex.Data.Repository;

namespace CRM.BackgroundJob.Criteria.TurnoverControl;

public class InsertTOControlAutoFeedbackInfoCommand : NonQueryCommand
{
    public string xml { get; set; }

    public override string GetSettingKey() => "Acc_CRM_TOControl_AutoFeedbackInfoIns";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(xml);
}