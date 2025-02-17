namespace CRM.BackgroundJob.Models.ChatBotResulAmendment
{
    using System;
    using CRM.BackgroundJob.Core.Extensions.Xml;

    [Serializable]
    [XmlName("r")]
    public class AmendmentResultTicketEvent
    {
        [XmlName("TicketId")]
        public int TicketId { get; set; }

        [XmlName("TicketEventId")]
        public int TicketEventId { get; set; }

        [XmlName("Seq")]
        public int Seq { get; set; }

        [XmlName("CustId")]
        public int CustId { get; set; }

        [XmlName("Username")]
        public string Username { get; set; }

        [XmlName("Recommend")]
        public int Recommend { get; set; }

        [XmlName("Mrecommend")]
        public int Mrecommend { get; set; }

        [XmlName("Srecommend")]
        public int Srecommend { get; set; }

        [XmlName("IsTest")]
        public int IsTest { get; set; }
    }
}