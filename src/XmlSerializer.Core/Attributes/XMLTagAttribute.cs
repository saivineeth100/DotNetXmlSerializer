using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlSerializer.Core.Attributes;

[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
public class XMLTagAttribute : Attribute
{
    public XMLTagAttribute()
    {
    }

    public XMLTagAttribute(string xmlTag)
    {
        XmlTag = xmlTag;
    }

    public  string XmlTag { get; set; }

    public string? Namespace { get; set; }

    public bool IsAttribute { get; set; }


}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public class XMLTagAttribute<T> : XMLTagAttribute
{
    public XMLTagAttribute(string xmlTag) : base(xmlTag)
    {
    }
}
