namespace CRM.BackgroundJob.Models.AbnormalBet
{
    using System;
    using CRM.BackgroundJob.Core.Extensions.Xml;

    [Serializable]
    [XmlName("r")]
    public class VoidedInfo
    {
        [XmlName("VoidingHistoryCollectionId")]
        public int VoidingHistoryCollectionId { get; set; }

        [XmlName("RequestId")]
        public int RequestId { get; set; }

        [XmlName("MessageId")]
        public int MessageId { get; set; }

        [XmlName("IssueTypeId")]
        public int IssueTypeId { get; set; }

        [XmlName("IsParlay")]
        public bool IsParlay { get; set; }

        [XmlName("RequestBy")]
        public string RequestBy { get; set; }

        [XmlName("DecidedBy")]
        public string DecidedBy { get; set; }

        [XmlName("VoidBy")]
        public string VoidBy { get; set; }

        [XmlName("MatchId")]
        public int MatchId { get; set; }

        [XmlName("TransIdList")]
        public string TransIdList { get; set; }

        [XmlName("OriginalTransId")]
        public string OriginalTransId { get; set; }

        [XmlName("UsernameList")]
        public string UsernameList { get; set; }
    }
}