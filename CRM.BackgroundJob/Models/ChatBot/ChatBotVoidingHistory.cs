namespace CRM.BackgroundJob.Models.ChatBot
{
    using System;
    using CRM.BackgroundJob.Core.Extensions.Xml;

    [Serializable]
    [XmlName("r")]
    public class ChatBotVoidingHistory
    {
        [XmlName("RequestID")]
        public int RequestID { get; set; }

        [XmlName("VoidingHistoryID")]
        public int VoidingHistoryID { get; set; }

        [XmlName("UsernameList")]
        public string UsernameList { get; set; }

        [XmlName("VoidingHistoryCollectionId")]
        public int VoidingHistoryCollectionId { get; set; }
    }
}