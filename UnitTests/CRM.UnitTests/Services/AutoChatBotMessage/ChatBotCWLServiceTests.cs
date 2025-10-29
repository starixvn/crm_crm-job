namespace CRM.UnitTests.Services.AutoChatBotMessage;

using AutoFixture;
using CRM.Constants;
using CRM.Criteria.ChatBotCWL;
using CRM.Models.ChatBotCWL;
using CRM.Services.AutoChatBotMessage;
using CRM.Services.Common;
using Fanex.RemotingRef;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

public class ChatBotCWLServiceTests
{
    private IBaseService baseService;
    private IRemotingRefService remotingRefService;

    [Fact]
    public async Task GetChatbotCWLLogInfoAsync_WhenCalled_ReturnCWLInfo()
    {
        // Arrange
        var fixture = new Fixture();
        var expected = fixture.Create<IEnumerable<ChatBotCWLInfo>>();
        var service = GetService();
        baseService.GetDataAsync<ChatBotCWLInfo>(Arg.Any<GetCWLInfoByLogIdCriteria>()).Returns(expected);

        // Act
        var actual = await service.GetChatbotCWLLogInfoAsync(0, 10);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task ImportCWLLogInfoAsync_WhenCalled_ReturnExpectedCommandResult()
    {
        // Arrange
        var fixture = new Fixture();
        var service = GetService();
        var cWLInfos = new List<ChatBotCWLInfo>
        {
            fixture.Create<ChatBotCWLInfo>()
        };

        const string XmlTemplate = "<r LogId='{0}' CustId='{1}' CurrencyName='{2}' " +
                                   "Username='{3}' Recommend='{4}' Mrecommend='{5}' Srecommend='{6}' " +
                                   "Winloss='{7}' ProductNameEN='{8}' ProductNameSC='{9}' ProductNameTH='{10}' " +
                                   "ProductNameVN='{11}' LevelId='{12}' GroupId='{13}' TotalWinAmtRM='{14}' " +
                                   "CasinoType='{15}' IsNormalGame='{16}'/>";
        var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag.ToLowerInvariant());

        service.GetProductName(Arg.Any<string>(), "en-US").Returns("a");
        service.GetProductName(Arg.Any<string>(), "zh-CN").Returns("b");
        service.GetProductName(Arg.Any<string>(), "th-TH").Returns("c");
        service.GetProductName(Arg.Any<string>(), "vi-VN").Returns("d");
        var cWLInfo = cWLInfos.First();
        stringBuilder.AppendFormat(XmlTemplate,
            cWLInfo.LogId,
            cWLInfo.CustId,
            cWLInfo.CurrencyName,
            cWLInfo.Username,
            cWLInfo.Recommend,
            cWLInfo.Mrecommend,
            cWLInfo.Srecommend,
            cWLInfo.Winloss,
            "a",
            "b",
            "c",
            "d",
            cWLInfo.LevelId,
            cWLInfo.GroupId,
            cWLInfo.TotalWinAmtRM,
            cWLInfo.CasinoType,
            cWLInfo.IsNormalGame ? 1 : 0);
        stringBuilder.Append(BackgroundJobConstants.EndRootTag.ToLowerInvariant());
        var criteria = new ImportCWLLogInfoCommand();

        var commandResult = fixture.Create<CRM.Models.Common.CommandResult>();
        baseService.ExecuteNonQueryAsync(Arg.Do<ImportCWLLogInfoCommand>(x => criteria = x)).Returns(commandResult);
        var valueTuple = new ValueTuple<CRM.Models.Common.CommandResult, ImportCWLLogInfoCommand>(commandResult, criteria);
        // Act
        var actual = await service.ImportCWLLogInfoAsync(cWLInfos);

        // Assert
        Assert.Equal(valueTuple.Item1, actual.Item1);
        Assert.Equal(stringBuilder.ToString(), criteria.XmlData);
        Assert.Equal(BackgroundJobConstants.DefaultEnvironment, criteria.Environment);
        Assert.Equal(0, criteria.ErrCode);
        Assert.Null(criteria.ErrMsg);
    }

    #region Helpers

    private ChatBotCWLService GetService()
    {
        baseService = Substitute.For<IBaseService>();
        remotingRefService = Substitute.For<IRemotingRefService>();
        return new ChatBotCWLService(baseService, remotingRefService);
    }

    #endregion Helpers
}