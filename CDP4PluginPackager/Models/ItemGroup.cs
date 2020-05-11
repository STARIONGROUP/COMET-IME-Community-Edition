namespace CDP4PluginPackager.Models
{
	using System.Collections.Generic;
	using System.Xml.Serialization;

	[XmlRoot(ElementName = "ItemGroup", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
	public class ItemGroup
	{
		[XmlElement(ElementName = "Reference", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public List<Reference> Reference { get; set; }

		[XmlElement(ElementName = "Compile", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public List<Compile> Compile { get; set; }

		[XmlElement(ElementName = "ProjectReference", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public ProjectReference ProjectReference { get; set; }

		[XmlElement(ElementName = "Page", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public List<Page> Page { get; set; }

		[XmlElement(ElementName = "EmbeddedResource", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public EmbeddedResource EmbeddedResource { get; set; }

		[XmlElement(ElementName = "None", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public None None { get; set; }
	}
}
