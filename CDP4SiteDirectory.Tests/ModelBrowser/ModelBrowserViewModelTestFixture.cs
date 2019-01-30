// -------------------------------------------------------------------------------------------------
// <copyright file="ModelBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests
{
    using System;
    using System.Linq;
    using System.Reflection;
    using CDP4Common.CommonData;
    using CDP4Common.Types;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    using CDP4SiteDirectory.ViewModels;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ModelBrowserViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPanelNavigationService> navigationService;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private SiteDirectory siteDirectory;
        private Uri uri;        
        private Person person;
        private PropertyInfo revPropertyInfo;
        private Assembler assembler;

        [SetUp]
        public void SetUp()
        {
            this.revPropertyInfo = typeof (SiteDirectory).GetProperty("RevisionNumber");
            this.uri = new Uri("http://www.rheagroup.com");
            this.assembler = new Assembler(this.uri);

            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri) { GivenName = "John", Surname = "Doe" };
            
            
            this.navigationService = new Mock<IPanelNavigationService>();            
            this.permissionService = new Mock<IPermissionService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(siteDirectory);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            this.session.Setup(x => x.Assembler).Returns(this.assembler);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var viewmodel = new ModelBrowserViewModel(this.session.Object, this.session.Object.RetrieveSiteDirectory(), null, this.navigationService.Object, null, null);
            
            Assert.That(viewmodel.ToolTip, Is.Not.Null.Or.Empty);
            Assert.That(viewmodel.Caption, Is.Not.Null.Or.Empty);
        }

        [Test]
        public void VerifyThatSelectedModelSetupAreDisplayedInProperty()
        {
            var viewmodel = new ModelBrowserViewModel(this.session.Object, this.session.Object.RetrieveSiteDirectory(), null, this.navigationService.Object, null, null);

            var model = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri);
            this.siteDirectory.Model.Add(model);
            revPropertyInfo.SetValue(this.siteDirectory, 50);

            CDPMessageBus.Current.SendObjectChangeEvent(this.siteDirectory, EventKind.Updated);

            viewmodel.SelectedThing = viewmodel.ModelSetup.First();
            this.navigationService.Verify(x => x.Open(It.IsAny<Thing>(), this.session.Object));

            Assert.AreEqual("Engineering Model Setup", viewmodel.SelectedThingClassKindString);
        }

        [Test]
        public void VerifyThatSelectedIterationSetupAreDisplayedInProperty()
        {
            var viewmodel = new ModelBrowserViewModel(this.session.Object, this.session.Object.RetrieveSiteDirectory(), null, this.navigationService.Object, null, null);

            var model = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri);

            var iteration = new IterationSetup(Guid.NewGuid(), null, this.uri);
            model.IterationSetup.Add(iteration);

            this.siteDirectory.Model.Add(model);
            revPropertyInfo.SetValue(this.siteDirectory, 50);

            CDPMessageBus.Current.SendObjectChangeEvent(this.siteDirectory, EventKind.Updated);

            var modelrow = viewmodel.ModelSetup.First();
            Assert.AreEqual("Phase: Preparation Phase, Kind: Study Model", modelrow.Description);
            var iterationFolderRow = modelrow.ContainedRows[1];

            viewmodel.SelectedThing = iterationFolderRow.ContainedRows.First();
            this.navigationService.Verify(x => x.Open(It.IsAny<Thing>(), this.session.Object));
            Assert.AreEqual("IterationSetup", viewmodel.SelectedThing.Thing.ClassKind.ToString());
        }

        [Test]
        public void VerifyThatSelectedParticipantAreDisplayedInProperty()
        {
            var viewmodel = new ModelBrowserViewModel(this.session.Object, this.session.Object.RetrieveSiteDirectory(), null, this.navigationService.Object, null, null);

            var model = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri);

            var testOrganization = new Organization(Guid.NewGuid(), null, this.uri) { Name = "RHEA" };
            var testDomain = new DomainOfExpertise(Guid.NewGuid(), null, this.uri) { Name = "Thermal" };
            var participant = new Participant(Guid.NewGuid(), null, this.uri);
            participant.Domain.Add(testDomain);
            participant.Person = new Person(Guid.NewGuid(), null, this.uri) { GivenName = "blabla", Surname = "blabla", Organization = testOrganization };
            model.Participant.Add(participant);

            this.siteDirectory.Model.Add(model);
            revPropertyInfo.SetValue(this.siteDirectory, 50);

            CDPMessageBus.Current.SendObjectChangeEvent(this.siteDirectory, EventKind.Updated);

            var modelrow = viewmodel.ModelSetup.First();
            var participantFolderRow = modelrow.ContainedRows.First();

            viewmodel.SelectedThing = participantFolderRow.ContainedRows.First();
            var participantRow = participantFolderRow.ContainedRows.First() as ModelParticipantRowViewModel;
            Assert.NotNull(participantRow);
            Assert.AreEqual("DoE: Thermal, Organization: RHEA", participantRow.Description);
            this.navigationService.Verify(x => x.Open(It.IsAny<Thing>(), this.session.Object));
        }

        [Test]
        public void VerifyThatRowsAreAddedAndRemoved()
        {
            var viewmodel = new ModelBrowserViewModel(this.session.Object, this.session.Object.RetrieveSiteDirectory(), null, this.navigationService.Object, null, null);

            var model = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri);

            var participant = new Participant(Guid.NewGuid(), null, this.uri);
            participant.Person = new Person(Guid.NewGuid(), null, this.uri) { GivenName = "blabla", Surname = "blabla" };

            model.Participant.Add(participant);
            model.IterationSetup.Add(new IterationSetup());

            var domain = new DomainOfExpertise();
            model.ActiveDomain.Add(domain);

            this.siteDirectory.Model.Add(model);
            revPropertyInfo.SetValue(this.siteDirectory, 50);

            CDPMessageBus.Current.SendObjectChangeEvent(this.siteDirectory, EventKind.Updated);

            var modelrow = viewmodel.ModelSetup.First();
            var participantFolderRow = modelrow.ContainedRows.First();
            var iterationFolderRow = modelrow.ContainedRows[1];
            var domainFolderRow = modelrow.ContainedRows[2];

            Assert.AreEqual(1, participantFolderRow.ContainedRows.Count);
            Assert.AreEqual(1, iterationFolderRow.ContainedRows.Count);
            Assert.AreEqual(1, domainFolderRow.ContainedRows.Count);

            model.Participant.Clear();
            model.IterationSetup.Clear();
            model.ActiveDomain.Clear();

            var modelRevisionProperty = typeof (EngineeringModelSetup).GetProperty("RevisionNumber");
            modelRevisionProperty.SetValue(model, 5);

            CDPMessageBus.Current.SendObjectChangeEvent(model, EventKind.Updated);
            Assert.AreEqual(0, participantFolderRow.ContainedRows.Count);
            Assert.AreEqual(0, iterationFolderRow.ContainedRows.Count);
            Assert.AreEqual(0, domainFolderRow.ContainedRows.Count);

            this.siteDirectory.Model.Clear();
            revPropertyInfo.SetValue(this.siteDirectory, 52);

            CDPMessageBus.Current.SendObjectChangeEvent(this.siteDirectory, EventKind.Updated);
            Assert.AreEqual(0, viewmodel.ModelSetup.Count);
        }

        [Test]
        public void VerifyThatDisposeWorks()
        {
            var viewmodel = new ModelBrowserViewModel(this.session.Object, this.session.Object.RetrieveSiteDirectory(), null, this.navigationService.Object, null, null);

            var model = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri);

            var participant = new Participant(Guid.NewGuid(), null, this.uri);
            participant.Person = new Person(Guid.NewGuid(), null, this.uri) { GivenName = "blabla", Surname = "blabla" };

            model.Participant.Add(participant);
            model.IterationSetup.Add(new IterationSetup());

            var domain = new DomainOfExpertise();
            model.ActiveDomain.Add(domain);

            this.siteDirectory.Model.Add(model);
            this.revPropertyInfo.SetValue(this.siteDirectory, 50);
            CDPMessageBus.Current.SendObjectChangeEvent(this.siteDirectory, EventKind.Updated);
            
            Assert.AreEqual(50, viewmodel.RevisionNumber);
            viewmodel.Dispose();

            Assert.AreEqual(0, viewmodel.RevisionNumber);
            this.revPropertyInfo.SetValue(this.siteDirectory, 100);
            CDPMessageBus.Current.SendObjectChangeEvent(this.siteDirectory, EventKind.Updated);

            Assert.AreEqual(0, viewmodel.RevisionNumber);
        }

        [Test]
        public void VerifyThatMenuIsCorrectlyPopulated()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            var viewmodel = new ModelBrowserViewModel(this.session.Object, this.session.Object.RetrieveSiteDirectory(), null, this.navigationService.Object, null, null);
            var model = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri);
            model.StudyPhase = StudyPhaseKind.DESIGN_SESSION_PHASE;

            var participant = new Participant(Guid.NewGuid(), null, this.uri);
            participant.Person = new Person(Guid.NewGuid(), null, this.uri) { GivenName = "blabla", Surname = "blabla" };

            var participant1 = new Participant(Guid.NewGuid(), null, this.uri);
            participant1.Person = new Person(Guid.NewGuid(), null, this.uri) { GivenName = "Alabla1", Surname = "Alabla1" };


            model.Participant.Add(participant);
            model.Participant.Add(participant1);
            model.IterationSetup.Add(new IterationSetup());

            var domain = new DomainOfExpertise();
            model.ActiveDomain.Add(domain);

            this.siteDirectory.Model.Add(model);
            revPropertyInfo.SetValue(this.siteDirectory, 50);

            CDPMessageBus.Current.SendObjectChangeEvent(this.siteDirectory, EventKind.Updated);

            viewmodel.ComputePermission();
            Assert.IsFalse(viewmodel.CanCreateParticipant);
            Assert.IsFalse(viewmodel.CanCreateIterationSetup);

            var modelRow = viewmodel.ModelSetup.Single();
            viewmodel.SelectedThing = modelRow;

            viewmodel.ComputePermission();
            Assert.IsTrue(viewmodel.CanCreateParticipant);
            Assert.IsTrue(viewmodel.CanCreateIterationSetup);
            Assert.IsTrue(viewmodel.CanCreateEngineeringModelSetup);
            viewmodel.PopulateContextMenu();
            Assert.AreEqual(7, viewmodel.ContextMenu.Count);

            var participantFolderRow =
                modelRow.ContainedRows.OfType<FolderRowViewModel>().Single(x => x.Name == "Participants");
            var iterationFolderRow = modelRow.ContainedRows.OfType<FolderRowViewModel>().Single(x => x.Name == "Iterations");

            viewmodel.SelectedThing = participantFolderRow;
            viewmodel.ComputePermission();
            viewmodel.PopulateContextMenu();

            Assert.AreEqual(1, viewmodel.ContextMenu.Count);

            viewmodel.SelectedThing = iterationFolderRow;
            viewmodel.ComputePermission();
            viewmodel.PopulateContextMenu();

            Assert.AreEqual(1, viewmodel.ContextMenu.Count);

            var participantRow = participantFolderRow.ContainedRows.OfType<ModelParticipantRowViewModel>().Single(x => x.Name == "blabla blabla");
            viewmodel.SelectedThing = participantRow;
            viewmodel.ComputePermission();
            viewmodel.PopulateContextMenu();

            Assert.AreEqual(5, viewmodel.ContextMenu.Count);

            var iterationRow = iterationFolderRow.ContainedRows.Single();
            viewmodel.SelectedThing = iterationRow;
            viewmodel.ComputePermission();
            viewmodel.PopulateContextMenu();

            Assert.AreEqual(5, viewmodel.ContextMenu.Count);
        }

        [Test]
        public void VerifyThatForSingleParticipantDeleteMenuIsNotShown()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            var viewmodel = new ModelBrowserViewModel(this.session.Object, this.session.Object.RetrieveSiteDirectory(), null, this.navigationService.Object, null, null);
            var model = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri);
            model.StudyPhase = StudyPhaseKind.DESIGN_SESSION_PHASE;

            var participant = new Participant(Guid.NewGuid(), null, this.uri);
            participant.Person = new Person(Guid.NewGuid(), null, this.uri) { GivenName = "blabla", Surname = "blabla" };

            model.Participant.Add(participant);
            model.IterationSetup.Add(new IterationSetup());

            var domain = new DomainOfExpertise();
            model.ActiveDomain.Add(domain);

            this.siteDirectory.Model.Add(model);
            revPropertyInfo.SetValue(this.siteDirectory, 50);

            CDPMessageBus.Current.SendObjectChangeEvent(this.siteDirectory, EventKind.Updated);

            viewmodel.ComputePermission();
            Assert.IsFalse(viewmodel.CanCreateParticipant);
            Assert.IsFalse(viewmodel.CanCreateIterationSetup);

            var modelRow = viewmodel.ModelSetup.Single();
            viewmodel.SelectedThing = modelRow;

            viewmodel.ComputePermission();
            Assert.IsTrue(viewmodel.CanCreateParticipant);
            Assert.IsTrue(viewmodel.CanCreateIterationSetup);
            Assert.IsTrue(viewmodel.CanCreateEngineeringModelSetup);
            viewmodel.PopulateContextMenu();
            Assert.AreEqual(7, viewmodel.ContextMenu.Count);

            var participantFolderRow =
                modelRow.ContainedRows.OfType<FolderRowViewModel>().Single(x => x.Name == "Participants");
            var iterationFolderRow = modelRow.ContainedRows.OfType<FolderRowViewModel>().Single(x => x.Name == "Iterations");

            viewmodel.SelectedThing = participantFolderRow;
            viewmodel.ComputePermission();
            viewmodel.PopulateContextMenu();

            Assert.AreEqual(1, viewmodel.ContextMenu.Count);

            viewmodel.SelectedThing = iterationFolderRow;
            viewmodel.ComputePermission();
            viewmodel.PopulateContextMenu();

            Assert.AreEqual(1, viewmodel.ContextMenu.Count);

            var participantRow = participantFolderRow.ContainedRows.OfType<ModelParticipantRowViewModel>().Single(x => x.Name == "blabla blabla");
            viewmodel.SelectedThing = participantRow;
            viewmodel.ComputePermission();
            viewmodel.PopulateContextMenu();

            Assert.AreEqual(4, viewmodel.ContextMenu.Count);
            foreach (var conMenu in viewmodel.ContextMenu)
            {
                Assert.AreNotEqual("Delete this Participant", conMenu.Header);
            }

            var iterationRow = iterationFolderRow.ContainedRows.Single();
            viewmodel.SelectedThing = iterationRow;
            viewmodel.ComputePermission();
            viewmodel.PopulateContextMenu();

            Assert.AreEqual(5, viewmodel.ContextMenu.Count);
        }

        [Test]
        public void VerifyThatCreateParticipantCommandWorks()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            var viewmodel = new ModelBrowserViewModel(this.session.Object, this.session.Object.RetrieveSiteDirectory(), this.thingDialogNavigationService.Object, this.navigationService.Object, null, null);
            var model = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri);
            model.StudyPhase = StudyPhaseKind.DESIGN_SESSION_PHASE;

            var participant = new Participant(Guid.NewGuid(), null, this.uri);
            participant.Person = new Person(Guid.NewGuid(), null, this.uri) { GivenName = "blabla", Surname = "blabla" };

            model.Participant.Add(participant);
            model.IterationSetup.Add(new IterationSetup());

            var domain = new DomainOfExpertise();
            model.ActiveDomain.Add(domain);

            this.siteDirectory.Model.Add(model);
            revPropertyInfo.SetValue(this.siteDirectory, 50);

            CDPMessageBus.Current.SendObjectChangeEvent(this.siteDirectory, EventKind.Updated);

            var modelRow = viewmodel.ModelSetup.Single();
            viewmodel.SelectedThing = modelRow;

            viewmodel.ComputePermission();
            Assert.IsTrue(viewmodel.CanCreateParticipant);
            Assert.IsTrue(viewmodel.CanCreateIterationSetup);
            Assert.IsTrue(viewmodel.CanCreateEngineeringModelSetup);
            viewmodel.PopulateContextMenu();
            Assert.AreEqual(7, viewmodel.ContextMenu.Count);

            viewmodel.CreateParticipantCommand.Execute(null);
            this.thingDialogNavigationService.Verify(x => x.Navigate(It.IsAny<Participant>(), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, It.IsAny<EngineeringModelSetup>(), null));
        }

        [Test]
        public void VerifyThatCreateIterationCommandWorks()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            var viewmodel = new ModelBrowserViewModel(this.session.Object, this.session.Object.RetrieveSiteDirectory(), this.thingDialogNavigationService.Object, this.navigationService.Object, null, null);
            var model = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri);
            model.StudyPhase = StudyPhaseKind.DESIGN_SESSION_PHASE;

            var participant = new Participant(Guid.NewGuid(), null, this.uri);
            participant.Person = new Person(Guid.NewGuid(), null, this.uri) { GivenName = "blabla", Surname = "blabla" };

            model.Participant.Add(participant);
            model.IterationSetup.Add(new IterationSetup());

            var domain = new DomainOfExpertise();
            model.ActiveDomain.Add(domain);

            this.siteDirectory.Model.Add(model);
            revPropertyInfo.SetValue(this.siteDirectory, 50);

            CDPMessageBus.Current.SendObjectChangeEvent(this.siteDirectory, EventKind.Updated);

            var modelRow = viewmodel.ModelSetup.Single();
            var iterationFolderRow = modelRow.ContainedRows.OfType<FolderRowViewModel>().Single(x => x.Name == "Iterations");

            viewmodel.SelectedThing = iterationFolderRow;

            viewmodel.ComputePermission();
            Assert.IsTrue(viewmodel.CanCreateParticipant);
            Assert.IsTrue(viewmodel.CanCreateIterationSetup);
            Assert.IsTrue(viewmodel.CanCreateEngineeringModelSetup);
            viewmodel.PopulateContextMenu();
            Assert.AreEqual(1, viewmodel.ContextMenu.Count);

            viewmodel.CreateIterationSetupCommand.Execute(null);
            this.thingDialogNavigationService.Verify(x => x.Navigate(It.IsAny<IterationSetup>(), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, It.IsAny<EngineeringModelSetup>(), null));
        }

        [Test]
        public void VerifyThatCreateModelSetupCommandWorks()
        {
            this.assembler.Cache.TryAdd(new CacheKey(this.siteDirectory.Iid, null),
                new Lazy<Thing>(() => this.siteDirectory));

            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            var viewmodel = new ModelBrowserViewModel(this.session.Object, this.session.Object.RetrieveSiteDirectory(), this.thingDialogNavigationService.Object, this.navigationService.Object, null, null);
            var model = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri);
            model.StudyPhase = StudyPhaseKind.DESIGN_SESSION_PHASE;

            var participant = new Participant(Guid.NewGuid(), null, this.uri);
            participant.Person = new Person(Guid.NewGuid(), null, this.uri) { GivenName = "blabla", Surname = "blabla" };

            model.Participant.Add(participant);
            model.IterationSetup.Add(new IterationSetup());

            var domain = new DomainOfExpertise();
            model.ActiveDomain.Add(domain);

            this.siteDirectory.Model.Add(model);
            revPropertyInfo.SetValue(this.siteDirectory, 50);

            CDPMessageBus.Current.SendObjectChangeEvent(this.siteDirectory, EventKind.Updated);

            var modelRow = viewmodel.ModelSetup.Single();
            viewmodel.SelectedThing = modelRow;

            viewmodel.ComputePermission();
            viewmodel.PopulateContextMenu();
            Assert.AreEqual(7, viewmodel.ContextMenu.Count);

            viewmodel.CreateCommand.Execute(null);
            this.thingDialogNavigationService.Verify(x => x.Navigate(It.IsAny<EngineeringModelSetup>(), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, It.IsAny<SiteDirectory>(), null));
        }
    }
}