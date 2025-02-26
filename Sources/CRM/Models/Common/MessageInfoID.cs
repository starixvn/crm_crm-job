namespace CRM.Models.Common;

using System;
using CRM.Extensions.Xml;

[Serializable]
[XmlName("r")]
public class MessageInfoID
{
    [XmlName("MessageId")]
    public int MessageId { get; set; }

    [XmlName("BetTransId")]
    public string BetTransId { get; set; }

    [XmlName("ListBetTransId")]
    public string ListBetTransId { get; set; }

    [XmlName("Content_en")]
    public string Content_en { get; set; }

    [XmlName("Content_cs")]
    public string Content_cs { get; set; }

    [XmlName("Content_yn")]
    public string Content_yn { get; set; }

    [XmlName("MsgTargetId")]
    public int MsgTargetId { get; set; }

    [XmlName("TargetId")]
    public int TargetId { get; set; }

    [XmlName("MatchId")]
    public int MatchId { get; set; }

    [XmlName("CreatorName")]
    public string CreatorName { get; set; }
}