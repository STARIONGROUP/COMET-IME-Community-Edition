// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementModuleSettingsTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
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

namespace CDP4Requirements.Tests.Settings
{
    using NUnit.Framework;

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.DAL;

    using CDP4Requirements.ReqIFDal;
    using CDP4Requirements.Settings.JsonConverters;
    using CDP4Requirements.ViewModels;

    using Moq;

    using ReqIFSharp;

    using File = System.IO.File;

    [TestFixture]
    public class RequirementModuleSettingsTestFixture
    {
        private RequirementsModuleSettings settings;
        private ImportMappingConfiguration mappingConfiguration;
        private PluginSettingsService pluginSettingsService;
        private string expectedSettingsPath;
        private Iteration iteration;
        private string uri;
        private ISession session;
        private ReqIF reqIf;
        private DataTypeDefinitionMapConverter dataTypeDefinitionMapConverter;
        private SpecObjectTypeMapConverter specObjectTypeMapConverter;
        private SpecRelationTypeMapConverter specRelationTypeMapConverter;
        private RelationGroupTypeMapConverter relationGroupTypeMapConverter;
        private SpecificationTypeMapConverter specificationTypeMapConverter;
        private TextParameterType parameterType0;
        private TextParameterType parameterType1;
        private ParameterizedCategoryRule rule0;
        private ParameterizedCategoryRule rule1;
        private Category category0;
        private Category category1;
        private BinaryRelationshipRule binaryRelationshipRule0;
        private BinaryRelationshipRule binaryRelationshipRule1;

        [SetUp]
        public void Setup()
        {
            this.pluginSettingsService = new PluginSettingsService();
            
            this.expectedSettingsPath =
                Path.Combine(
                    this.pluginSettingsService.AppDataFolder,
                    this.pluginSettingsService.Cdp4ConfigurationDirectoryFolder,
                    "CDP4Requirements.settings.json");

            this.iteration = new Iteration() { };

            this.uri = "http://www.rheagroup.com/";
            var credentials = new Credentials("John", "Doe", new Uri(this.uri));
            this.session = new Session(new Mock<IDal>().Object, credentials);

            var reqIfPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Settings","testreq.reqif");
            this.reqIf = new ReqIFDeserializer().Deserialize(reqIfPath);

            this.dataTypeDefinitionMapConverter = new DataTypeDefinitionMapConverter(this.reqIf, this.session);
            this.specObjectTypeMapConverter = new SpecObjectTypeMapConverter(this.reqIf, this.session);
            this.specRelationTypeMapConverter = new SpecRelationTypeMapConverter(this.reqIf, this.session);
            this.relationGroupTypeMapConverter = new RelationGroupTypeMapConverter(this.reqIf, this.session);
            this.specificationTypeMapConverter = new SpecificationTypeMapConverter(this.reqIf, this.session);
        }

