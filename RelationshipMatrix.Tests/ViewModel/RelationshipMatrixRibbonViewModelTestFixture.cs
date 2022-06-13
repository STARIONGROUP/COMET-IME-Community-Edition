// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationshipMatrixRibbonViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski
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

namespace CDP4RelationshipMatrix.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using CDP4RelationshipMatrix.ViewModels;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="RelationshipMatrixRibbonViewModel"/> class
    /// </summary>
    [TestFixture]
    public class RelationshipMatrixRibbonViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IPluginSettingsService> pluginSettingsService;
        private Mock<IFilterStringService> filterStringService;
        private readonly Uri uri = new Uri("http://www.rheagroup.com");
        private Mock<IServiceLocator> serviceLocator;
        private Assembler assembler;
        private SiteDirectory sitedir;
        private Person person;
        private Participant participant;
        private DomainOfExpertise domainOfExpertise;
        private EngineeringModelSetup engineeringModelSetup;
        private EngineeringModel engineeringModel;
        private IterationSetup iterationSetup;
        private Iteration iteration;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.pluginSettingsService = new Mock<IPluginSettingsService>();
            this.filterStringService = new Mock<IFilterStringService>();
            this.assembler = new Assembler(this.uri);
            this.cache = this.assembler.Cache;

            this.serviceLocator.Setup(x => x.GetInstance<IPermissionService>()).Returns(this.permissionService.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>()).Returns(this.thingDialogNavigationService.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IPanelNavigationService>()).Returns(this.panelNavigationService.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IDialogNavigationService>()).Returns(this.dialogNavigationService.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IPluginSettingsService>()).Returns(this.pluginSettingsService.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IFilterStringService>()).Returns(this.filterStringService.Object);

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);

            this.person = new Person(Guid.NewGuid(), this.cache, this.uri);
            this.sitedir.Person.Add(this.person);

            this.participant = new Participant(Guid.NewGuid(), this.cache, this.uri)
            {
                Person = this.person
            };
            this.domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri);

            this.engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri);
            this.engineeringModelSetup.IterationSetup.Add(this.iterationSetup);
            this.engineeringModelSetup.Participant.Add(this.participant);

            this.engineeringModel = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri);
            this.engineeringModel.EngineeringModelSetup = this.engineeringModelSetup;
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);
            this.iteration.IterationSetup = this.iterationSetup;
            this.engineeringModel.Iteration.Add(this.iteration);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.IsVersionSupported(It.IsAny<Version>())).Returns(true);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            var openIterations = new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>();
            openIterations.Add(this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domainOfExpertise, this.participant));

            this.session.Setup(x => x.OpenIterations).Returns(openIterations);

            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            this.pluginSettingsService.Setup(x => 
                x.Read<RelationshipMatrixPluginSettings>(It.IsAny<bool>())
            ).Returns(new RelationshipMatrixPluginSettings());

            this.filterStringService.Setup(x => x.ShowDeprecatedThings).Returns(false);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void Verify_That_RibbonViewModel_Can_Be_Constructed()
        {
            Assert.DoesNotThrow(() => new RelationshipMatrixRibbonViewModel());
        }

        [Test]
        public void Verify_That_InstantiatePanelViewModel_Returns_Expected_ViewModel()
        {
            var viewmodel = RelationshipMatrixRibbonViewModel.InstantiatePanelViewModel(
                this.iteration,
                this.session.Object,
                this.thingDialogNavigationService.Object,
                this.panelNavigationService.Object,
                this.dialogNavigationService.Object,
                this.pluginSettingsService.Object);

            Assert.That(viewmodel, Is.InstanceOf<RelationshipMatrixViewModel>());
        }
    }
}
