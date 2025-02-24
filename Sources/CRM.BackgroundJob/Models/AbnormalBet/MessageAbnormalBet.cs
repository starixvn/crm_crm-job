namespace CRM.BackgroundJob.Models.AbnormalBet;

using CRM.Extensions.Xml;
using System;

[Serializable]
[XmlName("r")]
public class MessageAbnormalBet
{
    [XmlName("VoidingHistoryCollectionId")]
    public int VoidingHistoryCollectionId { get; set; }

    [XmlName("MessageId")]
    public int MessageId { get; set; }

    [XmlName("Content_en")]
    public string Content_en { get; set; }

    [XmlName("Content_cs")]
    public string Content_cs { get; set; }

    [XmlName("Content_yn")]
    public string Content_yn { get; set; }
}