        private void SetupData()
        {
            var datatypeDefinition0 = new DatatypeDefinitionString() { Identifier = this.reqIf.CoreContent.FirstOrDefault()?.DataTypes[0].Identifier };
            var datatypeDefinition1 = new DatatypeDefinitionString() { Identifier = this.reqIf.CoreContent.FirstOrDefault()?.DataTypes[1].Identifier };
            
            var specObjectType0 = new SpecObjectType() { Identifier = this.reqIf.CoreContent.FirstOrDefault()?.SpecTypes.OfType<SpecObjectType>().ToList()[0].Identifier };
            var specObjectType1 = new SpecObjectType() { Identifier = this.reqIf.CoreContent.FirstOrDefault()?.SpecTypes.OfType<SpecObjectType>().ToList()[1].Identifier };

            var specRelationType0 = new SpecRelationType() { Identifier = this.reqIf.CoreContent.FirstOrDefault()?.SpecTypes.OfType<SpecRelationType>().ToList()[0].Identifier };

            this.rule0 = new ParameterizedCategoryRule(Guid.NewGuid(), this.session.Assembler.Cache, new Uri(this.uri));
            this.rule1 = new ParameterizedCategoryRule(Guid.NewGuid(), this.session.Assembler.Cache, new Uri(this.uri));
            this.session.Assembler.Cache.TryAdd(new CacheKey(this.rule0.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.rule0));
            this.session.Assembler.Cache.TryAdd(new CacheKey(this.rule1.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.rule1));
            
            this.category0 = new Category(Guid.NewGuid(), this.session.Assembler.Cache, new Uri(this.uri));
            this.category1 = new Category(Guid.NewGuid(), this.session.Assembler.Cache, new Uri(this.uri));
            this.session.Assembler.Cache.TryAdd(new CacheKey(this.category0.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.category0));
            this.session.Assembler.Cache.TryAdd(new CacheKey(this.category1.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.category1));

            var specObjectTypeMap0 = new SpecObjectTypeMap(specObjectType0,
                new[] { this.rule0, this.rule1 },
                new[] { this.category0, this.category1 },
                new[]
                {
                    new AttributeDefinitionMap(new AttributeDefinitionString() { Identifier = this.reqIf.CoreContent.FirstOrDefault()?.SpecTypes[0].SpecAttributes[0].Identifier }, AttributeDefinitionMapKind.NAME ),
                    new AttributeDefinitionMap(new AttributeDefinitionString() { Identifier = this.reqIf.CoreContent.FirstOrDefault()?.SpecTypes[0].SpecAttributes[1].Identifier }, AttributeDefinitionMapKind.SHORTNAME )
                },
                true);

            var specObjectTypeMap1 = new SpecObjectTypeMap(specObjectType1,
                new[] { this.rule0, this.rule1 },
                new[] { this.category0, this.category1 },
                new[]
                {
                    new AttributeDefinitionMap(new AttributeDefinitionString() { Identifier = this.reqIf.CoreContent.FirstOrDefault()?.SpecTypes[0].SpecAttributes[0].Identifier }, AttributeDefinitionMapKind.NAME ),
                    new AttributeDefinitionMap(new AttributeDefinitionString() { Identifier = this.reqIf.CoreContent.FirstOrDefault()?.SpecTypes[0].SpecAttributes[1].Identifier }, AttributeDefinitionMapKind.SHORTNAME )
                },
                true);

            this.binaryRelationshipRule0 = new BinaryRelationshipRule(Guid.NewGuid(), this.session.Assembler.Cache, new Uri(this.uri));
            this.binaryRelationshipRule1 = new BinaryRelationshipRule(Guid.NewGuid(), this.session.Assembler.Cache, new Uri(this.uri));

            this.session.Assembler.Cache.TryAdd(new CacheKey(this.binaryRelationshipRule0.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.binaryRelationshipRule0));
            this.session.Assembler.Cache.TryAdd(new CacheKey(this.binaryRelationshipRule1.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.binaryRelationshipRule1));
            
            var specRelationTypeMap0 = new SpecRelationTypeMap(specRelationType0,
                new[] { this.rule0, this.rule1 },
                new[] { this.category0, this.category1 },
                new[]
                {
                    new AttributeDefinitionMap(new AttributeDefinitionString() { Identifier = this.reqIf.CoreContent.FirstOrDefault()?.SpecTypes[0].SpecAttributes[0].Identifier }, AttributeDefinitionMapKind.NAME ),
                    new AttributeDefinitionMap(new AttributeDefinitionString() { Identifier = this.reqIf.CoreContent.FirstOrDefault()?.SpecTypes[0].SpecAttributes[1].Identifier }, AttributeDefinitionMapKind.SHORTNAME )
                },
                new[] { this.binaryRelationshipRule0, this.binaryRelationshipRule1 });

            var relationGroupType0 = new RelationGroupType() { Identifier = this.reqIf.CoreContent.FirstOrDefault()?.SpecTypes.OfType<SpecRelationType>().ToList()[0].Identifier };
            
            var relationGroupTypeMap0 = new RelationGroupTypeMap(relationGroupType0,
                new[] { this.rule0, this.rule1 },
                new[] { this.category0, this.category1 },
                new[]
                {
                    new AttributeDefinitionMap(new AttributeDefinitionString() { Identifier = this.reqIf.CoreContent.FirstOrDefault()?.SpecTypes[0].SpecAttributes[0].Identifier }, AttributeDefinitionMapKind.NAME ),
                    new AttributeDefinitionMap(new AttributeDefinitionString() { Identifier = this.reqIf.CoreContent.FirstOrDefault()?.SpecTypes[0].SpecAttributes[1].Identifier }, AttributeDefinitionMapKind.SHORTNAME )
                },
                new[] { this.binaryRelationshipRule0, this.binaryRelationshipRule1 });

            var specificationType0 = this.reqIf.CoreContent.FirstOrDefault()?.SpecTypes.OfType<SpecificationType>().First();

            var specificationTypeMap0 = new SpecTypeMap(specificationType0,
                new[] { this.rule0, this.rule1 },
                new[] { this.category0, this.category1 },
                new[]
                {
                    new AttributeDefinitionMap(new AttributeDefinitionString() { Identifier = this.reqIf.CoreContent.FirstOrDefault()?.SpecTypes[0].SpecAttributes[0].Identifier }, AttributeDefinitionMapKind.NAME ),
                    new AttributeDefinitionMap(new AttributeDefinitionString() { Identifier = this.reqIf.CoreContent.FirstOrDefault()?.SpecTypes[0].SpecAttributes[1].Identifier }, AttributeDefinitionMapKind.SHORTNAME )
                });

            this.parameterType0 = new TextParameterType(Guid.NewGuid(), this.session.Assembler.Cache, new Uri(this.uri));
            this.parameterType1 = new TextParameterType(Guid.NewGuid(), this.session.Assembler.Cache, new Uri(this.uri));
            this.session.Assembler.Cache.TryAdd(new CacheKey(this.parameterType0.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.parameterType0));
            this.session.Assembler.Cache.TryAdd(new CacheKey(this.parameterType1.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.parameterType1));
            
            this.mappingConfiguration = new ImportMappingConfiguration()
            {
                Name = "TestName",
                Description = "TestDescription",
                DatatypeDefinitionMap =
                {
                    { datatypeDefinition0, new DatatypeDefinitionMap(datatypeDefinition0, this.parameterType0, new Dictionary<EnumValue, EnumerationValueDefinition>() {{new EnumValue(), new EnumerationValueDefinition()} }) },
                    { datatypeDefinition1, new DatatypeDefinitionMap(datatypeDefinition1, this.parameterType1, new Dictionary<EnumValue, EnumerationValueDefinition>() {{new EnumValue(), new EnumerationValueDefinition()} }) }
                },
                SpecObjectTypeMap =
                {
                    { specObjectType0, specObjectTypeMap0 },
                    { specObjectType1, specObjectTypeMap1 }
                },
                SpecRelationTypeMap =
                {
                    { specRelationType0, specRelationTypeMap0 }
                },
                RelationGroupTypeMap =
                {
                    { relationGroupType0, relationGroupTypeMap0 }
                },
                SpecificationTypeMap =
                {
                    { specificationType0, specificationTypeMap0 }
                }
            };

            this.settings = new RequirementsModuleSettings() { SavedConfigurations = { this.mappingConfiguration } };
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(this.expectedSettingsPath))
            {
                //File.Delete(this.expectedSettingsPath);
            }
        }

