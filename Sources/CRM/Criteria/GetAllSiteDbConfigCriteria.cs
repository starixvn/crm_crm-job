namespace CRM.Criteria;

using Fanex.Data.Repository;

public class GetAllSiteDbConfigCriteria : CriteriaBase
{
    public override string GetSettingKey() => "Acc_SiteDBConfigSel";

    public override bool IsValid() => true;
}