namespace CRM.BackgroundJob.Criteria.WinLossLimit
{
    using Fanex.Data.Repository;
    public class GetWinLossSystemSettingCriteria : CriteriaBase
    {
        public override string GetSettingKey() => "Acc_CRM_WinLimit_SystemSettingSel";

        public override bool IsValid() => true;
    }
}
