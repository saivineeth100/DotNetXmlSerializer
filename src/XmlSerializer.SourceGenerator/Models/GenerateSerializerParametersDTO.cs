using System.Collections.Immutable;

namespace XmlSerializer.SourceGenerator.Models;
public class GenerateSerializerParametersDTO : BaseTypeInfo
{
    public GenerateSerializerParametersDTO(string name,
                                           string nameSpace,
                                           bool isValueType,
                                           bool isReferenceType)
    {
        Name = name;
        Namespace = nameSpace;
        IsValueType = isValueType;
        IsReferenceType = isReferenceType;
        DefaultXmlTag = Name;
    }

    public string Name { get; set; }

    /// <summary>
    /// Namespace of class
    /// </summary>
    public string Namespace { get; set; }
    public string DefaultXmlTag { get; set; }

    public List<SerializerField> Fields { get; set; } = new();

}
public class SerializerField : BaseTypeInfo
{
    public SerializerField(string propertyName,
                           bool isNullable,
                           SpecialType specialType,
                           bool isReferenceType,
                           bool isValueType)
    {
        Name = propertyName;
        IsNullable = isNullable;
        SpecialType = specialType;
        IsReferenceType = isReferenceType;
        IsValueType = isValueType;
    }

    public string Name { get; set; }
    public string? Converter { get; set; }
    public bool IsList { get; set; }

    public SerializerFieldXmlProperties XmlProperties { get; set; }
    public string? ListXmlTag { get; set; }
    public string TypeName { get; set; }
    public bool IsEnum { get; internal set; }
    public bool IsNullable { get; }
    public SpecialType SpecialType { get; }
}
public class SerializerFieldXmlProperties
{
    public SerializerFieldXmlProperties(string xmlTag)
    {
        XmlTag = xmlTag;
    }

    public string XmlTag { get; set; }
    public bool IsAttribute { get; set; }
    public bool IsComment { get; set; }

}
public class BaseTypeInfo
{
    public bool IsValueType { get; set; }
    public bool IsReferenceType { get; set; }

}