namespace CRM.BackgroundJob.UnitTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using CRM.Models;
    using CRM.Services;
    using CRM.BackgroundJob.Criteria.ChatBot_ArchivingDA;
    using CRM.BackgroundJob.Models.ChatBot;
    using CRM.BackgroundJob.Services;
    using CRM.Constants;
    using CRM.Extensions.Xml;
    using NSubstitute;
    using Xunit;
    using CRM.BackgroundJob.Constants;

    public class ChatBotArchivingDAServiceTests
    {
        private IBaseService baseService;

        [Fact]
        public async Task GetChatbotCTOSettingInfoAsync_WhenCalled_ReturnTOReachInfor()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture.Create<IEnumerable<ChatbotArchivingDAInfor>>();
            var service = GetService();
            baseService.GetDataAsync<ChatbotArchivingDAInfor>(Arg.Any<GETChatbotArchivedAccountsCriteria>()).Returns(expected);

            // Act
            var actual = await service.GetChatbotArchivingDASettingInfoAsync(0, 10);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ImportArchivedAccountsAsync_WhenCalled_ReturnExpectedCommandResult()
        {
            var fixture = new Fixture();
            var archivingDAInfors = fixture.Create<IEnumerable<ChatbotArchivingDAInfor>>();
            var expectedCommand = archivingDAInfors.ToXML("root").ToString();
            var service = GetService();
            var criteria = fixture.Create<ImportArchivedAccountsCommand>();

            var commandResult = fixture.Create<CommandResult>();
            baseService.ExecuteNonQueryAsync(Arg.Do<ImportArchivedAccountsCommand>(x => criteria = x)).Returns(commandResult);
            var valueTupe = new ValueTuple<CommandResult, ImportArchivedAccountsCommand>(commandResult, criteria);


            // Act
            var actual = await service.ImportArchivedAccountsAsync(archivingDAInfors, BackgroundJobConstants.DefaultEnvironment);

            // Assert
            Assert.Equal(valueTupe.Item1, actual.Item1);
            Assert.Equal(expectedCommand, criteria.XmlData);
        }

        #region Helpers

        private ChatBotArchivingDAService GetService()
        {
            baseService = Substitute.For<IBaseService>();
            return Substitute.For<ChatBotArchivingDAService>(baseService);
        }

        #endregion Helpers
    }
}
