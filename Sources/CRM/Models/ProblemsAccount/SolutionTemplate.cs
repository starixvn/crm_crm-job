namespace CRM.Models.ProblemsAccount;

using System.Xml.Serialization;

[XmlRoot("ROOT")]
public class SolutionTemplate
{
    public string SolutionENSC { get; set; }
    public string SolutionENYN { get; set; }
}