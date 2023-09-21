using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using XmlSerializer.SourceGenerator.Extensions;
using XmlSerializer.SourceGenerator.Extensions.Symbols;

namespace XmlSerializer.SourceGenerator.Generators;
[Generator(LanguageNames.CSharp)]
public class XMLMethodGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //if (!Debugger.IsAttached)
        //{
        //    Debugger.Launch();
        //}
        IncrementalValueProvider<ImmutableArray<INamedTypeSymbol>> syntaxProvider = context.SyntaxProvider
          .CreateSyntaxProvider(SyntaxPredicate, SematicTransform)
          .Where(static (type) => type != null)!.Collect()!;

        context.RegisterSourceOutput(syntaxProvider, Execute);
    }


    private bool SyntaxPredicate(SyntaxNode node, CancellationToken token)
    {
        if (node == null) return false;
        if (node is ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.HasOrPotentiallyHasBaseTypes() || classDeclaration.HasOrPotentiallyHasAttributes();
        }
        return false;
    }

    private INamedTypeSymbol? SematicTransform(GeneratorSyntaxContext context, CancellationToken token)
    {
        if (!context.SemanticModel.Compilation.HasLanguageVersionAtLeastEqualTo(LanguageVersion.CSharp8))
        {
            return null;
        }
        var classDeclaration = Unsafe.As<ClassDeclarationSyntax>(context.Node);
        INamedTypeSymbol? symbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration, token);
        if (symbol == null)
        {
            return null;
        }

        const string Name = "XmlSerializer.Core.IXmlSerializable";
        if (symbol.HasOrInheritsFromFullyQualifiedMetadataName(Name) || symbol.HasInterfaceWithFullyQualifiedMetadataName(Name))
        {
            return symbol;
        };
        return null;
    }


    private void Execute(SourceProductionContext context, ImmutableArray<INamedTypeSymbol> source)
    {
        var xMLMethodGeneratorExecute = new XMLMethodGeneratorExecute(source, context);
        xMLMethodGeneratorExecute.Execute();
    }
}
