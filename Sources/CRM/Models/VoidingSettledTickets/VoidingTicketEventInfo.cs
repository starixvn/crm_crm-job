namespace CRM.Models.VoidingSettledTickets;

using System;
using CRM.Extensions.Xml;

[Serializable]
[XmlName("r")]
public class VoidingTicketEventInfo
{
    [XmlName("MessageId")]
    public int MessageId { get; set; }

    [XmlName("TransId")]
    public long TransId { get; set; }

    [XmlName("CustID")]
    public int CustID { get; set; }

    [XmlName("UserName")]
    public string UserName { get; set; }

    [XmlName("HeadlineID")]
    public int HeadlineID { get; set; }

    [XmlName("HeadlineName")]
    public string HeadlineName { get; set; }

    [XmlName("MatchId")]
    public int MatchId { get; set; }

    [XmlName("MatchCode")]
    public string MatchCode { get; set; }

    [XmlName("EventDate")]
    public DateTime EventDate { get; set; }

    [XmlName("Bettype")]
    public int Bettype { get; set; }

    [XmlName("EventType")]
    public short EventType { get; set; }

    [XmlName("ProductID")]
    public int ProductID { get; set; }

    [XmlName("ListMsgTargetId")]
    public string ListMsgTargetId { get; set; }
}