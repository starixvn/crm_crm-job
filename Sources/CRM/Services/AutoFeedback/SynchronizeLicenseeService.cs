namespace CRM.Services.AutoFeedback;

using CRM.Constants;
using CRM.Criteria.SynchronizeLicensee;
using CRM.Models.LicenseeInfo;
using CRM.Services.Common;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public interface ISynchronizeLicenseeService
{
    Task<IEnumerable<LicenseeStatus>> GetOICLicenseeInfo();

    Task<CRM.Models.Common.CommandResult> ImportLicenseeStatus(IEnumerable<LicenseeStatus> licensees, string Environment);
}

public class SynchronizeLicenseeService(IBaseService baseService) : ISynchronizeLicenseeService
{
    public async Task<IEnumerable<LicenseeStatus>> GetOICLicenseeInfo()
    {
        var criteria = new GetLicenseeStatus();
        var licensees = await baseService.GetDataAsync<LicenseeStatus>(criteria);
        return licensees;
    }

    public async Task<CRM.Models.Common.CommandResult> ImportLicenseeStatus(IEnumerable<LicenseeStatus> licensees, string Environment)
    {
        const string XmlTemplate = "<r Name='{0}' status='{1}'/>";
        var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);
        foreach (var licensee in licensees)
        {
            stringBuilder.AppendFormat(XmlTemplate, licensee.LicenseeName, licensee.StatusId);
        }
        stringBuilder.Append(BackgroundJobConstants.EndRootTag);
        var criteria = new ImportLicenseeStatusCommand
        {
            Licensees = stringBuilder.ToString(),
            Environment = Environment
        };
        return await baseService.ExecuteNonQueryAsync(criteria);
    }
}