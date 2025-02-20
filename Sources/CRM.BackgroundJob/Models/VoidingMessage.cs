namespace CRM.BackgroundJob.Models;

using System;
using CRM.Extensions.Xml;

[Serializable]
[XmlName("r")]
public class VoidingMessage
{
    [XmlName("VoidingHistoryCollectionId")]
    public int VoidingHistoryCollectionId { get; set; }

    [XmlName("MessageId")]
    public int MessageId { get; set; }

    [XmlName("RequestId")]
    public int RequestId { get; set; }
}