namespace CRM.BackgroundJob.Criteria;

using Fanex.Data.Repository;

public class GetSystemSettingCriteria : CriteriaBase
{
    public string ServiceName { get; set; }

    public string Environment { get; set; }

    public override string GetSettingKey() => "Acc_CRM_AutoFB_SystemSetting_Sel";

    public override bool IsValid() => true;
}