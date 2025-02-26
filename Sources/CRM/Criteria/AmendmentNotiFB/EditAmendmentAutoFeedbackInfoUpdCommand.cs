namespace CRM.Criteria.AmendmentNotiFB;

using Fanex.Data.Repository;

public class EditAmendmentAutoFeedbackInfoUpdCommand : NonQueryCommand
{
    public int LogID { get; set; }
    public string Status { get; set; }
    public string Description { get; set; }

    public override string GetSettingKey() => "Acc_CRM_AmendmentNoti_AutoFeedbackInfoUpd";

    public override bool IsValid() => LogID > 0 && !string.IsNullOrWhiteSpace(Status);
}