namespace Fanex.CRM.BackgroundJob.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.CRM.BackgroundJob.Constants;
    using Fanex.CRM.BackgroundJob.Criteria;
    using Fanex.CRM.BackgroundJob.Models;
    using global::CRM.BackgroundJob.Core.Services;

    public interface ISettingService
    {
        Task<IEnumerable<SettingItem>> GetAllSettingAsync();

        Task<IEnumerable<SystemSettingMapping>> GetSystemSettingMappingAsync(string[] settingNames);

        Task<IEnumerable<SiteSettingMapping>> GetSiteSettingMappingAsync();
    }

    public class SettingService : ISettingService
    {
        private readonly IBaseService baseService;

        public SettingService(IBaseService baseService)
        {
            this.baseService = baseService;
        }

        public async Task<IEnumerable<SettingItem>> GetAllSettingAsync()
        {
            var criteria = new GetAllSettingCriteria();
            var settings = await baseService.GetDataAsync<SettingItem>(criteria);
            if (settings.Any())
            {
                settings = settings.Where(x => x.IsActive);
                IEnumerable<SettingItem> relations = await GetSettingRelationAsync();
                foreach (SettingItem setting in settings)
                {
                    setting.ParentIds = relations.Where(x => x.SettingId == setting.SettingId).Select(x => x.ParentId);
                }
            }

            return settings;
        }

        public async Task<IEnumerable<SystemSettingMapping>> GetSystemSettingMappingAsync(string[] settingNames)
        {
            var criteria = new GetSystemSettingMappingCriteria
            {
                Settings = string.Join(",", settingNames)
            };
            return await baseService.GetDataAsync<SystemSettingMapping>(criteria);
        }

        public async Task<IEnumerable<SiteSettingMapping>> GetSiteSettingMappingAsync()
        {
            var criteria = new GetSystemSettingMappingCriteria
            {
                Settings = SettingName.Site
            };
            return await baseService.GetDataAsync<SiteSettingMapping>(criteria);
        }

        private async Task<IEnumerable<SettingItem>> GetSettingRelationAsync()
        {
            var criteria = new GetAllSettingRelationCriteria();
            return await baseService.GetDataAsync<SettingItem>(criteria);
        }
    }
}