// ------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------


namespace ProductTree.Tests.ProductTreeRows
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    using CDP4ProductTree.ViewModels;

    using Moq;
    using NUnit.Framework;

    [TestFixture]
    internal class ElementDefinitionRowViewModelTestFixture
    {
        private Mock<IPermissionService> permissionService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<ISession> session;
        private readonly Uri uri = new Uri("http://www.rheagroup.com");

        private SiteDirectory siteDir;
        private EngineeringModel model;
        private Iteration iteration;
        private EngineeringModelSetup modelSetup;
        private IterationSetup iterationSetup;
        private Person person;
        private Participant participant;
        private Option option;
        private ElementDefinition elementDef;
        private ElementDefinition elementDef2;
        private DomainOfExpertise domain;
        private ElementUsage elementUsage;

        [SetUp]
        public void Setup()
        {
            this.permissionService = new Mock<IPermissionService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.session = new Mock<ISession>();
            this.domain = new DomainOfExpertise(Guid.NewGuid(), null, this.uri) { Name = "domain" , ShortName = "dom"};

            this.siteDir = new SiteDirectory(Guid.NewGuid(), null, this.uri);
            this.person = new Person(Guid.NewGuid(), null, this.uri);
            this.model = new EngineeringModel(Guid.NewGuid(), null, this.uri);
            this.modelSetup = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri);
            this.iteration = new Iteration(Guid.NewGuid(), null, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), null, this.uri);
            this.participant = new Participant(Guid.NewGuid(), null, this.uri);
            this.option = new Option(Guid.NewGuid(), null, this.uri);
            this.elementDef = new ElementDefinition(Guid.NewGuid(), null, this.uri) {Owner = this.domain};

            this.elementDef2 = new ElementDefinition(Guid.NewGuid(), null, this.uri) { Owner = this.domain };
            this.elementUsage = new ElementUsage(Guid.NewGuid(), null, this.uri) { ElementDefinition = this.elementDef2, Owner = this.domain};

            this.siteDir.Person.Add(this.person);
            this.siteDir.Model.Add(this.modelSetup);
            this.modelSetup.IterationSetup.Add(this.iterationSetup);
            this.modelSetup.Participant.Add(this.participant);
            this.participant.Person = this.person;

            this.model.Iteration.Add(this.iteration);
            this.model.EngineeringModelSetup = this.modelSetup;
            this.iteration.IterationSetup = this.iterationSetup;
            this.iteration.Option.Add(this.option);
            this.iteration.TopElement = this.elementDef;
            this.iteration.Element.Add(this.elementDef);
            this.iteration.Element.Add(this.elementDef2);
            this.elementDef.ContainedElement.Add(this.elementUsage);

            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var vm = new ElementDefinitionRowViewModel(this.elementDef, this.option, this.session.Object, null);

            Assert.AreEqual("domain", vm.OwnerName);
            Assert.AreEqual("dom", vm.OwnerShortName );

            Assert.AreEqual(1, vm.ContainedRows.Count);
        }

        [Test]
        public void VerifyThatUsagesAreAddedOrRemoved()
        {
            var vm = new ElementDefinitionRowViewModel(this.elementDef, this.option, this.session.Object, null);

            var usage = new ElementUsage(Guid.NewGuid(), null, this.uri) { Owner = this.domain };
            usage.ElementDefinition = this.elementDef2;

            var revisionProperty = typeof (ElementDefinition).GetProperty("RevisionNumber");
            revisionProperty.SetValue(this.elementDef, 50);
            this.elementDef.ContainedElement.Add(usage);

            CDPMessageBus.Current.SendObjectChangeEvent(this.elementDef, EventKind.Updated);
            Assert.AreEqual(2, vm.ContainedRows.Count);
            Assert.IsTrue(vm.ContainedRows.Any(x => x.Thing == usage));

            this.elementDef.ContainedElement.Remove(usage);
            revisionProperty.SetValue(this.elementDef, 100);

            CDPMessageBus.Current.SendObjectChangeEvent(this.elementDef, EventKind.Updated);
            Assert.AreEqual(1, vm.ContainedRows.Count);
            Assert.IsFalse(vm.ContainedRows.Any(x => x.Thing == usage));
        }

        [Test]
        public void VerifyThatGroupAreAddedCorrectly()
        {
            var revisionProperty = typeof (ElementDefinition).GetProperty("RevisionNumber");

            var group1 = new ParameterGroup(Guid.NewGuid(), null, this.uri);
            var group11 = new ParameterGroup(Guid.NewGuid(), null, this.uri) { ContainingGroup = group1 };

            this.elementDef.ParameterGroup.Add(group1);
            this.elementDef.ParameterGroup.Add(group11);

            var vm = new ElementDefinitionRowViewModel(this.elementDef, this.option, this.session.Object, null);

            var group1row = vm.ContainedRows.OfType<ParameterGroupRowViewModel>().Single();
            Assert.AreSame(group1, group1row.Thing);

            var group11row = group1row.ContainedRows.OfType<ParameterGroupRowViewModel>().Single();
            Assert.AreSame(group11, group11row.Thing);

            // move group11
            group11.ContainingGroup = null;
            revisionProperty.SetValue(group11, 10);

            CDPMessageBus.Current.SendObjectChangeEvent(group11, EventKind.Updated);
            Assert.AreEqual(2, vm.ContainedRows.OfType<ParameterGroupRowViewModel>().Count());
            Assert.AreEqual(0, group1row.ContainedRows.OfType<ParameterGroupRowViewModel>().Count());

            // move group11 under group1
            group11.ContainingGroup = group1;
            revisionProperty.SetValue(group11, 20);

            CDPMessageBus.Current.SendObjectChangeEvent(group11, EventKind.Updated);
            Assert.AreEqual(1, vm.ContainedRows.OfType<ParameterGroupRowViewModel>().Count());
            Assert.AreSame(group11, group1row.ContainedRows.OfType<ParameterGroupRowViewModel>().Single().Thing);

            // add group2 and move group11 under group2
            var group2 = new ParameterGroup(Guid.NewGuid(), null, this.uri);
            group11.ContainingGroup = group2;
            this.elementDef.ParameterGroup.Add(group2);
            revisionProperty.SetValue(group11, 30);
            revisionProperty.SetValue(this.elementDef, 30);

            CDPMessageBus.Current.SendObjectChangeEvent(this.elementDef, EventKind.Updated);
            CDPMessageBus.Current.SendObjectChangeEvent(group11, EventKind.Updated);
            Assert.AreEqual(2, vm.ContainedRows.OfType<ParameterGroupRowViewModel>().Count());
            Assert.AreEqual(0, group1row.ContainedRows.OfType<ParameterGroupRowViewModel>().Count());

            var group2row = vm.ContainedRows.OfType<ParameterGroupRowViewModel>().Single(x => x.Thing == group2);
            Assert.AreEqual(1, group2row.ContainedRows.OfType<ParameterGroupRowViewModel>().Count());

            // remove group11
            this.elementDef.ParameterGroup.Remove(group11);
            revisionProperty.SetValue(this.elementDef, 40);

            CDPMessageBus.Current.SendObjectChangeEvent(this.elementDef, EventKind.Updated);
            Assert.AreEqual(2, vm.ContainedRows.OfType<ParameterGroupRowViewModel>().Count());
            Assert.AreEqual(0, group1row.ContainedRows.OfType<ParameterGroupRowViewModel>().Count());
            Assert.AreEqual(0, group2row.ContainedRows.OfType<ParameterGroupRowViewModel>().Count());
        }

        [Test]
        public void VerifyThatParametersAreCorrectlyHandled()
        {
            var revisionProperty = typeof(ElementDefinition).GetProperty("RevisionNumber");

            var group1 = new ParameterGroup(Guid.NewGuid(), null, this.uri);
            var group11 = new ParameterGroup(Guid.NewGuid(), null, this.uri) { ContainingGroup = group1 };

            this.elementDef.ParameterGroup.Add(group1);
            this.elementDef.ParameterGroup.Add(group11);

            var type1 = new EnumerationParameterType(Guid.NewGuid(), null, this.uri) {Name = "type1"};
            var parameter1 = new Parameter(Guid.NewGuid(), null, this.uri) {ParameterType = type1, Owner = this.domain};
            var valueset = new ParameterValueSet(Guid.NewGuid(), null, this.uri);
            valueset.Published = new ValueArray<string>(new List<string>{"1"});
            valueset.Manual = new ValueArray<string>(new List<string> { "1" });
            valueset.ValueSwitch = ParameterSwitchKind.MANUAL;
            parameter1.ValueSet.Add(valueset);

            this.elementDef.Parameter.Add(parameter1);

            var vm = new ElementDefinitionRowViewModel(this.elementDef, this.option, this.session.Object, null);

            var param1row = vm.ContainedRows.OfType<ParameterRowViewModel>().Single();
            Assert.AreSame(parameter1, param1row.Thing);
        }

        [Test]
        public void VerifyThatOptionDependencyIsHandled()
        {
            var vm = new ElementDefinitionRowViewModel(this.elementDef, this.option, this.session.Object, null);
            Assert.IsTrue(vm.ContainedRows.Select(x => x.Thing).Contains(this.elementUsage));

            var revisionProperty = typeof(ElementDefinition).GetProperty("RevisionNumber");
            revisionProperty.SetValue(this.elementUsage, 20);

            this.elementUsage.ExcludeOption.Add(this.option);

            CDPMessageBus.Current.SendObjectChangeEvent(this.elementUsage, EventKind.Updated);
            Assert.IsFalse(vm.ContainedRows.Select(x => x.Thing).Contains(this.elementUsage));

            revisionProperty.SetValue(this.elementUsage, 30);
            this.elementUsage.ExcludeOption.Clear();

            CDPMessageBus.Current.SendObjectChangeEvent(this.elementUsage, EventKind.Updated);
            Assert.IsTrue(vm.ContainedRows.Select(x => x.Thing).Contains(this.elementUsage));
        }
    }
}