namespace CRM.BackgroundJob.UnitTests.Services
{
    using AutoFixture;
    using CRM.BackgroundJob.Constants;
    using CRM.BackgroundJob.Criteria.ChatbotProblemAccount;
    using CRM.BackgroundJob.Models.ChatBotProblemsAccount;
    using CRM.BackgroundJob.Services;
    using CRM.Models;
    using CRM.Services;
    using NSubstitute;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;

    public class ChatBotProblemAccountServiceTests
    {
        private IBaseService baseService;
        private ICommonFeedback commonFeedback;

        [Fact]
        public async Task GetChatbotProblemAccountInfoAsync_WhenCalled_ReturnExpectedMessages()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture.Create<IEnumerable<ChatBotProblemsAccountInfo>>();
            var service = GetService();
            baseService.GetDataAsync<ChatBotProblemsAccountInfo>(Arg.Any<GetProblemsAccountByLogId>()).Returns(expected);

            // Act
            var actual = await service.GetChatbotProblemAccountInfoAsync(0, 10);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ImportProblemsAccountInfoAsync_WhenCalled_ReturnExpectedCommandResult()
        {
            // Arrange
            var fixture = new Fixture();
            var service = GetService();
            var problemsAccounts = new List<ChatBotProblemsAccountInfo>
            {
                new()
                {
                    LogId =1,
                    UserName = "1",
                    CustId = 1,
                    Recommend = 1,
                    Mrecommend = 1,
                    Srecommend = 1,
                    CategoryId = 1,
                    IsTest = 0
                }
            };

            const string XmlTemplate = "<r LogId='{0}' CustId='{1}' Username='{2}' " +
                    "Recommend='{3}' Mrecommend='{4}' Srecommend='{5}' CategoryId='{6}' " +
                    "CategoryNameEN='{7}' CategoryNameSC='{8}' CategoryNameTH='{9}' CategoryNameYN='{10}' " +
                    "IsTest='{11}' />";
            var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag.ToLowerInvariant());
            var problemsAccount = problemsAccounts.First();

            commonFeedback.GetCategoryName(Arg.Any<string>(), "en-US").Returns("a");
            commonFeedback.GetCategoryName(Arg.Any<string>(), "zh-CN").Returns("b");
            commonFeedback.GetCategoryName(Arg.Any<string>(), "th-TH").Returns("c");
            commonFeedback.GetCategoryName(Arg.Any<string>(), "vi-VN").Returns("d");
            stringBuilder.AppendFormat(XmlTemplate,
                    problemsAccount.LogId,
                    problemsAccount.CustId,
                    problemsAccount.UserName,
                    problemsAccount.Recommend,
                    problemsAccount.Mrecommend,
                    problemsAccount.Srecommend,
                    problemsAccount.CategoryId,
                    'a',
                    'b',
                    'c',
                    'd',
                    problemsAccount.IsTest);
            stringBuilder.Append(BackgroundJobConstants.EndRootTag.ToLowerInvariant());
            var criteria = new ProblemsAccountImportCommand();
            var commandResult = fixture.Create<CommandResult>();
            var valueTuple = new ValueTuple<CommandResult, ProblemsAccountImportCommand>(commandResult, criteria);
            baseService.ExecuteNonQueryAsync(Arg.Do<ProblemsAccountImportCommand>(x => criteria = x)).Returns(valueTuple.Item1);

            // Act
            var actual = await service.ImportProblemsAccountInfoAsync(problemsAccounts);

            // Assert
            Assert.Equal(valueTuple.Item1, actual.Item1);
            Assert.Equal(stringBuilder.ToString(), criteria.XmlData);
            Assert.Equal(BackgroundJobConstants.DefaultEnvironment, criteria.Environment);
            Assert.Equal(0, criteria.ErrCode);
            Assert.Null(criteria.ErrMsg);
        }

        #region Helpers

        private ChatBotProblemAccountService GetService()
        {
            baseService = Substitute.For<IBaseService>();
            commonFeedback = Substitute.For<ICommonFeedback>();
            return new ChatBotProblemAccountService(baseService, commonFeedback);
        }

        #endregion Helpers
    }
}