// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagramBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru, Nathanael Smiechowski.
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4DiagramEditor.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CDP4DiagramEditor.ViewModels;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    internal class DiagramBrowserViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private readonly Uri uri = new Uri("http://test.com");
        private SiteDirectory sitedir;
        private EngineeringModelSetup modelsetup;
        private IterationSetup iterationsetup;
        private Person person;
        private Participant participant;
        private EngineeringModel model;
        private Iteration iteration;
        private DiagramCanvas diagram;
        private DomainOfExpertise domain;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri) { Name = "model" };
            this.iterationsetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri);
            this.person = new Person(Guid.NewGuid(), this.cache, this.uri);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "domain" };
            this.participant = new Participant(Guid.NewGuid(), this.cache, this.uri) { Person = this.person, SelectedDomain = this.domain };
            
            this.sitedir.Model.Add(this.modelsetup);
            this.sitedir.Person.Add(this.person);
            this.sitedir.Domain.Add(this.domain);
            this.modelsetup.IterationSetup.Add(this.iterationsetup);
            this.modelsetup.Participant.Add(this.participant);

            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri) { EngineeringModelSetup = this.modelsetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri) { IterationSetup = this.iterationsetup };
            this.diagram = new DiagramCanvas(Guid.NewGuid(), this.cache, this.uri) { Name = "diagram" };
            this.model.Iteration.Add(this.iteration);
            this.iteration.DiagramCanvas.Add(this.diagram);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());

            this.cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatRowsAreCreated()
        {
            var viewmodel = new DiagramBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);
        
            Assert.AreEqual(1, viewmodel.Diagrams.Count);

            Assert.That(viewmodel.Caption, Is.Not.Null.Or.Empty);
            Assert.That(viewmodel.ToolTip, Is.Not.Null.Or.Empty);
            Assert.That(viewmodel.DataSource, Is.Not.Null.Or.Empty);
            Assert.That(viewmodel.DomainOfExpertise, Is.Not.Null.Or.Empty);
            Assert.That(viewmodel.CurrentModel, Is.Not.Null.Or.Empty);

            var diagramrow = viewmodel.Diagrams.Single();
            Assert.That(diagramrow.Name, Is.Not.Null.Or.Empty);

            viewmodel.SelectedThing = diagramrow;

            viewmodel.UpdateCommand.Execute(null);
            this.panelNavigationService.Verify(x => x.Open(It.IsAny<DiagramEditorViewModel>(), true));
        }

        [Test]
        public void VerifyComputePermissions()
        {
            var vm = new DiagramBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);

            var diagramrow = vm.Diagrams.Single();
            Assert.That(diagramrow.Name, Is.Not.Null.Or.Empty);
            
            Assert.IsFalse(vm.UpdateCommand.CanExecute(null));
            vm.SelectedThing = diagramrow;
            vm.ComputePermission();
            Assert.IsTrue(vm.UpdateCommand.CanExecute(null));

            vm.UpdateCommand.Execute(null);
            this.panelNavigationService.Verify(x => x.Open(It.IsAny<DiagramEditorViewModel>(), true));
        }

        [Test]
        public void VerifyThatDiagramRowsAreUpdated()
        {
            var viewmodel = new DiagramBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);

            var newdiagram = new DiagramCanvas(Guid.NewGuid(), null, this.uri);
            this.iteration.DiagramCanvas.Add(newdiagram);

            var revision = typeof(Iteration).GetProperty("RevisionNumber");
            revision.SetValue(this.iteration, 2);

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            Assert.AreEqual(2, viewmodel.Diagrams.Count);

            this.iteration.DiagramCanvas.Clear();
            revision.SetValue(this.iteration, 3);

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            Assert.AreEqual(0, viewmodel.Diagrams.Count);
        }

        [Test]
        public void VerifyThatActiveDomainIsDisplayed()
        {
            var domain = new DomainOfExpertise(Guid.NewGuid(), null, this.uri) { Name = "domain" };

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                {this.iteration, new Tuple<DomainOfExpertise, Participant>(domain, null)}
            });

            var vm = new DiagramBrowserViewModel(this.iteration, this.session.Object, null, null, null, null);
            Assert.AreEqual("domain []", vm.DomainOfExpertise);

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                {this.iteration, null}
            });

            vm = new DiagramBrowserViewModel(this.iteration, this.session.Object, null, null, null, null);
            Assert.AreEqual("None", vm.DomainOfExpertise);

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());
            vm = new DiagramBrowserViewModel(this.iteration, this.session.Object, null, null, null, null);
            Assert.AreEqual("None", vm.DomainOfExpertise);
        }
    }
}