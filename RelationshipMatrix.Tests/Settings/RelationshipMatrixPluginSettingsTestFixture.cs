// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationshipMatrixPluginSettingsTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2021 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
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

namespace CDP4RelationshipMatrix.Tests.Settings
{
    using System.Collections.Generic;
    using System.IO;

    using CDP4Common.CommonData;

    using CDP4Composition.PluginSettingService;

    using CDP4RelationshipMatrix.Settings;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="RelationshipMatrixPluginSettings"/>
    /// </summary>
    [TestFixture]
    public class RelationshipMatrixPluginSettingsTestFixture
    {
        private string expectedSettingsPath;
        private PluginSettingsService pluginSettingsService;
        private RelationshipMatrixModule relationshipMatrixModule;
        private RelationshipMatrixPluginSettings settings;

        [SetUp]
        public void SetUp()
        {
            this.pluginSettingsService = new PluginSettingsService();

            this.expectedSettingsPath =
                Path.Combine(
                    this.pluginSettingsService.AppDataFolder,
                    this.pluginSettingsService.Cdp4ConfigurationDirectoryFolder,
                    "CDP4RelationshipMatrix.settings.json");

            this.relationshipMatrixModule = new RelationshipMatrixModule(null, null, null, null, null);

            this.settings = new RelationshipMatrixPluginSettings
            {
                PossibleClassKinds = new List<ClassKind>
                {
                    ClassKind.ElementDefinition,
                    ClassKind.ElementUsage,
                    ClassKind.NestedElement,
                    ClassKind.RequirementsSpecification,
                    ClassKind.RequirementsGroup,
                    ClassKind.Requirement
                },
                PossibleDisplayKinds =  new List<DisplayKind>
                {
                    DisplayKind.Name,
                    DisplayKind.ShortName
                }
            };
        }

        [TearDown]
        public void TearDown()
        {
            var fileInfo = new FileInfo(this.expectedSettingsPath);
            fileInfo.Delete();
        }

        [Test]
        public void Verify_that_the_pluginservice_settings_are_serialized_as_expected()
        {
            var expectedSettingsContent = File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "Settings", "expectedSettings.settings.json"));

            Assert.DoesNotThrow(() => this.pluginSettingsService.Write(this.settings));

            var writtenContent = File.ReadAllText(this.expectedSettingsPath);
            Assert.That(expectedSettingsContent, Is.EqualTo(writtenContent));
        }

        [Test]
        public void Verify_that_the_pluginservice_settings_are_deserialized_as_expected()
        {
            File.Copy(Path.Combine(TestContext.CurrentContext.TestDirectory, "Settings", "expectedSettings.settings.json"), this.expectedSettingsPath, true);

            this.settings = this.pluginSettingsService.Read<RelationshipMatrixPluginSettings>();

            var expectedClassKinds = new List<ClassKind>
            {
                ClassKind.ElementDefinition,
                ClassKind.ElementUsage,
                ClassKind.NestedElement,
                ClassKind.RequirementsSpecification,
                ClassKind.RequirementsGroup,
                ClassKind.Requirement
            };

            var expectedDisplayKinds = new List<DisplayKind>
            {
                DisplayKind.Name,
                DisplayKind.ShortName
            };

            Assert.That(this.settings.PossibleClassKinds, Is.EquivalentTo(expectedClassKinds));
            Assert.That(this.settings.PossibleDisplayKinds, Is.EquivalentTo(expectedDisplayKinds));
        }
    }
}
