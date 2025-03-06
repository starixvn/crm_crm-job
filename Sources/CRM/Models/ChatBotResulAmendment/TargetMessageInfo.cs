namespace CRM.Models.ChatBotResulAmendment;

using System;
using ChatBot;
using CRM.Extensions.Xml;

[Serializable]
[XmlName("r")]
public class TargetMessageInfo : ChatBotVoidingTicketInfo
{
    [XmlName("Username")]
    public string Username { get; set; }
}