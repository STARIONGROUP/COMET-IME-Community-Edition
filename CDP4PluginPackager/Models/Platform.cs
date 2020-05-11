namespace CDP4PluginPackager.Models
{
    using System.Xml.Serialization;

    [XmlRoot(ElementName = "Platform", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
	public class Platform
	{
		[XmlAttribute(AttributeName = "Condition")]
		public string Condition { get; set; }

		[XmlText]
		public string Text { get; set; }
	}
}
