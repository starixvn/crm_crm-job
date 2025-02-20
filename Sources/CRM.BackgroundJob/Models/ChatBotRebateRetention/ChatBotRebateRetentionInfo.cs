namespace CRM.BackgroundJob.Models.ChatBotRebateRetention;

using System;
using CRM.Extensions.Xml;

[Serializable]
[XmlName("r")]
public class ChatBotRebateRetentionInfo
{
    [XmlName("SiteDBGroupID")]
    public int SiteDBGroupID {  get; set; }

    [XmlName("CustId")]
    public int CustId { get; set; }

    [XmlName("Recommend")]
    public int Recommend {  get; set; }

    [XmlName("Mrecommend")]
    public int Mrecommend { get; set; }

    [XmlName("Srecommend")]
    public int Srecommend { get; set; }

    [XmlName("AffectedMember")]
    public string AffectedMember { get; set; }

    [XmlName("IsTest")]
    public int IsTest { get; set; }
}