namespace CRM.BackgroundJob.Models.ChatBotResulAmendment;

using System;
using CRM.Extensions.Xml;

[Serializable]
[XmlName("r")]
public class MessageContent
{
    [XmlName("MessageId")]
    public int MessageId { get; set; }

    [XmlName("RequestId")]
    public int RequestId { get; set; }

    [XmlName("Content_en")]
    public string Content_en { get; set; }

    [XmlName("Content_cs")]
    public string Content_cs { get; set; }

    [XmlName("Content_th")]
    public string Content_th { get; set; }

    [XmlName("Content_vn")]
    public string Content_vn { get; set; }

    [XmlName("VoidingHistoryCollectionId")]
    public int VoidingHistoryCollectionId { get; set; }
}