namespace CRM.BackgroundJob.UnitTests.Services
{
    using AutoFixture;
    using CRM.BackgroundJob.Constants;
    using CRM.BackgroundJob.Criteria.ChatBot_CTO;
    using CRM.BackgroundJob.Models.ChatBotCTO;
    using CRM.BackgroundJob.Services;
    using CRM.Models;
    using CRM.Services;
    using NSubstitute;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;

    public class ChatBotCTOServiceTests
    {
        private IBaseService baseService;
        private ICommonFeedback commonFeedback;

        [Fact]
        public async Task GetChatbotCTOSettingInfoAsync_WhenCalled_ReturnTOReachInfo()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture.Create<IEnumerable<TOReachInfo>>();
            var service = GetService();
            baseService.GetDataAsync<TOReachInfo>(Arg.Any<GetTOControlSettingByLastLogIdCriteria>()).Returns(expected);

            // Act
            var actual = await service.GetChatbotCTOSettingInfoAsync(0, 10);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task GetChatbotCTOSMAInfoAsync_WhenCalled_ReturnSMAInfo()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture.Create<IEnumerable<TurnoverSMAInfo>>();
            var tOReachInfo = new List<TOReachInfo>
            {
                new() { MatchId = 1, CustId = 1, LogId = 1}
            };
            var service = GetService();
            baseService.GetDataAsync<TurnoverSMAInfo>(Arg.Any<GetTOControlSMAInfoCriteria>()).Returns(expected);

            // Act
            var actual = await service.GetChatbotCTOSMAInfoAsync(tOReachInfo);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ImportTOSMAInfoAsync_WhenCalled_ReturnExpectedCommandResult()
        {
            // Arrange
            var fixture = new Fixture();
            var service = GetService();
            var tOSMAInfo = new List<TurnoverSMAInfo>
            {
                fixture.Create<TurnoverSMAInfo>()
            };

            const string XmlTemplate = "<r LogId='{0}' CustId='{1}' Username='{2}' Recommend='{3}'" +
                " Mrecommend='{4}' Srecommend='{5}' CurrencyName='{6}'" +
                " RoleEN='{7}' RoleSC='{8}' RoleTH='{9}' RoleYN='{10}' " +
                " SportNameEN='{11}'  SportNameSC='{12}'  SportNameTH='{13}'  SportNameYN='{14}'" +
                " LeagueNameEN='{15}' LeagueNameSC='{16}' LeagueNameTH='{17}' LeagueNameYN ='{18}'" +
                " HomeNameEN ='{19}' HomeNameSC='{20}' HomeNameTH='{21}' HomeNameYN='{22}'" +
                " AwayNameEN='{23}' AwayNameSC='{24}' AwayNameTH='{25}' AwayNameYN='{26}' " +
                " Outstanding='{27}'> </r>";

            var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag.ToLowerInvariant());
            var smaInfo = tOSMAInfo[0];
            commonFeedback.GetSportName(smaInfo.SportType.ToString(), Language.en_US).Returns("sportNameEN");
            commonFeedback.GetSportName(smaInfo.SportType.ToString(), Language.zh_CN).Returns("sportNameSC");
            commonFeedback.GetSportName(smaInfo.SportType.ToString(), Language.th_TH).Returns("sportNameTH");
            commonFeedback.GetSportName(smaInfo.SportType.ToString(), Language.vi_VN).Returns("sportNameYN");

            commonFeedback.GetLeagueName(smaInfo.LeagueId.ToString(), Language.en_US).Returns("leagueNameEN");
            commonFeedback.GetLeagueName(smaInfo.LeagueId.ToString(), Language.zh_CN).Returns("leagueNameSC");
            commonFeedback.GetLeagueName(smaInfo.LeagueId.ToString(), Language.th_TH).Returns("leagueNameTH");
            commonFeedback.GetLeagueName(smaInfo.LeagueId.ToString(), Language.vi_VN).Returns("leagueNameYN");

            commonFeedback.GetTeamName(smaInfo.HomeId.ToString(), Language.en_US).Returns("homeNameEN");
            commonFeedback.GetTeamName(smaInfo.HomeId.ToString(), Language.zh_CN).Returns("homeNameSC");
            commonFeedback.GetTeamName(smaInfo.HomeId.ToString(), Language.th_TH).Returns("homeNameTH");
            commonFeedback.GetTeamName(smaInfo.HomeId.ToString(), Language.vi_VN).Returns("homeNameYN");

            commonFeedback.GetTeamName(smaInfo.AwayId.ToString(), Language.en_US).Returns("awayNameEN");
            commonFeedback.GetTeamName(smaInfo.AwayId.ToString(), Language.zh_CN).Returns("awayNameSC");
            commonFeedback.GetTeamName(smaInfo.AwayId.ToString(), Language.th_TH).Returns("awayNameTH");
            commonFeedback.GetTeamName(smaInfo.AwayId.ToString(), Language.vi_VN).Returns("awayNameYN");

            var smaLevelYN = service.Protected("GetSMALevel", smaInfo.RoleId, Language.en_US);
            var smaLevelEN = service.Protected("GetSMALevel", smaInfo.RoleId, Language.en_US);
            var smaLevelSC = service.Protected("GetSMALevel", smaInfo.RoleId, Language.zh_CN);
            var smaLevelTH = service.Protected("GetSMALevel", smaInfo.RoleId, Language.th_TH);

            stringBuilder.AppendFormat(XmlTemplate,
                    smaInfo.LogId,
                    smaInfo.CustId,
                    smaInfo.UserName,
                    smaInfo.Recommend,
                    smaInfo.Mrecommend,
                    smaInfo.Srecommend,
                    smaInfo.CurrencyName,
                    smaLevelEN,
                    smaLevelSC,
                    smaLevelTH,
                    smaLevelYN,
                    "sportNameEN",
                    "sportNameSC",
                    "sportNameTH",
                    "sportNameYN",
                    "leagueNameEN",
                    "leagueNameSC",
                    "leagueNameTH",
                    "leagueNameYN",
                    "homeNameEN",
                    "homeNameSC",
                    "homeNameTH",
                    "homeNameYN",
                    "awayNameEN",
                    "awayNameSC",
                    "awayNameTH",
                    "awayNameYN",
                    smaInfo.Outstanding
                );

            stringBuilder.Append(BackgroundJobConstants.EndRootTag.ToLowerInvariant());
            var criteria = fixture.Create<TOControlImportCommand>();

            var commandResult = fixture.Create<CommandResult>();
            baseService.ExecuteNonQueryAsync(Arg.Do<TOControlImportCommand>(x => criteria = x)).Returns(commandResult);
            var valueTuple = new ValueTuple<CommandResult, TOControlImportCommand>(commandResult, criteria);

            // Act
            var actual = await service.ImportTOSMAInfoAsync(tOSMAInfo);

            // Assert
            Assert.Equal(valueTuple.Item1, actual.Item1);
            Assert.Equal(stringBuilder.ToString(), criteria.XmlData);
        }

        #region Helpers

        private ChatBotCTOService GetService()
        {
            baseService = Substitute.For<IBaseService>();
            commonFeedback = Substitute.For<ICommonFeedback>();
            return Substitute.For<ChatBotCTOService>(baseService, commonFeedback);
        }

        #endregion Helpers
    }
}