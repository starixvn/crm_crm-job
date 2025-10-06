namespace CRM.Services.AutoChatBotMessage;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CRM.Constants;
using CRM.Criteria.ChatBotCWL;
using CRM.Models.ChatBotCWL;
using CRM.Services.Common;
using Fanex.RemotingRef;

public interface IChatBotCWLService
{
    Task<IEnumerable<ChatBotCWLInfo>> GetChatbotCWLLogInfoAsync(long LastLogId, int BatchSize);

    Task<ValueTuple<CRM.Models.Common.CommandResult, ImportCWLLogInfoCommand>> ImportCWLLogInfoAsync(
        IEnumerable<ChatBotCWLInfo> cwlInfo,
        string Environment = BackgroundJobConstants.DefaultEnvironment);
}

public class ChatBotCWLService(IBaseService baseService, IRemotingRefService remotingRefService)
    : IChatBotCWLService
{
    public async Task<IEnumerable<ChatBotCWLInfo>> GetChatbotCWLLogInfoAsync(long LastLogId, int BatchSize)
    {
        var criteria = new GetCWLInfoByLogIdCriteria
        {
            LastLogId = LastLogId,
            BatchSize = BatchSize
        };
        return await baseService.GetDataAsync<ChatBotCWLInfo>(criteria);
    }

    public async Task<ValueTuple<CRM.Models.Common.CommandResult, ImportCWLLogInfoCommand>> ImportCWLLogInfoAsync(
        IEnumerable<ChatBotCWLInfo> cwlInfo,
        string Environment = BackgroundJobConstants.DefaultEnvironment)
    {
        const string XmlTemplate = "<r LogId='{0}' CustId='{1}' CurrencyName='{2}' " +
                                   "Username='{3}' Recommend='{4}' Mrecommend='{5}' Srecommend='{6}' " +
                                   "Winloss='{7}' ProductNameEN='{8}' ProductNameSC='{9}' ProductNameTH='{10}' " +
                                   "ProductNameVN='{11}' LevelId='{12}' GroupId='{13}' TotalWinAmtRM='{14}' " +
                                   "CasinoType='{15}' IsNormalGame='{16}'/>";
        var stringBuilder = new StringBuilder(BackgroundJobConstants.BeginRootTag.ToLowerInvariant());
        foreach (var cwl in cwlInfo)
        {
            var productEN = GetProductName(cwl.ProductId.ToString(), "en-US");
            var productSC = GetProductName(cwl.ProductId.ToString(), "zh-CN");
            var productTH = GetProductName(cwl.ProductId.ToString(), "th-TH");
            var productVN = GetProductName(cwl.ProductId.ToString(), "vi-VN");

            stringBuilder.AppendFormat(XmlTemplate,
                cwl.LogId,
                cwl.CustId,
                cwl.CurrencyName,
                cwl.Username,
                cwl.Recommend,
                cwl.Mrecommend,
                cwl.Srecommend,
                cwl.Winloss,
                productEN,
                productSC,
                productTH,
                productVN,
                cwl.LevelId,
                cwl.GroupId,
                cwl.TotalWinAmtRM,
                cwl.CasinoType,
                cwl.IsNormalGame ? 1 : 0);
        }

        stringBuilder.Append(BackgroundJobConstants.EndRootTag.ToLowerInvariant());
        var criteria = new ImportCWLLogInfoCommand
        {
            XmlData = stringBuilder.ToString(),
            Environment = Environment
        };

        var commandResult = await baseService.ExecuteNonQueryAsync(criteria);
        return new ValueTuple<CRM.Models.Common.CommandResult, ImportCWLLogInfoCommand>(commandResult, criteria);
    }

    public string GetProductName(string productId, string languageCode)
    {
        var productName = remotingRefService.GetResourceDataByRefIdAndResourceId("Product", productId, languageCode, string.Empty);
        return productName;
    }
}