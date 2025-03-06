namespace CRM.Criteria.VoidingBetID;

public class ImportVoidBetIDCommand : CRM.Models.Common.BaseNonQueryCommand
{
    public string Messages { get; set; }

    public string Environment { get; set; }

    public string IP { get; set; } = string.Empty;

    public int MAXMsgTargetId { get; set; }

    public override string GetSettingKey() => "Acc_CRM_AutoFB_VoidByBetID_Import";

    public override bool IsValid() => true;
}