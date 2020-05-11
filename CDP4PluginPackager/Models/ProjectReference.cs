namespace CDP4PluginPackager.Models
{
    using System.Xml.Serialization;

    [XmlRoot(ElementName = "ProjectReference", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
	public class ProjectReference
	{
		[XmlElement(ElementName = "Project", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public string Project { get; set; }

		[XmlElement(ElementName = "Name", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public string Name { get; set; }

		[XmlElement(ElementName = "Private", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public string Private { get; set; }

		[XmlAttribute(AttributeName = "Include")]
		public string Include { get; set; }
	}
}
