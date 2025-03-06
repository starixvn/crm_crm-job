namespace CRM.Models.AbnormalBet;

using System;
using CRM.Extensions.Xml;

[Serializable]
[XmlName("r")]
public class VoidingMessageUserName
{
    [XmlName("VoidingHistoryCollectionId")]
    public int VoidingHistoryCollectionId { get; set; }

    [XmlName("MessageId")]
    public int MessageId { get; set; }

    [XmlName("RequestId")]
    public int RequestId { get; set; }

    [XmlName("UsernameList")]
    public string UsernameList { get; set; }

    [XmlName("CustIdList")]
    public string CustIdList { get; set; }
}