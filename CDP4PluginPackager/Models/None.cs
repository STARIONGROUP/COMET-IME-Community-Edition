namespace CDP4PluginPackager.Models
{
    using System.Xml.Serialization;

    [XmlRoot(ElementName = "None", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
	public class None
	{
		[XmlAttribute(AttributeName = "Include")]
		public string Include { get; set; }
	}
}
