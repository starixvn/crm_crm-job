namespace CRM.Models.ChatBot;

using System;
using CRM.Extensions.Xml;

[Serializable]
[XmlName("r")]
public class ChatBotVoidingHistoryCollection
{
    [XmlName("VoidingHistoryCollectionId")]
    public int VoidingHistoryCollectionId { get; set; }

    [XmlName("MessageId")]
    public int MessageId { get; set; }

    [XmlName("IssueTypeId")]
    public int IssueTypeId { get; set; }

    [XmlName("TransIdList")]
    public string TransIdList { get; set; }

    [XmlName("UsernameList")]
    public string UsernameList { get; set; }

    [XmlName("FilterBy")]
    public int FilterBy { get; set; }
}