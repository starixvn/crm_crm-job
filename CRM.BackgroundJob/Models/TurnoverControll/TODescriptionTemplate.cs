namespace CRM.BackgroundJob.Models.TurnoverControll
{
    using System.Xml.Serialization;

    [XmlRoot("ROOT")]
    public class TODescriptionTemplate
    {
        [XmlElement("Description")]
        public string Desc { get; set; }
    }
}
