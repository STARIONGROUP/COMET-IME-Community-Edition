// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnnotationFloatingDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4CommonView.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4CommonView.ViewModels;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    internal class AnnotationFloatingDialogViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Assembler assembler;
        private Uri uri = new Uri("http://test.com");
        private EngineeringModel model;
        private Iteration iteration;
        private ElementDefinition ed;
        private ReviewItemDiscrepancy rid;
        private ModellingThingReference reference;

        private SiteDirectory sitedir;
        private Person person;
        private EngineeringModelSetup modelsetup;
        private IterationSetup iterationsetup;
        private Participant participant;
        private DomainOfExpertise domain;

        private PropertyInfo rev = typeof(Thing).GetProperty("RevisionNumber");
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.assembler = new Assembler(this.uri, this.messageBus);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri) { Surname = "surname", GivenName = "given", ShortName = "short" };
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.iterationsetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri) { IterationNumber = 1 };
            this.participant = new Participant(Guid.NewGuid(), this.assembler.Cache, this.uri) { Person = this.person };
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri) { EngineeringModelSetup = this.modelsetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri) { IterationSetup = this.iterationsetup };
            this.ed = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.rid = new ReviewItemDiscrepancy(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.reference = new ModellingThingReference(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.reference.ReferencedThing = this.ed;
            this.rid.PrimaryAnnotatedThing = this.reference;

            this.sitedir.Person.Add(this.person);
            this.sitedir.Domain.Add(this.domain);
            this.sitedir.Model.Add(this.modelsetup);
            this.modelsetup.Participant.Add(this.participant);

            this.model.Iteration.Add(this.iteration);
            this.iteration.Element.Add(this.ed);
            this.model.ModellingAnnotation.Add(this.rid);
            this.rid.RelatedThing.Add(this.reference);

            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>> { { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant) } });
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.Write(It.IsAny<OperationContainer>())).Returns(Task.CompletedTask);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.assembler.Cache.TryAdd(new CacheKey(this.model.Iid, null), new Lazy<Thing>(() => this.model));
            this.assembler.Cache.TryAdd(new CacheKey(this.rid.Iid, null), new Lazy<Thing>(() => this.rid));
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public async Task VerifyThatViewModelIsPopulatedAndCommandWorks()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);

            this.rid.Title = "title";
            this.rid.Content = "content";
            this.rid.ShortName = "sn";

            var vm = new AnnotationFloatingDialogViewModel(this.rid, this.session.Object);
            Assert.AreEqual(vm.ShortName, this.rid.ShortName);
            Assert.AreEqual(vm.Title, this.rid.Title);
            Assert.AreEqual(vm.Content, this.rid.Content);
            Assert.AreEqual(0, vm.DiscussionRows.Count);
            Assert.IsFalse(((ICommand)vm.PostDiscussionItemCommand).CanExecute(null));
            vm.NewDiscussionItemText = "test";
            Assert.IsTrue(((ICommand)vm.PostDiscussionItemCommand).CanExecute(null));

            await vm.PostDiscussionItemCommand.Execute();
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }

        [Test]
        public async Task VerifyThatDiscussionsItemAreAddedAndRemoved()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);

            var discussion1 = new EngineeringModelDataDiscussionItem(Guid.NewGuid(), this.assembler.Cache, this.uri);
            discussion1.Author = this.participant;
            discussion1.Content = "discussion 1";

            this.rid.Discussion.Add(discussion1);

            var vm = new AnnotationFloatingDialogViewModel(this.rid, this.session.Object);
            Assert.AreEqual(1, vm.DiscussionRows.Count);

            var discussionRow = vm.DiscussionRows.Single();
            discussionRow.Content = "mod";

            await discussionRow.SaveCommand.Execute(default);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));

            await discussionRow.CancelCommand.Execute(default);
            Assert.AreEqual(discussion1.Content, discussionRow.Content);

            this.rid.Discussion.Clear();
            this.rev.SetValue(this.rid, 10);

            this.messageBus.SendObjectChangeEvent(this.rid, EventKind.Updated);
            Assert.AreEqual(0, vm.DiscussionRows.Count);
        }
    }
}
