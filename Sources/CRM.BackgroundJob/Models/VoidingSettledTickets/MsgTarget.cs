namespace CRM.BackgroundJob.Models.VoidingSettledTickets;

using System;
using CRM.Extensions.Xml;

[Serializable]
[XmlName("r")]
public class MsgTarget
{
    [XmlName("MessageId")]
    public int MessageId { get; set; }

    [XmlName("MsgTargetId")]
    public string MsgTargetId { get; set; }
}