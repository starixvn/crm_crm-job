using System.Xml.Serialization;

namespace CRM.BackgroundJob.Models.TurnoverControl;

[XmlRoot("ROOT")]
public class TOSolutionTemplate
{
    public string SolutionEN { get; set; }
    public string SolutionSC { get; set; }
    public string SolutionYN { get; set; }
}