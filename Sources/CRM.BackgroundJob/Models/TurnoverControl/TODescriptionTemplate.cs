using System.Xml.Serialization;

namespace CRM.BackgroundJob.Models.TurnoverControl;

[XmlRoot("ROOT")]
public class TODescriptionTemplate
{
    [XmlElement("Description")]
    public string Desc { get; set; }
}