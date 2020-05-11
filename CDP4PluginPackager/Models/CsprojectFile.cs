namespace CDP4PluginPackager.Models
{
	using System.Collections.Generic;
	using System.Xml.Serialization;

	[XmlRoot(ElementName = "Project", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
	public class CsprojectFile
	{
		[XmlElement(ElementName = "Import", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public List<Import> Import { get; set; }

        [XmlElement(ElementName = "PropertyGroup", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public List<PropertyGroup> PropertyGroup { get; set; }

        [XmlElement(ElementName = "ItemGroup", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public List<ItemGroup> ItemGroup { get; set; }

        [XmlElement(ElementName = "Target", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public Target Target { get; set; }

        [XmlAttribute(AttributeName = "ToolsVersion")]
		public string ToolsVersion { get; set; }

        [XmlAttribute(AttributeName = "xmlns")]
		public string Xmlns { get; set; }
	}
}