        [Test]
        public void VerifySettingsCanBeSerialized()
        {
            this.SetupData();

            Assert.DoesNotThrow(() =>
            {
                this.pluginSettingsService.Write(this.settings,
                    this.dataTypeDefinitionMapConverter,
                    this.specObjectTypeMapConverter,
                    this.specRelationTypeMapConverter,
                    this.relationGroupTypeMapConverter,
                    this.specificationTypeMapConverter);
            });
        }

        [Test]
        public void VerifySettingsCanBeDeserialized()
        {
            this.SetupData();

            Assert.DoesNotThrow(() =>
            {
                this.pluginSettingsService.Write(this.settings,
                    this.dataTypeDefinitionMapConverter,
                    this.specObjectTypeMapConverter,
                    this.specRelationTypeMapConverter,
                    this.relationGroupTypeMapConverter,
                    this.specificationTypeMapConverter);
            });

            ImportMappingConfiguration newSettings = null;

            Assert.DoesNotThrow(() => newSettings = new PluginSettingsService().Read<RequirementsModuleSettings>(true,
                this.dataTypeDefinitionMapConverter,
                this.specObjectTypeMapConverter,
                this.specRelationTypeMapConverter,
                this.relationGroupTypeMapConverter,
                this.specificationTypeMapConverter).SavedConfigurations.Cast<ImportMappingConfiguration>().First());

            Assert.IsNotNull(newSettings);
            Assert.AreEqual(0, newSettings.DatatypeDefinitionMap.Count);
            Assert.AreEqual(2, newSettings.SpecObjectTypeMap.Count);
            Assert.AreEqual(1, newSettings.SpecRelationTypeMap.Count);
            Assert.AreEqual(0, newSettings.RelationGroupTypeMap.Count);
            Assert.AreEqual(1, newSettings.SpecificationTypeMap.Count);
        }
    }
}
