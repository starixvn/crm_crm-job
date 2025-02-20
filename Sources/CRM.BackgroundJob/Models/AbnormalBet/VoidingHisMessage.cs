namespace CRM.BackgroundJob.Models.AbnormalBet;

using System;
using CRM.Extensions.Xml;

[Serializable]
[XmlName("r")]
public class VoidingHisMessage
{
    [XmlName("VoidingHistoryCollectionId")]
    public int VoidingHistoryCollectionId { get; set; }

    [XmlName("MessageID")]
    public int MessageId { get; set; }

    [XmlName("RequestID")]
    public int RequestId { get; set; }

    [XmlName("VoidingHistoryID")]
    public int VoidingHistoryId { get; set; }

    [XmlName("UsernameList")]
    public string UsernameList { get; set; }
}