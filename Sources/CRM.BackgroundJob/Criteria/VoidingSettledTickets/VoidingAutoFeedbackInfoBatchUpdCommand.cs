namespace CRM.BackgroundJob.Criteria.VoidingSettledTickets;

using Fanex.Data.Repository;

public class VoidingAutoFeedbackInfoBatchUpdCommand : NonQueryCommand
{
    public int LogVoidingID { get; set; }
    public string Status { get; set; }
    public string Description { get; set; }
    public string Environment { get; set; }

    public override string GetSettingKey() => "Acc_CRM_VoidingAutoFeedbackInfo_BatchUpd";

    public override bool IsValid() => LogVoidingID > 0 && !string.IsNullOrWhiteSpace(Status) && !string.IsNullOrWhiteSpace(Description);
}