// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationshipEditorViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4RelationshipEditor.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CDP4RelationshipEditor.ViewModels;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class RelationshipEditorViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IDropInfo> dropinfo;
        private readonly Uri uri = new Uri("http://test.com");
        private Assembler assembler;

        private SiteDirectory sitedir;
        private EngineeringModelSetup modelsetup;
        private IterationSetup iterationsetup;
        private Person person;
        private Participant participant;
        private EngineeringModel model;
        private Iteration iteration;
        private DomainOfExpertise domain;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>();
            this.messageBus = new CDPMessageBus();
            this.assembler = new Assembler(this.uri, this.messageBus);
            this.permissionService = new Mock<IPermissionService>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.dropinfo = new Mock<IDropInfo>();
            this.cache = this.assembler.Cache;

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri) { Name = "model" };
            this.iterationsetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri);
            this.person = new Person(Guid.NewGuid(), this.cache, this.uri);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "domain" };

            this.participant = new Participant(Guid.NewGuid(), this.cache, this.uri)
            {
                Person = this.person,
                SelectedDomain = this.domain
            };

            this.sitedir.Model.Add(this.modelsetup);
            this.sitedir.Person.Add(this.person);
            this.sitedir.Domain.Add(this.domain);
            this.modelsetup.IterationSetup.Add(this.iterationsetup);
            this.modelsetup.Participant.Add(this.participant);

            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri)
            {
                EngineeringModelSetup = this.modelsetup
            };

            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri) { IterationSetup = this.iterationsetup };
            this.model.Iteration.Add(this.iteration);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var viewmodel = new RelationshipEditorViewModel(this.iteration, this.participant, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);
            Assert.That(viewmodel.Caption, Is.Not.Null.Or.Empty);
            Assert.That(viewmodel.ToolTip, Is.Not.Null.Or.Empty);
            Assert.IsNotNull(viewmodel.RelationshipRules);
        }

        [Test]
        public void VerifyThatSubscriptionsWork()
        {
            var viewmodel = new RelationshipEditorViewModel(this.iteration, this.participant, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);

            Assert.AreEqual(0, viewmodel.RelationshipRules.Count);

            var newBinaryRule = new BinaryRelationshipRule(Guid.NewGuid(), this.cache, this.uri);
            var newMultiRule = new MultiRelationshipRule(Guid.NewGuid(), this.cache, this.uri);

            this.messageBus.SendObjectChangeEvent(newBinaryRule, EventKind.Added);
            Assert.AreEqual(1, viewmodel.RelationshipRules.Count);

            this.messageBus.SendObjectChangeEvent(newMultiRule, EventKind.Added);
            Assert.AreEqual(2, viewmodel.RelationshipRules.Count);

            this.messageBus.SendObjectChangeEvent(newBinaryRule, EventKind.Removed);
            Assert.AreEqual(1, viewmodel.RelationshipRules.Count);

            this.messageBus.SendObjectChangeEvent(newMultiRule, EventKind.Removed);
            Assert.AreEqual(0, viewmodel.RelationshipRules.Count);
        }

        [Test]
        public void VerifyDragOver()
        {
            var vm = new RelationshipEditorViewModel(this.iteration, this.participant, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);

            this.dropinfo.Setup(x => x.Payload).Returns(this.domain);
            vm.DragOver(this.dropinfo.Object);
            this.dropinfo.VerifySet(x => x.Effects = It.IsAny<DragDropEffects>(), Times.Once);
        }
    }
}
