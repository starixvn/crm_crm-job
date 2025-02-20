namespace CRM.BackgroundJob.Models.ArchivingDA;

using System.Xml.Serialization;

[XmlRoot("ROOT")]
public class ArchivingTemplate
{
    public string SolutionENSC { get; set; }
    public string SolutionENYN { get; set; }
    public string Description { get; set; }
}