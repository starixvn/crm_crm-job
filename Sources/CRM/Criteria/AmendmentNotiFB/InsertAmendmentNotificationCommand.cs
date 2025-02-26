namespace CRM.Criteria.AmendmentNotiFB;

using Fanex.Data.Repository;

public class InsertAmendmentNotificationCommand : NonQueryCommand
{
    public string Xml { get; set; }
    public string Environment { get; set; }
    public int ErrCode { get; set; }
    public string ErrMsg { get; set; }
    public override string GetSettingKey() => "Acc_CRM_AmendmentNoti_AutoFeedbackInfoIns";
    public override bool IsValid() => !string.IsNullOrWhiteSpace(Xml);
}