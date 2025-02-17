namespace CRM.BackgroundJob.Core.Services
{
    using CRM.BackgroundJob.Core.Criteria;
    using CRM.BackgroundJob.Core.Models;
    using CRM.BackgroundJob.Core.Repositories;
    using Fanex.Caching;
    using Fanex.Data;
    using Fanex.Data.Repository;
    using Fanex.Data.SqlServer;
    using Fanex.Logging.Extensions;
    using Fanex.Logging.Extensions.Database;
    using Microsoft.Data.SqlClient;

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

    public class BaseService : IBaseService
    {
        private readonly IDynamicRepository dynamicRepository;
        private readonly IBaseRepository baseRepository;
        private readonly ICacheService cacheService;
        private readonly ICrmLogService logService;
        private const string Config_SiteDb_Key = "ConfigSiteDbKey";
        private const int Command_Timeout = 120;
        private const bool Ignore_Redundant_Parameters = false;

        public BaseService(
            IDynamicRepository dynamicRepository,
            IBaseRepository baseRepository,
            ICacheService cacheService,
            ICrmLogService logService)
        {
            this.dynamicRepository = dynamicRepository;
            this.baseRepository = baseRepository;
            this.cacheService = cacheService;
            this.logService = logService;
        }

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
                cacheService.Set(Config_SiteDb_Key, siteConfigs);
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
                //(var sqlRunner, var dbCommand) = CreateNewDbcommand(ipConfig, connectionAlias, criteria);
                //return await sqlRunner.QueryAsync<T>(dbCommand);
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

        private static ValueTuple<SqlStoredProcedureRunner, DbCommandDefinition> CreateNewDbcommand(string ipConfig, string connectionAlias, IBaseCriteriaExtend criteria)
        {
            var dbsettingProvider = DbSettingProviderManager.GetCurrentDbSettingProvider();
            dbsettingProvider.SettingDataSource.TryGetValue(criteria.GetSettingKey(), out IDbSetting oldDbSetting);
            var newSqlConnectionStringBuilder = new SqlConnectionStringBuilder(oldDbSetting.ConnectionString);
            newSqlConnectionStringBuilder.DataSource = ipConfig;
            var newSqlRunner = new SqlStoredProcedureRunner(connectionAlias, newSqlConnectionStringBuilder.ToString());
            var newDbCommand = newSqlRunner.CreateDbCommandDefinition(oldDbSetting.SpName, Command_Timeout, Ignore_Redundant_Parameters, oldDbSetting.ColumnMaps);
            newDbCommand.AssignParameters(criteria);
            return new ValueTuple<SqlStoredProcedureRunner, DbCommandDefinition>(newSqlRunner, newDbCommand);
        }

        public async Task<T> GetDataEntityAsync<T>(ICriteria criteria)
        {
            if (criteria != null && criteria.IsValid())
            {
                return await dynamicRepository.GetAsync<T>(criteria);
            }

            return default(T);
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
}