// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModelBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4SiteDirectory.Tests
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.Types;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Events;
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

            var selectedThingChangedRaised = false;
            CDPMessageBus.Current.Listen<SelectedThingChangedEvent>().Subscribe(_ => selectedThingChangedRaised = true);

            viewmodel.SelectedThing = viewmodel.ModelSetup.First();
            Assert.IsTrue(selectedThingChangedRaised);

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

            var selectedThingChangedRaised = false;
            CDPMessageBus.Current.Listen<SelectedThingChangedEvent>().Subscribe(_ => selectedThingChangedRaised = true);

            viewmodel.SelectedThing = iterationFolderRow.ContainedRows.First();
            Assert.IsTrue(selectedThingChangedRaised);

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

            var selectedThingChangedRaised = false;
            CDPMessageBus.Current.Listen<SelectedThingChangedEvent>().Subscribe(_ => selectedThingChangedRaised = true);

            viewmodel.SelectedThing = participantFolderRow.ContainedRows.First();
            var participantRow = participantFolderRow.ContainedRows.First() as ModelParticipantRowViewModel;
            Assert.NotNull(participantRow);
            Assert.AreEqual("Organization: RHEA", participantRow.Description);
            Assert.IsTrue(selectedThingChangedRaised);
            Assert.That(participantRow.ContainedRows.Single().Thing, Is.EqualTo(testDomain));
        }

        [Test]
        public void VerifyThatRowsAreAddedAndRemoved()
        {
            var viewmodel = new ModelBrowserViewModel(this.session.Object, this.session.Object.RetrieveSiteDirectory(), null, this.navigationService.Object, null, null);

            var model = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri);

            var domain = new DomainOfExpertise();
            model.ActiveDomain.Add(domain);

            var participant = new Participant(Guid.NewGuid(), null, this.uri);
            participant.Person = new Person(Guid.NewGuid(), null, this.uri) { GivenName = "blabla", Surname = "blabla" };
            participant.Domain.Add(domain);

            model.Participant.Add(participant);
            model.IterationSetup.Add(new IterationSetup());

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
            Assert.AreEqual(1, domainFolderRow.ContainedRows[0].ContainedRows.Count);

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

            Assert.AreEqual(2, viewmodel.ContextMenu.Count);

            viewmodel.SelectedThing = iterationFolderRow;
            viewmodel.ComputePermission();
            viewmodel.PopulateContextMenu();

            Assert.AreEqual(2, viewmodel.ContextMenu.Count);

            var participantRow = participantFolderRow.ContainedRows.OfType<ModelParticipantRowViewModel>().Single(x => x.Name == "blabla blabla");
            viewmodel.SelectedThing = participantRow;
            viewmodel.ComputePermission();
            viewmodel.PopulateContextMenu();

            Assert.AreEqual(4, viewmodel.ContextMenu.Count);

            var iterationRow = iterationFolderRow.ContainedRows.Single();
            viewmodel.SelectedThing = iterationRow;
            viewmodel.ComputePermission();
            viewmodel.PopulateContextMenu();

            Assert.AreEqual(4, viewmodel.ContextMenu.Count);
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

            Assert.AreEqual(2, viewmodel.ContextMenu.Count);

            viewmodel.SelectedThing = iterationFolderRow;
            viewmodel.ComputePermission();
            viewmodel.PopulateContextMenu();

            Assert.AreEqual(2, viewmodel.ContextMenu.Count);

            var participantRow = participantFolderRow.ContainedRows.OfType<ModelParticipantRowViewModel>().Single(x => x.Name == "blabla blabla");
            viewmodel.SelectedThing = participantRow;
            viewmodel.ComputePermission();
            viewmodel.PopulateContextMenu();

            Assert.AreEqual(3, viewmodel.ContextMenu.Count);
            foreach (var conMenu in viewmodel.ContextMenu)
            {
                Assert.AreNotEqual("Delete this Participant", conMenu.Header);
            }

            var iterationRow = iterationFolderRow.ContainedRows.Single();
            viewmodel.SelectedThing = iterationRow;
            viewmodel.ComputePermission();
            viewmodel.PopulateContextMenu();

            Assert.AreEqual(4, viewmodel.ContextMenu.Count);
        }

        [Test]
        public async Task VerifyThatCreateParticipantCommandWorks()
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

            await viewmodel.CreateParticipantCommand.Execute();
            this.thingDialogNavigationService.Verify(x => x.Navigate(It.IsAny<Participant>(), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, It.IsAny<EngineeringModelSetup>(), null));
        }

        [Test]
        public async Task VerifyThatCreateIterationCommandWorks()
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
            Assert.AreEqual(2, viewmodel.ContextMenu.Count);

            await viewmodel.CreateIterationSetupCommand.Execute();
            this.thingDialogNavigationService.Verify(x => x.Navigate(It.IsAny<IterationSetup>(), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, It.IsAny<EngineeringModelSetup>(), null));
        }

        [Test]
        public async Task VerifyThatCreateModelSetupCommandWorks()
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

            await viewmodel.CreateCommand.Execute();
            this.thingDialogNavigationService.Verify(x => x.Navigate(It.IsAny<EngineeringModelSetup>(), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, It.IsAny<SiteDirectory>(), null));
        }
    }
}
