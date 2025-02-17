namespace CRM.BackgroundJob.Models.ChatBot
{
    using System;
    using CRM.BackgroundJob.Core.Extensions.Xml;

    [Serializable]
    [XmlName("r")]
    public class ChatbotArchivingDAInfor
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
}