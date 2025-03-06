namespace CRM.Models.ChatBotResulAmendment;

using System;
using CRM.Extensions.Xml;

[Serializable]
[XmlName("r")]
public class MessageVoidSettled
{
    [XmlName("MessageId")]
    public int MessageId { get; set; }

    [XmlName("MsgTargetId")]
    public int MsgTargetId { get; set; }

    [XmlName("BetTransId")]
    public string BetTransId { get; set; }

    [XmlName("MsgContentsType")]
    public byte MsgContentsType { get; set; }

    [XmlName("MatchId")]
    public int MatchId { get; set; }
}