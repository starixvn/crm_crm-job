

using Fanex.Data.Repository;

namespace CRM.BackgroundJob.Criteria.ArchivingDA
{
    public class GetSystemSettingByServiceNameCriteria : CriteriaBase
    {
        public string ServiceName { get; set; }
        public override string GetSettingKey() => "Acc_CRM_Service_SystemSettingSel";

        public override bool IsValid() => true;
    }
}
