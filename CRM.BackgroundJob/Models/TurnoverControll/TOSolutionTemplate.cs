namespace CRM.BackgroundJob.Models.TurnoverControll
{
    using System.Xml.Serialization;

    [XmlRoot("ROOT")]
    public class TOSolutionTemplate
    {
        public string SolutionEN { get; set; }
        public string SolutionSC { get; set; }
        public string SolutionYN { get; set; }
    }
}
