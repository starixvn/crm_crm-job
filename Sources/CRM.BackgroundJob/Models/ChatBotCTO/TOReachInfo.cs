namespace CRM.BackgroundJob.Models.ChatBotCTO;

using CRM.Extensions.Xml;
using System;

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