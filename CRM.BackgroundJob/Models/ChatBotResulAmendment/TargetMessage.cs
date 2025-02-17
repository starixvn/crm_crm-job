namespace CRM.BackgroundJob.Models.ChatBotResulAmendment
{
    using System;
    using CRM.BackgroundJob.Core.Extensions.Xml;

    [Serializable]
    [XmlName("r")]
    public class TargetMessage
    {
        [XmlName("MessageId")]
        public int MessageId { get; set; }

        [XmlName("MsgTargetId")]
        public int MsgTargetId { get; set; }

        [XmlName("Type")]
        public string Type { get; set; }
    }
}