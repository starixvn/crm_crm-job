
namespace CRM.BackgroundJob.Services
{
    using CRM.BackgroundJob.Criteria.SynchronizeLicensee;
    using CRM.BackgroundJob.Models.LicenseeInfo;
    using CRM.BackgroundJob.Core.Models;
    using CRM.BackgroundJob.Core.Services;
    using Fanex.CRM.BackgroundJob.Constants;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public interface ISynchronizeLicenseeService
    {
        Task<IEnumerable<LicenseeStatus>> GetOICLicenseeInfo();
        Task<CommandResult> ImportLicenseeStatus(IEnumerable<LicenseeStatus> licensees, string Environment);
    }
    public class SynchronizeLicenseeService : ISynchronizeLicenseeService
    {
        private readonly IBaseService baseService;

        public SynchronizeLicenseeService(
            IBaseService baseService
            )
        {
            this.baseService = baseService;
        }

        public async Task<IEnumerable<LicenseeStatus>> GetOICLicenseeInfo()
        {
            var criteria = new GetLicenseeStatus();
            var licensees = await baseService.GetDataAsync<LicenseeStatus>(criteria);
            return licensees;
        }

        public async Task<CommandResult> ImportLicenseeStatus(IEnumerable<LicenseeStatus> licensees, string Environment)
        {
            const string XmlTemplate = "<r Name='{0}' status='{1}'/>";
            var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag);
            foreach(var licensee in licensees)
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
}
