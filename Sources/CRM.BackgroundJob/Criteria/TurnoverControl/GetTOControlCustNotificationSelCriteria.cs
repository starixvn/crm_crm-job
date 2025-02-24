using Fanex.Data.Repository;

namespace CRM.BackgroundJob.Criteria.TurnoverControl;

public class GetTOControlCustNotificationSelCriteria : CriteriaBase
{
    public long LastLogID { get; set; }
    public int BatchSize { get; set; }
    public bool ConfigUAT { get; set; }
    public bool ConfigPRO { get; set; }
    public bool IsCredit { get; set; }
    public bool IsDeposit { get; set; }
    public string Environment { get; set; }

    public override string GetSettingKey() => "Acc_CRM_TOControl_CustNotificationSel";

    public override bool IsValid() => BatchSize > 0;
}