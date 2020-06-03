// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OldSchoolPluginPackagerTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft,
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

    /// <summary>
    /// Suite of tests for The <see cref="OldSchoolPluginPackager"/> class
    /// </summary>
    [TestFixture]
    public class OldSchoolPluginPackagerTestFixture
    {
        private const string TargetProject = "CDP4Scripting";

        private OldSchoolPluginPackager oldSchoolPluginPackager;

        [OneTimeSetUp]
        public void Setup()
        {
            var prefix = Path.Combine(Assembly.GetExecutingAssembly().Location, @"../../../../../");
            var testDirectory = Path.Combine(prefix, TargetProject);
            Directory.SetCurrentDirectory(testDirectory);

            this.oldSchoolPluginPackager = new OldSchoolPluginPackager(testDirectory, true);
            this.oldSchoolPluginPackager.Start();
        }

        [Ignore("")]
        public void VerifyProperties()
        {
            Assert.IsNotNull(this.oldSchoolPluginPackager.Csproj);
            Assert.IsNotEmpty(this.oldSchoolPluginPackager.Csproj.ItemGroup);
        }

        [Ignore("")]
        public void VerifyManifest()
        {
            var json = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText($"{Path.Combine(this.oldSchoolPluginPackager.OutputPath, this.oldSchoolPluginPackager.Manifest.Name)}.plugin.manifest"));
            Assert.IsNotNull(json);
            Assert.AreEqual(this.oldSchoolPluginPackager.Manifest.Name, json.Name);
            Assert.AreEqual(this.oldSchoolPluginPackager.Manifest.ProjectGuid, json.ProjectGuid);
            Assert.AreEqual(this.oldSchoolPluginPackager.Manifest.Author, json.Author);
            Assert.AreEqual(this.oldSchoolPluginPackager.Manifest.Description, json.Description);
            Assert.AreEqual(this.oldSchoolPluginPackager.Manifest.ReleaseNote, json.ReleaseNote);
            Assert.AreEqual(this.oldSchoolPluginPackager.Manifest.Version, json.Version);
        }

        [Ignore("")]
        public void VerifyPackageIntegrity()
        {
            Assert.IsNotNull(Directory.EnumerateFiles(this.oldSchoolPluginPackager.OutputPath).First(f => Path.GetFileName(f) == $"{this.oldSchoolPluginPackager.Manifest.Name}.cdp4ck"));

            using (var zipFile = ZipFile.OpenRead(Path.Combine(this.oldSchoolPluginPackager.OutputPath, $"{this.oldSchoolPluginPackager.Manifest.Name}.cdp4ck")))
            {
                Assert.AreEqual(zipFile.Entries.Count, Directory.EnumerateFiles(this.oldSchoolPluginPackager.OutputPath).Count(f => !f.EndsWith(".pdb") && !f.EndsWith(".cdp4ck")));
            }
        }

        [Ignore("")]
        public void VerifyLicense()
        {
            var license = this.oldSchoolPluginPackager.GetLicense();

            Assert.IsNotNull(license);
            Assert.IsNotEmpty(license);
            Assert.IsFalse(license.Contains("$YEAR"));
            Assert.IsFalse(license.Contains("$PLUGIN_NAME"));
        }

        [Ignore("")]
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

        [Ignore("")]
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