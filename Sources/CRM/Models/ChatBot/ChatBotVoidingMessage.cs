namespace CRM.Models.ChatBot;

using System;
using CRM.Extensions.Xml;

[Serializable]
[XmlName("r")]
public class ChatBotVoidingMessage
{
    [XmlName("MessageID")]
    public int MessageId { get; set; }

    [XmlName("RequestID")]
    public int RequestId { get; set; }
}