namespace XmlSerializer.SourceGenerator.Extensions.Symbols;
public static class IPropertySymbolExtensions
{
    public static string? GetXmlTagFromPropertyAttributes(this IPropertySymbol propertySymbol)
    {
        System.Collections.Immutable.ImmutableArray<AttributeData> attributeData = propertySymbol.GetAttributes();
        foreach (AttributeData attributeDataAttribute in attributeData)
        {
            if (attributeDataAttribute.GetAttrubuteMetaName() == "System.Xml.Serialization.XmlElementAttribute")
            {
                if (attributeDataAttribute.ConstructorArguments != null && attributeDataAttribute.ConstructorArguments.Length > 0)
                {
                    var name = attributeDataAttribute.ConstructorArguments.FirstOrDefault().Value?.ToString();
                    if (name != null)
                    {
                        return name;
                    }
                }
            }
        }
        return null;
    }
    
}
public class XMlProperties
{
    public XMlProperties(string xmlTag)
    {
        XMLTag = xmlTag;
    }
    public XMlProperties(string xmlTag, bool isAttribute)
    {
        XMLTag = xmlTag;
        IsAttribute = true;
    }

    public string XMLTag { get; set; }

    public bool IsAttribute { get; set; }
}


public class TDLFieldProperties
{
    public string? Set { get; set; }
    public bool ExcludeInFetch { get; set; }
    public string? Use { get; set; }
    public string? TallyType { get; set; }
    public string? Format { get; set; }
    public string? CollectionName { get; set; }
}
public class TDLClassProperties
{
    public string CollectionName { get; set; } = null!;
    public string? Type { get; set; }
    public string? Filters { get; set; }
    public string? ComputeFields { get; set; }
    public string? ComputeVars { get; set; }
    public string? Objects { get; set; }
    public bool Include { get; set; }
    public bool Initialize { get; set; }
}