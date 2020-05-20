// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Geren�, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft,
//            Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4PluginPackager.Tests
{
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Reflection;

    using CDP4PluginPackager.Models;

    using Newtonsoft.Json;

    using NUnit.Framework;
    
    [TestFixture]
    public class AppTestFixture
    {
        private string[] args;

        private const string TargetProject = "CDP4Dashboard";

        [OneTimeSetUp]
        public void Setup()
        {
            var prefix = Path.Combine(Assembly.GetExecutingAssembly().Location, @"../../../../../");
            var testDirectory = Path.Combine(prefix, TargetProject);

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
        
        [Test, Ignore("IME version not added at the moment")]
        public void VerifyIMEVersion()
        {
            var app = new App();
            app.Start();
            var version = app.GetCurrentIMEVersion(); 
            var json = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText($"{Path.Combine(app.OutputPath, app.Manifest.Name)}.plugin.manifest"));
            Assert.AreEqual(version, json.CompatibleIMEVersion);
        }

        [Test]
        public void VerifyManifestIntegrity()
        {
            var app = new App();
            app.Start();
            var json = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText($"{Path.Combine(app.OutputPath, app.Manifest.Name)}.plugin.manifest"));
            Assert.IsNotNull(json);
            Assert.AreEqual(app.Manifest.Name, json.Name);
            Assert.AreEqual(app.Manifest.ProjectGuid, json.ProjectGuid);
            Assert.AreEqual(app.Manifest.Author, json.Author);
            Assert.AreEqual(app.Manifest.Description, json.Description);
            Assert.AreEqual(app.Manifest.ReleaseNote, json.ReleaseNote);
            Assert.AreEqual(app.Manifest.Version, json.Version);
        }

        [Test]
        public void VerifyPackageIntegrity()
        {
            var app = new App(this.args);
            app.Start();

            Assert.IsNotNull(Directory.EnumerateFiles(app.OutputPath).First(f => Path.GetFileName(f) == $"{app.Manifest.Name}.cdp4ck"));

            using var zipFile = ZipFile.OpenRead(Path.Combine(app.OutputPath, $"{app.Manifest.Name}.cdp4ck"));

            Assert.AreEqual(zipFile.Entries.Count, Directory.EnumerateFiles(app.OutputPath).Count(f => !f.EndsWith(".pdb") && !f.EndsWith(".cdp4ck")));
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

        [Test]
        public void VerifyThatPropertiesAreTheSameOnManifestClasses()
        {
            var pluginPackagerManifestProperties = typeof(Manifest).GetProperties();
            var compositionManifestProperties = typeof(CDP4Composition.Modularity.Manifest).GetProperties();
            Assert.AreEqual(pluginPackagerManifestProperties.Length, compositionManifestProperties.Length);

            foreach (var property in pluginPackagerManifestProperties)
            {
                var propertyInComposition = compositionManifestProperties.Single(p => p.Name == property.Name);
                Assert.IsNotNull(propertyInComposition);
                Assert.AreEqual(property.PropertyType, propertyInComposition.PropertyType);
            }
        }

        [Test]
        public void VerifyThatMethodsAreTheSameOnManifestClasses()
        {
            var pluginPackagerManifestProperties = typeof(Manifest).GetMethods();
            var compositionManifestProperties = typeof(CDP4Composition.Modularity.Manifest).GetMethods();
            Assert.AreEqual(pluginPackagerManifestProperties.Length, compositionManifestProperties.Length);

            foreach (var property in pluginPackagerManifestProperties)
            {
                var propertyInComposition = compositionManifestProperties.Single(p => p.Name == property.Name);
                Assert.IsNotNull(propertyInComposition);
                Assert.AreEqual(property.ReturnType, propertyInComposition.ReturnType);
            }
        }
    }
}