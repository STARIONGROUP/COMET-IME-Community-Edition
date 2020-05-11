using NUnit.Framework;

namespace CDP4PluginPackager.Tests
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Runtime.InteropServices.ComTypes;
    using System.Threading;

    using CDP4PluginPackager.Models;

    using Newtonsoft.Json;

    using NUnit.Framework.Internal;

    [TestFixture]
    public class AppTestFixture
    {
        private string[] args;

        private const string TargetPath = @"C:\CODE\CDP4-IME-Community-Edition\CDP4Dashboard";
        private const string TargetProject = "CDP4Dashboard";

        [OneTimeSetUp]
        public void Setup()
        {
            var currentDirectory = Directory.GetCurrentDirectory().Split(Path.DirectorySeparatorChar);
            var prefix = Path.Join(currentDirectory.SkipLast(4).ToArray());
            var testDirectory = Path.Join(prefix, TargetProject);

            Directory.SetCurrentDirectory(testDirectory);
            this.args = new string[] { testDirectory, "pack" };
        }

        [Test]
        public void VerifyCsprojGetsDeSerialized()
        {
            var app = new App();
            app.Deserialize();
            Assert.IsNotNull(app.Csproj);
            Assert.IsNotEmpty(app.Csproj.ItemGroup);
        }

        [Test]
        public void VerifyAssemblyInfoAreLoaded()
        {
            var app = new App();
            app.Deserialize();
            app.GetAssemblyInfo();
            Assert.IsNotNull(app.AssemblyInfo);
            Assert.IsNotNull(app.AssemblyInfo.Version);
        }

        [Test]
        public void VerifyManifestIntegrity()
        {
            var app = new App();
            app.Start();
            var json = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText($"{Path.Join(app.OutputPath, app.Manifest.Name)}.plugin.manifest"));
            Assert.IsNotNull(json);
            Assert.AreEqual(app.Manifest.Name, json.Name);
            Assert.AreEqual(app.Manifest.ProjectGuid, json.ProjectGuid);
            Assert.AreEqual(app.Manifest.Author, json.Author);
            Assert.AreEqual(app.Manifest.Description, json.Description);
            Assert.AreEqual(app.Manifest.License, json.License);
            Assert.AreEqual(app.Manifest.ReleaseNote, json.ReleaseNote);
            Assert.AreEqual(app.Manifest.Version, json.Version);
            json.References.ForEach(r => Assert.IsTrue(app.Manifest.References.Any(m => m.Include == r.Include)));
        }

        [Test, Apartment(ApartmentState.STA)]
        public void VerifyPackage()
        {
            var app = new App(this.args);
            app.Start();

            Assert.IsNotNull(Directory.EnumerateFiles(app.OutputPath).First(f => Path.GetFileName(f) == $"{app.Manifest.Name}.cdp4ck"));

            using var zipFile = ZipFile.OpenRead(Path.Combine(app.OutputPath, $"{app.Manifest.Name}.cdp4ck"));

            Assert.AreEqual(zipFile.Entries.Count, Directory.EnumerateFiles(app.OutputPath).Count() -1);
        }


        [Test]
        public void VerifyLicense()
        {
            var app = new App();
            app.Deserialize();
            app.GetAssemblyInfo();
            var license = app.GetLicense();
            Assert.IsNotNull(license);
            Assert.IsNotEmpty(license);
            Assert.IsFalse(license.Contains("$YEAR"));
            Assert.IsFalse(license.Contains("$PLUGIN_NAME"));
            Assert.IsTrue(license.Contains(app.AssemblyInfo.Name));
        }
    }
}