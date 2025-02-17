namespace CRM.BackgroundJob.Models
{
    using System.Xml.Serialization;

    [XmlRoot("ROOT")]
    public class FeedbackTemplate
    {
        [XmlElement("Feedback")]
        public Template Templates { get; set; }
    }

    public class Template
    {
        public string RNGTemplateENYN { get; set; }

        public string RNGTemplateENSC { get; set; }

        public string JackpotPrizeTemplateENYN { get; set; }

        public string JackpotPrizeTemplateENSC { get; set; }

        public string YellowTemplateENYN { get; set; }

        public string YellowTemplateENSC { get; set; }

        public string RedTemplateENYN { get; set; }

        public string RedTemplateENSC { get; set; }

        public string GreyTemplateENYN { get; set; }

        public string GreyTemplateENSC { get; set; }

        public string BlackTemplateYN { get; set; }

        public string BlackTemplateSC { get; set; }
    }
}
