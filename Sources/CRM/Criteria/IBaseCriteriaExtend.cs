namespace CRM.Criteria;

using Fanex.Data.Repository;

public interface IBaseCriteriaExtend : ICriteria
{
    int SiteDBGroupID { get; set; }
}