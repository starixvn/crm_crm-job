namespace CRM.BackgroundJob.UnitTests.Services
{
    using AutoFixture;
    using CRM.BackgroundJob.Constants;
    using CRM.BackgroundJob.Criteria.ChatBot_ArchivingDA;
    using CRM.BackgroundJob.Models.ChatBot;
    using CRM.BackgroundJob.Services;
    using CRM.Extensions.Xml;
    using CRM.Models;
    using CRM.Services;
    using NSubstitute;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;

    public class ChatBotArchivingDAServiceTests
    {
        private IBaseService baseService;

        [Fact]
        public async Task GetChatbotCTOSettingInfoAsync_WhenCalled_ReturnTOReachInfo()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture.Create<IEnumerable<ChatbotArchivingDAInfo>>();
            var service = GetService();
            baseService.GetDataAsync<ChatbotArchivingDAInfo>(Arg.Any<GETChatbotArchivedAccountsCriteria>()).Returns(expected);

            // Act
            var actual = await service.GetChatbotArchivingDASettingInfoAsync(0, 10);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ImportArchivedAccountsAsync_WhenCalled_ReturnExpectedCommandResult()
        {
            var fixture = new Fixture();
            var archivingDAInfo = fixture.Create<IEnumerable<ChatbotArchivingDAInfo>>();
            var expectedCommand = archivingDAInfo.ToXML("root").ToString();
            var service = GetService();
            var criteria = fixture.Create<ImportArchivedAccountsCommand>();

            var commandResult = fixture.Create<CommandResult>();
            baseService.ExecuteNonQueryAsync(Arg.Do<ImportArchivedAccountsCommand>(x => criteria = x)).Returns(commandResult);
            var valueTuple = new ValueTuple<CommandResult, ImportArchivedAccountsCommand>(commandResult, criteria);

            // Act
            var actual = await service.ImportArchivedAccountsAsync(archivingDAInfo, BackgroundJobConstants.DefaultEnvironment);

            // Assert
            Assert.Equal(valueTuple.Item1, actual.Item1);
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