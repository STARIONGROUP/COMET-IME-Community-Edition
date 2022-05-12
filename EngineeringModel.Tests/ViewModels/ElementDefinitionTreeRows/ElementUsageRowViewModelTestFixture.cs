// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementUsageRowViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
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
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using CDP4EngineeringModel.Services;
    using CDP4EngineeringModel.ViewModels;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    internal class ElementUsageRowViewModelTestFixture
    {
        private Mock<IPermissionService> permissionService;
        private Mock<ISession> session;
        private readonly Uri uri = new Uri("http://test.com");
        private Assembler assembler;
        private Mock<IObfuscationService> obfuscationService;

        private ElementDefinition elementDefinition;
        private ElementDefinition elementDefinitionForUsage1;
        private ElementDefinition elementDefinitionForUsage2;
        private ElementUsage elementUsage1;
        private ElementUsage elementUsage2;
        private ParameterGroup parameterGroup1;
        private ParameterGroup parameterGroup2;
        private ParameterGroup parameterGroup3;
        private ParameterGroup parameterGroup1ForUsage1;
        private ParameterGroup parameterGroup2ForUsage2;
        private ParameterGroup parameterGroup3ForUsage1;
        private Parameter parameter1;
        private Parameter parameter2;
        private Parameter parameter3;
        private Parameter parameter4;
        private Parameter parameter5ForSubscription;
        private Parameter parameter6ForOverride;
        private Parameter parameterArray;
        private Parameter parameterCompound;
        private Parameter parameterCompoundForSubscription;
        private Parameter parameterForOptions;
        private ParameterOverride parameter6Override;
        private ParameterSubscription parameter5Subscription;
        private ParameterOverride parameterOverrideCompound;
        private ParameterSubscription parameterSubscriptionCompound;
        private ParameterOverride parameterOverrideOption;
        private ParameterSubscription parameterSubscriptionOption;
        private Parameter parameterForStates;
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

        [SetUp]
        public void SetUp()
        {
            this.permissionService = new Mock<IPermissionService>();
            this.obfuscationService = new Mock<IObfuscationService>();
            this.session = new Mock<ISession>();
            this.assembler = new Assembler(this.uri);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.option1 = new Option(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "1" };
            this.option2 = new Option(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "2" };

            this.stateList = new ActualFiniteStateList(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.state1 = new PossibleFiniteState(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.state2 = new PossibleFiniteState(Guid.NewGuid(), this.assembler.Cache, this.uri);

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

            this.activeDomain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.someotherDomain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.qqParamType = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "PTName",
                ShortName = "PTShortName"
            };

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
                ParameterType = this.qqParamType
            });

            this.cptType.Component.Add(new ParameterTypeComponent(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Iid = Guid.NewGuid(),
                ParameterType = this.qqParamType
            });

            this.elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Owner = this.activeDomain
            };

            var engModel = new EngineeringModel(Guid.NewGuid(), null, null);
            var modelSetup = new EngineeringModelSetup(Guid.NewGuid(), null, null);
            var person = new Person(Guid.NewGuid(), null, null) { GivenName = "test", Surname = "test" };
            var participant = new Participant(Guid.NewGuid(), null, null) { Person = person };
            modelSetup.Participant.Add(participant);
            engModel.EngineeringModelSetup = modelSetup;
            this.session.Setup(x => x.ActivePerson).Returns(person);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());

            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.iteration.Element.Add(this.elementDefinition);
            this.iteration.Option.Add(this.option1);
            this.iteration.Option.Add(this.option2);
            engModel.Iteration.Add(this.iteration);
            this.elementDefinitionForUsage1 = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Owner = this.someotherDomain
            };

            this.elementDefinitionForUsage2 = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Owner = this.someotherDomain
            };

            this.elementUsage1 = new ElementUsage(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Owner = this.someotherDomain
            };

            this.elementUsage2 = new ElementUsage(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Owner = this.someotherDomain
            };

            this.elementUsage1.ElementDefinition = this.elementDefinitionForUsage1;
            this.elementUsage2.ElementDefinition = this.elementDefinitionForUsage2;

            this.parameterGroup1 = new ParameterGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.parameterGroup2 = new ParameterGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.parameterGroup3 = new ParameterGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.parameterGroup1ForUsage1 = new ParameterGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.parameterGroup2ForUsage2 = new ParameterGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.parameterGroup3ForUsage1 = new ParameterGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.parameter1 = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = this.qqParamType,
                Owner = this.activeDomain
            };

            this.parameter2 = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = this.qqParamType,
                Owner = this.activeDomain
            };

            this.parameter3 = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = this.qqParamType,
                Owner = this.someotherDomain
            };

            this.parameter4 = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = this.qqParamType,
                Owner = this.someotherDomain
            };

            this.parameterForStates = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = this.qqParamType,
                Owner = this.someotherDomain,
                StateDependence = this.stateList
            };

            this.parameter5ForSubscription = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = this.qqParamType,
                Owner = this.someotherDomain
            };

            this.parameter6ForOverride = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = this.qqParamType,
                Owner = this.activeDomain
            };

            this.parameter6Override = new ParameterOverride(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Parameter = this.parameter6ForOverride,
                Owner = this.activeDomain
            };

            this.parameterArray = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = this.apType,
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

            this.parameterForOptions = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = this.cptType,
                Owner = this.someotherDomain,
                IsOptionDependent = true
            };

            this.elementDefinition.ParameterGroup.Add(this.parameterGroup1);
            this.elementDefinition.ParameterGroup.Add(this.parameterGroup2);
            this.elementDefinition.ParameterGroup.Add(this.parameterGroup3);

            this.elementDefinitionForUsage2.ParameterGroup.Add(this.parameterGroup2ForUsage2);


            this.iteration.Element.Add(elementDefinitionForUsage1);
            this.iteration.Element.Add(elementDefinitionForUsage2);

            this.parameterGroup3.ContainingGroup = this.parameterGroup1;
            this.parameterGroup3ForUsage1.ContainingGroup = this.parameterGroup1ForUsage1;

            this.parameter4.Group = this.parameterGroup3;
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test, TestCaseSource(typeof(MessageBusContainerCases), "GetCases")]
        public void VerifyThatParameterBaseElementAreHandledCorrectly(IViewModelBase<Thing> container, string scenario)
        {
            var revision = typeof (Thing).GetProperty("RevisionNumber");

            // Test input
            var valueSet = new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var valueSetOverride = new ParameterOverrideValueSet(Guid.NewGuid(), this.assembler.Cache, this.uri) { ParameterValueSet = valueSet };

            var manualSet = new ValueArray<string>(new List<string> { "manual" });
            var referenceSet = new ValueArray<string>(new List<string> { "ref" });
            var computedSet = new ValueArray<string>(new List<string> { "computed" });
            var publishedSet = new ValueArray<string>(new List<string> { "published" });

            valueSet.Manual = manualSet;
            valueSet.Reference = referenceSet;
            valueSet.Computed = computedSet;
            valueSet.Published = publishedSet;

            valueSetOverride.Manual = manualSet;
            valueSetOverride.Reference = referenceSet;
            valueSetOverride.Computed = computedSet;
            valueSetOverride.Published = publishedSet;

            this.parameter6ForOverride.ValueSet.Add(valueSet);
            this.parameter1.ValueSet.Add(valueSet);

            this.parameter6Override.ValueSet.Add(valueSetOverride);

            this.elementDefinitionForUsage1.Parameter.Add(this.parameter6ForOverride);
            this.elementDefinitionForUsage1.Parameter.Add(this.parameter1);

            this.elementUsage1.ParameterOverride.Add(this.parameter6Override);

            this.elementDefinition.ContainedElement.Add(this.elementUsage1);
            // ***************************************

            var row = new ElementUsageRowViewModel(this.elementUsage1, this.activeDomain, this.session.Object, container, this.obfuscationService.Object);

            // Verify That Override is displayed instead of parameter
            Assert.AreEqual(2, row.ContainedRows.Count);
            var overrideRow = row.ContainedRows.SingleOrDefault(x => x.Thing == this.parameter6Override);
            var parameterRow = row.ContainedRows.SingleOrDefault(x => x.Thing == this.parameter1);
            Assert.IsNotNull(overrideRow);
            Assert.IsNotNull(parameterRow);
            // **********************************

            // Add a subscription to parameter and see that its replaced.
            var subscription = new ParameterSubscription(Guid.NewGuid(), this.assembler.Cache, this.uri) { Owner = this.activeDomain };
            subscription.ValueSet.Add(new ParameterSubscriptionValueSet(Guid.NewGuid(), this.assembler.Cache, this.uri) { SubscribedValueSet = valueSet });
            this.parameter1.ParameterSubscription.Add(subscription);

            revision.SetValue(this.elementDefinitionForUsage1, 1);
            CDPMessageBus.Current.SendObjectChangeEvent(this.elementDefinitionForUsage1, EventKind.Updated);

            Assert.AreEqual(2, row.ContainedRows.Count);
            var subscriptionRow = row.ContainedRows.SingleOrDefault(x => x.Thing == subscription);
            Assert.IsNotNull(subscriptionRow);

            parameterRow = row.ContainedRows.SingleOrDefault(x => x.Thing == this.parameter1);
            Assert.IsNull(parameterRow);

            // Add a subscription to the override of the usage
            var subscriptionOverride = new ParameterSubscription(Guid.NewGuid(), this.assembler.Cache, this.uri) { Owner = this.activeDomain };
            subscriptionOverride.ValueSet.Add(new ParameterSubscriptionValueSet(Guid.NewGuid(), this.assembler.Cache, this.uri) { SubscribedValueSet = valueSet });
            this.parameter6Override.ParameterSubscription.Add(subscriptionOverride);

            revision.SetValue(this.elementUsage1, 1);
            CDPMessageBus.Current.SendObjectChangeEvent(this.elementUsage1, EventKind.Updated);

            Assert.AreEqual(2, row.ContainedRows.Count);
            var subscriptionOverrideRow = row.ContainedRows.SingleOrDefault(x => x.Thing == subscriptionOverride);
            subscriptionRow = row.ContainedRows.SingleOrDefault(x => x.Thing == subscription);

            Assert.IsNotNull(subscriptionRow);
            Assert.IsNotNull(subscriptionOverrideRow);

            // removes the subscriptions
            this.parameter6Override.ParameterSubscription.Clear();
            revision.SetValue(this.elementUsage1, 2);
            
            CDPMessageBus.Current.SendObjectChangeEvent(this.elementUsage1, EventKind.Updated);
            Assert.AreEqual(2, row.ContainedRows.Count);
            overrideRow = row.ContainedRows.SingleOrDefault(x => x.Thing == this.parameter6Override);

            Assert.IsNotNull(overrideRow);
            
            this.parameter1.ParameterSubscription.Clear();
            revision.SetValue(this.elementDefinitionForUsage1, 2);
            CDPMessageBus.Current.SendObjectChangeEvent(this.elementDefinitionForUsage1, EventKind.Updated);
            Assert.AreEqual(2, row.ContainedRows.Count);
            parameterRow = row.ContainedRows.SingleOrDefault(x => x.Thing == this.parameter1);
            Assert.IsNotNull(parameterRow);
        }

        [Test]
        public void VerifyThatExcludingOptionsPopulatesWorks()
        {
            this.elementDefinition.ContainedElement.Add(this.elementUsage1);
            // ***************************************

            var row = new ElementUsageRowViewModel(this.elementUsage1, this.activeDomain, this.session.Object, null, this.obfuscationService.Object);
            var container = new TestMessageBusHandlerContainerViewModel();
            var row2 = new ElementUsageRowViewModel(this.elementUsage1, this.activeDomain, this.session.Object, container, this.obfuscationService.Object);

            Assert.AreEqual(2, row.AllOptions.Count);
            Assert.AreEqual(2, row2.AllOptions.Count);
            Assert.AreEqual(0, row.ExcludedOptions.Count);
            Assert.IsTrue(row.HasExcludes.HasValue);
            Assert.IsFalse(row.HasExcludes.Value);
            Assert.AreEqual("This ElementUsage is used in all options.", row.OptionToolTip);

            var newOption = new Option(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "3" };
            this.iteration.Option.Add(newOption);

            CDPMessageBus.Current.SendObjectChangeEvent(newOption, EventKind.Added);
            Assert.AreEqual(3, row.AllOptions.Count);
            Assert.AreEqual(3, row2.AllOptions.Count);

            row.SelectedOptions = new ReactiveList<Option> {this.option1, newOption};
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Exactly(1));
            row.Thing.ExcludeOption = new List<Option>{this.option2};

            Assert.AreEqual(1, row.ExcludedOptions.Count);
            Assert.IsTrue(row.HasExcludes.HasValue);
            Assert.IsTrue(row.HasExcludes.Value);
            Assert.AreSame(this.option2, row.ExcludedOptions.Single());

            row.SelectedOptions = new ReactiveList<Option>();
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Exactly(2));
            row.Thing.ExcludeOption = new List<Option> { this.option2, this.option1, newOption };

            row.SelectedOptions = new ReactiveList<Option> {newOption};
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Exactly(3));
            row.Thing.ExcludeOption = new List<Option> { this.option1, this.option2 };

            // re-ordering
            row.AllOptions = new ReactiveList<Option>{this.option2, newOption, this.option1};

            row.SelectedOptions = new ReactiveList<Option> {newOption};
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Exactly(3));

            Assert.AreEqual(2, row.ExcludedOptions.Count);
            Assert.IsTrue(row.HasExcludes.HasValue);
        }

        [Test]
        public void VerifyThatDragOverParameterGroupInUSageIsForbidden()
        {
            this.elementDefinition.ContainedElement.Add(this.elementUsage1);
            this.elementDefinitionForUsage1.ParameterGroup.Add(this.parameterGroup1ForUsage1);
            // ***************************************

            var row = new ElementUsageRowViewModel(this.elementUsage1, this.activeDomain, this.session.Object, null, this.obfuscationService.Object);

            var simpleQuantityKind = new SimpleQuantityKind(Guid.NewGuid(), null, null);
            var ratioScale = new RatioScale(Guid.NewGuid(), null, null);
            simpleQuantityKind.DefaultScale = ratioScale;
            var payload = new Tuple<ParameterType, MeasurementScale>(simpleQuantityKind, ratioScale);
            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(payload);
            dropInfo.SetupProperty(x => x.Effects);

            var groupRow = row.ContainedRows.OfType<ParameterGroupRowViewModel>().Single();
            groupRow.DragOver(dropInfo.Object);

            Assert.AreEqual(dropInfo.Object.Effects, DragDropEffects.None);
        }
    }
}