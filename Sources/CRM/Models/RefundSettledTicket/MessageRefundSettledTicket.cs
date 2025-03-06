namespace CRM.Models.RefundSettledTicket;

using System;
using CRM.Extensions.Xml;

[Serializable]
[XmlName("r")]
public class MessageRefundSettledTicket
{
    [XmlName("MessageId")]
    public int MessageId { get; set; }

    [XmlName("MatchId")]
    public int MatchId { get; set; }

    [XmlName("Content_en")]
    public string Content_en { get; set; }

    [XmlName("Content_cs")]
    public string Content_cs { get; set; }

    [XmlName("Content_yn")]
    public string Content_yn { get; set; }
}