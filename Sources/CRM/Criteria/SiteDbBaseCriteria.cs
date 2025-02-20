namespace CRM.Criteria;

public abstract class SiteDbBaseCriteria : IBaseCriteriaExtend
{
    public int SiteDBGroupID { get; set; }

    public abstract string GetSettingKey();

    public abstract bool IsValid();

    public bool IsInvalid()
    {
        return !IsValid();
    }
}