namespace CDP4PluginPackager.Models
{
    using System.Xml.Serialization;

    [XmlRoot(ElementName = "Csc", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
	public class Csc
	{
		[XmlAttribute(AttributeName = "ToolExe")]
		public string ToolExe { get; set; }

		[XmlAttribute(AttributeName = "ToolPath")]
		public string ToolPath { get; set; }

		[XmlAttribute(AttributeName = "YieldDuringToolExecution")]
		public string YieldDuringToolExecution { get; set; }
	}
}
