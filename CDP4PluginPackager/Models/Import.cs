namespace CDP4PluginPackager.Models
{
    using System.Xml.Serialization;

    [XmlRoot(ElementName = "Import", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
	public class Import
	{
		[XmlAttribute(AttributeName = "Project")]
		public string Project { get; set; }

		[XmlAttribute(AttributeName = "Condition")]
		public string Condition { get; set; }
	}
}

