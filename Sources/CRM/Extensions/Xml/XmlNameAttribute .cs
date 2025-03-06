namespace CRM.Extensions.Xml;

using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public class XmlNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}