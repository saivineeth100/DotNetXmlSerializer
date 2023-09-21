using System.Xml;

namespace XmlSerializer.SourceGenerator.Tests;

[TestClass]
public class SimpleTests
{
    [TestMethod]
    public async Task SimpleClassSerializeGenerate_Test()
    {
        const string source = @"
using XmlSerializer.Core;
using System;
using XmlSerializer.Core.Attributes;
namespace TallyConnector.Core.Models.Common.Request;
public partial class TestClass : IXmlSerializable
{
    public string Header { get; set; }

    public String Body { get; set; }

    public string WriteOnlyField { get; }
}";
        const string result = @"#nullable enable
namespace TallyConnector.Core.Models.Common.Request;
public partial class TestClass
{
    const string RootTag = ""TestClass"";
    public string ToXml()
    {
        using global::System.IO.StringWriter stringWriter = new();
        global::System.Xml.XmlWriterSettings settings = new()
        {
            Indent = true,
            Async = false,
            OmitXmlDeclaration = true,
            Encoding = global::System.Text.Encoding.Unicode,
            CheckCharacters = false
        };
        global::System.Xml.XmlWriter writer = global::System.Xml.XmlWriter.Create(stringWriter, settings);
        writer.WriteStartDocument();
        WriteXml(writer);
        writer.WriteEndDocument();
        writer.Flush();
        return stringWriter.ToString();
    }

    public void WriteXml(global::System.Xml.XmlWriter writer, string rootTag = RootTag)
    {
        writer.WriteStartElement(null, rootTag, null);
        if (Header != null)
        {
            writer.WriteElementString(null, ""Header"", null, Header);
        }

        if (Body != null)
        {
            writer.WriteElementString(null, ""Body"", null, Body);
        }

        if (WriteOnlyField != null)
        {
            writer.WriteElementString(null, ""WriteOnlyField"", null, WriteOnlyField);
        }

        writer.WriteEndElement();
    }
}";

        await VerifyCS.VerifyGeneratorAsync(source, ("TestClass.Serializer.g.cs", result));
    }
    [TestMethod]
    public async Task SimpleClassWithValueTypeSerializeGenerate_Test()
    {
        const string source = @"
#nullable enable
using XmlSerializer.Core;
using System;
using XmlSerializer.Core.Attributes;
namespace TallyConnector.Core.Models.Common.Request;
public partial class TestClass : IXmlSerializable
{
    public string Header { get; set; } = null!;

    public bool Body { get; set; }

    public bool? WriteOnlyField { get; }
}";
        const string result = @"#nullable enable
namespace TallyConnector.Core.Models.Common.Request;
public partial class TestClass
{
    const string RootTag = ""TestClass"";
    public string ToXml()
    {
        using global::System.IO.StringWriter stringWriter = new();
        global::System.Xml.XmlWriterSettings settings = new()
        {
            Indent = true,
            Async = false,
            OmitXmlDeclaration = true,
            Encoding = global::System.Text.Encoding.Unicode,
            CheckCharacters = false
        };
        global::System.Xml.XmlWriter writer = global::System.Xml.XmlWriter.Create(stringWriter, settings);
        writer.WriteStartDocument();
        WriteXml(writer);
        writer.WriteEndDocument();
        writer.Flush();
        return stringWriter.ToString();
    }

    public void WriteXml(global::System.Xml.XmlWriter writer, string rootTag = RootTag)
    {
        writer.WriteStartElement(null, rootTag, null);
        if (Header != null)
        {
            writer.WriteElementString(null, ""Header"", null, Header);
        }

        writer.WriteElementString(null, ""Body"", null, global::System.Xml.XmlConvert.ToString(Body));
        if (WriteOnlyField != null)
        {
            writer.WriteElementString(null, ""WriteOnlyField"", null, global::System.Xml.XmlConvert.ToString(WriteOnlyField.Value));
        }

        writer.WriteEndElement();
    }
}";

        await VerifyCS.VerifyGeneratorAsync(source, ("TestClass.Serializer.g.cs", result));
    }
    
    [TestMethod]
    public async Task SimpleClasswithAttributesSerializeGenerate_Test()
    {
        const string source = @"
using XmlSerializer.Core;
using System;
using XmlSerializer.Core.Attributes;
namespace TallyConnector.Core.Models.Common.Request;
[XMLTag(""TESTCLASS"")]
public partial class TestClass : IXmlSerializable
{
    [XMLTag(XmlTag = ""HEADER"")]
    public string Header { get; set; }

    [XMLTag(""BODY"")]
    public String Body { get; set; }

    [XMLTag(""WRITEONLYFIELD"")]
    public string WriteOnlyField { get; }
}";
        const string result = @"#nullable enable
namespace TallyConnector.Core.Models.Common.Request;
public partial class TestClass
{
    const string RootTag = ""TESTCLASS"";
    public string ToXml()
    {
        using global::System.IO.StringWriter stringWriter = new();
        global::System.Xml.XmlWriterSettings settings = new()
        {
            Indent = true,
            Async = false,
            OmitXmlDeclaration = true,
            Encoding = global::System.Text.Encoding.Unicode,
            CheckCharacters = false
        };
        global::System.Xml.XmlWriter writer = global::System.Xml.XmlWriter.Create(stringWriter, settings);
        writer.WriteStartDocument();
        WriteXml(writer);
        writer.WriteEndDocument();
        writer.Flush();
        return stringWriter.ToString();
    }

    public void WriteXml(global::System.Xml.XmlWriter writer, string rootTag = RootTag)
    {
        writer.WriteStartElement(null, rootTag, null);
        if (Header != null)
        {
            writer.WriteElementString(null, ""HEADER"", null, Header);
        }

        if (Body != null)
        {
            writer.WriteElementString(null, ""BODY"", null, Body);
        }

        if (WriteOnlyField != null)
        {
            writer.WriteElementString(null, ""WRITEONLYFIELD"", null, WriteOnlyField);
        }

        writer.WriteEndElement();
    }
}";

        await VerifyCS.VerifyGeneratorAsync(source, ("TestClass.Serializer.g.cs", result));
    }
}
