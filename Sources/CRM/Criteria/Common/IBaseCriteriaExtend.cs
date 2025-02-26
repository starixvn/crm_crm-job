namespace CRM.Criteria.Common;

using Fanex.Data.Repository;

public interface IBaseCriteriaExtend : ICriteria
{
    int SiteDBGroupID { get; set; }
}