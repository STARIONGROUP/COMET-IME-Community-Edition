// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SdkPluginPackagerTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft,
//            Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

    /// <summary>
    /// Suite of tests for The <see cref="SdkPluginPackager"/> class
    /// </summary>
    [TestFixture]
    public class SdkPluginPackagerTestFixture
    {
        private const string TargetProject = "CDP4Dashboard";

        private SdkPluginPackager sdkPluginPackager;

        [SetUp]
        public void Setup()
        {
            var prefix = Path.Combine(Assembly.GetExecutingAssembly().Location, $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}");
            var testDirectory = Path.Combine(prefix, TargetProject);
            Directory.SetCurrentDirectory(testDirectory);

#if DEBUG
            this.sdkPluginPackager = new SdkPluginPackager(testDirectory, true, "Debug", "net8", "AnyCPU");
#else
            this.sdkPluginPackager = new SdkPluginPackager(testDirectory, true, "Release", "net8", "AnyCPU");
#endif
            this.sdkPluginPackager.Start();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            var file = Path.Combine(this.sdkPluginPackager.OutputPath, $"{this.sdkPluginPackager.Manifest.Name}.cdp4ck");

            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsNotNull(this.sdkPluginPackager.Csproj);
            CollectionAssert.IsNotEmpty(this.sdkPluginPackager.Csproj.PropertyGroup);
        }

        [Test]
        public void VerifyManifest()
        {
            var json = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText($"{Path.Combine(this.sdkPluginPackager.OutputPath, this.sdkPluginPackager.Manifest.Name)}.plugin.manifest"));
            Assert.IsNotNull(json);
            Assert.AreEqual(this.sdkPluginPackager.Manifest.Name, json.Name);
            Assert.AreEqual(this.sdkPluginPackager.Manifest.ProjectGuid, json.ProjectGuid);
            Assert.AreEqual(this.sdkPluginPackager.Manifest.Author, json.Author);
            Assert.AreEqual(this.sdkPluginPackager.Manifest.Description, json.Description);
            Assert.AreEqual(this.sdkPluginPackager.Manifest.ReleaseNote, json.ReleaseNote);
            Assert.AreEqual(this.sdkPluginPackager.Manifest.Version, json.Version);
            Assert.AreEqual(this.sdkPluginPackager.Manifest.MinIMEVersion, json.MinIMEVersion);
        }

        [Test]
        public void VerifyPackageIntegrity()
        {
            Assert.IsNotNull(Directory.EnumerateFiles(this.sdkPluginPackager.OutputPath).First(f => Path.GetFileName(f) == $"{this.sdkPluginPackager.Manifest.Name}.cdp4ck"));

            using var zipFile = ZipFile.OpenRead(Path.Combine(this.sdkPluginPackager.OutputPath, $"{this.sdkPluginPackager.Manifest.Name}.cdp4ck"));

            Assert.AreEqual(zipFile.Entries.Count, Directory.EnumerateFiles(this.sdkPluginPackager.OutputPath).Count(f => !f.EndsWith(".pdb") && !f.EndsWith(".cdp4ck")));
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