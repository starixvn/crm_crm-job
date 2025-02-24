namespace CRM.BackgroundJob.Criteria.CaptureSettlementData;

using Fanex.Data.Repository;

public class GetInitialSiteDBConfigCriteria : CriteriaBase
{
    public string Environment { get; set; }

    public override string GetSettingKey() => "MNC_SettlResultHist_Initial";

    public override bool IsValid() => true;
}