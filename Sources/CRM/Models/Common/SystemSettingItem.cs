namespace CRM.Models.Common;

using System;
using CRM.Extensions.Xml;

[Serializable]
[XmlName("r")]
public class SystemSettingItem
{
    [XmlName("Name")]
    public string SettingName { get; set; }

    public string Value { get; set; }
}