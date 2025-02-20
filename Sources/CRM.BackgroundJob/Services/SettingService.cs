namespace CRM.BackgroundJob.Services;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Constants;
using Criteria;
using Criteria.AutoFeedback;
using CRM.Services;
using Models;

public interface ISettingService
{
    Task<IEnumerable<SettingItem>> GetAllSettingAsync();

    Task<IEnumerable<SystemSettingMapping>> GetSystemSettingMappingAsync(string[] settingNames);

    Task<IEnumerable<SiteSettingMapping>> GetSiteSettingMappingAsync();
}

public class SettingService(IBaseService baseService) : ISettingService
{
    public async Task<IEnumerable<SettingItem>> GetAllSettingAsync()
    {
        var criteria = new GetAllSettingCriteria();
        var settings = await baseService.GetDataAsync<SettingItem>(criteria);
        if (settings.Any())
        {
            settings = settings.Where(x => x.IsActive);
            var relations = await GetSettingRelationAsync();
            foreach (var setting in settings)
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