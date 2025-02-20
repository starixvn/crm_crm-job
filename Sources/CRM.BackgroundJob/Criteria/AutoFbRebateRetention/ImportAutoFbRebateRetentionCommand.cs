namespace CRM.BackgroundJob.Criteria.AutoFbRebateRetention;

using CRM.Models;

public class ImportAutoFbRebateRetentionCommand : BaseNonQueryCommand
{
    public string XmlData { get; set; }

    public string NewLogSID { get; set; }

    public string Environment { get; set; }

    public override string GetSettingKey() => "Acc_CRM_AutoFB_RebateRetention_Import";

    public override bool IsValid() => true;
}