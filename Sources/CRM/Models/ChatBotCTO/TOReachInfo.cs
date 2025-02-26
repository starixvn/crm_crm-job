namespace CRM.Models.ChatBotCTO;

using System;
using CRM.Extensions.Xml;

[Serializable]
[XmlName("r")]
public class TOReachInfo
{
    [XmlName("LogId")]
    public long LogId { get; set; }

    [XmlName("CustId")]
    public int CustId { get; set; }

    [XmlName("MatchId")]
    public int MatchId { get; set; }
}