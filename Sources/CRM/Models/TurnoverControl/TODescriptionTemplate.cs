namespace CRM.Models.TurnoverControl;

using System.Xml.Serialization;

[XmlRoot("ROOT")]
public class TODescriptionTemplate
{
    [XmlElement("Description")]
    public string Desc { get; set; }
}