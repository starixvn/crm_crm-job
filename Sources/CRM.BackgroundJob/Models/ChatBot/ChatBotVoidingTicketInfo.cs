namespace CRM.BackgroundJob.Models.ChatBot;

using CRM.Extensions.Xml;

public class ChatBotVoidingTicketInfo
{
    [XmlName("MessageId")]
    public int MessageId { get; set; }

    [XmlName("CustId")]
    public int CustId { get; set; }

    public string UsernameList { get; set; }

    [XmlName("Srecommend")]
    public string Srecommend { get; set; }

    [XmlName("Mrecommend")]
    public string Mrecommend { get; set; }

    [XmlName("Recommend")]
    public string Recommend { get; set; }

    [XmlName("IsTest")]
    public bool IsTest { get; set; }

    public byte Type { get; set; }
    public int VoidingHistoryCollectionId { get; set; }
}