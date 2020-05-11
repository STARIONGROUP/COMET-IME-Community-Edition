namespace CDP4PluginPackager.Models
{
    using System.Xml.Serialization;

    [XmlRoot(ElementName = "Target", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
	public class Target
	{
		[XmlElement(ElementName = "Csc", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public Csc Csc { get; set; }

		[XmlAttribute(AttributeName = "Name")]
		public string Name { get; set; }
	}
}
