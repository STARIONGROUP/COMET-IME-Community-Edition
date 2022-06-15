// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationshipBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.ViewModels
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using CDP4EngineeringModel.ViewModels;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class RelationshipBrowserViewModelTestFixture
    {
        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>();
            this.assembler = new Assembler(this.uri);
            this.permissionService = new Mock<IPermissionService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.cache = this.assembler.Cache;
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri);
            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) { RequiredRdl = this.srdl };
            this.sitedir.SiteReferenceDataLibrary.Add(this.srdl);

            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri) { Name = "model" };
            this.modelsetup.RequiredRdl.Add(this.mrdl);

            this.iterationsetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri);
            this.person = new Person(Guid.NewGuid(), this.cache, this.uri);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "domain", ShortName = "DMN" };
            this.participant = new Participant(Guid.NewGuid(), this.cache, this.uri) { Person = this.person, SelectedDomain = this.domain };

            this.modelsetup.ActiveDomain = new List<DomainOfExpertise> { this.domain };
            this.sitedir.Model.Add(this.modelsetup);
            this.sitedir.Person.Add(this.person);
            this.sitedir.Domain.Add(this.domain);
            this.modelsetup.IterationSetup.Add(this.iterationsetup);
            this.modelsetup.Participant.Add(this.participant);

            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri) { EngineeringModelSetup = this.modelsetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri) { IterationSetup = this.iterationsetup };
            this.model.Iteration.Add(this.iteration);
            this.elementDefinition1 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri) { Name = "E1" };
            this.elementDefinition2 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri) { Name = "E2" };
            this.elementDefinition3 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri) { Name = "E3" };
            this.elementDefinition4 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri) { Name = "E4" };
            this.elementDefinition5 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri) { Name = "E5" };

            this.iteration.Element.Add(this.elementDefinition1);
            this.iteration.Element.Add(this.elementDefinition2);
            this.iteration.Element.Add(this.elementDefinition3);
            this.iteration.Element.Add(this.elementDefinition4);
            this.iteration.Element.Add(this.elementDefinition5);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.Write(It.IsAny<OperationContainer>())).Returns(Task.FromResult("some result"));
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>> { { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant) } });

            this.cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        private readonly PropertyInfo revision = typeof(Thing).GetProperty("RevisionNumber");
        private Mock<ISession> session;
        private Assembler assembler;
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
        private ElementDefinition elementDefinition1;
        private ElementDefinition elementDefinition2;
        private ElementDefinition elementDefinition3;
        private ElementDefinition elementDefinition4;
        private ElementDefinition elementDefinition5;
        private DomainOfExpertise domain;

        private SiteReferenceDataLibrary srdl;
        private ModelReferenceDataLibrary mrdl;

        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        [Test]
        public void VerifyThatBinaryRelationshipsAreAddedModifiedRemoved()
        {
            var viewmodel = new RelationshipBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);

            //Add relationship1 and relationship2
            var relationship = new BinaryRelationship(Guid.NewGuid(), this.cache, this.uri) { Source = this.elementDefinition1, Target = this.elementDefinition2, Owner = this.domain };
            this.iteration.Relationship.Add(relationship);

            this.revision.SetValue(this.iteration, 1);
            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);

            Assert.AreEqual(1, viewmodel.RelationshipTypes[0].ContainedRows.Count);

            //Modify element name
            this.elementDefinition1.Name = "EX";

            this.revision.SetValue(this.elementDefinition1, 1);
            CDPMessageBus.Current.SendObjectChangeEvent(this.elementDefinition1, EventKind.Updated);

            Assert.IsTrue(((BinaryRelationshipRowViewModel)viewmodel.RelationshipTypes[0].ContainedRows[0]).Name.Contains("EX"));

            //Modify relationship

            relationship.Source = this.elementDefinition3;

            this.revision.SetValue(relationship, 1);
            CDPMessageBus.Current.SendObjectChangeEvent(relationship, EventKind.Updated);

            Assert.IsTrue(((BinaryRelationshipRowViewModel)viewmodel.RelationshipTypes[0].ContainedRows[0]).Name.Contains("E3"));

            //Remove relationships

            this.iteration.Relationship.Clear();
            this.revision.SetValue(this.iteration, 2);
            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);

            Assert.AreEqual(0, viewmodel.RelationshipTypes[0].ContainedRows.Count);
        }

        [Test]
        public async Task VerifyThatCreateBinaryRelationshipWorks()
        {
            var viewmodel = new RelationshipBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);
            var creator = viewmodel.RelationshipCreator.BinaryRelationshipCreator;

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(this.elementDefinition1);
            dropinfo.SetupProperty(x => x.Effects);

            creator.SourceViewModel.DragOver(dropinfo.Object);
            dropinfo.VerifySet(x => x.Effects = DragDropEffects.All);

            await creator.SourceViewModel.Drop(dropinfo.Object);
            Assert.AreSame(creator.SourceViewModel.RelatedThing, this.elementDefinition1);
            Assert.AreEqual(string.Format("({0}) {1}", this.elementDefinition1.ClassKind, this.elementDefinition1.Name), creator.SourceViewModel.RelatedThingDenomination);

            var dropinfo2 = new Mock<IDropInfo>();
            dropinfo2.Setup(x => x.Payload).Returns(this.elementDefinition2);
            await creator.TargetViewModel.Drop(dropinfo2.Object);

            Assert.IsTrue(((ICommand)viewmodel.RelationshipCreator.CreateRelationshipCommand).CanExecute(null));
            await viewmodel.RelationshipCreator.CreateRelationshipCommand.Execute();

            creator.ReInitializeControl();
            Assert.IsNull(creator.SourceViewModel.RelatedThing);
            Assert.IsNull(creator.TargetViewModel.RelatedThing);
        }

        [Test]
        public async Task VerifyThatCreateMultiRelationshipWorks()
        {
            var viewmodel = new RelationshipBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);
            viewmodel.RelationshipCreator.SelectedRelationshipCreator = viewmodel.RelationshipCreator.MultiRelationshipCreator;
            var creator = viewmodel.RelationshipCreator.MultiRelationshipCreator;

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(this.elementDefinition1);
            dropinfo.SetupProperty(x => x.Effects);

            creator.DragOver(dropinfo.Object);
            dropinfo.VerifySet(x => x.Effects = DragDropEffects.Copy);

            await creator.Drop(dropinfo.Object);
            Assert.IsTrue(creator.RelatedThings.Any(x => x.Thing == this.elementDefinition1));

            var dropinfo2 = new Mock<IDropInfo>();
            dropinfo2.Setup(x => x.Payload).Returns(this.elementDefinition2);
            await creator.Drop(dropinfo2.Object);

            Assert.AreEqual(2, creator.RelatedThings.Count);
            await creator.RelatedThings.First().RemoveRelatedThingCommand.Execute();
            Assert.AreEqual(1, creator.RelatedThings.Count);

            Assert.IsTrue(((ICommand)viewmodel.RelationshipCreator.CreateRelationshipCommand).CanExecute(null));
            await viewmodel.RelationshipCreator.CreateRelationshipCommand.Execute();

            creator.ReInitializeControl();
            Assert.AreEqual(0, creator.RelatedThings.Count);

            creator.Dispose();
        }

        [Test]
        public void VerifyThatMultiRelationshipsAreAddedModifiedRemoved()
        {
            var viewmodel = new RelationshipBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);

            //Add relationship1 and relationship2 and relationship3
            var relationship = new MultiRelationship(Guid.NewGuid(), this.cache, this.uri) { Owner = this.domain, ModifiedOn = DateTime.Now };
            relationship.RelatedThing.Add(this.elementDefinition1);
            relationship.RelatedThing.Add(this.elementDefinition2);
            relationship.RelatedThing.Add(this.elementDefinition3);
            this.iteration.Relationship.Add(relationship);

            this.revision.SetValue(this.iteration, 1);
            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);

            Assert.AreEqual(1, viewmodel.RelationshipTypes[1].ContainedRows.Count);

            //Modify element definition name
            this.elementDefinition3.Name = "EG";

            this.revision.SetValue(this.elementDefinition3, 1);
            CDPMessageBus.Current.SendObjectChangeEvent(this.elementDefinition3, EventKind.Updated);

            Assert.IsTrue(((MultiRelationshipRowViewModel)viewmodel.RelationshipTypes[1].ContainedRows[0]).Name.Contains("EG"));

            //Modify relationship content

            relationship.RelatedThing.Add(this.elementDefinition4);
            this.revision.SetValue(relationship, 1);
            CDPMessageBus.Current.SendObjectChangeEvent(relationship, EventKind.Updated);

            Assert.IsTrue(((MultiRelationshipRowViewModel)viewmodel.RelationshipTypes[1].ContainedRows[0]).Name.Contains("E4"));

            //Remove relationships

            this.iteration.Relationship.Clear();
            this.revision.SetValue(this.iteration, 2);
            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);

            Assert.AreEqual(0, viewmodel.RelationshipTypes[1].ContainedRows.Count);
        }

        [Test]
        public void VerifyThatRelationsBrowserIsCreated()
        {
            var viewmodel = new RelationshipBrowserViewModel(this.iteration, this.session.Object,
                this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);

            Assert.AreEqual(2, viewmodel.RelationshipTypes.Count);

            Assert.That(viewmodel.Caption, Is.Not.Null.Or.Empty);
            Assert.That(viewmodel.ToolTip, Is.Not.Null.Or.Empty);
            Assert.That(viewmodel.DataSource, Is.Not.Null.Or.Empty);
            Assert.That(viewmodel.DomainOfExpertise, Is.Not.Null.Or.Empty);
            Assert.That(viewmodel.CurrentModel, Is.Not.Null.Or.Empty);
        }
    }
}
