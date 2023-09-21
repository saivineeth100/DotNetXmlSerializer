namespace XmlSerializer.SourceGenerator.Extensions.Symbols;
public static class ISymbolExtensions
{

    public static XMlProperties? GetXmlProperties(this ISymbol propertySymbol)
    {
        System.Collections.Immutable.ImmutableArray<AttributeData> attributeData = propertySymbol.GetAttributes();
        XMlProperties? xMlProperties = null;
        foreach (AttributeData attributeDataAttribute in attributeData)
        {
            if (attributeDataAttribute.HasFullyQualifiedMetadataName("XmlSerializer.Core.Attributes.XMLTagAttribute"))
            {
                if (attributeDataAttribute.ConstructorArguments != null && attributeDataAttribute.ConstructorArguments.Length > 0)
                {
                    var name = attributeDataAttribute.ConstructorArguments.FirstOrDefault().Value?.ToString();
                    if (name != null)
                    {
                        xMlProperties = new(name);
                    }
                }
                if (attributeDataAttribute.NamedArguments != null && attributeDataAttribute.NamedArguments.Length > 0)
                {
                    System.Collections.Immutable.ImmutableArray<KeyValuePair<string, TypedConstant>> namedArguments = attributeDataAttribute.NamedArguments;
                    foreach (var namedArgument in namedArguments)
                    {
                        switch (namedArgument.Key)
                        {
                            case "XmlTag":
                                var xmltag = (string?)namedArgument.Value.Value;
                                if (xmltag != null)
                                {
                                    xMlProperties ??= new(xmltag);
                                    xMlProperties.XMLTag = xmltag;
                                }
                                break;
                        }
                    }
                }

            }
        }

        return xMlProperties;
    }

}
