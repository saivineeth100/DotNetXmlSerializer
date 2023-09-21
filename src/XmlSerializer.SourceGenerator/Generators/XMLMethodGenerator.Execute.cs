using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Reflection;
using System.Xml.Linq;
using XmlSerializer.SourceGenerator.Extensions.Symbols;
using XmlSerializer.SourceGenerator.Models;

namespace XmlSerializer.SourceGenerator.Generators;


public class XMLMethodGeneratorExecute
{
    private ImmutableArray<INamedTypeSymbol> _symbols;
    private SourceProductionContext _context;

    public XMLMethodGeneratorExecute(ImmutableArray<INamedTypeSymbol> symbols,
                                     SourceProductionContext context)
    {
        _symbols = symbols;
        _context = context;
    }

    public void Execute()
    {
        List<GenerateSerializerParametersDTO> parameters = new();
        foreach (var _symbol in _symbols)
        {
            GenerateSerializerParameters(parameters, _symbol);
        }
        foreach (var parameter in parameters)
        {
            GenerateSerializerClass(parameter);
            GenerateDeSerializerClass(parameter);
        }
    }

    


    /// <summary>
    /// Generate parameter for each class and complex property
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="_symbol"></param>
    private static void GenerateSerializerParameters(List<GenerateSerializerParametersDTO> parameters,
                                                     INamedTypeSymbol _symbol)
    {
        string Namespace = _symbol.ContainingNamespace.ToDisplayString();
        string name = _symbol.OriginalDefinition.Name;
        // Ensure class is added only once
        if (parameters.Any(c => c.Name == name && Namespace == c.Namespace))
        {
            return;
        }
        GenerateSerializerParametersDTO generateSerializerParametersDTO = new(name,
                                                                              Namespace,
                                                                              _symbol.IsValueType,
                                                                              _symbol.IsReferenceType);
        XMlProperties? classXmlProperties = _symbol.GetXmlProperties();
        generateSerializerParametersDTO.DefaultXmlTag = classXmlProperties?.XMLTag ?? name;
        IEnumerable<ISymbol> symbols = _symbol.GetAllPropertiesAndFields();
        foreach (var symbol in symbols)
        {

            switch (symbol)
            {
                case IPropertySymbol property:
                    ITypeSymbol type = property.Type;
                    NullableAnnotation nullableAnnotation = type.NullableAnnotation;
                    if (type.IsValueType && nullableAnnotation == NullableAnnotation.Annotated && type is INamedTypeSymbol nullableSymbol)
                    {
                        type = nullableSymbol.TypeArguments.First();
                    }
                    string propertyName = symbol.Name;
                    XMlProperties? xMlProperties = property.GetXmlProperties();
                    string xmlTag = xMlProperties?.XMLTag ?? symbol.Name;
                    SerializerField field = new(propertyName,
                                                nullableAnnotation == NullableAnnotation.Annotated,
                                                type.SpecialType,
                                                type.IsReferenceType,
                                                type.IsValueType);
                    field.XmlProperties = new(xmlTag);
                    generateSerializerParametersDTO.Fields.Add(field);
                    break;
            }

        }
        parameters.Add(generateSerializerParametersDTO);
    }


    /// <summary>
    /// Generates partial class that has ToXml method
    /// </summary>
    /// <param name="parameter"></param>
    private void GenerateSerializerClass(GenerateSerializerParametersDTO parameter)
    {
        var methodDeclarationSyntax = GenerateSerializeMethods(parameter);
        CompilationUnitSyntax compilationUnitSyntax = CompilationUnit()
            .WithMembers(SingletonList<MemberDeclarationSyntax>(
                FileScopedNamespaceDeclaration(IdentifierName(parameter.Namespace))
                .WithNamespaceKeyword(Token(TriviaList(Trivia(NullableDirectiveTrivia(Token(SyntaxKind.EnableKeyword),
                                                                                      true))),
                                            SyntaxKind.NamespaceKeyword,
                                            TriviaList()))
                .WithMembers(List(new MemberDeclarationSyntax[]
                {
                    ClassDeclaration(parameter.Name)
                    .WithModifiers(TokenList(new[]{Token(SyntaxKind.PublicKeyword),Token(SyntaxKind.PartialKeyword)}))
                    .WithMembers(List(methodDeclarationSyntax))
                }))))

            .NormalizeWhitespace();
        string source = compilationUnitSyntax.ToFullString();
        _context.AddSource($"{parameter.Name}.Serializer.g.cs", source);

    }

