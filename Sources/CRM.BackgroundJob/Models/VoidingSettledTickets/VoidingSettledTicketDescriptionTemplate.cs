namespace CRM.BackgroundJob.Models.VoidingSettledTickets;

using System.Xml.Serialization;

[XmlRoot("ROOT")]
public class VoidingSettledTicketDescriptionTemplate
{
    public string SportsbookVirtualSports2 { get; set; }
    public string SportsbookVirtualSports2Racing { get; set; }
    public string VirtualSports { get; set; }
    public string VirtualSports181185 { get; set; }
    public string NumberGames { get; set; }
    public string Outright { get; set; }
}