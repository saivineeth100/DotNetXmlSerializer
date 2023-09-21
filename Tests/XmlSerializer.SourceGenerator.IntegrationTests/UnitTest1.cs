using Newtonsoft.Json.Linq;
using XmlSerializer.Core;
using XmlSerializer.Core.Attributes;

namespace XmlSerializer.SourceGenerator.IntegrationTests;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void SimpleClassGenerateXmlSerialize_Test()
    {
        TestSimpleClass testSimpleClass = new TestSimpleClass();
        var xml = testSimpleClass.ToXml();
        var expected = "<TestSimpleClass>\r\n  <Name>TestName</Name>\r\n  <Role>TestRoleSimple</Role>\r\n</TestSimpleClass>";
        Assert.AreEqual(expected,xml);
    }
    [TestMethod]
    public void SimpleClasswithValueTypeGenerateXmlSerialize_Test()
    {
        var testSimpleClass = new TestSimpleClasswithValueType();
        var xml = testSimpleClass.ToXml();
        var expected = "<TestSimpleClasswithValueType>\r\n  <Name>TestName</Name>\r\n  <Role>TestRoleSimple</Role>\r\n  <Id>0</Id>\r\n  <IsSuperAdmin>false</IsSuperAdmin>\r\n</TestSimpleClasswithValueType>";
        Assert.AreEqual(expected,xml);
    }
    [TestMethod]
    public void SimpleClasswithAttributesGenerateXmlSerialize_Test()
    {
        var xml = new TestSimpleClasswithAttributes().ToXml();
        var expected = "<TESTSIMPLECLASS>\r\n  <NAMEFROMATTRIBUTE>TestName</NAMEFROMATTRIBUTE>\r\n  <ROLE>TestRoleSimple</ROLE>\r\n</TESTSIMPLECLASS>";
        Assert.AreEqual(expected, xml);
    }
}

public partial class TestSimpleClass : IXmlSerializable
{
    public string Name { get; set; } = "TestName";
    public string Role { get; set; } = "TestRoleSimple";
   
}
public partial class TestSimpleClasswithValueType : IXmlSerializable
{
    public string Name { get; set; } = "TestName";
    public string Role { get; set; } = "TestRoleSimple";

    public int Id { get; set; }
    public bool? IsAdmin { get; set; } 
    public bool IsSuperAdmin { get; set; } 
   
}
[XMLTag("TESTSIMPLECLASS")]
public partial class TestSimpleClasswithAttributes  : IXmlSerializable
{
    [XMLTag(XmlTag = "NAMEFROMATTRIBUTE")]
    public string Name { get; set; } = "TestName";
    [XMLTag("ROLE")]
    public string Role { get; set; } = "TestRoleSimple";
}