    private void GenerateDeSerializerClass(GenerateSerializerParametersDTO parameter)
    {
        throw new NotImplementedException();
    }
    private IEnumerable<MemberDeclarationSyntax> GenerateSerializeMethods(GenerateSerializerParametersDTO parameter)
    {
        string AsyncMethodName = $"ToXmlAsync";
        const string MethodName = $"ToXml";
        List<MemberDeclarationSyntax> members = new();
        string xmlTag = parameter.DefaultXmlTag;
        members.Add(CreateStringConst("RootTag", xmlTag));
        MethodDeclarationSyntax methodWithoutArgs = GenerateMethodWithoutAnyArguments(MethodName);
        MethodDeclarationSyntax xmlWriterMethod = GenerateXmlWriterForClass(parameter);
        members.Add(methodWithoutArgs);
        members.Add(xmlWriterMethod);
        return members;

    }


    private MethodDeclarationSyntax GenerateMethodWithoutAnyArguments(string MethodName)
    {
        List<StatementSyntax> statements = new();

        statements.Add(LocalDeclarationStatement(VariableDeclaration(IdentifierName("global::System.IO.StringWriter"))
                       .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier("stringWriter"))
                       .WithInitializer(EqualsValueClause(ImplicitObjectCreationExpression()

                   )))))
                   .WithUsingKeyword(Token(SyntaxKind.UsingKeyword)));
        statements.Add(GetXmlSettingsDeclaration());
        statements.Add(LocalDeclarationStatement(VariableDeclaration(IdentifierName("global::System.Xml.XmlWriter"))
            .WithVariables(SingletonSeparatedList(
                VariableDeclarator(Identifier("writer"))
                .WithInitializer(EqualsValueClause(InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                           IdentifierName("global::System.Xml.XmlWriter"),
                                           IdentifierName("Create")))
                .WithArgumentList(ArgumentList(
                    SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]
                    {
                        Argument(IdentifierName("stringWriter")),
                        Token(SyntaxKind.CommaToken),
                        Argument(IdentifierName("settings"))
                    })))))))));
        statements.Add(ExpressionStatement(
                                      InvocationExpression(
                                          MemberAccessExpression(
                                              SyntaxKind.SimpleMemberAccessExpression,
                                              IdentifierName("writer"),
                                              IdentifierName("WriteStartDocument")))));
        statements.Add(ExpressionStatement(InvocationExpression(
                                              IdentifierName("WriteXml"))
            .WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]{
                Argument(IdentifierName("writer")),
            })))));
        statements.Add(ExpressionStatement(
                                      InvocationExpression(
                                          MemberAccessExpression(
                                              SyntaxKind.SimpleMemberAccessExpression,
                                              IdentifierName("writer"),
                                              IdentifierName("WriteEndDocument")))));
        statements.Add(ExpressionStatement(
                                      InvocationExpression(
                                          MemberAccessExpression(
                                              SyntaxKind.SimpleMemberAccessExpression,
                                              IdentifierName("writer"),
                                              IdentifierName("Flush")))));
        var returnStatement = ReturnStatement(InvocationExpression(IdentifierName("stringWriter.ToString")));
        statements.Add(returnStatement);
        var methodDeclarationSyntax = MethodDeclaration(PredefinedType(Token(SyntaxKind.StringKeyword)),
                                                                            Identifier(MethodName))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
            .WithBody(Block(statements));
        return methodDeclarationSyntax;
    }



    private MethodDeclarationSyntax GenerateXmlWriterForClass(GenerateSerializerParametersDTO parameter, bool isAsync = false)
    {
        List<StatementSyntax> statements = new();
        statements.Add(CreateStartRootElement(isAsync));
        foreach (var field in parameter.Fields)
        {
            statements.Add(CreateElementorAttribute(field));
            //statements.Add(CreateEndElement());
        }
        statements.Add(CreateEndElement(isAsync));
        var methodDeclarationSyntax = MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)),
                                                                            Identifier("WriteXml"))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
            .WithParameterList(ParameterList(SeparatedList<ParameterSyntax>(
                new SyntaxNodeOrToken[]
                {
                    Parameter(Identifier("writer"))
                    .WithType(IdentifierName("global::System.Xml.XmlWriter")),
                    Token(SyntaxKind.CommaToken),
                    Parameter(Identifier("rootTag"))
                    .WithType(PredefinedType(Token(SyntaxKind.StringKeyword)))
                    .WithDefault(EqualsValueClause(IdentifierName("RootTag")))
                })))
            .WithBody(Block(statements));
        return methodDeclarationSyntax;
    }

    private StatementSyntax GetXmlSettingsDeclaration(bool isAsync = false)
    {
        return LocalDeclarationStatement(
                                              VariableDeclaration(
                                                  IdentifierName("global::System.Xml.XmlWriterSettings"))
                                              .WithVariables(
                                                  SingletonSeparatedList(
                                                      VariableDeclarator(
                                                          Identifier("settings"))
                                                      .WithInitializer(
                                                          EqualsValueClause(
                                                              ImplicitObjectCreationExpression()
                                                              .WithInitializer(
                                                                  InitializerExpression(
                                                                      SyntaxKind.ObjectInitializerExpression,
                                                                      SeparatedList<ExpressionSyntax>(
                                                                          new SyntaxNodeOrToken[]{
                                                                AssignmentExpression(
                                                                    SyntaxKind.SimpleAssignmentExpression,
                                                                    IdentifierName("Indent"),
                                                                    LiteralExpression(
                                                                        SyntaxKind.TrueLiteralExpression)),
                                                                Token(SyntaxKind.CommaToken),
                                                                AssignmentExpression(
                                                                    SyntaxKind.SimpleAssignmentExpression,
                                                                    IdentifierName("Async"),
                                                                    LiteralExpression(
                                                                        isAsync ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression)),
                                                                Token(SyntaxKind.CommaToken),
                                                                AssignmentExpression(
                                                                    SyntaxKind.SimpleAssignmentExpression,
                                                                    IdentifierName("OmitXmlDeclaration"),
                                                                    LiteralExpression(SyntaxKind.TrueLiteralExpression)),
                                                                Token(SyntaxKind.CommaToken),
                                                                AssignmentExpression(
                                                                    SyntaxKind.SimpleAssignmentExpression,
                                                                    IdentifierName("Encoding"),
                                                                    MemberAccessExpression(
                                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                                        IdentifierName("global::System.Text.Encoding"),
                                                                        IdentifierName("Unicode"))),
                                                                Token(SyntaxKind.CommaToken),
                                                                AssignmentExpression(
                                                                    SyntaxKind.SimpleAssignmentExpression,
                                                                    IdentifierName("CheckCharacters"),
                                                                    LiteralExpression(
                                                                        SyntaxKind.FalseLiteralExpression))}))))))));


    }

    private static FieldDeclarationSyntax CreateStringConst(string IdentifierName, string value)
    {
        return FieldDeclaration(VariableDeclaration(PredefinedType(Token(SyntaxKind.StringKeyword)))
                        .WithVariables(SingletonSeparatedList(
                                VariableDeclarator(
                                    Identifier(IdentifierName))
                                .WithInitializer(
                                    EqualsValueClause(
                                        LiteralExpression(
                                            SyntaxKind.StringLiteralExpression,
                                            Literal(value)))))))
            .WithModifiers(TokenList(
                            new[]
                            {
                                Token(SyntaxKind.ConstKeyword)
                            }));
    }

    private static InvocationExpressionSyntax AddConfigureAwait(InvocationExpressionSyntax expression)
    {
        return InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expression, IdentifierName("ConfigureAwait")))
                        .WithArgumentList(ArgumentList(
                            SingletonSeparatedList(Argument(LiteralExpression(SyntaxKind.FalseLiteralExpression)))));
    }

    public static ExpressionStatementSyntax CreateStartRootElement(bool isAsync = false)
    {
        string Name = $"WriteStartElement{(isAsync ? "Async" : "")}";
        InvocationExpressionSyntax expression = InvocationExpression(
                                                    MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        IdentifierName("writer"),
                                                        IdentifierName(Name)))
                                                .WithArgumentList(
                                                    ArgumentList(
                                                        SeparatedList<ArgumentSyntax>(
                                                            new SyntaxNodeOrToken[]{
                                                Argument(
                                                    LiteralExpression(
                                                        SyntaxKind.NullLiteralExpression)),
                                                Token(SyntaxKind.CommaToken),
                                                Argument(IdentifierName("rootTag")),
                                                Token(SyntaxKind.CommaToken),
                                                Argument(
                                                    LiteralExpression(
                                                        SyntaxKind.NullLiteralExpression))})));

        return isAsync ? ExpressionStatement(AwaitExpression(AddConfigureAwait(expression))) : ExpressionStatement(expression);
    }
    private StatementSyntax CreateElementorAttribute(SerializerField Field, bool isAsync = false)
    {
        var xmlField = Field.XmlProperties;
        string Name = xmlField.IsAttribute ? $"WriteAttributeString{(isAsync ? "Async" : "")}" : $"WriteElementString{(isAsync ? "Async" : "")}";
        ExpressionSyntax memberExpression;
        if (Field.SpecialType == SpecialType.System_String)
        {
            memberExpression = IdentifierName(Field.Name);
        }
        else if (Field.SpecialType == SpecialType.None)
        {
            memberExpression = IdentifierName(Field.Name);
        }
        else
        {
            memberExpression = InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                      IdentifierName("global::System.Xml.XmlConvert"),
                                                      IdentifierName("ToString")))
                .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(Field.IsValueType && Field.IsNullable ?
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName(Field.Name), IdentifierName("Value")) : IdentifierName(Field.Name)))));
        }
        ArgumentSyntax argumentSyntax;

        //argumentSyntax = Argument(InvocationExpression(IdentifierName("GetXMLText"))
        //    .WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[] { Argument(memberExpression) }))));
        argumentSyntax = Argument(memberExpression);



        InvocationExpressionSyntax expression = InvocationExpression(
                                                    MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        IdentifierName("writer"),
                                                        IdentifierName(Name)))
                                                .WithArgumentList(
                                                    ArgumentList(
                                                        SeparatedList<ArgumentSyntax>(
                                                            new SyntaxNodeOrToken[]{
                                                Argument(
                                                    LiteralExpression(
                                                        SyntaxKind.NullLiteralExpression)),
                                                Token(SyntaxKind.CommaToken),
                                                Argument(
                                                    LiteralExpression(
                                                        SyntaxKind.StringLiteralExpression,
                                                        Literal(xmlField.XmlTag))),
                                                Token(SyntaxKind.CommaToken),
                                                Argument(
                                                    LiteralExpression(
                                                        SyntaxKind.NullLiteralExpression)),
                                                Token(SyntaxKind.CommaToken),
                                                argumentSyntax
                                                            })));
        ExpressionStatementSyntax mainExpression = isAsync ? ExpressionStatement(AwaitExpression(AddConfigureAwait(expression))) : ExpressionStatement(expression);
        IfStatementSyntax ifStatementSyntax = IfStatement(BinaryExpression(
                                    SyntaxKind.NotEqualsExpression,
                                    IdentifierName(Field.Name),
                                    LiteralExpression(
                                        SyntaxKind.NullLiteralExpression)),
                                       Block(mainExpression));
        return Field.IsNullable || !Field.IsValueType ? ifStatementSyntax : mainExpression;
        //return isAsync ? ExpressionStatement(AwaitExpression(expression)) : ExpressionStatement(expression);
    }
    public static ExpressionStatementSyntax CreateEndElement(bool isAsync = false, bool isAttribute = false)
    {
        string Name = isAttribute ? $"WriteEndAttribute{(isAsync ? "Async" : "")}" : $"WriteEndElement{(isAsync ? "Async" : "")}";
        InvocationExpressionSyntax expression = InvocationExpression(
                                                    MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        IdentifierName("writer"),
                                                        IdentifierName(Name)));
        return isAsync ? ExpressionStatement(AwaitExpression(AddConfigureAwait(expression))) : ExpressionStatement(expression);
    }
}

