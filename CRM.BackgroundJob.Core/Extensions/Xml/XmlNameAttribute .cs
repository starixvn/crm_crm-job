namespace CRM.BackgroundJob.Core.Extensions.Xml
{
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class XmlNameAttribute : Attribute
    {
        public XmlNameAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; }
    }
}