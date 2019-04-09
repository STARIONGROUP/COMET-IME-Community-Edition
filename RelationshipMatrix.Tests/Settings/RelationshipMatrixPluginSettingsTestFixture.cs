// -------------------------------------------------------------------------------------------------
// <copyright file="RelationshipMatrixPluginSettingsTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2018-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Tests.Settings
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using CDP4Common.CommonData;
    using CDP4Composition.PluginSettingService;
    using CDP4RelationshipMatrix.Settings;
    using Microsoft.Practices.Prism.Regions;
    using Moq;
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
        private Mock<IRegionManager> regionManager;

        [SetUp]
        public void SetUp()
        {
            this.expectedSettingsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"RHEA/CDP4/CDP4RelationshipMatrix.settings.json");
            this.regionManager = new Mock<IRegionManager>();

            this.pluginSettingsService = new PluginSettingsService();
            this.relationshipMatrixModule = new RelationshipMatrixModule(this.regionManager.Object, null, null, null, null, null);

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

            Assert.DoesNotThrow(() => this.pluginSettingsService.Write(settings, this.relationshipMatrixModule));

            var writtenContent = File.ReadAllText(this.expectedSettingsPath);
            Assert.That(expectedSettingsContent, Is.EqualTo(writtenContent));
        }

        [Test]
        public void Verify_that_the_pluginservice_settings_are_deserialized_as_expected()
        {
            File.Copy(Path.Combine(TestContext.CurrentContext.TestDirectory, "Settings", "expectedSettings.settings.json"), this.expectedSettingsPath, true);

            this.settings = this.pluginSettingsService.Read<RelationshipMatrixPluginSettings>(this.relationshipMatrixModule);

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