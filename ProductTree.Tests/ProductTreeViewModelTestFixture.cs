// ------------------------------------------------------------------------------------------------
// <copyright file="ProductTreeViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------


namespace ProductTree.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Dal.Operations;
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
    using ReactiveUI;

    [TestFixture]
    internal class ProductTreeViewModelTestFixture
    {
        private Mock<IPermissionService> permissionService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<ISession> session;
        private readonly Uri uri = new Uri("http://test.com");
        private SiteDirectory siteDir;
        private EngineeringModel model;
        private Iteration iteration;
        private EngineeringModelSetup modelSetup;
        private IterationSetup iterationSetup;
        private Person person;
        private Participant participant;
        private Option option;
        private ElementDefinition elementDef;
        private DomainOfExpertise domain;
        private Assembler assembler;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.permissionService = new Mock<IPermissionService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.session = new Mock<ISession>();
            this.assembler = new Assembler(this.uri);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri) { GivenName = "John", Surname = "Doe" };
            this.model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.modelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.modelSetup.Name = "model name";
            this.modelSetup.ShortName = "modelshortname";

            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.participant = new Participant(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.option = new Option(Guid.NewGuid(), this.assembler.Cache, this.uri)
                              {
                                  Name = "option name",
                                  ShortName = "optionshortname"
                              };

            this.elementDef = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri)
                              {
                                  Name = "domain",
                                  ShortName = "domainshortname"
                              };

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

            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<IDialogViewModel>())).Returns(new BaseDialogResult(true));
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                {this.iteration,new Tuple<DomainOfExpertise, Participant>(this.domain, null)}
            });
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var vm = new ProductTreeViewModel(this.option, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null);
            
            Assert.That(vm.Caption, Is.Not.Null.Or.Empty);
            Assert.That(vm.ToolTip, Is.Not.Null.Or.Empty);

            Assert.AreEqual("model name", vm.CurrentModel);
            Assert.AreEqual("option name", vm.CurrentOption);
            Assert.AreEqual("domain [domainshortname]", vm.DomainOfExpertise);
            Assert.AreEqual("John Doe", vm.Person);
            Assert.AreEqual(0, vm.CurrentIteration);

            Assert.IsNotNull(vm.ActiveParticipant);
            Assert.AreEqual(1, vm.TopElement.Count);
        }

        [Test]
        public void VerifyThatUpdateOptionEventWorks()
        {
            var vm = new ProductTreeViewModel(this.option, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null);

            Assert.AreEqual("option name", vm.CurrentOption);

            var revisionNumber = typeof(Option).GetProperty("RevisionNumber");
            revisionNumber.SetValue(this.option, 50);
            this.option.Name = "blablabla";

            CDPMessageBus.Current.SendObjectChangeEvent(this.option, EventKind.Updated);
            Assert.AreEqual("blablabla", vm.CurrentOption);
        }

        [Test]
        public void VerifyThatUpdateIterationSetupEventIsHandled()
        {
            var vm = new ProductTreeViewModel(this.option, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null);

            Assert.AreEqual(0, vm.CurrentIteration);

            var revisionNumber = typeof(IterationSetup).GetProperty("RevisionNumber");
            revisionNumber.SetValue(this.iterationSetup, 50);

            var iterationNumber = typeof(IterationSetup).GetProperty("IterationNumber");
            iterationNumber.SetValue(this.iterationSetup, 1);

            CDPMessageBus.Current.SendObjectChangeEvent(this.iterationSetup, EventKind.Updated);
            Assert.AreEqual(1, vm.CurrentIteration);
        }

        [Test]
        public void VerifyThatUpdateDomainEventIsHandled()
        {
            var vm = new ProductTreeViewModel(this.option, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null);
            
            var revisionNumber = typeof(DomainOfExpertise).GetProperty("RevisionNumber");
            revisionNumber.SetValue(this.domain, 50);

            this.domain.Name = "System";
            this.domain.ShortName = "SYS";

            CDPMessageBus.Current.SendObjectChangeEvent(this.domain, EventKind.Updated);
            Assert.AreEqual("System [SYS]", vm.DomainOfExpertise);
        }

        [Test]
        public void VerifyThatUpdatePersonEventIsHandled()
        {
            var vm = new ProductTreeViewModel(this.option, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null);

            var revisionNumber = typeof(Person).GetProperty("RevisionNumber");
            revisionNumber.SetValue(this.person, 50);

            this.person.GivenName = "Jane";

            CDPMessageBus.Current.SendObjectChangeEvent(this.person, EventKind.Updated);
            Assert.AreEqual("Jane Doe", vm.Person);
        }

        [Test]
        public void VerifyThatTopElementIsRemovedUponIterationUpdateWithNoTop()
        {
            var vm = new ProductTreeViewModel(this.option, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null);

            var revisionNumber = typeof(Iteration).GetProperty("RevisionNumber");
            revisionNumber.SetValue(this.iteration, 50);
            this.iteration.TopElement = null;

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            Assert.AreEqual(0, vm.TopElement.Count);
        }

        [Test]
        public void VerifyThatTopElementIsModifiedUponNewTopElement()
        {
            var vm = new ProductTreeViewModel(this.option, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null);

            var revisionNumber = typeof(Iteration).GetProperty("RevisionNumber");
            revisionNumber.SetValue(this.iteration, 50);

            var elementdef = new ElementDefinition(Guid.NewGuid(), null, this.uri);
            this.iteration.TopElement = elementdef;

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            Assert.AreSame(elementdef, vm.TopElement.Single().Thing);
        }

        [Test]
        public void VerifyExecuteCreateSubscriptionCommand()
        {
            var vm = new ProductTreeViewModel(this.option, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null);

            var revisionNumber = typeof(Iteration).GetProperty("RevisionNumber");
            revisionNumber.SetValue(this.iteration, 50);

            var elementdef = new ElementDefinition(Guid.NewGuid(), null, this.uri) { Container = this.iteration };
            var anotherDomain = new DomainOfExpertise(Guid.NewGuid(), null, this.uri) { Name = "Not owned" };
            var boolParamType = new BooleanParameterType(Guid.NewGuid(), null, this.uri);
            var parameter = new Parameter(Guid.NewGuid(), null, this.uri) { Owner = anotherDomain, Container = elementdef, ParameterType = boolParamType };
            var published = new ValueArray<string>(new List<string> { "published" });
            var paramValueSet = new ParameterValueSet(Guid.NewGuid(), null, this.uri) { Published = published, Manual = published, Computed = published, ValueSwitch = ParameterSwitchKind.COMPUTED};
            parameter.ValueSet.Add(paramValueSet);
            elementdef.Parameter.Add(parameter);
            this.iteration.TopElement = elementdef;

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            Assert.AreSame(elementdef, vm.TopElement.Single().Thing);
            Assert.AreEqual(1, vm.TopElement.Single().ContainedRows.Count);
            var paramRow = vm.TopElement.Single().ContainedRows.First() as ParameterOrOverrideBaseRowViewModel;
            Assert.NotNull(paramRow);
            vm.SelectedThing = paramRow;


            Assert.IsTrue(vm.CreateSubscriptionCommand.CanExecute(null));
            Assert.AreEqual(0, paramRow.Thing.ParameterSubscription.Count);

            vm.SelectedThing = null;
            vm.CreateSubscriptionCommand.Execute(null);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Never);

            vm.SelectedThing = vm.TopElement.Single();
            vm.CreateSubscriptionCommand.Execute(null);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Never);

            vm.SelectedThing = paramRow;
            vm.CreateSubscriptionCommand.Execute(null);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Exactly(1));
        }

        [Test]
        public void VerifyExecuteDeleteSubscriptionCommand()
        {
            var vm = new ProductTreeViewModel(this.option, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object);

            var revisionNumber = typeof(Iteration).GetProperty("RevisionNumber");
            revisionNumber.SetValue(this.iteration, 50);

            var elementdef = new ElementDefinition(Guid.NewGuid(), null, this.uri) { Container = this.iteration };
            var anotherDomain = new DomainOfExpertise(Guid.NewGuid(), null, this.uri) { Name = "Not owned" };
            var boolParamType = new BooleanParameterType(Guid.NewGuid(), null, this.uri);
            var parameter = new Parameter(Guid.NewGuid(), null, this.uri) { Owner = anotherDomain, Container = elementdef, ParameterType = boolParamType };
            parameter.ParameterSubscription.Add(new ParameterSubscription(Guid.NewGuid(), null, this.uri) { Owner = this.domain });
            var published = new ValueArray<string>(new List<string> { "published" });
            var paramValueSet = new ParameterValueSet(Guid.NewGuid(), null, this.uri) { Published = published, Manual = published, Computed = published, ValueSwitch = ParameterSwitchKind.COMPUTED };
            parameter.ValueSet.Add(paramValueSet);
            elementdef.Parameter.Add(parameter);
            this.iteration.TopElement = elementdef;

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            Assert.AreSame(elementdef, vm.TopElement.Single().Thing);
            Assert.AreEqual(1, vm.TopElement.Single().ContainedRows.Count);
            var paramRow = vm.TopElement.Single().ContainedRows.First() as ParameterOrOverrideBaseRowViewModel;
            Assert.NotNull(paramRow);
            vm.SelectedThing = paramRow;

            vm.PopulateContextMenu();
            Assert.AreEqual(7, vm.ContextMenu.Count);

            Assert.IsTrue(vm.DeleteSubscriptionCommand.CanExecute(null));
            Assert.AreEqual(1, paramRow.Thing.ParameterSubscription.Count);
            vm.DeleteSubscriptionCommand.Execute(null);
        }

        [Test]
        public void VerifyThatActiveDomainIsDisplayed()
        {
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                {this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, null)}
            });

            var vm = new ProductTreeViewModel(this.option, this.session.Object, null, null, null);
            Assert.AreEqual("domain [domainshortname]", vm.DomainOfExpertise);

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                {this.iteration, new Tuple<DomainOfExpertise, Participant>(null, null)}
            });

            vm = new ProductTreeViewModel(this.option, this.session.Object, null, null, null);
            Assert.AreEqual("None", vm.DomainOfExpertise);

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());
            vm = new ProductTreeViewModel(this.option, this.session.Object, null, null, null);
            Assert.AreEqual("None", vm.DomainOfExpertise);
        }

        [Test]
        public void VerifyCreateParameterOverride()
        {
            var vm = new ProductTreeViewModel(this.option, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object);
            var revisionNumber = typeof(Iteration).GetProperty("RevisionNumber");
            revisionNumber.SetValue(this.iteration, 50);
            var elementdef = new ElementDefinition(Guid.NewGuid(), null, this.uri) { Container = this.iteration };
            var boolParamType = new BooleanParameterType(Guid.NewGuid(), null, this.uri);
            var elementUsage = new ElementUsage(Guid.NewGuid(), null, this.uri)
                                   {
                                       Container = elementdef,
                                       ElementDefinition = elementdef
                                   };
            var parameter = new Parameter(Guid.NewGuid(), null, this.uri) { Owner = this.domain, Container = elementUsage, ParameterType = boolParamType };
            elementdef.Parameter.Add(parameter);
            var published = new ValueArray<string>(new List<string> { "published" });
            var paramValueSet = new ParameterValueSet(Guid.NewGuid(), null, this.uri) { Published = published, Manual = published, Computed = published, ValueSwitch = ParameterSwitchKind.COMPUTED };
            parameter.ValueSet.Add(paramValueSet);

            var usageRow = new ElementUsageRowViewModel(elementUsage, this.option, this.session.Object, null);
            var parameterRow = new ParameterRowViewModel(parameter, this.option, this.session.Object, usageRow);

            this.iteration.TopElement = elementdef;
            vm.SelectedThing = parameterRow;

            Assert.IsTrue(vm.CreateOverrideCommand.CanExecute(null));

            vm.SelectedThing = null;
            vm.CreateOverrideCommand.Execute(null);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Never);

            vm.SelectedThing = vm.TopElement.Single();
            vm.CreateOverrideCommand.Execute(null);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Never);

            vm.SelectedThing = parameterRow;
            vm.CreateOverrideCommand.Execute(parameter);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));

            vm.PopulateContextMenu();
            Assert.AreEqual(6, vm.ContextMenu.Count);
        }

        [Test]
        public void VerifyToggleNamesAndShortNames()
        {
            var vm = new ProductTreeViewModel(this.option, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object);
            Assert.IsFalse(vm.IsDisplayShortNamesOn);
            Assert.IsTrue(vm.ToggleUsageNamesCommand.CanExecute(null));

            vm.ToggleUsageNamesCommand.Execute(null);
            Assert.IsTrue(vm.IsDisplayShortNamesOn);
            vm.ToggleUsageNamesCommand.Execute(null);
            Assert.IsFalse(vm.IsDisplayShortNamesOn);
            Assert.DoesNotThrow(vm.Dispose);
        }


        [Test]
        public void VerifyContextMenuPopulation()
        {
            var vm = new ProductTreeViewModel(this.option, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object);
            Assert.AreEqual(0, vm.ContextMenu.Count);
            Assert.IsNull(vm.SelectedThing);
            vm.PopulateContextMenu();
            Assert.AreEqual(0, vm.ContextMenu.Count);

            var elemDef = vm.TopElement.Single();
            vm.SelectedThing = elemDef;
            vm.PopulateContextMenu();
            Assert.AreEqual(5, vm.ContextMenu.Count);
        }
    }
}