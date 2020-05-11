namespace CDP4PluginPackager.Models
{
    using System.Xml.Serialization;

    [XmlRoot(ElementName = "Compile", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
	public class Compile
	{
		[XmlAttribute(AttributeName = "Include")]
		public string Include { get; set; }

		[XmlElement(ElementName = "DependentUpon", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public string DependentUpon { get; set; }
	}
}
