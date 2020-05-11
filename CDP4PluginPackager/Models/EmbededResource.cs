namespace CDP4PluginPackager.Models
{
    using System.Xml.Serialization;

    [XmlRoot(ElementName = "EmbeddedResource", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
    public class EmbeddedResource
    {
        [XmlElement(ElementName = "SubType", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string SubType { get; set; }

        [XmlAttribute(AttributeName = "Include")]
        public string Include { get; set; }
    }
}
