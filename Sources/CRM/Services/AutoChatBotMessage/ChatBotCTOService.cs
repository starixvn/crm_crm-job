namespace CRM.Services.AutoChatBotMessage;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using CRM.Constants;
using CRM.Criteria.ChatBot_CTO;
using CRM.Extensions.Xml;
using CRM.Models.ChatBotCTO;
using CRM.Services.AutoFeedback;
using CRM.Services.Common;

public interface IChatBotCTOService
{
    Task<IEnumerable<TOReachInfo>> GetChatbotCTOSettingInfoAsync(long LastLogId, int BatchSize);

    Task<IEnumerable<TurnoverSMAInfo>> GetChatbotCTOSMAInfoAsync(IEnumerable<TOReachInfo> tOReachInfo);

    Task<ValueTuple<CRM.Models.Common.CommandResult, TOControlImportCommand>> ImportTOSMAInfoAsync(IEnumerable<TurnoverSMAInfo> tOSMAInfo,
        string Environment = BackgroundJobConstants.DefaultEnvironment);
}

public class ChatBotCTOService(
    IBaseService baseService,
    ICommonFeedback commonFeedback)
    : IChatBotCTOService
{
    public async Task<IEnumerable<TOReachInfo>> GetChatbotCTOSettingInfoAsync(long LastLogId, int BatchSize)
    {
        var criteria = new GetTOControlSettingByLastLogIdCriteria
        {
            LastLogId = LastLogId,
            BatchSize = BatchSize
        };
        return await baseService.GetDataAsync<TOReachInfo>(criteria);
    }

    public async Task<IEnumerable<TurnoverSMAInfo>> GetChatbotCTOSMAInfoAsync(IEnumerable<TOReachInfo> tOReachInfo)
    {
        var criteria = new GetTOControlSMAInfoCriteria
        {
            XML = tOReachInfo.ToXML("root").ToString()
        };
        return await baseService.GetDataAsync<TurnoverSMAInfo>(criteria);
    }

    public async Task<ValueTuple<CRM.Models.Common.CommandResult, TOControlImportCommand>> ImportTOSMAInfoAsync(IEnumerable<TurnoverSMAInfo> tOSMAInfo,
        string Environment = BackgroundJobConstants.DefaultEnvironment)
    {
        const string XmlTemplate = "<r LogId='{0}' CustId='{1}' Username='{2}' Recommend='{3}'" +
                                   " Mrecommend='{4}' Srecommend='{5}' CurrencyName='{6}'" +
                                   " RoleEN='{7}' RoleSC='{8}' RoleTH='{9}' RoleYN='{10}' " +
                                   " SportNameEN='{11}'  SportNameSC='{12}'  SportNameTH='{13}'  SportNameYN='{14}'" +
                                   " LeagueNameEN='{15}' LeagueNameSC='{16}' LeagueNameTH='{17}' LeagueNameYN ='{18}'" +
                                   " HomeNameEN ='{19}' HomeNameSC='{20}' HomeNameTH='{21}' HomeNameYN='{22}'" +
                                   " AwayNameEN='{23}' AwayNameSC='{24}' AwayNameTH='{25}' AwayNameYN='{26}' " +
                                   " Outstanding='{27}'> </r>";

        var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag.ToLowerInvariant());
        foreach (var smaInfo in tOSMAInfo)
        {
            var sportNameEN = commonFeedback.GetSportName(smaInfo.SportType.ToString(), Language.en_US);
            var sportNameSC = commonFeedback.GetSportName(smaInfo.SportType.ToString(), Language.zh_CN);
            var sportNameTH = commonFeedback.GetSportName(smaInfo.SportType.ToString(), Language.th_TH);
            var sportNameYN = commonFeedback.GetSportName(smaInfo.SportType.ToString(), Language.vi_VN);

            var leagueNameEN = commonFeedback.GetLeagueName(smaInfo.LeagueId.ToString(), Language.en_US);
            var leagueNameSC = commonFeedback.GetLeagueName(smaInfo.LeagueId.ToString(), Language.zh_CN);
            var leagueNameTH = commonFeedback.GetLeagueName(smaInfo.LeagueId.ToString(), Language.th_TH);
            var leagueNameYN = commonFeedback.GetLeagueName(smaInfo.LeagueId.ToString(), Language.vi_VN);

            var homeNameEN = commonFeedback.GetTeamName(smaInfo.HomeId.ToString(), Language.en_US);
            var homeNameSC = commonFeedback.GetTeamName(smaInfo.HomeId.ToString(), Language.zh_CN);
            var homeNameTH = commonFeedback.GetTeamName(smaInfo.HomeId.ToString(), Language.th_TH);
            var homeNameYN = commonFeedback.GetTeamName(smaInfo.HomeId.ToString(), Language.vi_VN);

            var awayNameEN = commonFeedback.GetTeamName(smaInfo.AwayId.ToString(), Language.en_US);
            var awayNameSC = commonFeedback.GetTeamName(smaInfo.AwayId.ToString(), Language.zh_CN);
            var awayNameTH = commonFeedback.GetTeamName(smaInfo.AwayId.ToString(), Language.th_TH);
            var awayNameYN = commonFeedback.GetTeamName(smaInfo.AwayId.ToString(), Language.vi_VN);

            var smaLevelYN = GetSMALevel(smaInfo.RoleId, Language.en_US);
            var smaLevelEN = GetSMALevel(smaInfo.RoleId, Language.en_US);

            stringBuilder.AppendFormat(XmlTemplate,
                smaInfo.LogId,
                smaInfo.CustId,
                smaInfo.UserName,
                smaInfo.Recommend,
                smaInfo.Mrecommend,
                smaInfo.Srecommend,
                smaInfo.CurrencyName,
                smaLevelEN,
                GetSMALevel(smaInfo.RoleId, Language.zh_CN),
                GetSMALevel(smaInfo.RoleId, Language.th_TH),
                smaLevelYN,
                HttpUtility.HtmlEncode(sportNameEN),
                HttpUtility.HtmlEncode(sportNameSC),
                HttpUtility.HtmlEncode(sportNameTH),
                HttpUtility.HtmlEncode(sportNameYN),
                HttpUtility.HtmlEncode(leagueNameEN),
                HttpUtility.HtmlEncode(leagueNameSC),
                HttpUtility.HtmlEncode(leagueNameTH),
                HttpUtility.HtmlEncode(leagueNameYN),
                HttpUtility.HtmlEncode(homeNameEN),
                HttpUtility.HtmlEncode(homeNameSC),
                HttpUtility.HtmlEncode(homeNameTH),
                HttpUtility.HtmlEncode(homeNameYN),
                HttpUtility.HtmlEncode(awayNameEN),
                HttpUtility.HtmlEncode(awayNameSC),
                HttpUtility.HtmlEncode(awayNameTH),
                HttpUtility.HtmlEncode(awayNameYN),
                smaInfo.Outstanding
            );
        }

        stringBuilder.Append(BackgroundJobConstants.EndRootTag.ToLowerInvariant());
        var criteria = new TOControlImportCommand
        {
            XmlData = stringBuilder.ToString(),
            Environment = Environment
        };
        var commandResult = await baseService.ExecuteNonQueryAsync(criteria);
        return new ValueTuple<CRM.Models.Common.CommandResult, TOControlImportCommand>(commandResult, criteria);
    }

    protected internal virtual string GetSMALevel(int roleId, string language)
    {
        const byte SuperID = 4;
        const byte MasterID = 3;

        string smaLevel;
        switch (language)
        {
            case Language.en_US:
                var isMasterUS = roleId == MasterID ? SMALevel.Master : SMALevel.Agent;
                smaLevel = roleId == SuperID ? SMALevel.Super : isMasterUS;
                break;

            case Language.zh_CN:
                var isMasterSC = roleId == MasterID ? SMALevel.MasterSC : SMALevel.AgentSC;
                smaLevel = roleId == SuperID ? SMALevel.SuperSC : isMasterSC;
                break;

            case Language.th_TH:
                var isMasterTH = roleId == MasterID ? SMALevel.MasterTH : SMALevel.AgentTH;
                smaLevel = roleId == SuperID ? SMALevel.SuperTH : isMasterTH;
                break;

            default:
                smaLevel = "";
                break;
        }
        return smaLevel;
    }
}