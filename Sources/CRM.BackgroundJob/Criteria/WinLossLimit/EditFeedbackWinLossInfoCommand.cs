namespace CRM.BackgroundJob.Criteria.WinLossLimit;

using Fanex.Data.Repository;

public class EditFeedbackWinLossInfoCommand : NonQueryCommand
{
    public int LogID { get; set; }
    public string Status { get; set; }
    public string Description { get; set; }

    public override string GetSettingKey() => "Acc_CRM_WinLimit_AutoFeedbackInfoUpd";

    public override bool IsValid() => true;
}