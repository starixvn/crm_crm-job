
namespace CRM.BackgroundJob.Criteria.SynchronizeLicensee;

using Fanex.Data.Repository;

public class GetLicenseeStatus : CriteriaBase
{
    public override string GetSettingKey() => "Acc_CRM_OICLicenseeSel";
    public override bool IsValid() => true;
}