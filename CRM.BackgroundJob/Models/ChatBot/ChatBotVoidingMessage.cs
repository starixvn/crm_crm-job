namespace Fanex.CRM.BackgroundJob.Models.ChatBot
{
    using System;
    using global::CRM.BackgroundJob.Core.Extensions.Xml;

    [Serializable]
    [XmlName("r")]
    public class ChatBotVoidingMessage
    {
        [XmlName("MessageID")]
        public int MessageId { get; set; }

        [XmlName("RequestID")]
        public int RequestId { get; set; }
    }
}