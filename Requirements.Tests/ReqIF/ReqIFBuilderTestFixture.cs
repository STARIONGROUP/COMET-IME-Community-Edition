// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReqIFBuilderTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests
{
    using System;
    using System.IO;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Services;

    using CDP4Dal;

    using CDP4Requirements.ReqIFDal;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReqIFSharp;

    [TestFixture]
    internal class ReqIFBuilderTestFixture
    {
        private Mock<ISession> session;
        private Assembler assembler;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IMessageBoxService> messageBoxService;

        private SiteDirectory sitedir;
        private EngineeringModelSetup modelsetup;
        private SiteReferenceDataLibrary srdl;
        private ModelReferenceDataLibrary mrdl;
        private IterationSetup iterationsetup;
        private EngineeringModel model;
        private Iteration iteration;

        private RequirementsSpecification reqSpec2;
        private RequirementsSpecification reqSpec;
        private RequirementsSpecification deprecatedRequirementsSpecification;
        private RequirementsGroup group1;
        private RequirementsGroup group11;
        private Requirement req;
        private Requirement req1;
        private Requirement req11;
        private Requirement req2;
        private Requirement deprecatedRequirement;

        private BinaryRelationship deriveRelationship1;
        private BinaryRelationship deriveRelationship2;
        private BinaryRelationship deriveRelationship3;
        private BinaryRelationship deriveRelationship4;
        private BinaryRelationship specDeriveRelationship1;
        private BinaryRelationship specDeriveRelationship2;
        private BinaryRelationshipRule derivedRule;
        private BinaryRelationshipRule specRuleType;

        private Category deriveCat;
        private Category functionalReq;

        private BooleanParameterType booleanParameterType;

        private SimpleParameterValue reqValue;
        private SimpleParameterValue reqValue1;
        private SimpleParameterValue reqValue11;
        private SimpleParameterValue reqValue2;
        private SimpleParameterValue deprecatedReqValue;

        private ParameterizedCategoryRule parameRule;

        private Category reqCategory;
        private Category specCategory;

        private Category specRelationRuleCategory;

        private readonly Uri uri = new Uri("https://www.stariongroup.eu");
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.serviceLocator = new Mock<IServiceLocator>();
            this.messageBoxService = new Mock<IMessageBoxService>();

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IMessageBoxService>()).Returns(this.messageBoxService.Object);

            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.assembler = new Assembler(this.uri, this.messageBus);
            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.sitedir.Model.Add(this.modelsetup);
            this.iterationsetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.modelsetup.IterationSetup.Add(this.iterationsetup);
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.sitedir.SiteReferenceDataLibrary.Add(this.srdl);
            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) { RequiredRdl = this.srdl };
            this.modelsetup.RequiredRdl.Add(this.mrdl);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri) { EngineeringModelSetup = this.modelsetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri) { IterationSetup = this.iterationsetup };
            this.model.Iteration.Add(this.iteration);

            this.reqSpec = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.reqSpec2 = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.iteration.RequirementsSpecification.Add(this.reqSpec);
            this.iteration.RequirementsSpecification.Add(this.reqSpec2);

            this.deprecatedRequirementsSpecification = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.deprecatedRequirementsSpecification.IsDeprecated = true;
            this.iteration.RequirementsSpecification.Add(this.deprecatedRequirementsSpecification);

            this.deprecatedRequirement = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.deprecatedRequirement.IsDeprecated = true;
            this.deprecatedRequirementsSpecification.Requirement.Add(this.deprecatedRequirement);

            this.group1 = new RequirementsGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.reqSpec.Group.Add(this.group1);
            this.group11 = new RequirementsGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.group1.Group.Add(this.group11);

            this.req = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var definition = new Definition(Guid.NewGuid(), this.assembler.Cache, this.uri) { Content = "def0" };
            this.req.Definition.Add(definition);

            this.req1 = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { Group = this.group1 };
            var definition1 = new Definition(Guid.NewGuid(), this.assembler.Cache, this.uri) { Content = "def1" };
            this.req1.Definition.Add(definition1);

            this.req11 = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { Group = this.group11 };
            var definition11 = new Definition(Guid.NewGuid(), this.assembler.Cache, this.uri) { Content = "def11" };
            this.req11.Definition.Add(definition11);

            this.req2 = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var definition2 = new Definition(Guid.NewGuid(), this.assembler.Cache, this.uri) { Content = "def2" };
            this.req2.Definition.Add(definition2);

            this.reqSpec.Requirement.Add(this.req);
            this.reqSpec.Requirement.Add(this.req1);
            this.reqSpec.Requirement.Add(this.req11);

            this.reqSpec2.Requirement.Add(this.req2);

            this.booleanParameterType = new BooleanParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "bool", ShortName = "bool" };
            this.srdl.ParameterType.Add(this.booleanParameterType);

            this.functionalReq = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "Functional", ShortName = "Func" };

            this.deriveCat = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "Derive Category", ShortName = "Derive" };

            this.reqCategory = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "req cat", ShortName = "reqcat" };
            this.specCategory = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "spec cat", ShortName = "speccat" };

            this.specRelationRuleCategory = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "Spec Link", ShortName = "SpecLink" };

            this.srdl.DefinedCategory.Add(this.functionalReq);
            this.srdl.DefinedCategory.Add(this.deriveCat);
            this.srdl.DefinedCategory.Add(this.reqCategory);
            this.srdl.DefinedCategory.Add(this.specCategory);

            this.reqCategory.SuperCategory.Add(this.functionalReq);

            this.specRuleType = new BinaryRelationshipRule(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "Spec Link", ShortName = "SpecLink" };
            this.specRuleType.SourceCategory = this.specCategory;
            this.specRuleType.TargetCategory = this.specCategory;
            this.specRuleType.RelationshipCategory = this.specRelationRuleCategory;

            this.derivedRule = new BinaryRelationshipRule(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "Derive", ShortName = "Derive" };
            this.derivedRule.SourceCategory = this.reqCategory;
            this.derivedRule.TargetCategory = this.reqCategory;
            this.derivedRule.RelationshipCategory = this.deriveCat;

            this.parameRule = new ParameterizedCategoryRule(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "param Rule", ShortName = "ParamRule" };
            this.parameRule.ParameterType.Add(this.booleanParameterType);
            this.parameRule.Category = this.functionalReq;

            this.srdl.Rule.Add(this.specRuleType);
            this.srdl.Rule.Add(this.derivedRule);
            this.srdl.Rule.Add(this.parameRule);

            this.reqSpec.Category.Add(this.specCategory);
            this.reqSpec2.Category.Add(this.specCategory);
            this.deprecatedRequirementsSpecification.Category.Add(this.specCategory);

            this.req.Category.Add(this.reqCategory);
            this.req1.Category.Add(this.reqCategory);
            this.req11.Category.Add(this.reqCategory);
            this.req2.Category.Add(this.reqCategory);
            this.deprecatedRequirement.Category.Add(this.reqCategory);

            this.deriveRelationship1 = new BinaryRelationship(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Source = this.req,
                Target = this.req2
            };

            this.deriveRelationship2 = new BinaryRelationship(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Source = this.req1,
                Target = this.req
            };

            this.deriveRelationship3 = new BinaryRelationship(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Source = this.req11,
                Target = this.req1
            };

            this.deriveRelationship4 = new BinaryRelationship(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Source = this.req,
                Target = this.deprecatedRequirement
            };

            this.deriveRelationship1.Category.Add(this.deriveCat);
            this.deriveRelationship2.Category.Add(this.deriveCat);
            this.deriveRelationship3.Category.Add(this.deriveCat);
            this.deriveRelationship4.Category.Add(this.deriveCat);

            this.specDeriveRelationship1 = new BinaryRelationship(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Source = this.reqSpec,
                Target = this.reqSpec2
            };

            this.specDeriveRelationship2 = new BinaryRelationship(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Source = this.reqSpec,
                Target = this.deprecatedRequirementsSpecification
            };

            this.specDeriveRelationship1.Category.Add(this.specRelationRuleCategory);
            this.specDeriveRelationship2.Category.Add(this.specRelationRuleCategory);

            this.iteration.Relationship.Add(this.deriveRelationship1);
            this.iteration.Relationship.Add(this.deriveRelationship2);
            this.iteration.Relationship.Add(this.deriveRelationship3);
            this.iteration.Relationship.Add(this.deriveRelationship4);
            this.iteration.Relationship.Add(this.specDeriveRelationship1);
            this.iteration.Relationship.Add(this.specDeriveRelationship2);

            this.reqValue = new SimpleParameterValue(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = this.booleanParameterType,
                Value = new ValueArray<string>(new[] { "true" })
            };

            this.reqValue1 = new SimpleParameterValue(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = this.booleanParameterType,
                Value = new ValueArray<string>(new[] { "true" })
            };

            this.reqValue11 = new SimpleParameterValue(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = this.booleanParameterType,
                Value = new ValueArray<string>(new[] { "true" })
            };

            this.reqValue2 = new SimpleParameterValue(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = this.booleanParameterType,
                Value = new ValueArray<string>(new[] { "true" })
            };

            this.deprecatedReqValue = new SimpleParameterValue(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = this.booleanParameterType,
                Value = new ValueArray<string>(new[] { "true" })
            };

            this.req.ParameterValue.Add(this.reqValue);
            this.req1.ParameterValue.Add(this.reqValue1);
            this.req11.ParameterValue.Add(this.reqValue11);
            this.req2.ParameterValue.Add(this.reqValue2);
            this.deprecatedRequirement.ParameterValue.Add(this.deprecatedReqValue);
        }

        [Test]
        public void VerifyThatRequirementSpecificationCanBeExportedIntoReqIF_DefaultExcludingDeprecated()
        {
            var builder = new ReqIFBuilder();

            var reqif = builder.BuildReqIF(this.session.Object, this.iteration);

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(reqif);

                // 2 + 1 extra datatype for requriement text
                Assert.AreEqual(3, reqif.CoreContent.DataTypes.Count); // booleanPt and boolean and Text datatype (OMG default, no XHTML)
                Assert.AreEqual(6, reqif.CoreContent.SpecObjects.Count); // 4 requirements + 2 groups
                Assert.AreEqual(2, reqif.CoreContent.Specifications.Count); // 2 specification
                Assert.AreEqual(5, reqif.CoreContent.SpecTypes.Count); // 1 group type, 1 Req type, 1 Spec type, 1 Relation type, 1 relationGroup type
                Assert.AreEqual(3, reqif.CoreContent.SpecRelations.Count); // 3 specRelation from 3 relationship
                Assert.AreEqual(1, reqif.CoreContent.SpecRelationGroups.Count); // 1 RelationGroup from 1 binaryRelationship

                Assert.IsNotEmpty(reqif.CoreContent.SpecRelationGroups.Single().SpecRelations);
            });

            var serializer = new ReqIFSerializer(false);
            serializer.Serialize(reqif, @"output.xml", (o, e) => { throw new Exception(); });
        }

        [Test]
        public void VerifyThatRequirementsWithDifferentParametersShareOneCollapsedType()
        {
            // a second requirement whose parameter differs from the boolean one used by all the others
            var textParameterType = new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "text", ShortName = "txt" };
            this.srdl.ParameterType.Add(textParameterType);

            var requirementWithText = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "ReqText", ShortName = "RT" };
            requirementWithText.Category.Add(this.reqCategory);
            requirementWithText.ParameterValue.Add(new SimpleParameterValue(Guid.NewGuid(), this.assembler.Cache, this.uri) { ParameterType = textParameterType, Value = new ValueArray<string>(new[] { "hello" }) });
            this.reqSpec.Requirement.Add(requirementWithText);

            var builder = new ReqIFBuilder();

            var reqif = builder.BuildReqIF(this.session.Object, this.iteration, true);

            // all requirements share the same rule-set, so there is exactly ONE requirement SpecObjectType
            var requirementTypes = reqif.CoreContent.SpecTypes.OfType<SpecObjectType>()
                .Where(x => !x.LongName.StartsWith(ThingToReqIfMapper.GroupNamePrefix))
                .ToList();

            var requirementType = requirementTypes.FirstOrDefault();

            // the single collapsed type carries attribute definitions for BOTH parameter types (the union)
            var datatypeIdentifiers = requirementType?.SpecAttributes.Select(x => x.DatatypeDefinition.Identifier).ToArray() ?? Array.Empty<string>();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, requirementTypes.Count);
                Assert.AreEqual(this.parameRule.ShortName, requirementType?.LongName);
                Assert.Contains(this.booleanParameterType.Iid.ToString(), datatypeIdentifiers);
                Assert.Contains(textParameterType.Iid.ToString(), datatypeIdentifiers);
            });
        }

        [Test]
        public void VerifyThatOmgProfileExportsNativeNamesAndNoXhtml()
        {
            var builder = new ReqIFBuilder();

            var reqif = builder.BuildReqIF(this.session.Object, this.iteration, true, null, ReqIfExportProfile.Omg);

            var attributeNames = reqif.CoreContent.SpecTypes.SelectMany(st => st.SpecAttributes).Select(a => a.LongName).ToList();

            Assert.Multiple(() =>
            {
                Assert.IsFalse(reqif.CoreContent.DataTypes.OfType<DatatypeDefinitionXHTML>().Any(), "no XHTML datatype in the OMG profile");
                Assert.IsEmpty(reqif.CoreContent.SpecObjects.SelectMany(so => so.Values).OfType<AttributeValueXHTML>(), "no XHTML values in the OMG profile");
                Assert.Contains(ThingToReqIfMapper.ShortNameAttributeDefName, attributeNames);
                Assert.Contains(ThingToReqIfMapper.NameAttributeDefName, attributeNames);
                Assert.IsFalse(attributeNames.Contains(ThingToReqIfMapper.ReqIfNameAttributeDefName), "the OMG profile does not use the reserved names");
            });
        }

        [Test]
        public void VerifyThatDoorsCapellaProfileUsesReservedNamesAndXhtml()
        {
            var builder = new ReqIFBuilder();

            var reqif = builder.BuildReqIF(this.session.Object, this.iteration, true, null, ReqIfExportProfile.DoorsCapella);

            var attributeNames = reqif.CoreContent.SpecTypes.SelectMany(st => st.SpecAttributes).Select(a => a.LongName).ToList();

            Assert.Multiple(() =>
            {
                Assert.IsTrue(reqif.CoreContent.DataTypes.OfType<DatatypeDefinitionXHTML>().Any(), "the DOORS/Capella profile exports an XHTML datatype");
                Assert.Contains(ThingToReqIfMapper.ReqIfNameAttributeDefName, attributeNames);
                Assert.Contains(ThingToReqIfMapper.ReqIfForeignIdAttributeDefName, attributeNames);
                Assert.IsFalse(attributeNames.Contains(ThingToReqIfMapper.NameAttributeDefName), "the DOORS/Capella profile uses the reserved names instead of the native ones");
            });
        }

        [Test]
        public void VerifyThatRequirementTextIsExportedAsXhtmlAndRoundtrips()
        {
            var builder = new ReqIFBuilder();

            var reqif = builder.BuildReqIF(this.session.Object, this.iteration, true, null, ReqIfExportProfile.DoorsCapella);

            var xhtmlValues = reqif.CoreContent.SpecObjects
                .SelectMany(specObject => specObject.Values)
                .OfType<AttributeValueXHTML>()
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.IsTrue(reqif.CoreContent.DataTypes.OfType<DatatypeDefinitionXHTML>().Any(), "an XHTML datatype is exported");
                Assert.IsNotEmpty(xhtmlValues, "requirement text is also exported as XHTML");
                Assert.IsTrue(xhtmlValues.Any(v => v.TheValue.Contains("def1")), "the requirement definition content is wrapped in the XHTML value");
                Assert.IsTrue(xhtmlValues.All(v => v.TheValue.Contains("http://www.w3.org/1999/xhtml")), "the XHTML value declares the XHTML namespace so it is valid");

                // the plain-text string representation is kept so the COMET importer keeps round-tripping
                Assert.IsTrue(reqif.CoreContent.SpecObjects.SelectMany(so => so.Values).OfType<AttributeValueString>().Any(v => v.TheValue == "def1"));
            });

            // the produced document is valid ReqIF: it serializes and re-deserializes without error
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "xhtml_roundtrip.reqif");
            new ReqIFSerializer(false).Serialize(reqif, path, (o, e) => throw new Exception());

            var reloaded = new ReqIFDeserializer().Deserialize(path, false, null).Single();
            var reloadedXhtml = reloaded.CoreContent.SpecObjects.SelectMany(so => so.Values).OfType<AttributeValueXHTML>().ToList();

            Assert.Multiple(() =>
            {
                Assert.IsNotEmpty(reloadedXhtml);
                Assert.IsTrue(reloadedXhtml.Any(v => v.TheValue.Contains("def1")));
            });
        }

        [Test]
        public void VerifyThatOnlySelectedRequirementsSpecificationsAreExported()
        {
            var builder = new ReqIFBuilder();

            var reqif = builder.BuildReqIF(this.session.Object, this.iteration, false, new[] { this.reqSpec });

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(reqif);
                Assert.AreEqual(1, reqif.CoreContent.Specifications.Count); // only the single selected specification
                Assert.AreEqual(this.reqSpec.Iid.ToString(), reqif.CoreContent.Specifications.Single().Identifier);
            });
        }

        [Test]
        public void VerifyThatRequirementSpecificationCanBeExportedIntoReqIF_IncludingDeprecated()
        {
            var builder = new ReqIFBuilder();

            var reqif = builder.BuildReqIF(this.session.Object, this.iteration, true);

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(reqif);

                // 2 + 1 extra datatype for requriement text
                Assert.AreEqual(3, reqif.CoreContent.DataTypes.Count); // booleanPt and boolean and Text datatype (OMG default, no XHTML)
                Assert.AreEqual(7, reqif.CoreContent.SpecObjects.Count); // 5 requirements + 2 groups
                Assert.AreEqual(3, reqif.CoreContent.Specifications.Count); // 3 specification
                Assert.AreEqual(5, reqif.CoreContent.SpecTypes.Count); // 1 group type, 1 Req type, 1 Spec type, 1 Relation type, 1 relationGroup type
                Assert.AreEqual(4, reqif.CoreContent.SpecRelations.Count); // 4 specRelation from 3 relationship
                Assert.AreEqual(2, reqif.CoreContent.SpecRelationGroups.Count); // 2 RelationGroup from 1 binaryRelationship

                Assert.IsNotEmpty(reqif.CoreContent.SpecRelationGroups.First().SpecRelations);
                Assert.IsNotEmpty(reqif.CoreContent.SpecRelationGroups.Skip(1).First().SpecRelations);
            });

            var serializer = new ReqIFSerializer(false);
            serializer.Serialize(reqif, @"output.xml", (o, e) => { throw new Exception(); });
        }

        [Test]
        public void VerifyThatRequirementSpecificationCanBeExportedIntoReqIF_HavingDeprecatedRequirementsSpecification()
        {
            var builder = new ReqIFBuilder();

            this.deprecatedRequirementsSpecification.IsDeprecated = false;

            var reqif = builder.BuildReqIF(this.session.Object, this.iteration, true);

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(reqif);

                // 2 + 1 extra datatype for requriement text
                Assert.AreEqual(3, reqif.CoreContent.DataTypes.Count); // booleanPt and boolean and Text datatype (OMG default, no XHTML)
                Assert.AreEqual(7, reqif.CoreContent.SpecObjects.Count); // 4 requirements + 2 groups
                Assert.AreEqual(3, reqif.CoreContent.Specifications.Count); // 2 specification
                Assert.AreEqual(5, reqif.CoreContent.SpecTypes.Count); // 1 group type, 1 Req type, 1 Spec type, 1 Relation type, 1 relationGroup type
                Assert.AreEqual(4, reqif.CoreContent.SpecRelations.Count); // 3 specRelation from 3 relationship
                Assert.AreEqual(2, reqif.CoreContent.SpecRelationGroups.Count); // 1 RelationGroup from 1 binaryRelationship

                Assert.IsNotEmpty(reqif.CoreContent.SpecRelationGroups.First().SpecRelations);
                Assert.IsNotEmpty(reqif.CoreContent.SpecRelationGroups.Skip(1).First().SpecRelations);
            });

            var serializer = new ReqIFSerializer(false);
            serializer.Serialize(reqif, @"output.xml", (o, e) => { throw new Exception(); });
        }

        [Test]
        public void VerifyThatRequirementSpecificationCanBeExportedIntoReqIF_HavingDeprecatedRequirement()
        {
            var builder = new ReqIFBuilder();

            this.deprecatedRequirement.IsDeprecated = false;

            var reqif = builder.BuildReqIF(this.session.Object, this.iteration, true);

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(reqif);

                // 2 + 1 extra datatype for requriement text
                Assert.AreEqual(3, reqif.CoreContent.DataTypes.Count); // booleanPt and boolean and Text datatype (OMG default, no XHTML)
                Assert.AreEqual(7, reqif.CoreContent.SpecObjects.Count); // 4 requirements + 2 groups
                Assert.AreEqual(3, reqif.CoreContent.Specifications.Count); // 2 specification
                Assert.AreEqual(5, reqif.CoreContent.SpecTypes.Count); // 1 group type, 1 Req type, 2 Spec type, 1 Relation type, 1 relationGroup type
                Assert.AreEqual(4, reqif.CoreContent.SpecRelations.Count); // 3 specRelation from 3 relationship
                Assert.AreEqual(2, reqif.CoreContent.SpecRelationGroups.Count); // 1 RelationGroup from 1 binaryRelationship

                Assert.IsNotEmpty(reqif.CoreContent.SpecRelationGroups.First().SpecRelations);
                Assert.IsNotEmpty(reqif.CoreContent.SpecRelationGroups.Skip(1).First().SpecRelations);
            });

            var serializer = new ReqIFSerializer(false);
            serializer.Serialize(reqif, @"output.xml", (o, e) => { throw new Exception(); });
        }

        [Test]
        public void VerifyThatRequirementSpecificationCanBeExportedIntoReqIF_Default_HavingDeprecatedRequirementsSpecification()
        {
            var builder = new ReqIFBuilder();

            this.deprecatedRequirementsSpecification.IsDeprecated = false;

            var reqif = builder.BuildReqIF(this.session.Object, this.iteration);

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(reqif);

                // 2 + 1 extra datatype for requriement text
                Assert.AreEqual(3, reqif.CoreContent.DataTypes.Count); // booleanPt and boolean and Text datatype (OMG default, no XHTML)
                Assert.AreEqual(6, reqif.CoreContent.SpecObjects.Count); // 4 requirements + 2 groups
                Assert.AreEqual(3, reqif.CoreContent.Specifications.Count); // 2 specification
                Assert.AreEqual(5, reqif.CoreContent.SpecTypes.Count); // 1 group type, 1 Req type, 1 Spec type, 1 Relation type, 1 relationGroup type
                Assert.AreEqual(3, reqif.CoreContent.SpecRelations.Count); // 3 specRelation from 3 relationship
                Assert.AreEqual(2, reqif.CoreContent.SpecRelationGroups.Count); // 1 RelationGroup from 1 binaryRelationship

                Assert.IsNotEmpty(reqif.CoreContent.SpecRelationGroups.First().SpecRelations);
                Assert.IsEmpty(reqif.CoreContent.SpecRelationGroups.Skip(1).First().SpecRelations);
            });

            var serializer = new ReqIFSerializer(false);
            serializer.Serialize(reqif, @"output.xml", (o, e) => { throw new Exception(); });
        }

        [Test]
        public void VerifyThatRequirementSpecificationCanBeExportedIntoReqIF_Default_HavingDeprecatedRequirement()
        {
            var builder = new ReqIFBuilder();

            this.deprecatedRequirement.IsDeprecated = false;

            var reqif = builder.BuildReqIF(this.session.Object, this.iteration);

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(reqif);

                // 2 + 1 extra datatype for requriement text
                Assert.AreEqual(3, reqif.CoreContent.DataTypes.Count); // booleanPt and boolean and Text datatype (OMG default, no XHTML)
                Assert.AreEqual(6, reqif.CoreContent.SpecObjects.Count); // 4 requirements + 2 groups
                Assert.AreEqual(2, reqif.CoreContent.Specifications.Count); // 2 specification
                Assert.AreEqual(5, reqif.CoreContent.SpecTypes.Count); // 1 group type, 1 Req type, 2 Spec type, 1 Relation type, 1 relationGroup type
                Assert.AreEqual(3, reqif.CoreContent.SpecRelations.Count); // 3 specRelation from 3 relationship
                Assert.AreEqual(1, reqif.CoreContent.SpecRelationGroups.Count); // 1 RelationGroup from 1 binaryRelationship

                Assert.IsNotEmpty(reqif.CoreContent.SpecRelationGroups.Single().SpecRelations);
            });

            var serializer = new ReqIFSerializer(false);
            serializer.Serialize(reqif, @"output.xml", (o, e) => { throw new Exception(); });
        }

        [Test]
        public void VerifyThatRequirementsWithIdenticalTypeShareASingleSpecObjectType()
        {
            // All exported requirements share the same category/rule and parameter type, so they must
            // collapse onto a single requirement SpecObjectType instead of one per requirement (GH #1487)
            var builder = new ReqIFBuilder();

            var reqif = builder.BuildReqIF(this.session.Object, this.iteration);

            var requirementSpecObjectTypes = reqif.CoreContent.SpecTypes
                .OfType<SpecObjectType>()
                .Where(x => !x.LongName.StartsWith(ThingToReqIfMapper.GroupNamePrefix))
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, requirementSpecObjectTypes.Count);

                // the attribute definition created for the requirement's parameter value carries the parameter type name
                Assert.IsTrue(requirementSpecObjectTypes.Any(t => t.SpecAttributes.Any(x => x.LongName == this.booleanParameterType.ShortName)));
            });
        }

        [Test]
        public void VerifyThatWhenSessionIsNulArgumentNullExceptionIsThrown()
        {
            var builder = new ReqIFBuilder();
            Assert.Throws<ArgumentNullException>(() => builder.BuildReqIF(null, this.iteration));
        }

        [Test]
        public void VerifyThatWhenIterationIsNulArgumentNullExceptionIsThrown()
        {
            var builder = new ReqIFBuilder();
            Assert.Throws<ArgumentNullException>(() => builder.BuildReqIF(this.session.Object, null));
        }
    }
}
