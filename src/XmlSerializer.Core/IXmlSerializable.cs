namespace XmlSerializer.Core;
public interface IXmlSerializable
{
    //string ToXml();
    //string ToXml(XmlWriter writer);
    //Task<string> ToXmlAsync();
    //Task<string> ToXmlAsync(XmlWriter writer);


    //string FromXml();
    //Task<string> FromXmlAsync();
}
public interface IXmlSerializeConverter<T> where T : class
{
    string ToXmlString(T instance);

}