namespace Fanex.CRM.BackgroundJob.Models
{
    using System;
    using global::CRM.BackgroundJob.Core.Extensions.Xml;

    [Serializable]
    [XmlName("r")]
    public class SystemSettingItem
    {
        [XmlName("Name")]
        public string SettingName { get; set; }

        public string Value { get; set; }
    }
}