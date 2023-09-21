namespace XmlSerializer.Core.Converters;
public class PrimitiveXmlConverter<T> : IXmlSerializeConverter<T> where T : class
{

    public string ToXmlString(T instance)
    {

        return instance switch
        {
            string val => val,
            char val => XmlConvert.ToString(val),
            int val => XmlConvert.ToString(val),
            bool val => XmlConvert.ToString(val),
            short val => XmlConvert.ToString(val),
            long val => XmlConvert.ToString(val),
            float val => XmlConvert.ToString(val),
            double val => XmlConvert.ToString(val),
            decimal val => XmlConvert.ToString(val),
            _ => "",
        };
    }

}
