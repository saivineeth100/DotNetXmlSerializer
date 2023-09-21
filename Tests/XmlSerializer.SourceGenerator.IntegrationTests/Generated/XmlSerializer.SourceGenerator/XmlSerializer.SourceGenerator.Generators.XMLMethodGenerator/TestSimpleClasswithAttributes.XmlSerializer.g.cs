#nullable enable
namespace XmlSerializer.SourceGenerator.IntegrationTests;
public partial class TestSimpleClasswithAttributes
{
    const string RootTag = "TESTSIMPLECLASS";
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
        if (Name != null)
        {
            writer.WriteElementString(null, "NAMEFROMATTRIBUTE", null, Name);
        }

        if (Role != null)
        {
            writer.WriteElementString(null, "ROLE", null, Role);
        }

        writer.WriteEndElement();
    }
}