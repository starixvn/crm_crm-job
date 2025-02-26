namespace CRM.Criteria.CaptureSettlementData;

using System;
using CRM.Criteria.Common;

public class GetSessionByDateCriteria : SiteDbBaseCriteria
{
    public string Environment { get; set; }

    public DateTime FDTime { get; set; }

    public override string GetSettingKey() => "MNC_SettlResultHist_GetSessionByDate";

    public override bool IsValid() => true;
}