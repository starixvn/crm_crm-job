using Fanex.Data.Repository;

namespace CRM.BackgroundJob.Criteria.TurnoverControl;

public class EditTOControlAutoFeedbackInfoUpdCommand : NonQueryCommand
{
    public string LogID { get; set; }
    public string Status { get; set; }
    public string Description { get; set; }

    public override string GetSettingKey() => "Acc_CRM_TOControl_AutoFeedbackInfoUpd";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(LogID) && !string.IsNullOrWhiteSpace(Status);
}