using NUnit.Framework;

namespace CDP4Requirements.Tests.Settings
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.PluginSettingService;
    using CDP4Composition.Services.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.DAL;

    using CDP4Requirements.Settings.JsonConverters;
    using CDP4Requirements.ViewModels;

    using ReqIFSharp;

    [TestFixture]
    public class RequirementModuleSettingsTestFixture
    {
        private RequirementsModuleSettings settings;
        private ImportMappingConfiguration mappingConfiguration;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void VerifySettingsCanBeSerialized()
        {
            var datatypeDefinition0 = new DatatypeDefinitionString() { Identifier = Guid.NewGuid().ToString() };
            var datatypeDefinition1 = new DatatypeDefinitionString() { Identifier = Guid.NewGuid().ToString() };
            
            this.mappingConfiguration = new ImportMappingConfiguration()
            {
                Name = "Nameeeeeeeeeee", Description = "Descriptionnnnnnnnnnnnnn",
                DatatypeDefinitionMap =
                {
                    { datatypeDefinition0, new DatatypeDefinitionMap(datatypeDefinition0, new TextParameterType(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a")), new Dictionary<EnumValue, EnumerationValueDefinition>() {{new EnumValue(), new EnumerationValueDefinition()} }) },
                    { datatypeDefinition1, new DatatypeDefinitionMap(datatypeDefinition1, new TextParameterType(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a")), new Dictionary<EnumValue, EnumerationValueDefinition>() {{new EnumValue(), new EnumerationValueDefinition()} }) }
                }
            };

            this.settings = new RequirementsModuleSettings() { SavedConfigurations = { this.mappingConfiguration } };
            Assert.DoesNotThrow(() => new PluginSettingsService().Write(this.settings, new RequirementsModuleSettingsConverter(new ReqIF(), null, null)));
        }
    }
}
