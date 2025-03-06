namespace CRM.UnitTests.Services.AutoChatBotMessage
{
    using AutoFixture;
    using CRM.Constants;
    using CRM.Criteria.ChatBot_ArchivingDA;
    using CRM.Extensions.Xml;
    using CRM.Models.ChatBot;
    using CRM.Services.AutoChatBotMessage;
    using CRM.Services.Common;
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

            var commandResult = fixture.Create<CRM.Models.Common.CommandResult>();
            baseService.ExecuteNonQueryAsync(Arg.Do<ImportArchivedAccountsCommand>(x => criteria = x)).Returns(commandResult);
            var valueTuple = new ValueTuple<CRM.Models.Common.CommandResult, ImportArchivedAccountsCommand>(commandResult, criteria);

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