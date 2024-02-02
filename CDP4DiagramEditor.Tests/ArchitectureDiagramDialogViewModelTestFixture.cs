// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArchitectureDiagramDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4DiagramEditor.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.MetaInfo;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using CDP4DiagramEditor.ViewModels;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    internal class ArchitectureDiagramDialogViewModelTestFixture
    {
        private Uri uri;
        private SiteDirectory siteDir;
        private ThingTransaction transaction;

        private EngineeringModel model;
        private EngineeringModelSetup modelSetup;
        private Iteration iteration;

        private Mock<ISession> session;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IThingDialogNavigationService> navigation;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Mock<IPermissionService> permissionService;
        private CDPMessageBus messageBus;
        
        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.uri = new Uri("http://www.rheagroup.com");
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigation = new Mock<IThingDialogNavigationService>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>()).Returns(this.navigation.Object);
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);

            this.session = new Mock<ISession>();
            var person = new Person(Guid.NewGuid(), this.cache, this.uri) { Container = this.siteDir };
            this.session.Setup(x => x.ActivePerson).Returns(person);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.siteDir.Person.Add(person);

            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri);
            this.modelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            this.modelSetup.ActiveDomain.Add(new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "System" });
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);

            this.model.Iteration.Add(this.iteration);
            this.model.EngineeringModelSetup = this.modelSetup;

            var transactionContext = TransactionContextResolver.ResolveContext(this.iteration);
            this.transaction = new ThingTransaction(transactionContext, null);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new HashSet<ReferenceDataLibrary>(this.siteDir.SiteReferenceDataLibrary));
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
            
            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());

            this.cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public async Task VerifyThatCreateDiagramWorks()
        {
            var clone = this.iteration.Clone(false);
            this.transaction.CreateOrUpdate(clone);
            var diagram = new DiagramCanvas();
            var viewmodel = new DiagramCanvasDialogViewModel(diagram, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.navigation.Object, clone, null);

            var nameCheck = viewmodel["Name"];
            Assert.IsFalse(viewmodel.OkCanExecute);

            viewmodel.Name = "test";

            nameCheck = viewmodel["Name"];
            Assert.IsTrue(viewmodel.OkCanExecute);

            await viewmodel.OkCommand.Execute();

            Assert.AreNotEqual(default, diagram.CreatedOn);
        }

        [Test]
        public async Task VerifyDiagramPropertiesCanBeSet()
        {
            var clone = this.iteration.Clone(false);
            this.transaction.CreateOrUpdate(clone);
            var diagram = new ArchitectureDiagram();
            this.iteration.DiagramCanvas.Add(diagram);
            var viewmodel = new ArchitectureDiagramDialogViewModel(diagram, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.navigation.Object, clone, null);

            var nameCheck = viewmodel["Name"];
            var descCheck = viewmodel["Description"];
            Assert.IsFalse(viewmodel.OkCanExecute);

            viewmodel.Name = "test";
            viewmodel.Description = "testDec";

            nameCheck = viewmodel["Name"];
            descCheck = viewmodel["Description"];

            Assert.IsNotEmpty(nameCheck);
            Assert.IsNotEmpty(descCheck);

            Assert.IsTrue(viewmodel.OkCanExecute);

            await viewmodel.OkCommand.Execute();

            Assert.AreNotEqual(default, diagram.CreatedOn);
        }
    }
}
