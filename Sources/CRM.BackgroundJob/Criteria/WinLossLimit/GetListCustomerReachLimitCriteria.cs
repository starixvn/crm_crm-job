namespace CRM.BackgroundJob.Criteria.WinLossLimit;

using Fanex.Data.Repository;
public class GetListCustomerReachLimitCriteria : CriteriaBase
{
    public int BatchSize { get; set; }
    public bool ConfigUAT { get; set; }
    public bool ConfigPRO { get; set; }
    public long LastLogID { get; set; }
    public bool IsCredit { get; set; }
    public bool IsDeposit { get; set; }
    public string Environment { get; set; }
    public override string GetSettingKey() => "Acc_CRM_CustWinLimitSel";

    public override bool IsValid() => LastLogID > 0 && BatchSize > 0;
}