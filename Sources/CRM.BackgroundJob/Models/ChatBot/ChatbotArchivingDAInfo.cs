namespace CRM.BackgroundJob.Models.ChatBot;

using CRM.Extensions.Xml;
using System;

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