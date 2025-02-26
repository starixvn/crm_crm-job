namespace CRM.Models.TurnoverControl;

using System.Xml.Serialization;

[XmlRoot("ROOT")]
public class TOSolutionTemplate
{
    public string SolutionEN { get; set; }
    public string SolutionSC { get; set; }
    public string SolutionYN { get; set; }
}