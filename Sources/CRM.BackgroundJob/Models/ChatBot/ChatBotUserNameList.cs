namespace CRM.BackgroundJob.Models.ChatBot;

using System;
using CRM.Extensions.Xml;

[Serializable]
[XmlName("r")]
public class ChatBotUserNameList : ChatBotVoidingMessage
{
    [XmlName("UsernameList")]
    public string UsernameList { get; set; }

    [XmlName("VoidingHistoryCollectionId")]
    public int VoidingHistoryCollectionId { get; set; }

    [XmlName("VoidingHistoryID")]
    public int? VoidingHistoryId { get; set; }
}