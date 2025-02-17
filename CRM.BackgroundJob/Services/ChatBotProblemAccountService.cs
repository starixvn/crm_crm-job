namespace CRM.BackgroundJob.Services
{
    using CRM.BackgroundJob.Criteria.ChatbotProblemAccount;
    using CRM.BackgroundJob.Models.ChatBotProblemsAccount;
    using CRM.BackgroundJob.Core.Models;
    using CRM.BackgroundJob.Core.Services;
    using Fanex.CRM.BackgroundJob.Constants;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public interface IChatBotProblemAccountService
    {
        Task<IEnumerable<ChatBotProblemsAccountInfor>> GetChatbotProblemAccountInfoAsync(long LastLogId, int BatchSize);

        Task<ValueTuple<CommandResult, ProblemsAccountImportCommand>> ImportProblemsAccountInfoAsync(IEnumerable<ChatBotProblemsAccountInfor> problemsAccounts,
                                                  string Environment = BackgroundJobConstants.DefaultEnvironment);
    }

    public class ChatBotProblemAccountService : IChatBotProblemAccountService
    {
        private readonly IBaseService baseService;
        private readonly ICommonFeedback commonFeedback;

        public ChatBotProblemAccountService(IBaseService baseService, ICommonFeedback commonFeedback)
        {
            this.baseService = baseService;
            this.commonFeedback = commonFeedback;
        }

        public async Task<IEnumerable<ChatBotProblemsAccountInfor>> GetChatbotProblemAccountInfoAsync(long LastLogId, int BatchSize)
        {
            var criteria = new GetProblemsAccountByLogId
            {
                LastLogId = LastLogId,
                BatchSize = BatchSize
            };
            return await baseService.GetDataAsync<ChatBotProblemsAccountInfor>(criteria);
        }

        public async Task<ValueTuple<CommandResult, ProblemsAccountImportCommand>> ImportProblemsAccountInfoAsync(IEnumerable<ChatBotProblemsAccountInfor> problemsAccounts,
                                                  string Environment = BackgroundJobConstants.DefaultEnvironment)
        {
            const string XmlTemplate = "<r LogId='{0}' CustId='{1}' Username='{2}' " +
                "Recommend='{3}' Mrecommend='{4}' Srecommend='{5}' CategoryId='{6}' " +
                "CategoryNameEN='{7}' CategoryNameSC='{8}' CategoryNameTH='{9}' CategoryNameYN='{10}' " +
                "IsTest='{11}' />";
            var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag.ToLowerInvariant());
            foreach (ChatBotProblemsAccountInfor problemAccount in problemsAccounts)
            {
                var CategoryNameEN = commonFeedback.GetCategogyName(problemAccount.CategoryId.ToString(), Language.en_US);
                var CategoryNameSC = commonFeedback.GetCategogyName(problemAccount.CategoryId.ToString(), Language.zh_CN);
                var CategoryNameTH = commonFeedback.GetCategogyName(problemAccount.CategoryId.ToString(), Language.th_TH);
                var CategoryNameYN = commonFeedback.GetCategogyName(problemAccount.CategoryId.ToString(), Language.vi_VN);

                stringBuilder.AppendFormat(XmlTemplate,
                    problemAccount.LogId,
                    problemAccount.CustId,
                    problemAccount.UserName,
                    problemAccount.Recommend,
                    problemAccount.Mrecommend,
                    problemAccount.Srecommend,
                    problemAccount.CategoryId,
                    CategoryNameEN,
                    CategoryNameSC,
                    CategoryNameTH,
                    CategoryNameYN,
                    problemAccount.IsTest);
            }

            stringBuilder.Append(BackgroundJobConstants.EndRootTag.ToLowerInvariant());
            var criteria = new ProblemsAccountImportCommand
            {
                XmlData = stringBuilder.ToString(),
                Environment = Environment
            };
            var commandResult = await baseService.ExecuteNonQueryAsync(criteria);
            return new ValueTuple<CommandResult, ProblemsAccountImportCommand>(commandResult, criteria);
        }
    }
}
