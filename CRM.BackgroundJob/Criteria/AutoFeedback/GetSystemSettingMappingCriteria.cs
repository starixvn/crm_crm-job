namespace Fanex.CRM.BackgroundJob.Criteria
{
    using Fanex.Data.Repository;

    public class GetSystemSettingMappingCriteria : CriteriaBase
    {
        public string Settings { get; set; }

        public override string GetSettingKey() => "Acc_CRM_SystemSettingMappingSel";

        public override bool IsValid() => !string.IsNullOrWhiteSpace(Settings);
    }
}