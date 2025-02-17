namespace CRM.BackgroundJob.Models.AbnormalBet
{
    using System;
    using CRM.BackgroundJob.Core.Extensions.Xml;

    [Serializable]
    [XmlName("r")]
    public class MessageAbnormelBet
    {
        [XmlName("VoidingHistoryCollectionId")]
        public int VoidingHistoryCollectionId { get; set; }

        [XmlName("MessageId")]
        public int MessageId { get; set; }

        [XmlName("Content_en")]
        public string Content_en { get; set; }

        [XmlName("Content_cs")]
        public string Content_cs { get; set; }

        [XmlName("Content_yn")]
        public string Content_yn { get; set; }
    }
}