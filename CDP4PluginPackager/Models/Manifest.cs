namespace CDP4PluginPackager.Models
{
    using System;
    using System.Collections.Generic;

    public class Manifest
    {
        public Guid ProjectGuid { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Version { get; set; }

        public string ReleaseNote { get; set; }
        
        public string Author { get; set; }
        
        public string Website { get; set; }

        public string License { get; set; }

        public List<Reference> References { get; set; }

        public string TargetFramework { get; set; }
    }
}
