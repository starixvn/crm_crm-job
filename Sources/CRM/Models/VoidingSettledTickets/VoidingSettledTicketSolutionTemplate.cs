namespace CRM.Models.VoidingSettledTickets;

using System.Xml.Serialization;

[XmlRoot("ROOT")]
public class VoidingSettledTicketSolutionTemplate
{
    public string SolutionENSC { get; set; }
    public string SolutionENYN { get; set; }
}