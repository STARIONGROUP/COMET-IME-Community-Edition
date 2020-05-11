namespace CDP4PluginPackager.Models
{
    using System.Xml.Serialization;

	[XmlRoot(ElementName = "Reference", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
	public class Reference
	{
		[XmlElement(ElementName = "HintPath", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public string HintPath { get; set; }

		[XmlElement(ElementName = "Private", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public string Private { get; set; }

		[XmlAttribute(AttributeName = "Include")]
		public string Include { get; set; }

		[XmlElement(ElementName = "EmbedInteropTypes", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public string EmbedInteropTypes { get; set; }

		[XmlElement(ElementName = "SpecificVersion", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public string SpecificVersion { get; set; }
	}
}
