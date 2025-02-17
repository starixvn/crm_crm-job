namespace CRM.BackgroundJob.Models.ChatBotResulAmendment
{
    using System;
    using CRM.BackgroundJob.Core.Extensions.Xml;
    using Fanex.CRM.BackgroundJob.Models.ChatBot;

    [Serializable]
    [XmlName("r")]
    public class TargetMessageInfo : ChatBotVoidingTicketInfo
    {
        [XmlName("Username")]
        public string Username { get; set; }
    }
}