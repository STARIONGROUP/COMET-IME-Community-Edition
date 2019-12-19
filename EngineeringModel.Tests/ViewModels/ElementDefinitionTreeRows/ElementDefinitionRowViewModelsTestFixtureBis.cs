// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionRowViewModelsTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.ViewModels.ElementDefinitionTreeRows
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    using CDP4EngineeringModel.Utilities;
    using CDP4EngineeringModel.ViewModels;

    using Microsoft.Practices.ServiceLocation;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ElementDefinitionRowViewModelsTestFixtureBis
    {
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialognavigationService;
        private Mock<IThingCreator> thingCreator;
        private readonly Uri uri = new Uri("http://test.com");
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Mock<IServiceLocator> serviceLocator;

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
        private Mock<ISession> session;
        private ParameterOverride parameterOverrideCompound;
        private ParameterSubscription parameterSubscriptionCompound;

        private ParameterOverride parameterOverrideOption;
        private ParameterSubscription parameterSubscriptionOption;

        private Parameter parameterForStates;

        private EngineeringModel model;
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

        private SiteDirectory sitedir;
        private EngineeringModelSetup modelsetup;
        private IterationSetup iterationsetup;
        private SiteReferenceDataLibrary srdl;
        private ModelReferenceDataLibrary mrdl;

        [SetUp]
        public void SetUp()
        {
            this.permissionService = new Mock<IPermissionService>();
            this.thingDialognavigationService = new Mock<IThingDialogNavigationService>();
            this.thingCreator = new Mock<IThingCreator>();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.serviceLocator = new Mock<IServiceLocator>();

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingCreator>())
                .Returns(this.thingCreator.Object);

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            this.iterationsetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri);
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri);
            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) {RequiredRdl = this.srdl};

            this.modelsetup.RequiredRdl.Add(this.mrdl);
            this.modelsetup.IterationSetup.Add(this.iterationsetup);
            this.sitedir.Model.Add(this.modelsetup);
            this.sitedir.SiteReferenceDataLibrary.Add(this.srdl);

            this.option1 = new Option(Guid.NewGuid(), this.cache, this.uri);
            this.option2 = new Option(Guid.NewGuid(), this.cache, this.uri);

            this.stateList = new ActualFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            this.state1 = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri);
            this.state2 = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri);

            this.posStateList = new PossibleFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            this.posStateList.PossibleState.Add(this.state1);
            this.posStateList.PossibleState.Add(this.state2);
            this.posStateList.DefaultState = this.state1;

            this.stateList.ActualState.Add(new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri)
            {
                PossibleState = new List<PossibleFiniteState> { this.state1 },
                Kind = ActualFiniteStateKind.MANDATORY
            });

            this.stateList.ActualState.Add(new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri)
            {
                PossibleState = new List<PossibleFiniteState> { this.state2 },
                Kind = ActualFiniteStateKind.FORBIDDEN
            });

            this.activeDomain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri);
            this.someotherDomain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri);
            this.session = new Mock<ISession>();
            this.qqParamType = new SimpleQuantityKind(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "PTName",
                ShortName = "PTShortName"
            };

            // Array parameter type with components
            this.apType = new ArrayParameterType(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "APTName",
                ShortName = "APTShortName"
            };

            this.apType.Component.Add(new ParameterTypeComponent(Guid.NewGuid(), this.cache, this.uri)
            {
                Iid = Guid.NewGuid(),
                ParameterType = this.qqParamType
            });

            this.apType.Component.Add(new ParameterTypeComponent(Guid.NewGuid(), this.cache, this.uri)
            {
                Iid = Guid.NewGuid(),
                ParameterType = this.qqParamType
            });

            // compound parameter type with components
            this.cptType = new CompoundParameterType(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "APTName",
                ShortName = "APTShortName"
            };

            this.cptType.Component.Add(new ParameterTypeComponent(Guid.NewGuid(), this.cache, this.uri)
            {
                Iid = Guid.NewGuid(),
                ParameterType = this.qqParamType
            });

            this.cptType.Component.Add(new ParameterTypeComponent(Guid.NewGuid(), this.cache, this.uri)
            {
                Iid = Guid.NewGuid(),
                ParameterType = this.qqParamType
            });

            this.elementDefinition = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.activeDomain
            };

            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri) {EngineeringModelSetup = this.modelsetup};
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri) {IterationSetup = this.iterationsetup};
            var person = new Person(Guid.NewGuid(), null, null) { GivenName = "test", Surname = "test" };
            var participant = new Participant(Guid.NewGuid(), null, null) { Person = person, SelectedDomain = this.activeDomain };
            this.session.Setup(x => x.ActivePerson).Returns(person);
            this.modelsetup.Participant.Add(participant);
            this.model.Iteration.Add(this.iteration);
            this.iteration.Element.Add(this.elementDefinition);

            this.iteration.Option.Add(this.option1);
            this.iteration.Option.Add(this.option2);

            this.elementDefinitionForUsage1 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.someotherDomain
            };

            this.elementDefinitionForUsage2 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.someotherDomain
            };

            this.elementUsage1 = new ElementUsage(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.someotherDomain
            };

            this.elementUsage2 = new ElementUsage(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.someotherDomain
            };

            this.elementUsage1.ElementDefinition = this.elementDefinitionForUsage1;
            this.elementUsage2.ElementDefinition = this.elementDefinitionForUsage2;

            this.parameterGroup1 = new ParameterGroup(Guid.NewGuid(), this.cache, this.uri);
            this.parameterGroup2 = new ParameterGroup(Guid.NewGuid(), this.cache, this.uri);
            this.parameterGroup3 = new ParameterGroup(Guid.NewGuid(), this.cache, this.uri);

            this.parameterGroup1ForUsage1 = new ParameterGroup(Guid.NewGuid(), this.cache, this.uri);
            this.parameterGroup2ForUsage2 = new ParameterGroup(Guid.NewGuid(), this.cache, this.uri);
            this.parameterGroup3ForUsage1 = new ParameterGroup(Guid.NewGuid(), this.cache, this.uri);

            this.parameter1 = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = this.qqParamType,
                Owner = this.activeDomain
            };

            this.parameter2 = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = this.qqParamType,
                Owner = this.activeDomain
            };

            this.parameter3 = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = this.qqParamType,
                Owner = this.someotherDomain
            };

            this.parameter4 = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = this.qqParamType,
                Owner = this.someotherDomain
            };

            this.parameterForStates = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = this.qqParamType,
                Owner = this.someotherDomain,
                StateDependence = this.stateList
            };

            this.parameter5ForSubscription = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = this.qqParamType,
                Owner = this.someotherDomain
            };

            this.parameter6ForOverride = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = this.qqParamType,
                Owner = this.activeDomain
            };

            this.parameter6Override = new ParameterOverride(Guid.NewGuid(), this.cache, this.uri)
            {
                Parameter = this.parameter6ForOverride,
                Owner = this.activeDomain
            };

            this.parameterArray = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = this.apType,
                Owner = this.someotherDomain
            };

            this.parameterCompound = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = this.cptType,
                Owner = this.someotherDomain
            };

            this.parameterCompoundForSubscription = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = this.cptType,
                Owner = this.someotherDomain
            };

            this.parameterSubscriptionCompound = new ParameterSubscription(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.activeDomain
            };

            this.parameterCompoundForSubscription.ParameterSubscription.Add(this.parameterSubscriptionCompound);

            this.parameterForOptions = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = this.cptType,
                Owner = this.someotherDomain,
                IsOptionDependent = true
            };

            this.elementDefinition.ParameterGroup.Add(this.parameterGroup1);
            this.elementDefinition.ParameterGroup.Add(this.parameterGroup2);
            this.elementDefinition.ParameterGroup.Add(this.parameterGroup3);

            this.elementDefinitionForUsage1.ParameterGroup.Add(this.parameterGroup1ForUsage1);
            this.elementDefinitionForUsage2.ParameterGroup.Add(this.parameterGroup2ForUsage2);
            this.elementDefinitionForUsage1.ParameterGroup.Add(this.parameterGroup3ForUsage1);

            this.iteration.Element.Add(elementDefinitionForUsage1);
            this.iteration.Element.Add(elementDefinitionForUsage2);

            this.parameterGroup3.ContainingGroup = this.parameterGroup1;
            this.parameterGroup3ForUsage1.ContainingGroup = this.parameterGroup1ForUsage1;

            this.parameter4.Group = this.parameterGroup3;
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatGroupAreCorrectlyHandled()
        {
            var revision = typeof (ElementDefinition).GetProperty("RevisionNumber");

            var vm = new ElementDefinitionRowViewModel(this.elementDefinition, this.activeDomain, this.session.Object, null);

            var groups = vm.ContainedRows.OfType<ParameterGroupRowViewModel>().ToList();
            var group1Row = groups.Single(x => x.Thing == this.parameterGroup1);
            var group2Row = groups.Single(x => x.Thing == this.parameterGroup2);

            Assert.AreEqual(2, groups.Count);
            Assert.AreEqual(1, group1Row.ContainedRows.Count);

            // move group3 to group2
            this.parameterGroup3.ContainingGroup = this.parameterGroup2;
            revision.SetValue(this.elementDefinition, 1);
            CDPMessageBus.Current.SendObjectChangeEvent(this.elementDefinition, EventKind.Updated);

            groups = vm.ContainedRows.OfType<ParameterGroupRowViewModel>().ToList();
            Assert.AreEqual(2, groups.Count);
            Assert.AreEqual(1, group2Row.ContainedRows.Count);
            Assert.IsEmpty(group1Row.ContainedRows);

            // move group3 to root
            this.parameterGroup3.ContainingGroup = null;
            revision.SetValue(this.elementDefinition, 2);
            CDPMessageBus.Current.SendObjectChangeEvent(this.elementDefinition, EventKind.Updated);

            groups = vm.ContainedRows.OfType<ParameterGroupRowViewModel>().ToList();
            var group3Row = groups.Single(x => x.Thing == this.parameterGroup3);

            Assert.AreEqual(3, groups.Count);
            Assert.IsEmpty(group2Row.ContainedRows);
            Assert.IsEmpty(group1Row.ContainedRows);

            // move group1 to group3
            this.parameterGroup1.ContainingGroup = this.parameterGroup3;
            revision.SetValue(this.elementDefinition, 3);
            CDPMessageBus.Current.SendObjectChangeEvent(this.elementDefinition, EventKind.Updated);

            groups = vm.ContainedRows.OfType<ParameterGroupRowViewModel>().ToList();
            Assert.AreEqual(2, groups.Count);
            Assert.AreEqual(1, group3Row.ContainedRows.Count);
        }

        [Test]
        public void VerifyThatParametersArePlacedCorrectly()
        {
            var revision = typeof(ElementDefinition).GetProperty("RevisionNumber");

            // Test input
            var valueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri);
            var manualSet = new ValueArray<string>(new List<string> { "manual" });
            var referenceSet = new ValueArray<string>(new List<string> { "ref" });
            var computedSet = new ValueArray<string>(new List<string> { "computed" });
            var publishedSet = new ValueArray<string>(new List<string> { "published" });
            valueSet.Manual = manualSet;
            valueSet.Reference = referenceSet;
            valueSet.Computed = computedSet;
            valueSet.Published = publishedSet;

            this.parameter1.ValueSet.Add(valueSet);
            this.elementDefinition.Parameter.Add(this.parameter1);
            // **********

            var vm = new ElementDefinitionRowViewModel(this.elementDefinition, this.activeDomain, this.session.Object, null);
            var group1Row = vm.ContainedRows.Single(x => x.Thing == this.parameterGroup1);
            var group3Row = group1Row.ContainedRows.Single();

            Assert.AreEqual(3, vm.ContainedRows.Count);

            // move parameter to group1
            revision.SetValue(this.parameter1, 1);
            this.parameter1.Group = this.parameterGroup1;
            CDPMessageBus.Current.SendObjectChangeEvent(this.parameter1, EventKind.Updated);
            Assert.AreEqual(2, vm.ContainedRows.Count);
            Assert.AreEqual(2, group1Row.ContainedRows.Count);

            // move parameter to group3
            revision.SetValue(this.parameter1, 2);
            this.parameter1.Group = this.parameterGroup3;
            CDPMessageBus.Current.SendObjectChangeEvent(this.parameter1, EventKind.Updated);
            Assert.AreEqual(2, vm.ContainedRows.Count);
            Assert.AreEqual(1, group1Row.ContainedRows.Count);
            Assert.AreEqual(1, group3Row.ContainedRows.Count);

            // move parameter to root
            revision.SetValue(this.parameter1, 3);
            this.parameter1.Group = null;
            CDPMessageBus.Current.SendObjectChangeEvent(this.parameter1, EventKind.Updated);
            Assert.AreEqual(3, vm.ContainedRows.Count);
            Assert.AreEqual(1, group1Row.ContainedRows.Count);
            Assert.AreEqual(0, group3Row.ContainedRows.Count);
        }

        [Test]
        public void VerifyThatElementUsagesCanBeAddedOrRemoved()
        {
            var revision = typeof(ElementDefinition).GetProperty("RevisionNumber");
            var vm = new ElementDefinitionRowViewModel(this.elementDefinition, this.activeDomain, this.session.Object, null);

            // add new usage
            this.elementDefinition.ContainedElement.Add(this.elementUsage1);
            revision.SetValue(this.elementDefinition, 1);

            CDPMessageBus.Current.SendObjectChangeEvent(this.elementDefinition, EventKind.Updated);
            var usagesRow = vm.ContainedRows.OfType<ElementUsageRowViewModel>().ToList();

            Assert.IsNotEmpty(usagesRow);

            this.elementDefinition.ContainedElement.Clear();
            revision.SetValue(this.elementDefinition, 2);

            CDPMessageBus.Current.SendObjectChangeEvent(this.elementDefinition, EventKind.Updated);
            usagesRow = vm.ContainedRows.OfType<ElementUsageRowViewModel>().ToList();
            Assert.IsEmpty(usagesRow);
        }

        [Test]
        public void VerifyThatParameterSubscriptionCanBeAdded()
        {
            var revision = typeof(ElementDefinition).GetProperty("RevisionNumber");

            // Test input
            var valueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri);
            var manualSet = new ValueArray<string>(new List<string> { "manual" });
            var referenceSet = new ValueArray<string>(new List<string> { "ref" });
            var computedSet = new ValueArray<string>(new List<string> { "computed" });
            var publishedSet = new ValueArray<string>(new List<string> { "published" });
            valueSet.Manual = manualSet;
            valueSet.Reference = referenceSet;
            valueSet.Computed = computedSet;
            valueSet.Published = publishedSet;

            this.parameter5ForSubscription.ValueSet.Add(valueSet);

            var subscription = new ParameterSubscription(Guid.NewGuid(), this.cache, this.uri) {Owner = this.activeDomain};
            subscription.ValueSet.Add(new ParameterSubscriptionValueSet(Guid.NewGuid(), this.cache, this.uri){SubscribedValueSet = valueSet});

            this.parameter5ForSubscription.ParameterSubscription.Add(subscription);

            this.elementDefinition.Parameter.Add(this.parameter5ForSubscription);
            this.elementDefinition.ParameterGroup.Clear();
            // **********

            // init the tested class
            var vm = new ElementDefinitionRowViewModel(this.elementDefinition, this.activeDomain, this.session.Object, null);

            // the subscription should replace the parameter
            var subscriptionRow = vm.ContainedRows.Single();
            Assert.IsTrue(subscriptionRow is ParameterSubscriptionRowViewModel);

            // remove the subscription
            this.parameter5ForSubscription.ParameterSubscription.Clear();
            revision.SetValue(this.elementDefinition, 1);

            CDPMessageBus.Current.SendObjectChangeEvent(this.elementDefinition, EventKind.Updated);
            Assert.AreEqual(1, vm.ContainedRows.Count);

            var paramRow = vm.ContainedRows.Single();
            Assert.AreSame(this.parameter5ForSubscription, paramRow.Thing);
        }

        [Test]
        public void VerifyThatExistingParameterTypeDragOverSetsNoneEffectNoPermission()
        {
            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), null, null);

            elementDefinition.Owner = domainOfExpertise;
            var parameterGroup = new ParameterGroup(Guid.NewGuid(), null, null);
            elementDefinition.ParameterGroup.Add(parameterGroup);

            var row = new ElementDefinitionRowViewModel(this.elementDefinition, this.activeDomain, this.session.Object, null);

            var simpleQuantityKind = new SimpleQuantityKind(Guid.NewGuid(), null, null);
            var ratioScale = new RatioScale(Guid.NewGuid(), null, null);
            simpleQuantityKind.DefaultScale = ratioScale;

            this.srdl.ParameterType.Add(simpleQuantityKind);

            var payload = new Tuple<ParameterType, MeasurementScale>(simpleQuantityKind, ratioScale);
            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(payload);
            dropInfo.SetupProperty(x => x.Effects);

            row.DragOver(dropInfo.Object);

            Assert.AreEqual(DragDropEffects.None, dropInfo.Object.Effects);
        }

        [Test]
        public void VerifyThatExistingParameterTypeDragOverSetsNoneEffectParameterExists()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), null, null);

            elementDefinition.Owner = domainOfExpertise;
            var parameterGroup = new ParameterGroup(Guid.NewGuid(), null, null);
            elementDefinition.ParameterGroup.Add(parameterGroup);

            var row = new ElementDefinitionRowViewModel(this.elementDefinition, this.activeDomain, this.session.Object, null);

            var simpleQuantityKind = new SimpleQuantityKind(Guid.NewGuid(), null, null);
            var ratioScale = new RatioScale(Guid.NewGuid(), null, null);
            simpleQuantityKind.DefaultScale = ratioScale;

            this.srdl.ParameterType.Add(simpleQuantityKind);

            // add parameter of the type that is going to be dragged over
            var parameter = new Parameter(Guid.NewGuid(), null, null);
            parameter.ParameterType = simpleQuantityKind;
            parameter.Scale = ratioScale;
            parameter.Owner = domainOfExpertise;
            elementDefinition.Parameter.Add(parameter);

            var valueset = new ParameterValueSet(Guid.NewGuid(), null, null);
            parameter.ValueSet.Add(valueset);

            var payload = new Tuple<ParameterType, MeasurementScale>(simpleQuantityKind, ratioScale);
            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(payload);
            dropInfo.SetupProperty(x => x.Effects);

            row.DragOver(dropInfo.Object);

            Assert.AreEqual(DragDropEffects.None, dropInfo.Object.Effects);
        }

        [Test]
        public void VerifyThatNewParameterTypeDragOverSetsCopyEffect()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), null, null);
            elementDefinition.Owner = domainOfExpertise;
            var parameterGroup = new ParameterGroup(Guid.NewGuid(), null, null);
            elementDefinition.ParameterGroup.Add(parameterGroup);

            var row = new ElementDefinitionRowViewModel(this.elementDefinition, this.activeDomain, this.session.Object, null);

            var simpleQuantityKind = new SimpleQuantityKind(Guid.NewGuid(), null, null);
            var ratioScale = new RatioScale(Guid.NewGuid(), null, null);
            simpleQuantityKind.DefaultScale = ratioScale;

            this.srdl.ParameterType.Add(simpleQuantityKind);

            var payload = new Tuple<ParameterType, MeasurementScale>(simpleQuantityKind, ratioScale);
            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(payload);
            dropInfo.SetupProperty(x => x.Effects);

            row.DragOver(dropInfo.Object);

            Assert.AreEqual(DragDropEffects.Copy, dropInfo.Object.Effects);
        }

        [Test]
        public async Task VerifyThatParameterGetsCreatedWhenParameterTypeIsDropped()
        {
            var row = new ElementDefinitionRowViewModel(this.elementDefinition, this.activeDomain, this.session.Object, null);
            row.ThingCreator = this.thingCreator.Object;

            var simpleQuantityKind = new SimpleQuantityKind(Guid.NewGuid(), null, null);
            var ratioScale = new RatioScale(Guid.NewGuid(), null, null);
            simpleQuantityKind.DefaultScale = ratioScale;

            var payload = new Tuple<ParameterType, MeasurementScale>(simpleQuantityKind, ratioScale);
            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(payload);
            dropInfo.SetupProperty(x => x.Effects);

            await row.Drop(dropInfo.Object);

            this.thingCreator.Verify(x => x.CreateParameter(this.elementDefinition, null, simpleQuantityKind, ratioScale, this.activeDomain, this.session.Object));
        }

        [Test]
        public void VerifyThatStartDragWorks()
        {
            var row1 = new ElementDefinitionRowViewModel(this.elementDefinitionForUsage1, this.activeDomain, this.session.Object, null);

            var draginfo = new Mock<IDragInfo>();
            row1.StartDrag(draginfo.Object);

            draginfo.VerifySet(x => x.Payload = this.elementDefinitionForUsage1);
        }

        [Test]
        public void VerifyThatDragOverWorksWithElementDefinition()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            var row = new ElementDefinitionRowViewModel(this.elementDefinition, this.activeDomain, this.session.Object, null);
            
            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(this.elementDefinitionForUsage1);

            row.DragOver(dropinfo.Object);

            dropinfo.VerifySet(x => x.Effects = DragDropEffects.Copy);
        }

        [Test]
        public void Verify_that_dragover_sets_move_for_template_ElementDefenition()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            var row = new ElementDefinitionRowViewModel(this.elementDefinition, this.activeDomain, this.session.Object, null);

            var payloadElementDefenition = new ElementDefinition(Guid.Empty, null, null);
            
            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(payloadElementDefenition);

            row.DragOver(dropInfo.Object);

            dropInfo.VerifySet(x => x.Effects = DragDropEffects.Move);
        }

        [Test]
        public void VerifyThatDragOverWorksWithElementDefinition2()
        {
            var row = new ElementDefinitionRowViewModel(this.elementDefinitionForUsage1, this.activeDomain, this.session.Object, null);

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(this.elementDefinitionForUsage1);

            row.DragOver(dropinfo.Object);

            dropinfo.VerifySet(x => x.Effects = DragDropEffects.None);
        }

        [Test]
        public async Task VerifyThatDropsWorks()
        {
            var row = new ElementDefinitionRowViewModel(this.elementDefinition, this.activeDomain, this.session.Object, null);

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(this.elementDefinitionForUsage1);
            dropinfo.Setup(x => x.Effects).Returns(DragDropEffects.Copy);

            await row.Drop(dropinfo.Object);

            this.thingCreator.Verify(x => x.CreateElementUsage(It.IsAny<ElementDefinition>(), It.IsAny<ElementDefinition>(), It.IsAny<DomainOfExpertise>(), It.IsAny<ISession>()));
        }

        [Test]
        public void VerifyThatParameterTypeNotInChainRdlCannotBeDrop()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), null, null);
            elementDefinition.Owner = domainOfExpertise;
            var parameterGroup = new ParameterGroup(Guid.NewGuid(), null, null);
            elementDefinition.ParameterGroup.Add(parameterGroup);

            var row = new ElementDefinitionRowViewModel(this.elementDefinition, this.activeDomain, this.session.Object, null);

            var simpleQuantityKind = new SimpleQuantityKind(Guid.NewGuid(), null, null);
            var ratioScale = new RatioScale(Guid.NewGuid(), null, null);
            simpleQuantityKind.DefaultScale = ratioScale;

            var payload = new Tuple<ParameterType, MeasurementScale>(simpleQuantityKind, ratioScale);
            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(payload);
            dropInfo.SetupProperty(x => x.Effects);

            row.DragOver(dropInfo.Object);

            Assert.AreEqual(DragDropEffects.None, dropInfo.Object.Effects);
        }
    }
}
