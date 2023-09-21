namespace XmlSerializer.SourceGenerator.Extensions.Symbols;
public static class INamedTypeSymbolExtensions
{
    /// <summary>
    /// Checks whether or not a given type symbol has a specified fully qualified metadata name.
    /// </summary>
    /// <param name="symbol">The input <see cref="ITypeSymbol"/> instance to check.</param>
    /// <param name="name">The full name to check.</param>
    /// <returns>Whether <paramref name="symbol"/> has a full name equals to <paramref name="name"/>.</returns>
    public static bool HasFullyQualifiedMetadataName(this ITypeSymbol symbol, string name)
    {
        return symbol.OriginalDefinition.ToString() == name;

    }
    /// <summary>
    /// Checks whether or not a given <see cref="ITypeSymbol"/> implements an interface with a specified name.
    /// </summary>
    /// <param name="typeSymbol">The target <see cref="ITypeSymbol"/> instance to check.</param>
    /// <param name="name">The full name of the type to check for interface implementation.</param>
    /// <returns>Whether or not <paramref name="typeSymbol"/> has an interface with the specified name.</returns>
    public static bool HasInterfaceWithFullyQualifiedMetadataName(this ITypeSymbol typeSymbol, string name)
    {
        foreach (INamedTypeSymbol interfaceType in typeSymbol.AllInterfaces)
        {
            if (interfaceType.HasFullyQualifiedMetadataName(name))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks whether or not a given <see cref="ITypeSymbol"/> has or inherits from a specified type.
    /// </summary>
    /// <param name="typeSymbol">The target <see cref="ITypeSymbol"/> instance to check.</param>
    /// <param name="name">The full name of the type to check for inheritance.</param>
    /// <returns>Whether or not <paramref name="typeSymbol"/> is or inherits from <paramref name="name"/>.</returns>
    public static bool HasOrInheritsFromFullyQualifiedMetadataName(this ITypeSymbol typeSymbol, string name)
    {
        for (ITypeSymbol? currentType = typeSymbol; currentType is not null; currentType = currentType.BaseType)
        {
            if (currentType.HasFullyQualifiedMetadataName(name))
            {
                return true;
            }
        }

        return false;
    }
    public static bool CheckInterface(this INamedTypeSymbol symbol, string FullIntefaceName)
    {
        foreach (var item in symbol.Interfaces)
        {
            string v1 = item.OriginalDefinition.ToString();
            bool v = FullIntefaceName == v1;
            if (v) return v;
        }
        foreach (var item in symbol.Interfaces)
        {
            bool v = item.CheckInterface(FullIntefaceName);
            if (v) return true;
        }
        if (symbol.BaseType != null)
        {
            bool v = symbol.BaseType.CheckInterface(FullIntefaceName);
            if (v) return true;

        }
        return false;
    }
    public static bool CheckBaseClass(this INamedTypeSymbol symbol, string FullClassName)
    {
        if (symbol.BaseType != null)
        {
            string v1 = symbol.BaseType.OriginalDefinition.ToString();
            bool v = FullClassName == v1;
            if (v) { return v; }
            return symbol.BaseType.CheckBaseClass(FullClassName);
        }

        return false;
    }
    public static IEnumerable<ISymbol> GetPropertiesAndFields(this INamedTypeSymbol symbol)
    {
        return symbol
            .GetMembers()
            .Where(c =>
            {
                bool includeField = c.Kind == SymbolKind.Field && c.GetAttributes().Any(c => c.GetAttrubuteMetaName() == "System.Xml.Serialization.XmlElementAttribute");
                bool include = c.Kind == SymbolKind.Property || includeField;
                return include && !c.IsAbstract;
            });
    }
    // Get Properties of current class and Base class 
    public static IEnumerable<ISymbol> GetAllPropertiesAndFields(this INamedTypeSymbol getType, bool onlycurrent = false)
    {
        IEnumerable<ISymbol> info = getType.GetPropertiesAndFields();
        if (getType.BaseType != null && getType.BaseType.OriginalDefinition.ToString() != "object" && !onlycurrent)
        {
            info = info.Concat(getType.BaseType.GetAllPropertiesAndFields());
        }
        return info;
    }
    public static string GetXmlRootFromClassSymbol(this INamedTypeSymbol symbol)
    {
        System.Collections.Immutable.ImmutableArray<AttributeData> attributeDatas = symbol.GetAttributes();
        var Name = symbol.Name.ToUpper();
        foreach (AttributeData attributeData in attributeDatas)
        {
            if (attributeData.GetAttrubuteMetaName() == "System.Xml.Serialization.XmlRootAttribute")
            {
                if (attributeData.ConstructorArguments != null && attributeData.ConstructorArguments.Length > 0)
                {
                    Name = attributeData.ConstructorArguments.FirstOrDefault().Value?.ToString();
                }
            }


        }

        return Name;
    }

    public static string GetClassMetaName(this INamedTypeSymbol namedTypeSymbol)
    {
        if (namedTypeSymbol.IsGenericType)
        {
            string name = namedTypeSymbol.OriginalDefinition.ToString();
            name = name.Split('<').First();
            return name;
        }
        else
        {
            string attributeMetaName = namedTypeSymbol.OriginalDefinition.ToString();
            return attributeMetaName;
        }

    }

}
