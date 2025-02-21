namespace CRM.Services;

using Criteria;
using Fanex.Caching;
using Fanex.Data.Repository;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.Database;
using Models;
using Repositories;

public interface IBaseService
{
    Task<IEnumerable<T>> GetDataAsync<T>(ICriteria criteria);

    Task<IEnumerable<SiteDbConfig>> GetSiteDbConfigAsync();

    Task<IEnumerable<T>> GetDataByIpConfigAsync<T>(string ipConfig, string connectionAlias, IBaseCriteriaExtend criteria);

    Task<IEnumerable<T>> GetDataFromSiteDbAsync<T>(IBaseCriteriaExtend criteria);

    Task<T> GetDataEntityAsync<T>(ICriteria criteria);

    Task<ValueTuple<IEnumerable<TFirst>, IEnumerable<TSecond>>> FetchMultipleAsync<TFirst, TSecond>(ICriteria criteria);

    Task<ValueTuple<IEnumerable<TFirst>, IEnumerable<TSecond>, IEnumerable<TThird>>> FetchMultipleAsync<TFirst, TSecond, TThird>(ICriteria criteria);

    Task<CommandResult> ExecuteNonQueryAsync(INonQueryCommand command);
}

public class BaseService(
    IDynamicRepository dynamicRepository,
    IBaseRepository baseRepository,
    ICacheService cacheService,
    ICrmLogService logService)
    : IBaseService
{
    private const string Config_SiteDb_Key = "ConfigSiteDbKey";

    public async Task<IEnumerable<T>> GetDataAsync<T>(ICriteria criteria)
    {
        if (criteria != null && criteria.IsValid())
        {
            return await dynamicRepository.FetchAsync<T>(criteria);
        }

        return new List<T>();
    }

    public virtual async Task<IEnumerable<SiteDbConfig>> GetSiteDbConfigAsync()
    {
        var siteConfigs = cacheService.Get<IEnumerable<SiteDbConfig>>(Config_SiteDb_Key);
        if (siteConfigs == null || !siteConfigs.Any())
        {
            siteConfigs = await GetDataAsync<SiteDbConfig>(new GetAllSiteDbConfigCriteria());
            await cacheService.SetAsync(Config_SiteDb_Key, siteConfigs);
        }
        return siteConfigs;
    }

    public async Task<IEnumerable<T>> GetDataFromSiteDbAsync<T>(IBaseCriteriaExtend criteria)
    {
        var siteConfigs = await GetSiteDbConfigAsync();
        var results = new List<T>();
        foreach (var siteConfig in siteConfigs)
        {
            criteria.SiteDBGroupID = siteConfig.SiteDBGroupID;
            var result = await GetDataByIpConfigAsync<T>(siteConfig.AGIP, siteConfig.LinkedServerName, criteria);
            results.AddRange(result);
        }

        return results;
    }

    public virtual async Task<IEnumerable<T>> GetDataByIpConfigAsync<T>(string ipConfig, string connectionAlias, IBaseCriteriaExtend criteria)
    {
        try
        {
            return await dynamicRepository.FetchAsync<T>(criteria, ipConfig);
        }
        catch (Exception ex)
        {
            var taskEmpty = Enumerable.Empty<T>();
            var errorMessage = new LogMessage(ex.Message)
                .AddSection("Database Info", DatabaseInfoGetters.GetDbExceptionInfo(ex))
                .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
                .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(ex))
                .AddMessage("Ipconfig: " + ipConfig)
                .ToString();
            await logService.ErrorMessageAsync(errorMessage);
            return taskEmpty;
        }
    }

    public async Task<T> GetDataEntityAsync<T>(ICriteria criteria)
    {
        if (criteria != null && criteria.IsValid())
        {
            return await dynamicRepository.GetAsync<T>(criteria);
        }

        return default;
    }

    public async Task<ValueTuple<IEnumerable<TFirst>, IEnumerable<TSecond>>> FetchMultipleAsync<TFirst, TSecond>(ICriteria criteria)
    {
        if (criteria != null && criteria.IsValid())
        {
            return await dynamicRepository.FetchMultipleAsync<TFirst, TSecond>(criteria);
        }

        return new ValueTuple<IEnumerable<TFirst>, IEnumerable<TSecond>>(new List<TFirst>(), new List<TSecond>());
    }

    public async Task<(IEnumerable<TFirst>, IEnumerable<TSecond>, IEnumerable<TThird>)> FetchMultipleAsync<TFirst, TSecond, TThird>(ICriteria criteria)
    {
        if (criteria != null && criteria.IsValid())
        {
            return await dynamicRepository.FetchMultipleAsync<TFirst, TSecond, TThird>(criteria);
        }

        return new ValueTuple<IEnumerable<TFirst>, IEnumerable<TSecond>, IEnumerable<TThird>>(new List<TFirst>(), new List<TSecond>(), new List<TThird>());
    }

    public async Task<CommandResult> ExecuteNonQueryAsync(INonQueryCommand command)
    {
        var commandResult = new CommandResult();
        if (command != null && command.IsValid())
        {
            commandResult = await baseRepository.ExecuteNonQueryAsync(command);
        }

        return commandResult;
    }
}