#nullable enable
namespace XmlSerializer.SourceGenerator.IntegrationTests;
public partial class TestSimpleClass
{
    const string RootTag = "TestSimpleClass";
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
            writer.WriteElementString(null, "Name", null, Name);
        }

        if (Role != null)
        {
            writer.WriteElementString(null, "Role", null, Role);
        }

        writer.WriteEndElement();
    }
}