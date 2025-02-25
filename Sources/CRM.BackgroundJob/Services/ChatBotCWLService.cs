namespace CRM.BackgroundJob.Services;

using Constants;
using CRM.BackgroundJob.Criteria.ChatBotCWL;
using CRM.BackgroundJob.Models.ChatBotCWL;
using CRM.Models;
using CRM.Services;
using Fanex.RemotingRef;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public interface IChatBotCWLService
{
    Task<IEnumerable<ChatBotCWLInfo>> GetChatbotCWLLogInfoAsync(long LastLogId, int BatchSize);

    Task<ValueTuple<CommandResult, ImportCWLLogInfoCommand>> ImportCWLLogInfoAsync(
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

    public async Task<ValueTuple<CommandResult, ImportCWLLogInfoCommand>> ImportCWLLogInfoAsync(
        IEnumerable<ChatBotCWLInfo> cwlInfo,
        string Environment = BackgroundJobConstants.DefaultEnvironment)
    {
        const string XmlTemplate = "<r LogId='{0}' CustId='{1}' CurrencyName='{2}' " +
                                   "Username='{3}' Recommend='{4}' Mrecommend='{5}' Srecommend='{6}' " +
                                   "Winloss='{7}' ProductNameEN='{8}' ProductNameSC='{9}' ProductNameTH='{10}' " +
                                   "ProductNameVN='{11}' LevelId='{12}' GroupId='{13}'/>";
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
                cwl.GroupId);
        }

        stringBuilder.Append(BackgroundJobConstants.EndRootTag.ToLowerInvariant());
        var criteria = new ImportCWLLogInfoCommand
        {
            XmlData = stringBuilder.ToString(),
            Environment = Environment
        };

        var commandResult = await baseService.ExecuteNonQueryAsync(criteria);
        return new ValueTuple<CommandResult, ImportCWLLogInfoCommand>(commandResult, criteria);
    }

    public string GetProductName(string productId, string languageCode)
    {
        var productName = remotingRefService.GetResourceDataByRefIdAndResourceId("Product", productId, languageCode, string.Empty);
        return productName;
    }
}