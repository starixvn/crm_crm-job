namespace CRM.Criteria.AutoFbMessageNotificationCenter;

public class ImportMessageNotificationCenterCommand : CRM.Models.Common.BaseNonQueryCommand
{
    public string MsgInfo { get; set; }

    public string Environment { get; set; }

    public string IP { get; set; } = string.Empty;

    public override string GetSettingKey() => "Acc_CRM_AutoFB_MessageNotificationCenter_Import";

    public override bool IsValid() => true;
}