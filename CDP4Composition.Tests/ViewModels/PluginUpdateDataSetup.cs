namespace CDP4Composition.Tests.ViewModels
{
    using System.IO;
    using System.Security.AccessControl;

    using CDP4Composition.Modularity;
    using CDP4Composition.Services.PluginUpdaterService;

    using Newtonsoft.Json;

    using NUnit.Framework;

    public class PluginUpdateDataSetup
    {
        protected (FileInfo, Manifest) Plugin;

        protected Manifest Manifest;
        protected IPluginFileSystemService PluginFileSystem;
        protected string BasePath;
        protected string TempPath;
        protected string InstallPath;
        protected string UpdatePath;

        public virtual void Setup()
        {
            this.BasePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "PluginUpdateTestFixture", this.GetType().Name);
            this.TempPath = Path.Combine(this.BasePath, "Temp");
            this.InstallPath = Path.Combine(this.BasePath, "Plugins");
            this.UpdatePath = Path.Combine(this.BasePath, "Download");

            if (!Directory.Exists(this.UpdatePath))
            {
                Directory.CreateDirectory(this.UpdatePath);
            }

            var dataPath = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, "ViewModels/PluginMockData/"));

            foreach (var file in dataPath.EnumerateFiles())
            {
                File.Copy(file.FullName, Path.Combine(this.UpdatePath, file.Name), true);
            }

            if (!Directory.Exists(this.InstallPath))
            {
                Directory.CreateDirectory(this.InstallPath);
            }

            if (!Directory.Exists(this.TempPath))
            {
                Directory.CreateDirectory(this.TempPath);
            }

            this.Manifest = new Manifest
            {
                Name = "Name",
                Description = "Description",
                Author = "Author",
                Website = "Website",
                Version = "Version",
                ReleaseNote = "ReleaseNote"
            };

            var cdp4Ck = new FileInfo(Path.Combine(this.UpdatePath, "CDP4BasicRdl.cdp4ck"));
            this.Manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(Path.Combine(this.UpdatePath, "CDP4BasicRdl.plugin.manifest")));

            this.PluginFileSystem = new PluginFileSystemService((cdp4Ck, this.Manifest))
            {
                TemporaryPath = new DirectoryInfo(Path.Combine(this.TempPath, this.Manifest.Name)),
                InstallationPath = new DirectoryInfo(Path.Combine(this.InstallPath, this.Manifest.Name))
            };

            this.Plugin = (new FileInfo("test"), this.Manifest);
        }

    }
}
