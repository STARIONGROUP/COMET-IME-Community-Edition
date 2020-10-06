// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginSettingsService.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
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

namespace CDP4Composition.Tests.PluginSettingService
{
    using System;
    using System.IO;
    using System.Linq;

    using CDP4Composition.PluginSettingService;
    using CDP4Composition.Tests.Views;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="PluginSettingsService"/> class.
    /// </summary>
    [TestFixture]
    public class PluginSettingsServiceTestFixture
    {
        private PluginSettingsService pluginSettingsService;
        private string expectedSettingsPath;
        private TestSettings testSettings;

        [SetUp]
        public void SetUp()
        {
            this.pluginSettingsService = new PluginSettingsService();
         
            this.expectedSettingsPath =
                Path.Combine(
                    this.pluginSettingsService.AppDataFolder,
                    this.pluginSettingsService.Cdp4ConfigurationDirectoryFolder,
                    "CDP4Composition.Tests.settings.json");

            this.testSettings = new TestSettings
            {
                Identifier = Guid.Parse("78d90eda-bc57-45fe-8bfa-b9ca23130a00"),
                Description = "this is a description"
            };
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(this.expectedSettingsPath))
            {
                File.Delete(this.expectedSettingsPath);
            }
        }

        [Test]
        public void Verify_that_on_write_ArgumentNullException_is_thrown()
        {
            Assert.Throws<ArgumentNullException>(() => this.pluginSettingsService.Write(default(TestSettings)));
        }

        [Test]
        public void Verify_that_the_settings_can_be_written_to_disk()
        {
            Assert.DoesNotThrow(() => this.pluginSettingsService.Write(this.testSettings));

            var expectedSettingsContent = File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "PluginSettingService", "expectedSettings.settings.json"));
            var writtenContent = File.ReadAllText(this.expectedSettingsPath);
            Assert.AreEqual(expectedSettingsContent, writtenContent);
        }

        [Test]
        public void Verify_that_the_settings_can_be_read_from_disk()
        {
            this.pluginSettingsService.CheckApplicationConfigurationDirectory();

            File.Copy(Path.Combine(TestContext.CurrentContext.TestDirectory, "PluginSettingService", "expectedSettings.settings.json"), this.expectedSettingsPath);
            Assert.IsTrue(File.Exists(this.expectedSettingsPath));

            var readSettings = this.pluginSettingsService.Read<TestSettings>();
            Assert.AreEqual(this.testSettings.Identifier, readSettings.Identifier);
            Assert.AreEqual(this.testSettings.Description, readSettings.Description);
        }

        [Test]
        public void Verify_that_settings_can_be_read_and_written_to_disk()
        {
            this.pluginSettingsService.CheckApplicationConfigurationDirectory();

            File.Copy(Path.Combine(TestContext.CurrentContext.TestDirectory, "PluginSettingService", "expectedSettings.settings.json"), this.expectedSettingsPath);
            Assert.IsTrue(File.Exists(this.expectedSettingsPath));

            var readSettings = this.pluginSettingsService.Read<TestSettings>();
            Assert.AreEqual(this.testSettings.Identifier, readSettings.Identifier);
            Assert.AreEqual(this.testSettings.Description, readSettings.Description);

            var id = Guid.NewGuid();
            var description = "this is a new description";
            var ownTestConfigurationProperty = "NotNull";

            readSettings.Identifier = id;
            readSettings.Description = description;
            readSettings.SavedConfigurations.Add(new TestConfiguration() { OwnTestConfigurationProperty = ownTestConfigurationProperty });

            this.pluginSettingsService.Write(readSettings);
            var newSettings = this.pluginSettingsService.Read<TestSettings>();

            Assert.AreEqual(id, newSettings.Identifier);
            Assert.AreEqual(description, newSettings.Description);
            Assert.AreNotEqual($"-{ownTestConfigurationProperty}", newSettings.SavedConfigurations.OfType<TestConfiguration>().First().OwnTestConfigurationProperty);
        }
    }
}
