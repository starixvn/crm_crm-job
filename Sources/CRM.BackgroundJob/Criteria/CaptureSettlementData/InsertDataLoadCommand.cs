namespace CRM.BackgroundJob.Criteria.CaptureSettlementData;

using CRM.Models;

public class InsertDataLoadCommand : BaseNonQueryCommand
{
    public string Environment { get; set; }

    public string JSSettlResultHist { get; set; }

    public override string GetSettingKey() => "MNC_SettlResultHist_DataLoad";

    public override bool IsValid() => true;
}