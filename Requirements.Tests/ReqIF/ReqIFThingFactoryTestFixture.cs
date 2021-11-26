// -------------------------------------------------------------------------------------------------
// <copyright file="ReqIFThingFactoryTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski
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
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.ReqIF
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using CDP4Requirements.ReqIFDal;
    using CDP4Requirements.ViewModels;

    using Moq;

    using NUnit.Framework;

    using ReqIFSharp;

    [TestFixture]
    internal class ReqIFThingFactoryTestFixture
    {
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;

        private Assembler assembler;
        private Uri uri = new Uri("http://test.com");

        #region Things

        private SiteDirectory sitedir;
        private EngineeringModelSetup modelsetup;
        private IterationSetup iterationSetup;
        private DomainOfExpertise domain;
        private EngineeringModel model;
        private Iteration iteration;
        private Person person;
        private Participant participant;

        private SiteReferenceDataLibrary srdl;
        private ModelReferenceDataLibrary mrdl;

        private ParameterType pt;

        private Category specCategory;
        private Category reqCateory;
        private Category specRelationCategory;
        private Category relationGroupCategory;
        private Category specLinkCategory;
        private Category reqLinkCategory;

        private BinaryRelationshipRule specRule;
        private BinaryRelationshipRule reqRule;

        private ParameterizedCategoryRule parameterRule;

        #endregion

        #region ReqIF data

        private ReqIF reqIf;
        private ReqIFContent corecontent;
        private DatatypeDefinitionString stringDatadef;
        private SpecificationType specificationtype;
        private SpecObjectType specobjecttype;
        private SpecRelationType specrelationtype;
        private RelationGroupType relationgrouptype;

        private Specification specification1;
        private Specification specification2;

        private SpecObject specobject1;
        private SpecObject specobject2;
        private SpecRelation specrelation;
        private RelationGroup relationgroup;

        private AttributeDefinitionString specAttribute;

        private AttributeDefinitionString reqAttribute;

        private AttributeDefinitionString specRelationAttribute;
        private AttributeDefinitionString relationgroupAttribute;

        private AttributeValueString specValue1;
        private AttributeValueString specValue2;

        private AttributeValueString objectValue1;
        private AttributeValueString objectValue2;

        private AttributeValueString specrelationValue;
        private AttributeValueString relationgroupValue;

        #endregion

        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.permissionService = new Mock<IPermissionService>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.assembler = new Assembler(this.uri);

            this.SetupThings();
            this.SetupReqIf();

            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>> { { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant) } });
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);

            this.assembler.Cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
            this.assembler.Cache.TryAdd(new CacheKey(this.sitedir.Iid, null), new Lazy<Thing>(() => this.sitedir));
        }

        [Test]
        public void VerifyThatThingFactoryWorks()
        {
            var datatypeMap = new Dictionary<DatatypeDefinition, DatatypeDefinitionMap>();
            var datatypedef1 = new DatatypeDefinitionMap(this.stringDatadef, this.pt, null);
            datatypeMap.Add(this.stringDatadef, datatypedef1);

            var spectypeMap = new Dictionary<SpecType, SpecTypeMap>();
            var specificationMap = new SpecTypeMap(this.specificationtype, null, new[] { this.specCategory }, new[] { new AttributeDefinitionMap(this.specAttribute, AttributeDefinitionMapKind.NAME) });
            var requirementMap = new SpecObjectTypeMap(this.specobjecttype, null, new[] { this.reqCateory }, new[] { new AttributeDefinitionMap(this.reqAttribute, AttributeDefinitionMapKind.FIRST_DEFINITION) }, true);
            var specRelationMap = new SpecRelationTypeMap(this.specrelationtype, new[] { this.parameterRule }, new[] { this.specRelationCategory }, new[] { new AttributeDefinitionMap(this.specRelationAttribute, AttributeDefinitionMapKind.PARAMETER_VALUE) }, new[] { this.reqRule });
            var relationGroupMap = new RelationGroupTypeMap(this.relationgrouptype, null, new[] { this.relationGroupCategory }, new[] { new AttributeDefinitionMap(this.relationgroupAttribute, AttributeDefinitionMapKind.NONE) }, new[] { this.specRule });
            spectypeMap.Add(this.specificationtype, specificationMap);
            spectypeMap.Add(this.specobjecttype, requirementMap);
            spectypeMap.Add(this.specrelationtype, specRelationMap);
            spectypeMap.Add(this.relationgrouptype, relationGroupMap);

            var factory = new ThingFactory(this.iteration, datatypeMap, spectypeMap, this.domain, this.reqIf.Lang);
            factory.ComputeRequirementThings(this.reqIf);

            Assert.AreEqual(1, factory.RelationGroupMap.Count);
            Assert.AreEqual(1, factory.SpecRelationMap.Count);
            Assert.AreEqual(2, factory.SpecificationMap.Count);
            Assert.IsTrue(factory.SpecificationMap.All(x => x.Value.Requirement.Count == 1));

            var reqSpec1 = factory.SpecificationMap[this.specification1];
            var reqSpec2 = factory.SpecificationMap[this.specification2];
            var req1 = reqSpec1.Requirement.Single();
            var req2 = reqSpec2.Requirement.Single();

            var specificationRelationship = factory.RelationGroupMap.Single().Value;
            var reqRelatinoship = factory.SpecRelationMap.Single().Value;

            Assert.AreSame(specificationRelationship.Source, reqSpec1);
            Assert.AreSame(specificationRelationship.Target, reqSpec2);
            Assert.AreSame(reqRelatinoship.Source, req1);
            Assert.AreSame(reqRelatinoship.Target, req2);

            Assert.IsNotEmpty(req1.Definition);
            Assert.IsNotEmpty(req2.Definition);

            Assert.AreEqual(reqSpec1.Name, this.specValue1.TheValue);
            Assert.AreEqual(reqSpec2.Name, this.specValue2.TheValue);

            var parameterValue = reqRelatinoship.ParameterValue.Single();
            Assert.AreEqual(parameterValue.Value[0], this.specrelationValue.TheValue);

            //todo to complete
        }

        private void SetupThings()
        {
            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) { RequiredRdl = this.srdl };
            this.sitedir.SiteReferenceDataLibrary.Add(this.srdl);
            this.modelsetup.RequiredRdl.Add(this.mrdl);

            this.model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri) { EngineeringModelSetup = this.modelsetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri) { IterationSetup = this.iterationSetup };

            this.sitedir.Model.Add(this.modelsetup);
            this.modelsetup.IterationSetup.Add(this.iterationSetup);
            this.sitedir.Domain.Add(this.domain);
            this.model.Iteration.Add(this.iteration);

            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.participant = new Participant(Guid.NewGuid(), this.assembler.Cache, this.uri) { Person = this.person };
            this.sitedir.Person.Add(this.person);
            this.modelsetup.Participant.Add(this.participant);

            this.pt = new BooleanParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.srdl.ParameterType.Add(this.pt);

            this.specCategory = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.specCategory.PermissibleClass.Add(ClassKind.RequirementsSpecification);

            this.reqCateory = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.reqCateory.PermissibleClass.Add(ClassKind.Requirement);

            this.specRelationCategory = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.specRelationCategory.PermissibleClass.Add(ClassKind.BinaryRelationship);

            this.relationGroupCategory = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.relationGroupCategory.PermissibleClass.Add(ClassKind.BinaryRelationship);

            this.specLinkCategory = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.specLinkCategory.PermissibleClass.Add(ClassKind.BinaryRelationship);

            this.reqLinkCategory = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.reqLinkCategory.PermissibleClass.Add(ClassKind.BinaryRelationship);

            this.srdl.DefinedCategory.Add(this.specCategory);
            this.srdl.DefinedCategory.Add(this.reqCateory);
            this.srdl.DefinedCategory.Add(this.specRelationCategory);
            this.srdl.DefinedCategory.Add(this.relationGroupCategory);

            this.specRule = new BinaryRelationshipRule(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.specRule.SourceCategory = this.specCategory;
            this.specRule.TargetCategory = this.specCategory;
            this.specRule.RelationshipCategory = this.specLinkCategory;

            this.reqRule = new BinaryRelationshipRule(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.srdl.Rule.Add(this.specRule);
            this.srdl.Rule.Add(this.reqRule);
            this.reqRule.RelationshipCategory = this.reqLinkCategory;

            this.parameterRule = new ParameterizedCategoryRule(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.parameterRule.ParameterType.Add(this.pt);
            this.parameterRule.Category = this.specRelationCategory;

            this.srdl.Rule.Add(this.parameterRule);
        }

        private void SetupReqIf()
        {
            this.reqIf = new ReqIF();
            this.reqIf.Lang = "en";
            this.corecontent = new ReqIFContent();
            this.reqIf.CoreContent = this.corecontent;
            this.stringDatadef = new DatatypeDefinitionString();
            this.specificationtype = new SpecificationType();
            this.specobjecttype = new SpecObjectType();
            this.specrelationtype = new SpecRelationType();
            this.relationgrouptype = new RelationGroupType();

            this.specAttribute = new AttributeDefinitionString() { DatatypeDefinition = this.stringDatadef };

            this.reqAttribute = new AttributeDefinitionString() { DatatypeDefinition = this.stringDatadef };
            this.specRelationAttribute = new AttributeDefinitionString() { DatatypeDefinition = this.stringDatadef };
            this.relationgroupAttribute = new AttributeDefinitionString() { DatatypeDefinition = this.stringDatadef };

            this.specificationtype.SpecAttributes.Add(this.specAttribute);
            this.specobjecttype.SpecAttributes.Add(this.reqAttribute);
            this.specrelationtype.SpecAttributes.Add(this.specRelationAttribute);
            this.relationgrouptype.SpecAttributes.Add(this.relationgroupAttribute);

            this.specification1 = new Specification() { Type = this.specificationtype };
            this.specification2 = new Specification() { Type = this.specificationtype };

            this.specobject1 = new SpecObject() { Type = this.specobjecttype };
            this.specobject2 = new SpecObject() { Type = this.specobjecttype };

            this.specrelation = new SpecRelation() { Type = this.specrelationtype, Source = this.specobject1, Target = this.specobject2 };
            this.relationgroup = new RelationGroup() { Type = this.relationgrouptype, SourceSpecification = this.specification1, TargetSpecification = this.specification2 };

            this.specValue1 = new AttributeValueString() { AttributeDefinition = this.specAttribute, TheValue = "spec1" };
            this.specValue2 = new AttributeValueString() { AttributeDefinition = this.specAttribute, TheValue = "spec2" };
            this.objectValue1 = new AttributeValueString() { AttributeDefinition = this.reqAttribute, TheValue = "req1" };
            this.objectValue2 = new AttributeValueString() { AttributeDefinition = this.reqAttribute, TheValue = "req2" };
            this.relationgroupValue = new AttributeValueString() { AttributeDefinition = this.relationgroupAttribute, TheValue = "group" };
            this.specrelationValue = new AttributeValueString() { AttributeDefinition = this.specRelationAttribute, TheValue = "specrelation" };

            this.specification1.Values.Add(this.specValue1);
            this.specification2.Values.Add(this.specValue2);
            this.specobject1.Values.Add(this.objectValue1);
            this.specobject2.Values.Add(this.objectValue2);
            this.specrelation.Values.Add(this.specrelationValue);
            this.relationgroup.Values.Add(this.relationgroupValue);

            this.corecontent.DataTypes.Add(this.stringDatadef);
            this.corecontent.SpecTypes.AddRange(new SpecType[] { this.specobjecttype, this.specificationtype, this.specrelationtype, this.relationgrouptype });
            this.corecontent.SpecObjects.AddRange(new SpecObject[] { this.specobject1, this.specobject2 });
            this.corecontent.Specifications.AddRange(new Specification[] { this.specification1, this.specification2 });
            this.corecontent.SpecRelations.Add(this.specrelation);
            this.corecontent.SpecRelationGroups.Add(this.relationgroup);

            this.specification1.Children.Add(new SpecHierarchy() { Object = this.specobject1 });
            this.specification2.Children.Add(new SpecHierarchy() { Object = this.specobject2 });
        }
    }
}
