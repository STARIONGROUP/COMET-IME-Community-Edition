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

    using CDP4Dal;

    using CDP4Requirements.ReqIFDal;
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

            var specObjectType0 = new SpecObjectType() {Identifier = Guid.NewGuid().ToString()};
            var specObjectType1 = new SpecObjectType() {Identifier = Guid.NewGuid().ToString()};
            
            var specObjectTypeMap0 = new SpecObjectTypeMap(specObjectType0,
                new[]
                {
                    new ParameterizedCategoryRule(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a")),
                    new ParameterizedCategoryRule(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a"))
                }, 
                new[] 
                {
                    new Category(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a")),
                    new Category(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a"))
                }, 
                new[]
                {
                    new AttributeDefinitionMap(new AttributeDefinitionString() {Identifier = Guid.NewGuid().ToString()}, AttributeDefinitionMapKind.NAME ),
                    new AttributeDefinitionMap(new AttributeDefinitionString() {Identifier = Guid.NewGuid().ToString()}, AttributeDefinitionMapKind.SHORTNAME )
                },
                true);

            var specObjectTypeMap1 = new SpecObjectTypeMap(specObjectType1,
                new[]
                {
                    new ParameterizedCategoryRule(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a")),
                    new ParameterizedCategoryRule(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a"))
                }, 
                new[] 
                {
                    new Category(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a")),
                    new Category(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a"))
                }, 
                new[]
                {
                    new AttributeDefinitionMap(new AttributeDefinitionString() {Identifier = Guid.NewGuid().ToString()}, AttributeDefinitionMapKind.NAME ),
                    new AttributeDefinitionMap(new AttributeDefinitionString() {Identifier = Guid.NewGuid().ToString()}, AttributeDefinitionMapKind.SHORTNAME )
                },
                true);
            
            var specRelationType0 = new SpecRelationType() { Identifier = Guid.NewGuid().ToString() };
            var specRelationType1 = new SpecRelationType() { Identifier = Guid.NewGuid().ToString() };

            var specRelationTypeMap0 = new SpecRelationTypeMap(specRelationType0,
                new[]
                {
                    new ParameterizedCategoryRule(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a")),
                    new ParameterizedCategoryRule(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a"))
                },
                new[]
                {
                    new Category(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a")),
                    new Category(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a"))
                },
                new[]
                {
                    new AttributeDefinitionMap(new AttributeDefinitionString() {Identifier = Guid.NewGuid().ToString()}, AttributeDefinitionMapKind.NAME ),
                    new AttributeDefinitionMap(new AttributeDefinitionString() {Identifier = Guid.NewGuid().ToString()}, AttributeDefinitionMapKind.SHORTNAME )
                },
                new []
                {
                    new BinaryRelationshipRule(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a")), 
                    new BinaryRelationshipRule(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a")) 
                });

            var specRelationTypeMap1 = new SpecRelationTypeMap(specRelationType1,
                new[]
                {
                    new ParameterizedCategoryRule(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a")),
                    new ParameterizedCategoryRule(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a"))
                },
                new[]
                {
                    new Category(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a")),
                    new Category(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a"))
                },
                new[]
                {
                    new AttributeDefinitionMap(new AttributeDefinitionString() {Identifier = Guid.NewGuid().ToString()}, AttributeDefinitionMapKind.NAME ),
                    new AttributeDefinitionMap(new AttributeDefinitionString() {Identifier = Guid.NewGuid().ToString()}, AttributeDefinitionMapKind.SHORTNAME )
                },
                new[]
                {
                    new BinaryRelationshipRule(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a")),
                    new BinaryRelationshipRule(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a"))
                });

            var relationGroupType0 = new RelationGroupType() { Identifier = Guid.NewGuid().ToString() };
            var relationGroupType1 = new RelationGroupType() { Identifier = Guid.NewGuid().ToString() };

            var relationGroupTypeMap0 = new RelationGroupTypeMap(relationGroupType0,
                new[]
                {
                    new ParameterizedCategoryRule(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a")),
                    new ParameterizedCategoryRule(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a"))
                },
                new[]
                {
                    new Category(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a")),
                    new Category(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a"))
                },
                new[]
                {
                    new AttributeDefinitionMap(new AttributeDefinitionString() {Identifier = Guid.NewGuid().ToString()}, AttributeDefinitionMapKind.NAME ),
                    new AttributeDefinitionMap(new AttributeDefinitionString() {Identifier = Guid.NewGuid().ToString()}, AttributeDefinitionMapKind.SHORTNAME )
                },
                new[]
                {
                    new BinaryRelationshipRule(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a")),
                    new BinaryRelationshipRule(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a"))
                });

            var relationGroupTypeMap1 = new RelationGroupTypeMap(relationGroupType1,
                new[]
                {
                    new ParameterizedCategoryRule(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a")),
                    new ParameterizedCategoryRule(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a"))
                },
                new[]
                {
                    new Category(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a")),
                    new Category(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a"))
                },
                new[]
                {
                    new AttributeDefinitionMap(new AttributeDefinitionString() {Identifier = Guid.NewGuid().ToString()}, AttributeDefinitionMapKind.NAME ),
                    new AttributeDefinitionMap(new AttributeDefinitionString() {Identifier = Guid.NewGuid().ToString()}, AttributeDefinitionMapKind.SHORTNAME )
                },
                new[]
                {
                    new BinaryRelationshipRule(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a")),
                    new BinaryRelationshipRule(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a"))
                });


            var specificationType0 = new SpecificationType() { Identifier = Guid.NewGuid().ToString() };
            var specificationType1 = new SpecificationType() { Identifier = Guid.NewGuid().ToString() };

            var specificationTypeMap0 = new SpecTypeMap(specificationType0,
                new[]
                {
                    new ParameterizedCategoryRule(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a")),
                    new ParameterizedCategoryRule(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a"))
                },
                new[]
                {
                    new Category(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a")),
                    new Category(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a"))
                },
                new[]
                {
                    new AttributeDefinitionMap(new AttributeDefinitionString() {Identifier = Guid.NewGuid().ToString()}, AttributeDefinitionMapKind.NAME ),
                    new AttributeDefinitionMap(new AttributeDefinitionString() {Identifier = Guid.NewGuid().ToString()}, AttributeDefinitionMapKind.SHORTNAME )
                });

            var specificationTypeMap1 = new SpecTypeMap(specificationType1,
                new[]
                {
                    new ParameterizedCategoryRule(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a")),
                    new ParameterizedCategoryRule(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a"))
                },
                new[]
                {
                    new Category(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a")),
                    new Category(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a"))
                },
                new[]
                {
                    new AttributeDefinitionMap(new AttributeDefinitionString() {Identifier = Guid.NewGuid().ToString()}, AttributeDefinitionMapKind.NAME ),
                    new AttributeDefinitionMap(new AttributeDefinitionString() {Identifier = Guid.NewGuid().ToString()}, AttributeDefinitionMapKind.SHORTNAME )
                });

            this.mappingConfiguration = new ImportMappingConfiguration()
            {
                Name = "Nameeeeeeeeeee", Description = "Descriptionnnnnnnnnnnnnn",
                DatatypeDefinitionMap =
                {
                    { datatypeDefinition0, new DatatypeDefinitionMap(datatypeDefinition0, new TextParameterType(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a")), new Dictionary<EnumValue, EnumerationValueDefinition>() {{new EnumValue(), new EnumerationValueDefinition()} }) },
                    { datatypeDefinition1, new DatatypeDefinitionMap(datatypeDefinition1, new TextParameterType(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(), new Uri("http://a.a")), new Dictionary<EnumValue, EnumerationValueDefinition>() {{new EnumValue(), new EnumerationValueDefinition()} }) }
                },
                SpecObjectTypeMap =
                {
                    { specObjectType0, specObjectTypeMap0 },
                    { specObjectType1, specObjectTypeMap1 }
                },
                SpecRelationTypeMap =
                {
                    { specRelationType0, specRelationTypeMap0 },
                    { specRelationType1, specRelationTypeMap1 },
                },
                RelationGroupTypeMap =
                {
                    { relationGroupType0, relationGroupTypeMap0 },
                    { relationGroupType1, relationGroupTypeMap1 },
                },
                SpecificationTypeMap = 
                {
                    { specificationType0, specificationTypeMap0 },
                    { specificationType1, specificationTypeMap1 },
                }
            };

            this.settings = new RequirementsModuleSettings() { SavedConfigurations = { this.mappingConfiguration } };
            
            Assert.DoesNotThrow(() => new PluginSettingsService().Write(this.settings, 
                new DataTypeDefinitionMapConverter(new ReqIF(), null, null),
                new SpecObjectTypeMapConverter(new ReqIF(), null, null),
                new SpecRelationTypeMapConverter(new ReqIF(), null, null),
                new RelationGroupTypeMapConverter(new ReqIF(), null, null),
                new SpecificationTypeMapConverter(new ReqIF(), null, null)));
        }
    }
}
