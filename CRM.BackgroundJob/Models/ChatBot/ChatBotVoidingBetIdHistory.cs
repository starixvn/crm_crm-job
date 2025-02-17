namespace CRM.BackgroundJob.Models.ChatBot
{
    using System;
    using CRM.BackgroundJob.Core.Extensions.Xml;

    [Serializable]
    [XmlName("r")]
    public class ChatBotVoidingBetIdHistory
    {
        [XmlName("VoidingHistoryCollectionId")]
        public int VoidingHistoryCollectionId { get; set; }

        [XmlName("MessageId")]
        public int MessageId { get; set; }

        [XmlName("BetTransId")]
        public string BetTransId { get; set; }

        [XmlName("MsgTargetId")]
        public int MsgTargetId { get; set; }

        [XmlName("TargetId")]
        public int TargetId { get; set; }
    }
}