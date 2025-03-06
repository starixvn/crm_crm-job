namespace CRM.Criteria.AbnormalBet;

using Fanex.Data.Repository;

public class GetAbnormalBetVoidedInfoCriteria : CriteriaBase
{
    public int LastVoidingHistoryCollectionId { get; set; }

    public int BatchSize { get; set; }

    public int MaxVoidingHistoryCollectionId { get; set; }

    public override string GetSettingKey() => "Acc_CRM_AutoFB_AbnormalBets_GetVoidedInfo";

    public override bool IsValid() => BatchSize > 0;
}