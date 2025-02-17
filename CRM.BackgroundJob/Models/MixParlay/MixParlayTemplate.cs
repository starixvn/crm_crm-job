namespace CRM.BackgroundJob.Models.MixParlay
{
    using System.Xml.Serialization;

    [XmlRoot("ROOT")]
    public class MixParlayTemplate
    {
        public string SolutionCreditENSC { get; set; }
        public string SolutionCreditENYN { get; set; }
        public string DescriptionCredit { get; set; }
    }
}
