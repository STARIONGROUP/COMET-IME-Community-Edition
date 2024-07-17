﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterBaseRowViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
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

namespace CDP4EngineeringModel.Tests.ViewModels.ElementDefinitionTreeRows
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CDP4EngineeringModel.Services;
    using CDP4EngineeringModel.ViewModels;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    internal class ParameterBaseRowViewModelTestFixture
    {
        private Mock<IPermissionService> permissionService;
        private Mock<IObfuscationService> obfuscationService;
        private Mock<ISession> session;
        private readonly Uri uri = new Uri("http://test.com");
        private ElementDefinition elementDefinition;
        private ElementDefinition elementDefinitionForUsage1;
        private ElementDefinition elementDefinitionForUsage2;
        private ElementUsage elementUsage1;
        private ElementUsage elementUsage2;
        private Category category1;
        private Category category2;
        private Parameter parameter1;
        private Parameter parameter5ForSubscription;
        private Parameter parameterCompound;
        private Parameter parameterCompoundForSubscription;
        private ParameterSubscription parameter5Subscription;
        private ParameterSubscription parameterSubscriptionCompound;
        private Iteration iteration;
        private Option option1;
        private Option option2;
        private QuantityKind qqParamType;
        private CompoundParameterType cptType;
        private ArrayParameterType apType;
        private DomainOfExpertise activeDomain;
        private DomainOfExpertise someotherDomain;
        private ActualFiniteStateList stateList;
        private PossibleFiniteState state1;
        private PossibleFiniteState state2;
        private PossibleFiniteStateList posStateList;
        private Participant participant;
        private Person person;
        private BooleanParameterType boolPt;

        private Assembler assembler;
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            this.messageBus = new CDPMessageBus();
            this.assembler = new Assembler(this.uri, this.messageBus);

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            this.obfuscationService = new Mock<IObfuscationService>();
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.option1 = new Option(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "option1" };
            this.option2 = new Option(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "option2" };

            this.stateList = new ActualFiniteStateList(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.state1 = new PossibleFiniteState(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "state1" };
            this.state2 = new PossibleFiniteState(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "state2" };

            this.posStateList = new PossibleFiniteStateList(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.posStateList.PossibleState.Add(this.state1);
            this.posStateList.PossibleState.Add(this.state2);
            this.posStateList.DefaultState = this.state1;

            this.stateList.ActualState.Add(new ActualFiniteState(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                PossibleState = new List<PossibleFiniteState> { this.state1 },
                Kind = ActualFiniteStateKind.MANDATORY
            });

            this.stateList.ActualState.Add(new ActualFiniteState(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                PossibleState = new List<PossibleFiniteState> { this.state2 },
                Kind = ActualFiniteStateKind.FORBIDDEN
            });

            this.stateList.PossibleFiniteStateList.Add(this.posStateList);

            this.activeDomain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "active", ShortName = "active" };
            this.someotherDomain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "other", ShortName = "other" };

            this.qqParamType = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "PTName",
                ShortName = "PTShortName",
            };

            this.boolPt = new BooleanParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri);

            // Array parameter type with components
            this.apType = new ArrayParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "APTName",
                ShortName = "APTShortName"
            };

            this.apType.Component.Add(new ParameterTypeComponent(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Iid = Guid.NewGuid(),
                ParameterType = this.qqParamType
            });

            this.apType.Component.Add(new ParameterTypeComponent(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Iid = Guid.NewGuid(),
                ParameterType = this.qqParamType
            });

            // compound parameter type with components
            this.cptType = new CompoundParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "APTName",
                ShortName = "APTShortName"
            };

            this.cptType.Component.Add(new ParameterTypeComponent(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Iid = Guid.NewGuid(),
                ParameterType = this.qqParamType,
                ShortName = "c1"
            });

            this.cptType.Component.Add(new ParameterTypeComponent(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Iid = Guid.NewGuid(),
                ParameterType = this.qqParamType,
                ShortName = "c2"
            });

            this.category1 = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.category2 = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.qqParamType.Category = new List<Category>() { this.category1 };

            this.elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Owner = this.activeDomain
            };

            this.elementDefinition.Category.Add(this.category2);

            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.iteration.Element.Add(this.elementDefinition);

            this.iteration.Option.Add(this.option1);
            this.iteration.Option.Add(this.option2);
            var engineeringModel = new EngineeringModel(Guid.NewGuid(), null, null);
            this.iteration.Container = engineeringModel;

            this.elementDefinitionForUsage1 = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ShortName = "Elem1",
                Owner = this.someotherDomain
            };

            this.elementDefinitionForUsage1.Category.Add(this.category1);

            this.elementDefinitionForUsage2 = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ShortName = "Elem2",
                Owner = this.someotherDomain
            };

            this.elementDefinitionForUsage2.Category.Add(this.category1);

            this.elementUsage1 = new ElementUsage(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Container = this.elementDefinitionForUsage1,
                ShortName = "Usage1",
                Owner = this.someotherDomain
            };

            this.elementUsage2 = new ElementUsage(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Container = this.elementDefinitionForUsage2,
                ShortName = "Usage2",
                Owner = this.someotherDomain
            };

            this.elementUsage2.Category.Add(this.category2);

            this.elementUsage1.ElementDefinition = this.elementDefinitionForUsage1;
            this.elementUsage2.ElementDefinition = this.elementDefinitionForUsage2;

            this.parameter1 = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = this.qqParamType,
                Owner = this.activeDomain,
            };

            this.parameter5ForSubscription = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = this.qqParamType,
                Owner = this.someotherDomain
            };

            this.parameterCompound = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = this.cptType,
                Owner = this.someotherDomain
            };

            this.parameterCompoundForSubscription = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = this.cptType,
                Owner = this.someotherDomain
            };

            this.parameterSubscriptionCompound = new ParameterSubscription(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Owner = this.activeDomain
            };

            this.parameterCompoundForSubscription.ParameterSubscription.Add(this.parameterSubscriptionCompound);

            this.iteration.Element.Add(this.elementDefinitionForUsage1);
            this.iteration.Element.Add(this.elementDefinitionForUsage2);

            this.elementDefinition.Parameter.Add(this.parameter1);
            this.elementDefinition.Parameter.Add(this.parameterCompound);
            this.elementDefinition.Parameter.Add(this.parameter5ForSubscription);

            this.person = new Person(Guid.NewGuid(), null, null) { GivenName = "test", Surname = "test" };
            this.participant = new Participant(Guid.NewGuid(), null, null) { Person = this.person, SelectedDomain = this.activeDomain };
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            var modelSetup = new EngineeringModelSetup(Guid.NewGuid(), null, null);
            modelSetup.Participant.Add(this.participant);
            engineeringModel.EngineeringModelSetup = modelSetup;
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertyValuesAreSetWhenMultipleValueSets()
        {
            // Test input
            var valueSet1 = new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var valueSet2 = new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.SetScalarValueSet(valueSet1);
            this.SetScalarValueSet(valueSet2);

            this.parameter1.ValueSet.Add(valueSet1);
            this.parameter1.ValueSet.Add(valueSet2);

            // *******************

            var row = new ParameterRowViewModel(this.parameter1, this.session.Object, null, false);

            Assert.AreEqual("PTName", row.Name);
            Assert.AreEqual("active", row.OwnerName);
            Assert.AreEqual(string.Empty, row.Manual);
            Assert.AreEqual(string.Empty, row.Computed);
            Assert.AreEqual(string.Empty, row.Reference);
            Assert.AreEqual(string.Empty, row.Formula);
            Assert.That(row.ScaleName, Is.Null.Or.Empty);
        }

        [Test]
        public void VerifyThatPropertyValuesAreSetForCompoundValueSet()
        {
            // Test input
            var valueSet = new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.SetCompoundValueSet(valueSet);

            this.parameter1.ValueSet.Add(valueSet);

            // *******************

            var row = new ParameterRowViewModel(this.parameter1, this.session.Object, null, false);

            Assert.AreEqual("PTName", row.Name);
            Assert.AreEqual("active", row.OwnerName);
            Assert.AreEqual(null, row.Manual);
            Assert.AreEqual(string.Empty, row.Computed);
            Assert.AreEqual(null, row.Reference);
            Assert.AreEqual(string.Empty, row.Formula);
            Assert.That(row.ScaleName, Is.Null.Or.Empty);
        }

        [Test]
        public void VerifyThatNoOptionNoStateParameterIsSet()
        {
            // Test input
            var valueSet = new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.SetScalarValueSet(valueSet);

            this.parameter1.ValueSet.Add(valueSet);

            // *******************

            var row = new ParameterRowViewModel(this.parameter1, this.session.Object, null, false);

            Assert.AreEqual("PTName", row.Name);
            Assert.AreEqual("active", row.OwnerName);
            Assert.AreEqual("manual", row.Manual);
            Assert.AreEqual("computed", row.Computed);
            Assert.AreEqual("ref", row.Reference);
            Assert.AreEqual("formula", row.Formula);
            Assert.That(row.ScaleName, Is.Null.Or.Empty);
        }

        [Test]
        public void VerifyThatOptionPArameterIsSet()
        {
            // Test input
            var valueSetOption1 = new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var valueSetOption2 = new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.SetScalarValueSet(valueSetOption1);
            valueSetOption1.ActualOption = this.option1;

            this.SetScalarValueSet(valueSetOption2);
            valueSetOption2.ActualOption = this.option2;

            this.parameter1.IsOptionDependent = true;
            this.parameter1.ValueSet.Add(valueSetOption1);
            this.parameter1.ValueSet.Add(valueSetOption2);

            // *******************

            var row = new ParameterRowViewModel(this.parameter1, this.session.Object, null, false);

            Assert.AreEqual(2, row.ContainedRows.Count);
            var firstOption = (ParameterOptionRowViewModel)row.ContainedRows.First();

            Assert.AreEqual("option1", firstOption.Name);
            Assert.AreEqual("active", firstOption.OwnerName);
            Assert.AreEqual("manual", firstOption.Manual);
            Assert.AreEqual("computed", firstOption.Computed);
            Assert.AreEqual("formula", firstOption.Formula);
            Assert.AreEqual("ref", firstOption.Reference);
            Assert.That(firstOption.ScaleName, Is.Null.Or.Empty);
        }

        [Test]
        public void VerifyThatStateDependentPArameterAreSet()
        {
            var valueSetState1 = new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var valueSetState2 = new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.SetScalarValueSet(valueSetState1);
            this.SetScalarValueSet(valueSetState2);

            valueSetState1.ActualState = this.stateList.ActualState.First();

            this.parameter1.ValueSet.Add(valueSetState1);
            this.parameter1.ValueSet.Add(valueSetState2);

            this.parameter1.StateDependence = this.stateList;
            var row = new ParameterRowViewModel(this.parameter1, this.session.Object, null, false);

            Assert.AreEqual(1, row.ContainedRows.Count);
            var firstState = (ParameterStateRowViewModel)row.ContainedRows.First();

            Assert.AreEqual("state1", firstState.Name);
            Assert.AreEqual("active", firstState.OwnerName);
            Assert.AreEqual("manual", firstState.Manual);
            Assert.AreEqual("computed", firstState.Computed);
            Assert.AreEqual("formula", firstState.Formula);
            Assert.AreEqual("ref", firstState.Reference);
            Assert.That(firstState.ScaleName, Is.Null.Or.Empty);
        }

        [Test]
        public void VerifyThatCompoundParameterAreSet()
        {
            var valueSet = new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.SetCompoundValueSet(valueSet);

            this.parameterCompound.ValueSet.Add(valueSet);

            var row = new ParameterRowViewModel(this.parameterCompound, this.session.Object, null, false);

            Assert.AreEqual("APTName", row.Name);

            Assert.AreEqual(2, row.ContainedRows.Count);
            var c1 = (ParameterComponentValueRowViewModel)row.ContainedRows.First();

            Assert.AreEqual("c1", c1.Name);
            Assert.AreEqual("other", c1.OwnerName);
            Assert.AreEqual("manual1", c1.Manual);
            Assert.AreEqual("computed1", c1.Computed);
            Assert.AreEqual("formula1", c1.Formula);
            Assert.AreEqual("ref1", c1.Reference);
            Assert.That(c1.ScaleName, Is.Null.Or.Empty);
        }

        [Test]
        public void VerifyThatOptionAndStateDependentParameterIsSet()
        {
            var valueSeto1s1 = new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var valueSeto2s1 = new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.SetScalarValueSet(valueSeto1s1);
            this.SetScalarValueSet(valueSeto2s1);

            valueSeto1s1.ActualOption = this.option1;
            valueSeto1s1.ActualState = this.stateList.ActualState.First();

            valueSeto2s1.ActualOption = this.option2;
            valueSeto2s1.ActualState = this.stateList.ActualState.First();

            this.parameter1.ValueSet.Add(valueSeto1s1);
            this.parameter1.ValueSet.Add(valueSeto2s1);

            this.parameter1.IsOptionDependent = true;
            this.parameter1.StateDependence = this.stateList;
            var row = new ParameterRowViewModel(this.parameter1, this.session.Object, null, false);

            Assert.AreEqual("PTName", row.Name);
            Assert.AreEqual(2, row.ContainedRows.Count);

            var o2row = (ParameterOptionRowViewModel)row.ContainedRows.Last();
            Assert.AreEqual("option2", o2row.Name);
            Assert.IsNull(o2row.Manual);

            Assert.AreEqual(1, o2row.ContainedRows.Count);

            var stateRow = (ParameterStateRowViewModel)o2row.ContainedRows.Single();
            Assert.AreEqual("state1", stateRow.Name);
            Assert.AreEqual("active", stateRow.OwnerName);
            Assert.AreEqual("manual", stateRow.Manual);
            Assert.AreEqual("computed", stateRow.Computed);
            Assert.AreEqual("formula", stateRow.Formula);
            Assert.AreEqual("ref", stateRow.Reference);
            Assert.That(stateRow.ScaleName, Is.Null.Or.Empty);
        }

        [Test]
        public void VerifyThatOptionStateDependentCompoundParameterCanBeSet()
        {
            var valueSeto1s1 = new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var valueSeto2s1 = new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.SetCompoundValueSet(valueSeto1s1);
            this.SetCompoundValueSet(valueSeto2s1);

            valueSeto1s1.ActualOption = this.option1;
            valueSeto1s1.ActualState = this.stateList.ActualState.First();

            valueSeto2s1.ActualOption = this.option2;
            valueSeto2s1.ActualState = this.stateList.ActualState.First();

            this.parameterCompound.ValueSet.Add(valueSeto1s1);
            this.parameterCompound.ValueSet.Add(valueSeto2s1);

            this.parameterCompound.IsOptionDependent = true;
            this.parameterCompound.StateDependence = this.stateList;

            // Test starts
            var row = new ParameterRowViewModel(this.parameterCompound, this.session.Object, null, false);

            Assert.AreEqual("APTName", row.Name);
            Assert.AreEqual(2, row.ContainedRows.Count);

            var o2row = (ParameterOptionRowViewModel)row.ContainedRows.Last();
            Assert.AreEqual("option2", o2row.Name);
            Assert.IsNull(o2row.Manual);
            Assert.AreEqual(1, o2row.ContainedRows.Count);

            var stateRow = (ParameterStateRowViewModel)o2row.ContainedRows.Single();
            Assert.AreEqual("state1", stateRow.Name);
            Assert.AreEqual(2, stateRow.ContainedRows.Count);
            Assert.IsNull(stateRow.Manual);

            var c2row = (ParameterComponentValueRowViewModel)stateRow.ContainedRows.Last();
            Assert.AreEqual("c2", c2row.Name);
            Assert.AreEqual("other", c2row.OwnerName);
            Assert.AreEqual("manual2", c2row.Manual);
            Assert.AreEqual("computed2", c2row.Computed);
            Assert.AreEqual("formula2", c2row.Formula);
            Assert.AreEqual("ref2", c2row.Reference);
            Assert.That(c2row.ScaleName, Is.Null.Or.Empty);
        }

        [Test]
        public void VerifyThatStateDependentCompoundParameterCanBeSet()
        {
            var valueSetState1 = new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.SetCompoundValueSet(valueSetState1);

            valueSetState1.ActualState = this.stateList.ActualState.First();

            this.parameterCompound.ValueSet.Add(valueSetState1);

            this.parameterCompound.StateDependence = this.stateList;
            var row = new ParameterRowViewModel(this.parameterCompound, this.session.Object, null, false);

            Assert.AreEqual("APTName", row.Name);
            Assert.AreEqual(1, row.ContainedRows.Count);

            var firstState = (ParameterStateRowViewModel)row.ContainedRows.First();
            Assert.AreEqual("state1", firstState.Name);
            Assert.AreEqual(2, firstState.ContainedRows.Count);

            var c2Row = (ParameterComponentValueRowViewModel)firstState.ContainedRows.Last();

            Assert.AreEqual("c2", c2Row.Name);
            Assert.AreEqual("other", c2Row.OwnerName);
            Assert.AreEqual("manual2", c2Row.Manual);
            Assert.AreEqual("computed2", c2Row.Computed);
            Assert.AreEqual("formula2", c2Row.Formula);
            Assert.AreEqual("ref2", c2Row.Reference);
            Assert.That(c2Row.ScaleName, Is.Null.Or.Empty);
        }

        [Test]
        public void VerifyThatParameterSubscriptionHasRightOwnerValue()
        {
            this.parameter5Subscription = new ParameterSubscription(Guid.NewGuid(), this.assembler.Cache, this.uri) { Owner = this.activeDomain };
            this.parameter5ForSubscription.ParameterSubscription.Add(this.parameter5Subscription);

            var valueSet = new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.SetScalarValueSet(valueSet);

            var valueSetSub = new ParameterSubscriptionValueSet(Guid.NewGuid(), this.assembler.Cache, this.uri);
            valueSetSub.SubscribedValueSet = valueSet;

            this.parameter5Subscription.ValueSet.Add(valueSetSub);

            var row = new ParameterSubscriptionRowViewModel(this.parameter5Subscription, this.session.Object, null, false);
            Assert.AreEqual(0, row.ContainedRows.Count);
            Assert.AreEqual("PTName", row.Name);
            Assert.AreEqual("[other]", row.OwnerName);
            Assert.AreEqual("formula", row.Formula);
            Assert.AreEqual("ref", row.Reference);
            Assert.That(row.ScaleName, Is.Null.Or.Empty);
        }

        [Test]
        [TestCaseSource(typeof(MessageBusContainerCases), "GetCases")]
        public void VerifyThatParameterComponentRowsUpdateOnParameterType(IViewModelBase<Thing> container, string scenario)
        {
            var row = new ParameterRowViewModel(this.parameterCompound, this.session.Object, container, false);

            Assert.AreEqual(2, this.cptType.Component.Count);
            Assert.AreEqual(2, row.ContainedRows.Count);

            this.cptType.Component.Remove(this.cptType.Component.Last());
            Assert.AreEqual(1, this.cptType.Component.Count);

            this.messageBus.SendObjectChangeEvent(this.cptType, EventKind.Updated);
            Assert.AreEqual(1, row.ContainedRows.Count);
        }

        [Test]
        public void VerifyThatValidatePropertyWorks()
        {
            this.parameter1.ParameterType = this.boolPt;
            var row = new ParameterRowViewModel(this.parameter1, this.session.Object, null, false);

            Assert.That(row.ValidateProperty("Manual", "12,45"), Is.Not.Null.Or.Empty);
            Assert.That(row.ValidateProperty("Reference", "12,45"), Is.Not.Null.Or.Empty);
            Assert.That(row.ValidateProperty("Manual", true), Is.Null.Or.Empty);
            Assert.That(row.ValidateProperty("Reference", null), Is.Null.Or.Empty);
        }

        [Test]
        [TestCaseSource(typeof(MessageBusContainerCases), "GetCases")]
        public void VerifyThatStateDependentRowAreUpdated(IViewModelBase<Thing> container, string scenario)
        {
            var valueSetState1 = new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var valueSetState2 = new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.SetScalarValueSet(valueSetState1);
            this.SetScalarValueSet(valueSetState2);

            var astate1 = this.stateList.ActualState.First();
            var astate2 = this.stateList.ActualState.Last();

            valueSetState1.ActualState = astate1;
            valueSetState2.ActualState = astate2;

            this.parameter1.ValueSet.Add(valueSetState1);
            this.parameter1.ValueSet.Add(valueSetState2);

            this.parameter1.StateDependence = this.stateList;
            var row = new ParameterRowViewModel(this.parameter1, this.session.Object, container, false);

            Assert.AreEqual(1, row.ContainedRows.Count);

            astate1.Kind = ActualFiniteStateKind.FORBIDDEN;
            this.messageBus.SendObjectChangeEvent(astate1, EventKind.Updated);

            Assert.AreEqual(0, row.ContainedRows.Count);

            astate1.Kind = ActualFiniteStateKind.MANDATORY;
            this.messageBus.SendObjectChangeEvent(astate1, EventKind.Updated);
            Assert.AreEqual(1, row.ContainedRows.Count);
        }

        [Test]
        public void VerifyThatElementCategoryIsCollectedCorrectlyForElementUsage1()
        {
            this.elementDefinitionForUsage1.Parameter.Add(this.parameter1);

            var elementUsageRow = new ElementUsageRowViewModel(this.elementUsage1, this.activeDomain, this.session.Object, null, this.obfuscationService.Object);
            var row = new ParameterRowViewModel(this.parameter1, this.session.Object, elementUsageRow, false);

            Assert.AreEqual(2, row.Category.Count());

            var expectedCategories = new List<Category>
            {
                this.category1,
                this.category1
            };

            CollectionAssert.AreEquivalent(expectedCategories, row.Category);
        }

        [Test]
        public void VerifyThatElementCategoryIsCollectedCorrectlyForElementUsage2()
        {
            this.elementDefinitionForUsage2.Parameter.Add(this.parameter1);

            var elementUsageRow = new ElementUsageRowViewModel(this.elementUsage2, this.activeDomain, this.session.Object, null, this.obfuscationService.Object);
            var row = new ParameterRowViewModel(this.parameter1, this.session.Object, elementUsageRow, false);

            Assert.AreEqual(3, row.Category.Count());

            var expectedCategories = new List<Category>
            {
                this.category1,
                this.category2,
                this.category1,
            };

            CollectionAssert.AreEquivalent(expectedCategories, row.Category);
        }

        [Test]
        public void VerifyThatElementCategoryIsCollectedCorrectlyForElementDefinition()
        {
            var elementDefinitionRow = new ElementDefinitionRowViewModel(this.elementDefinition, this.activeDomain, this.session.Object, null, this.obfuscationService.Object);
            var row = new ParameterRowViewModel(this.parameter1, this.session.Object, elementDefinitionRow, false);

            Assert.AreEqual(2, row.Category.Count());

            var expectedCategories = new List<Category>
            {
                this.category2,
                this.category1
            };

            CollectionAssert.AreEquivalent(expectedCategories, row.Category);
        }

        [Test]
        public void VerifyThatCategoryIsCollectedCorrectlyForElementUsage1()
        {
            this.elementDefinitionForUsage1.Parameter.Add(this.parameter1);

            var elementUsageRow = new ElementUsageRowViewModel(this.elementUsage1, this.activeDomain, this.session.Object, null, this.obfuscationService.Object);
            var row = new ParameterRowViewModel(this.parameter1, this.session.Object, elementUsageRow, false);

            Assert.AreEqual(2, row.Category.Count());

            var expectedCategories = new List<Category>
            {
                this.category1,
                this.category1
            };

            CollectionAssert.AreEquivalent(expectedCategories, row.Category);
        }

        [Test]
        public void VerifyThatCategoryIsCollectedCorrectlyForElementUsage2()
        {
            this.elementDefinitionForUsage2.Parameter.Add(this.parameter1);

            var elementUsageRow = new ElementUsageRowViewModel(this.elementUsage2, this.activeDomain, this.session.Object, null, this.obfuscationService.Object);
            var row = new ParameterRowViewModel(this.parameter1, this.session.Object, elementUsageRow, false);

            Assert.AreEqual(3, row.Category.Count());

            var expectedCategories = new List<Category>
            {
                this.category1,
                this.category2,
                this.category1
            };

            CollectionAssert.AreEquivalent(expectedCategories, row.Category);
        }

        [Test]
        public void VerifyThatCategoryIsCollectedCorrectlyForElementDefinition()
        {
            var elementDefinitionRow = new ElementDefinitionRowViewModel(this.elementDefinition, this.activeDomain, this.session.Object, null, this.obfuscationService.Object);
            var row = new ParameterRowViewModel(this.parameter1, this.session.Object, elementDefinitionRow, false);

            Assert.AreEqual(2, row.Category.Count());

            var expectedCategories = new List<Category>
            {
                this.category1,
                this.category2
            };

            CollectionAssert.AreEquivalent(expectedCategories, row.Category);
        }

        /// <summary>
        /// Set a ValueSet for a scalar ParameterType
        /// </summary>
        /// <param name="valueset">The <see cref="ParameterValueSetBase"/> to set</param>
        private void SetScalarValueSet(ParameterValueSetBase valueset)
        {
            var manualSet = new ValueArray<string>(new List<string> { "manual" });
            var referenceSet = new ValueArray<string>(new List<string> { "ref" });
            var computedSet = new ValueArray<string>(new List<string> { "computed" });
            var publishedSet = new ValueArray<string>(new List<string> { "published" });
            var formulaSet = new ValueArray<string>(new List<string> { "formula" });

            valueset.Manual = manualSet;
            valueset.Reference = referenceSet;
            valueset.Computed = computedSet;
            valueset.Published = publishedSet;
            valueset.Formula = formulaSet;
            valueset.ValueSwitch = ParameterSwitchKind.REFERENCE;
        }

        /// <summary>
        /// Set a ValueSet for a compound ParameterType with 2 components
        /// </summary>
        /// <param name="valueset">The <see cref="ParameterValueSetBase"/> to set</param>
        private void SetCompoundValueSet(ParameterValueSetBase valueset)
        {
            var manualSet = new ValueArray<string>(new List<string> { "manual1", "manual2" });
            var referenceSet = new ValueArray<string>(new List<string> { "ref1", "ref2" });
            var computedSet = new ValueArray<string>(new List<string> { "computed1", "computed2" });
            var publishedSet = new ValueArray<string>(new List<string> { "published1", "published2" });
            var formulaSet = new ValueArray<string>(new List<string> { "formula1", "formula2" });

            valueset.Manual = manualSet;
            valueset.Reference = referenceSet;
            valueset.Computed = computedSet;
            valueset.Published = publishedSet;
            valueset.Formula = formulaSet;
            valueset.ValueSwitch = ParameterSwitchKind.REFERENCE;
        }
    }
}
