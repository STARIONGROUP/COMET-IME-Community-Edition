// -------------------------------------------------------------------------------------------------
// <copyright file="PluginSettingsService.cs" company="RHEA System S.A.">
//   Copyright (c) 2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.PluginSettingService
{
    using System;
    using System.IO;
    using CDP4Composition.PluginSettingService;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="PluginSettingsService"/> class.
    /// </summary>
    [TestFixture]
    public class PluginSettingsServiceTestFixture
    {
        private PluginSettingsService pluginSettingsService;
        private TestModule testModule;
        private string expectedSettingsPath;
        
        [SetUp]
        public void SetUp()
        {
            this.expectedSettingsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"RHEA/CDP4/CDP4Composition.Tests.settings.json");
            
            this.pluginSettingsService = new PluginSettingsService();
            this.testModule = new TestModule(this.pluginSettingsService);
        }

        [TearDown]
        public void TearDown()
        {
            var fileInfo = new FileInfo(this.expectedSettingsPath);
            fileInfo.Delete();
        }

        [Test]
        public void Verify_that_on_write_ArgumentNullException_is_thrown()
        {
            Assert.Throws<ArgumentNullException>(() => this.pluginSettingsService.Write<TestSettings>(null, this.testModule));

            var settings = new TestSettings();
            Assert.Throws<ArgumentNullException>(() => this.pluginSettingsService.Write(settings, null));
        }

        [Test]
        public void Verify_that_on_read_ArgumentNullException_is_thrown()
        {
            Assert.Throws<ArgumentNullException>(() => this.pluginSettingsService.Read<TestSettings>(null));
        }

        [Test]
        public void Verify_that_the_settings_can_be_written_to_disk()
        {
            var settings = new TestSettings
            {
                Identifier = Guid.Parse("78d90eda-bc57-45fe-8bfa-b9ca23130a00"),
                Description = "this is a description"
            };

            Assert.DoesNotThrow(() => this.pluginSettingsService.Write(settings, this.testModule));

            var expectedSettingsContent = File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "PluginSettingService", "expectedSettings.settings.json"));
            var writtenContent = File.ReadAllText(this.expectedSettingsPath);
            Assert.AreEqual(expectedSettingsContent, writtenContent);
        }

        [Test]
        public void Verify_that_the_settings_can_be_read_from_disk()
        {
            File.Copy(Path.Combine(TestContext.CurrentContext.TestDirectory, "PluginSettingService", "expectedSettings.settings.json"), this.expectedSettingsPath);
            
            var settings = this.pluginSettingsService.Read<TestSettings>(this.testModule);
            Assert.AreEqual(Guid.Parse("78d90eda-bc57-45fe-8bfa-b9ca23130a00"), settings.Identifier);
            Assert.AreEqual("this is a description", settings.Description);
        }

        [Test]
        public void Verify_that_settings_can_be_read_and_written_to_disk()
        {
            File.Copy(Path.Combine(TestContext.CurrentContext.TestDirectory, "PluginSettingService", "expectedSettings.settings.json"), this.expectedSettingsPath);

            var settings = this.pluginSettingsService.Read<TestSettings>(this.testModule);
            Assert.AreEqual(Guid.Parse("78d90eda-bc57-45fe-8bfa-b9ca23130a00"), settings.Identifier);
            Assert.AreEqual("this is a description", settings.Description);

            var id = Guid.NewGuid();
            var description = "this is a new description";

            settings.Identifier = id;
            settings.Description = description;

            this.pluginSettingsService.Write(settings, this.testModule);

            var newSettings = this.pluginSettingsService.Read<TestSettings>(this.testModule);

            Assert.AreEqual(id, newSettings.Identifier);
            Assert.AreEqual(description, newSettings.Description);
        }
    }
}