namespace CDP4PluginPackager.Models
{
    using System.Xml.Serialization;

    [XmlRoot(ElementName = "Page", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
	public class Page
	{
		[XmlElement(ElementName = "Generator", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public string Generator { get; set; }

		[XmlElement(ElementName = "SubType", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public string SubType { get; set; }

		[XmlAttribute(AttributeName = "Include")]
		public string Include { get; set; }
	}
}
