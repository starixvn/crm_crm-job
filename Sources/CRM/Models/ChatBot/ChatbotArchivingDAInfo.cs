namespace CRM.Models.ChatBot;

using System;
using CRM.Extensions.Xml;

[Serializable]
[XmlName("r")]
public class ChatbotArchivingDAInfo
{
    [XmlName("LogId")]
    public int LogId { get; set; }

    [XmlName("Username")]
    public string Username { get; set; }

    [XmlName("CustId")]
    public int CustId { get; set; }

    [XmlName("IsTest")]
    public bool IsTest { get; set; }
}