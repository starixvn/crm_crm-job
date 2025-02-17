namespace Fanex.CRM.BackgroundJob.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fanex.CRM.BackgroundJob.Criteria;
    using Fanex.CRM.BackgroundJob.Models;
    using Fanex.Data.Repository;
    using global::CRM.BackgroundJob.Core.Services;
    using global::CRM.BackgroundJob.Criteria.AmendmentNotiFB;
    using global::CRM.BackgroundJob.Criteria.AutoFeedback;
    using global::CRM.BackgroundJob.Criteria.ProblemsAccount;
    using global::CRM.BackgroundJob.Criteria.TurnoverControll;
    using global::CRM.BackgroundJob.Criteria.WinLossLimit;
    using global::CRM.BackgroundJob.Models.AmendmentNotiFB;
    using global::CRM.BackgroundJob.Models.ProblemsAccount;
    using global::CRM.BackgroundJob.Models.TurnoverControll;
    using global::CRM.BackgroundJob.Models.WinLossLimit;
    using Microsoft.Extensions.Configuration;

    public interface ICustomerService
    {
        Task<CustomerInfo> GetCustomerInfoAsync(string username, bool isGuest);

        Task<IEnumerable<GroupHeadline>> GetGroupHeadlineAsync(IEnumerable<int> custIds);

        Task<IEnumerable<GroupHeadline>> GetGroupHeadlineVipAsync(IEnumerable<int> custIds);

        Task<IEnumerable<CustomerWinLimit>> GetListCustReachLimitAsync(WinLossSystemSetting setting);

        Task<IEnumerable<TONotification>> GetListSMANotificationAsync(WinLossSystemSetting turnOverSetting);

        Task<IEnumerable<CusReachAmendmentNotification>> GetListResultAmendmentNotificationAsync(AmendmentNotiFbSetting resultAmendmentSetting);

        Task<IEnumerable<CustOutStanding>> GetListCustOutStandingAsync(IEnumerable<TONotification> custNotifications);

        Task<IEnumerable<CustCategoryInfor>> GetCustCategoryInfor(ProblemsAccountSetting problemsAccountSetting, bool isUAT);
    }

    public class CustomerService : ICustomerService
    {
        private readonly IDynamicRepository dynamicRepository;
        private readonly IBaseService baseService;
        private readonly string Environment;

        private const string DefaultEnvironment = Constants.BackgroundJobConstants.DefaultEnvironment;

        public CustomerService(
            IDynamicRepository dynamicRepository,
            IBaseService baseService,
            IConfiguration configuration)
        {
            this.dynamicRepository = dynamicRepository;
            this.baseService = baseService;
            Environment = string.IsNullOrEmpty(configuration.GetValue<string>("SiteEnvironment")) ? DefaultEnvironment : configuration.GetValue<string>("SiteEnvironment");
        }

        public async Task<CustomerInfo> GetCustomerInfoAsync(string username, bool isGuest)
        {
            var criteria = new GetCustomerInfoByUsernameCriteria
            {
                Username = username,
                IsGuest = isGuest
            };
            var result = await dynamicRepository.FetchMultipleAsync<CustomerInfo, int>(criteria);
            var customer = result.Item1.FirstOrDefault();
            if (customer != null)
            {
                customer.LanguageId = result.Item2.FirstOrDefault();
            }

            return customer;
        }

        public async Task<IEnumerable<GroupHeadline>> GetGroupHeadlineAsync(IEnumerable<int> custIds)
        {
            var criteria = new GetGroupHeadlineCriteria
            {
                CustIds = string.Join(",", custIds)
            };
            return await baseService.GetDataAsync<GroupHeadline>(criteria);
        }

        public async Task<IEnumerable<GroupHeadline>> GetGroupHeadlineVipAsync(IEnumerable<int> custIds)
        {
            var criteria = new GetGroupHeadlineVipCriteria
            {
                CustIds = string.Join(",", custIds)
            };
            return await baseService.GetDataAsync<GroupHeadline>(criteria);
        }

        public async Task<IEnumerable<CustomerWinLimit>> GetListCustReachLimitAsync(WinLossSystemSetting setting)
        {
            var criteria = new GetListCustomerReachLimitCriteria
            {
                BatchSize = setting.BatchSize,
                ConfigUAT = setting.ConfigUAT,
                ConfigPRO = setting.ConfigPRO,
                LastLogID = setting.LastLogId,
                IsCredit = setting.IsCredit,
                IsDeposit = setting.IsDeposit,
                Environment = Environment
            };
            return await baseService.GetDataAsync<CustomerWinLimit>(criteria);
        }

        public async Task<IEnumerable<TONotification>> GetListSMANotificationAsync(WinLossSystemSetting turnOverSetting)
        {
            var criteria = new GetTOControlCustNotificationSelCriteria
            {
                LastLogID = turnOverSetting.LastLogId,
                BatchSize = turnOverSetting.BatchSize,
                ConfigUAT = turnOverSetting.ConfigUAT,
                ConfigPRO = turnOverSetting.ConfigPRO,
                IsCredit = turnOverSetting.IsCredit,
                IsDeposit = turnOverSetting.IsDeposit,
                Environment = Environment
            };
            return await baseService.GetDataAsync<TONotification>(criteria);
        }

        public async Task<IEnumerable<CusReachAmendmentNotification>> GetListResultAmendmentNotificationAsync(AmendmentNotiFbSetting resultAmendmentSetting)
        {
            var LastSeqEnv = Environment == DefaultEnvironment ? resultAmendmentSetting.LastSeq : resultAmendmentSetting.LastSeqUAT;
            var LastSubmitDateEnv = Environment == DefaultEnvironment ? resultAmendmentSetting.LastSubmitDate : resultAmendmentSetting.LastSubmitDateUAT;

            var criteria = new GetAmendmentNotificationSelCriteria
            {
                LastSeq = LastSeqEnv,
                LastSubmitDate = LastSubmitDateEnv,
                BatchSizeTicket = resultAmendmentSetting.BatchSizeTicket,
                ConfigUAT = resultAmendmentSetting.ConfigUAT,
                ConfigPRO = resultAmendmentSetting.ConfigPRO,
                IsCredit = resultAmendmentSetting.IsCredit,
                IsDeposit = resultAmendmentSetting.IsDeposit,
                Environment = Environment
            };
            return await baseService.GetDataAsync<CusReachAmendmentNotification>(criteria);
        }

        public async Task<IEnumerable<CustOutStanding>> GetListCustOutStandingAsync(IEnumerable<TONotification> custNotifications)
        {
            const string XmlTemplate = "<r custid='{0}' matchid='{1}'/>";
            var stringBuilder = new StringBuilder("<Root>");
            foreach (TONotification custNoti in custNotifications)
            {
                stringBuilder.AppendFormat(XmlTemplate, custNoti.CustID, custNoti.MatchID);
            }
            stringBuilder.Append("</Root>");

            var criteria = new GetTOControllCustOutstandingCriteria
            {
                xml = stringBuilder.ToString()
            };
            return await baseService.GetDataAsync<CustOutStanding>(criteria);
        }

        public async Task<IEnumerable<CustCategoryInfor>> GetCustCategoryInfor(ProblemsAccountSetting problemsAccountSetting, bool isUAT)
        {
            var criteria = new GetProblemsAccountCustCategoryCriteria
            {
                LastLogID = isUAT ? problemsAccountSetting.LastLogIdUAT : problemsAccountSetting.LastLogId,
                BatchSize = problemsAccountSetting.BatchSize,
                ConfigUAT = problemsAccountSetting.ConfigUAT,
                ConfigPRO = problemsAccountSetting.ConfigPRO,
                Environment = Environment
            };
            return await baseService.GetDataAsync<CustCategoryInfor>(criteria);
        }
    }
}