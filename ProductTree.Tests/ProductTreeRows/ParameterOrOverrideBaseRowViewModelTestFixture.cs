// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterOrOverrideBaseRowViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
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
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ProductTree.Tests.ProductTreeRows
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO.Packaging;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Services;
    using CDP4Composition.Services.NestedElementTreeService;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4ProductTree.ViewModels;

    using Microsoft.Practices.ServiceLocation;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ParameterOrOverrideBaseRowViewModelTestFixture
    {
        private Mock<INestedElementTreeService> nestedElementTreeService;
        private Mock<ISession> session;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private readonly Uri uri = new Uri("http://www.rheagroup.com");
        private Parameter parameter1;
        private ParameterType parameterType1;
        private ActualFiniteStateList stateList;
        private ActualFiniteState state1;
        private ParameterValueSet valueset;
        private ElementDefinition elementdef1;
        private ElementUsage elementUsage1;
        private Category category1;
        private Category category2;
        private Iteration iteration;
        private IterationSetup iterationSetup;
        private EngineeringModel model;
        private EngineeringModelSetup modelSetup;
        private SiteDirectory siteDirectory;
        private Participant participant;
        private Person person;
        private Option option;
        private DomainOfExpertise domain;
        private ProductTreeIconUriConverter converter;
        private RelationalExpression relationalExpression;
        private Mock<IThingCreator> thingCreator;
        private Mock<IServiceLocator> serviceLocator;
        private readonly string nestedParameterPath = "PATH";

        [SetUp]
        public void Setup()
        {
            var ensurePackSchemeIsKnown = PackUriHelper.UriSchemePack;
            this.session = new Mock<ISession>();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "domain", ShortName = "dom" };
            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri);
            this.modelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri);
            this.person = new Person(Guid.NewGuid(), this.cache, this.uri) { GivenName = "test", Surname = "test" };
            this.participant = new Participant(Guid.NewGuid(), this.cache, this.uri) { Person = this.person };
            this.option = new Option(Guid.NewGuid(), this.cache, this.uri);

            this.elementdef1 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "Def1",
                Owner = this.domain
            };

            this.elementUsage1 = new ElementUsage(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "Usage1",
                Container = this.elementdef1,
                ElementDefinition = this.elementdef1,
                Owner = this.domain
            };

            this.category1 = new Category(Guid.NewGuid(), this.cache, this.uri);
            this.category2 = new Category(Guid.NewGuid(), this.cache, this.uri);

            this.siteDirectory.Model.Add(this.modelSetup);
            this.modelSetup.IterationSetup.Add(this.iterationSetup);
            this.modelSetup.Participant.Add(this.participant);
            this.siteDirectory.Person.Add(this.person);

            this.model.Iteration.Add(this.iteration);
            this.model.EngineeringModelSetup = this.modelSetup;
            this.iteration.IterationSetup = this.iterationSetup;
            this.iteration.Option.Add(this.option);
            this.iteration.Element.Add(this.elementdef1);

            this.parameterType1 = new EnumerationParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "pt1", Category = new List<Category>() { this.category1 } };
            this.parameter1 = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = this.parameterType1, Owner = this.domain };
            this.stateList = new ActualFiniteStateList(Guid.NewGuid(), this.cache, this.uri) { Owner = this.domain };
            this.state1 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            this.valueset = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri);
            this.stateList.ActualState.Add(this.state1);
            this.elementdef1.Parameter.Add(this.parameter1);

            this.elementdef1.Category.Add(this.category1);
            this.elementUsage1.Category.Add(this.category2);

            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());

            this.cache.TryAdd(new CacheKey(this.parameter1.Iid, null), new Lazy<Thing>(() => this.parameter1));
            this.converter = new ProductTreeIconUriConverter();

            this.relationalExpression = new RelationalExpression();
            this.serviceLocator = new Mock<IServiceLocator>();
            this.thingCreator = new Mock<IThingCreator>();

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.nestedElementTreeService = new Mock<INestedElementTreeService>();
            this.nestedElementTreeService.Setup(x => x.GetNestedParameterPath(It.IsAny<ParameterBase>(), It.IsAny<Option>())).Returns(this.nestedParameterPath);

            this.serviceLocator.Setup(x => x.GetInstance<INestedElementTreeService>()).Returns(this.nestedElementTreeService.Object);

            this.serviceLocator.Setup(x => x.GetInstance<IThingCreator>())
                .Returns(this.thingCreator.Object);

            this.thingCreator.Setup(x => x.IsCreateBinaryRelationshipForRequirementVerificationAllowed(It.IsAny<ParameterOrOverrideBase>(), It.IsAny<RelationalExpression>()))
                .Returns(true);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatGetPathWorks()
        {
            // **************************INPUT***************************************
            var published = new ValueArray<string>(new List<string> { "manual1", "manual2" }, this.valueset);
            var actual = new ValueArray<string>(new List<string> { "manual1", "manual2" }, this.valueset);

            this.valueset.Published = published;
            this.valueset.Manual = actual;
            this.valueset.ValueSwitch = ParameterSwitchKind.MANUAL;
            this.valueset.ActualOption = this.option;

            var compoundtype = new CompoundParameterType(Guid.NewGuid(), null, this.uri);

            var component1 = new ParameterTypeComponent(Guid.NewGuid(), null, this.uri)
            {
                ParameterType = this.parameterType1,
                ShortName = "c1"
            };

            var component2 = new ParameterTypeComponent(Guid.NewGuid(), null, this.uri)
            {
                ParameterType = this.parameterType1,
                ShortName = "c2"
            };

            compoundtype.Component.Add(component1);
            compoundtype.Component.Add(component2);

            var state2 = new ActualFiniteState(Guid.NewGuid(), null, this.uri);
            this.stateList.ActualState.Add(state2);

            this.parameter1.ParameterType = compoundtype;
            this.parameter1.ValueSet.Add(this.valueset);
            this.parameter1.IsOptionDependent = true;

            var valueset2 = new ParameterValueSet(Guid.NewGuid(), null, this.uri)
            {
                ActualState = state2,
                Published = published
            };

            this.parameter1.ValueSet.Add(valueset2);

            var row = new ParameterRowViewModel(this.parameter1, this.option, this.session.Object, null);
            Assert.AreEqual(this.nestedParameterPath, row.GetPath());

            var row2 = new ParameterTypeComponentRowViewModel(compoundtype.Component.First(), this.session.Object, null);
            Assert.AreEqual(string.Empty, row2.GetPath());

            var row3 = new ParameterTypeComponentRowViewModel(compoundtype.Component.First(), this.session.Object, row);
            Assert.AreEqual(this.nestedParameterPath, row3.GetPath());
        }

        [Test]
        public void VerifyThatPropertiesArePopulatedForScalarPropertiesNoStateNoOption()
        {
            // **********************************************************************
            var published = new ValueArray<string>(new List<string> { "manual" }, this.valueset);
            var actual = new ValueArray<string>(new List<string> { "different" }, this.valueset);

            this.valueset.Published = published;
            this.valueset.Manual = actual;
            this.valueset.ValueSwitch = ParameterSwitchKind.MANUAL;

            this.parameter1.ValueSet.Add(this.valueset);

            // **********************************************************************

            var row = new ParameterRowViewModel(this.parameter1, this.option, this.session.Object, null);

            Assert.IsTrue(row.Value.Contains("manual"));
            Assert.IsTrue(row.IsPublishable);
            Assert.IsNull(row.StateDependence);
            Assert.IsNull(row.MeasurementScale);
            Assert.AreEqual(0, row.ContainedRows.Count);
            Assert.AreEqual("domain", row.OwnerName);
            Assert.AreEqual("dom", row.OwnerShortName);
        }

        [Test]
        public void VerifyThatPropertiesArePopulatedForScalarPropertiesWithOption()
        {
            // **********************************************************************
            var published = new ValueArray<string>(new List<string> { "manual" }, this.valueset);
            var actual = new ValueArray<string>(new List<string> { "manual" }, this.valueset);

            this.valueset.Published = published;
            this.valueset.Manual = actual;
            this.valueset.ValueSwitch = ParameterSwitchKind.MANUAL;

            this.valueset.ActualOption = this.option;

            this.parameter1.ValueSet.Add(this.valueset);
            this.parameter1.IsOptionDependent = true;

            var valueset2 = new ParameterValueSet(Guid.NewGuid(), null, this.uri);
            this.parameter1.ValueSet.Add(valueset2);

            // **********************************************************************

            var row = new ParameterRowViewModel(this.parameter1, this.option, this.session.Object, null);

            Assert.IsFalse(row.IsPublishable);
            Assert.IsTrue(row.Value.Contains("manual"));
            Assert.IsNull(row.StateDependence);
            Assert.IsNull(row.MeasurementScale);
            Assert.AreEqual(0, row.ContainedRows.Count);
            Assert.AreEqual("domain", row.OwnerName);
            Assert.AreEqual("dom", row.OwnerShortName);
        }

        [Test]
        public void VerifyThatPropertiesArePopulatedForScalarStateDependent()
        {
            // **************************INPUT***************************************
            var published = new ValueArray<string>(new List<string> { "manual" }, this.valueset);
            var actual = new ValueArray<string>(new List<string> { "different" }, this.valueset);

            this.valueset.Published = published;
            this.valueset.Manual = actual;
            this.valueset.ValueSwitch = ParameterSwitchKind.MANUAL;
            this.valueset.ActualOption = this.option;
            this.valueset.ActualState = this.state1;

            this.parameter1.ValueSet.Add(this.valueset);
            this.parameter1.IsOptionDependent = true;
            this.parameter1.StateDependence = this.stateList;

            var valueset2 = new ParameterValueSet(Guid.NewGuid(), null, this.uri);
            this.parameter1.ValueSet.Add(valueset2);

            // **********************************************************************

            var row = new ParameterRowViewModel(this.parameter1, this.option, this.session.Object, null);

            Assert.IsTrue(row.IsPublishable);
            Assert.That(row.Value, Is.Null.Or.Empty);
            Assert.IsNotNull(row.StateDependence);
            Assert.IsNull(row.MeasurementScale);
            Assert.AreEqual(1, row.ContainedRows.Count);

            var staterow = row.ContainedRows.OfType<ParameterStateRowViewModel>().Single();
            Assert.IsTrue(staterow.Value.Contains("manual"));
            Assert.IsTrue(staterow.IsPublishable);

            // update publish value and check that isPublishable is false
            this.valueset.Manual = published;
            this.valueset.RevisionNumber = 10;
            CDPMessageBus.Current.SendObjectChangeEvent(this.valueset, EventKind.Updated);
            Assert.IsFalse(row.IsPublishable);

            Assert.AreEqual("domain", row.OwnerName);
            Assert.AreEqual("dom", row.OwnerShortName);
        }

        [Test]
        public void VerifyThatOnUpdateOfStateTheRowIsUpdated()
        {
            // **************************INPUT***************************************
            var published = new ValueArray<string>(new List<string> { "manual" }, this.valueset);
            var actual = new ValueArray<string>(new List<string> { "different" }, this.valueset);

            this.valueset.Published = published;
            this.valueset.Manual = actual;
            this.valueset.ValueSwitch = ParameterSwitchKind.MANUAL;
            this.valueset.ActualOption = this.option;
            this.valueset.ActualState = this.state1;

            this.parameter1.ValueSet.Add(this.valueset);
            this.parameter1.IsOptionDependent = true;
            this.parameter1.StateDependence = this.stateList;

            var valueset2 = new ParameterValueSet(Guid.NewGuid(), null, this.uri);
            this.parameter1.ValueSet.Add(valueset2);

            // **********************************************************************

            var row = new ParameterRowViewModel(this.parameter1, this.option, this.session.Object, null);
            Assert.AreEqual(1, row.ContainedRows.Count);
            this.state1.Kind = ActualFiniteStateKind.FORBIDDEN;
            CDPMessageBus.Current.SendObjectChangeEvent(this.state1, EventKind.Updated);
            Assert.AreEqual(0, row.ContainedRows.Count);
        }

        [Test]
        public void VerifyPropertiesForCompoundNoState()
        {
            // **************************INPUT***************************************
            var published = new ValueArray<string>(new List<string> { "manual1", "manual2" }, this.valueset);
            var actual = new ValueArray<string>(new List<string> { "manual1", "manual2" }, this.valueset);

            this.valueset.Published = published;
            this.valueset.Manual = actual;
            this.valueset.ValueSwitch = ParameterSwitchKind.MANUAL;
            this.valueset.ActualOption = this.option;

            var compoundtype = new CompoundParameterType(Guid.NewGuid(), null, this.uri);

            var component1 = new ParameterTypeComponent(Guid.NewGuid(), null, this.uri)
            {
                ParameterType = this.parameterType1,
                ShortName = "c1"
            };

            var component2 = new ParameterTypeComponent(Guid.NewGuid(), null, this.uri)
            {
                ParameterType = this.parameterType1,
                ShortName = "c2"
            };

            compoundtype.Component.Add(component1);
            compoundtype.Component.Add(component2);

            var state2 = new ActualFiniteState(Guid.NewGuid(), null, this.uri);
            this.stateList.ActualState.Add(state2);

            this.parameter1.ParameterType = compoundtype;
            this.parameter1.ValueSet.Add(this.valueset);
            this.parameter1.IsOptionDependent = true;

            var valueset2 = new ParameterValueSet(Guid.NewGuid(), null, this.uri)
            {
                ActualState = state2,
                Published = published
            };

            this.parameter1.ValueSet.Add(valueset2);

            // **********************************************************************

            var row = new ParameterRowViewModel(this.parameter1, this.option, this.session.Object, null);
            Assert.AreEqual("domain", row.OwnerName);
            Assert.AreEqual("dom", row.OwnerShortName);

            Assert.AreEqual(2, row.ContainedRows.Count);
            var c1row = row.ContainedRows.OfType<ParameterTypeComponentRowViewModel>().First();
            var c2row = row.ContainedRows.OfType<ParameterTypeComponentRowViewModel>().Last();

            Assert.AreEqual("c1", c1row.Name);
            Assert.IsTrue(c1row.Value.Contains("manual1"));
            Assert.AreEqual("dom", c1row.OwnerShortName);

            Assert.AreEqual("c2", c2row.Name);
            Assert.IsTrue(c2row.Value.Contains("manual2"));
            Assert.AreEqual("dom", c2row.OwnerShortName);
        }

        [Test]
        public void VerifyPropertiesForCompoundWithState()
        {
            // **************************INPUT***************************************
            var published1 = new ValueArray<string>(new List<string> { "s1value1", "s1value2" }, this.valueset);
            var published2 = new ValueArray<string>(new List<string> { "s2value1", "s2value2" });
            var actual = new ValueArray<string>(new List<string> { "s1value1", "manual2" }, this.valueset);
            var actual2 = new ValueArray<string>(new List<string> { "s2value1", "s2value2" });

            this.valueset.Published = published1;
            this.valueset.Manual = actual;
            this.valueset.ValueSwitch = ParameterSwitchKind.MANUAL;
            this.valueset.ActualOption = this.option;
            this.valueset.ActualState = this.state1;

            var compoundtype = new CompoundParameterType(Guid.NewGuid(), null, this.uri);

            var component1 = new ParameterTypeComponent(Guid.NewGuid(), null, this.uri)
            {
                ParameterType = this.parameterType1,
                ShortName = "c1"
            };

            var component2 = new ParameterTypeComponent(Guid.NewGuid(), null, this.uri)
            {
                ParameterType = this.parameterType1,
                ShortName = "c2"
            };

            compoundtype.Component.Add(component1);
            compoundtype.Component.Add(component2);

            var state2 = new ActualFiniteState(Guid.NewGuid(), null, this.uri);
            this.stateList.ActualState.Add(state2);

            this.parameter1.ParameterType = compoundtype;
            this.parameter1.StateDependence = this.stateList;
            this.parameter1.ValueSet.Add(this.valueset);
            this.parameter1.IsOptionDependent = true;

            var valueset2 = new ParameterValueSet(Guid.NewGuid(), null, this.uri)
            {
                ActualOption = this.option,
                ActualState = state2,
                Published = published2,
                Manual = actual2,
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            this.parameter1.ValueSet.Add(valueset2);

            // **********************************************************************

            var row = new ParameterRowViewModel(this.parameter1, this.option, this.session.Object, null);

            Assert.IsTrue(row.IsPublishable);

            Assert.AreEqual(2, row.ContainedRows.OfType<ParameterStateRowViewModel>().Count());
            var s1row = row.ContainedRows.OfType<ParameterStateRowViewModel>().First();
            var s2row = row.ContainedRows.OfType<ParameterStateRowViewModel>().Last();

            Assert.IsTrue(s1row.IsPublishable);
            Assert.IsFalse(s2row.IsPublishable);

            Assert.AreEqual(2, s1row.ContainedRows.OfType<ParameterTypeComponentRowViewModel>().Count());
            var s1c1 = s1row.ContainedRows.OfType<ParameterTypeComponentRowViewModel>().First();
            var s1c2 = s1row.ContainedRows.OfType<ParameterTypeComponentRowViewModel>().Last();
            Assert.IsFalse(s1c1.IsPublishable);
            Assert.IsTrue(s1c2.IsPublishable);
            Assert.IsTrue(s1c1.Value.Contains("s1value1"));
            Assert.IsTrue(s1c2.Value.Contains("s1value2"));

            Assert.AreEqual(2, s2row.ContainedRows.OfType<ParameterTypeComponentRowViewModel>().Count());
            var s2c1 = s2row.ContainedRows.OfType<ParameterTypeComponentRowViewModel>().First();
            var s2c2 = s2row.ContainedRows.OfType<ParameterTypeComponentRowViewModel>().Last();
            Assert.IsTrue(s2c1.Value.Contains("s2value1"));
            Assert.IsTrue(s2c2.Value.Contains("s2value2"));
        }

        [Test]
        public void VerifyThatParameterTypeUpdatesAreHandled()
        {
            // **********************************************************************
            var published = new ValueArray<string>(new List<string> { "manual" }, this.valueset);
            var actual = new ValueArray<string>(new List<string> { "different" }, this.valueset);

            this.valueset.Published = published;
            this.valueset.Manual = actual;
            this.valueset.ValueSwitch = ParameterSwitchKind.MANUAL;

            this.parameter1.ValueSet.Add(this.valueset);

            // **********************************************************************

            var row = new ParameterRowViewModel(this.parameter1, this.option, this.session.Object, null);

            Assert.AreEqual("pt1", row.Name);
            Assert.IsTrue(row.Value.Contains("manual"));
            Assert.IsTrue(row.IsPublishable);
            Assert.IsNull(row.StateDependence);
            Assert.IsNull(row.MeasurementScale);
            Assert.AreEqual(0, row.ContainedRows.Count);
            Assert.AreEqual("domain", row.OwnerName);
            Assert.AreEqual("dom", row.OwnerShortName);

            this.parameterType1.Name = "updatept1";
            this.parameterType1.RevisionNumber = 100;

            CDPMessageBus.Current.SendObjectChangeEvent(this.parameterType1, EventKind.Updated);

            Assert.AreEqual("updatept1", row.Name);
        }

        [Test]
        public void VerifyThatDragOverWorksForRelationalExpression()
        {
            var vm = new ParameterRowViewModel(this.parameter1, this.option, this.session.Object, null);

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(this.relationalExpression);

            dropinfo.SetupProperty(x => x.Effects);
            vm.DragOver(dropinfo.Object);

            Assert.AreEqual(DragDropEffects.Copy, dropinfo.Object.Effects);
        }

        [Test]
        public void VerifyThatDragOverWorksCorrectlyForNotSupportedType()
        {
            var vm = new ParameterRowViewModel(this.parameter1, this.option, this.session.Object, null);

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(this.parameter1);

            dropinfo.SetupProperty(x => x.Effects);
            vm.DragOver(dropinfo.Object);

            Assert.AreEqual(DragDropEffects.None, dropinfo.Object.Effects);
        }

        [Test]
        public async Task VerifyThatDropWorksForRelationalExpression()
        {
            var vm = new ParameterRowViewModel(this.parameter1, this.option, this.session.Object, null);

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(this.relationalExpression);
            dropinfo.Setup(x => x.Effects).Returns(DragDropEffects.Copy);

            dropinfo.SetupProperty(x => x.Effects);
            await vm.Drop(dropinfo.Object);

            this.thingCreator.Verify(x => x.CreateBinaryRelationshipForRequirementVerification(It.IsAny<ISession>(), It.IsAny<Iteration>(), It.IsAny<ParameterOrOverrideBase>(), It.IsAny<RelationalExpression>()), Times.Once);
        }

        [Test]
        public void VerifyThatDropWorksCorrectlyForNotSupportedType()
        {
            var vm = new ParameterRowViewModel(this.parameter1, this.option, this.session.Object, null);

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(this.parameter1);

            dropinfo.SetupProperty(x => x.Effects);
            vm.DragOver(dropinfo.Object);

            this.thingCreator.Verify(x => x.CreateBinaryRelationshipForRequirementVerification(It.IsAny<ISession>(), It.IsAny<Iteration>(), It.IsAny<ParameterOrOverrideBase>(), It.IsAny<RelationalExpression>()), Times.Never);
        }

        [Test]
        public void VerifyThatElementCategoryIsCollectedCorrectlyForElementUsage1()
        {
            var elementUsageRow = new ElementUsageRowViewModel(this.elementUsage1, this.option, this.session.Object, null);
            var row = new ParameterRowViewModel(this.parameter1, this.option, this.session.Object, elementUsageRow);

            Assert.AreEqual(2, row.Category.Count());

            var expectedCategories = new List<Category>
            {
                this.category1,
                this.category2
            };

            CollectionAssert.AreEquivalent(expectedCategories, row.Category);
        }

        [Test]
        public void VerifyThatElementCategoryIsCollectedCorrectlyForElementDefinition()
        {
            var elementDefinitionRow = new ElementDefinitionRowViewModel(this.elementdef1, this.option, this.session.Object, null);
            var row = new ParameterRowViewModel(this.parameter1, this.option, this.session.Object, elementDefinitionRow);

            Assert.AreEqual(1, row.Category.Count());

            var expectedCategories = new List<Category>
            {
                this.category1
            };

            CollectionAssert.AreEquivalent(expectedCategories, row.Category);
        }

        [Test]
        public void VerifyThatCategoryIsCollectedCorrectlyForElementUsage1()
        {
            var elementUsageRow = new ElementUsageRowViewModel(this.elementUsage1, this.option, this.session.Object, null);
            var row = new ParameterRowViewModel(this.parameter1, this.option, this.session.Object, elementUsageRow);

            Assert.AreEqual(2, row.Category.Count());

            var expectedCategories = new List<Category>
            {
                this.category1,
                this.category2,
            };

            CollectionAssert.AreEquivalent(expectedCategories, row.Category);
        }

        [Test]
        public void VerifyThatCategoryIsCollectedCorrectlyForElementDefinition()
        {
            var elementDefinitionRow = new ElementDefinitionRowViewModel(this.elementdef1, this.option, this.session.Object, null);
            var row = new ParameterRowViewModel(this.parameter1, this.option, this.session.Object, elementDefinitionRow);

            Assert.AreEqual(1, row.Category.Count());

            var expectedCategories = new List<Category>
            {
                this.category1
            };

            CollectionAssert.AreEquivalent(expectedCategories, row.Category);
        }
    }
}
