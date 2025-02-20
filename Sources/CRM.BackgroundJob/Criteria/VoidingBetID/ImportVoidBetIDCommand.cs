namespace CRM.BackgroundJob.Criteria.VoidingBetID;

using CRM.Models;

public class ImportVoidBetIDCommand : BaseNonQueryCommand
{
    public string Messages { get; set; }

    public string Environment { get; set; }

    public string IP { get; set; } = string.Empty;

    public int MAXMsgTargetId { get; set; }

    public override string GetSettingKey() => "Acc_CRM_AutoFB_VoidByBetID_Import";

    public override bool IsValid() => true;
}