namespace CRM.BackgroundJob.Criteria.AutoFbMessageNotificationCenter;

using CRM.Models;

public class ImportMessageNotificationCenterCommand : BaseNonQueryCommand
{
    public string MsgInfo { get; set; }

    public string Environment { get; set; }

    public string IP { get; set; } = string.Empty;

    public override string GetSettingKey() => "Acc_CRM_AutoFB_MessageNotificationCenter_Import";

    public override bool IsValid() => true;
}