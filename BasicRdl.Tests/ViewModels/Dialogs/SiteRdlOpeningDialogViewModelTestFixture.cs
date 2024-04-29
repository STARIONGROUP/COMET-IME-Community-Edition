﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiteRdlOpeningDialogViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

namespace BasicRDL.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using BasicRdl.ViewModels;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class SiteRdlOpeningDialogViewModelTestFixture
    {
        private Uri uri;
        private Mock<ISession> session;
        private SiteDirectory siteDirectory;
        private Mock<IPermissionService> permissionService;
        private Assembler assembler;
        private Person person;
        private readonly HashSet<ReferenceDataLibrary> openReferenceDataLibraries = new HashSet<ReferenceDataLibrary>();
        private readonly Mock<IServiceLocator> serviceLocator = new Mock<IServiceLocator>();
        private readonly Mock<IThingDialogNavigationService> navigation = new Mock<IThingDialogNavigationService>();
        private SiteReferenceDataLibrary siteRDL1;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>()).Returns(this.navigation.Object);

            this.person = new Person(Guid.NewGuid(), null, this.uri) { GivenName = "testPerson" };
            this.uri = new Uri("https://www.stariongroup.eu");
            this.session = new Mock<ISession>();
            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), null, new Uri("http://test.com")) { Name = "TestSiteDir" };
            var siteRDL2 = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null);
            this.siteRDL1 = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null) { RequiredRdl = siteRDL2 };
            this.siteDirectory.SiteReferenceDataLibrary.Add(this.siteRDL1);
            this.siteDirectory.SiteReferenceDataLibrary.Add(siteRDL2);
            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>()).Returns(this.navigation.Object);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDirectory);
            this.assembler = new Assembler(this.uri, this.messageBus);

            var lazysiteDirectory = new Lazy<Thing>(() => this.siteDirectory);
            this.assembler.Cache.GetOrAdd(new CacheKey(lazysiteDirectory.Value.Iid, null), lazysiteDirectory);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(this.openReferenceDataLibraries);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        /// <summary>
        /// The verify panel properties.
        /// </summary>
        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var sessions = new List<ISession> { this.session.Object };
            var siteRdl = new SiteReferenceDataLibrary();
            var viewmodel = new SiteRdlOpeningDialogViewModel(sessions);

            Assert.IsFalse(viewmodel.SelectedSiteRdls.Any());
            Assert.IsNull(viewmodel.DialogResult);
            Assert.AreEqual(1, viewmodel.SessionsAvailable.Count);

            viewmodel.SelectedSiteRdls.Add(new SiteRdlRowViewModel(siteRdl, this.session.Object, null));
            Assert.IsTrue(viewmodel.SelectedSiteRdls.Any());
        }

        [Test]
        public void VerifyThatSelectedItemCanOnlyContainSiteRdlRow()
        {
            var sessions = new List<ISession> { this.session.Object };
            var viewmodel = new SiteRdlOpeningDialogViewModel(sessions);

            viewmodel.SelectedSiteRdls.Add(new SiteRdlSessionRowViewModel(this.siteDirectory, this.session.Object, null));
            Assert.AreEqual(0, viewmodel.SelectedSiteRdls.Count);
            var siteRdl = new SiteReferenceDataLibrary();
            viewmodel.SelectedSiteRdls.Add(new SiteRdlRowViewModel(siteRdl, this.session.Object, null));
            Assert.AreEqual(1, viewmodel.SelectedSiteRdls.Count);
        }

        [Test]
        public void VerifyThatReactiveCommandsCanBeExecuted()
        {
            var sessions = new List<ISession> { this.session.Object };
            var viewmodel = new SiteRdlOpeningDialogViewModel(sessions);
            Assert.IsTrue(((ICommand)viewmodel.CancelCommand).CanExecute(null));
            Assert.IsFalse(((ICommand)viewmodel.OpenCommand).CanExecute(null));

            var siteRdl = new SiteReferenceDataLibrary();
            viewmodel.SelectedSiteRdls.Add(new SiteRdlRowViewModel(siteRdl, this.session.Object, null));
            Assert.IsTrue(((ICommand)viewmodel.OpenCommand).CanExecute(null));
        }

        [Test]
        public async Task VerifyThatCancelCommandsReturnCorrectResult()
        {
            var sessions = new List<ISession> { this.session.Object };
            var viewmodel = new SiteRdlOpeningDialogViewModel(sessions);
            await viewmodel.CancelCommand.Execute();

            Assert.NotNull(viewmodel.DialogResult);
            Assert.IsFalse((bool)viewmodel.DialogResult.Result);

            await viewmodel.OpenCommand.Execute();

            Assert.NotNull(viewmodel.DialogResult);
            Assert.IsTrue((bool)viewmodel.DialogResult.Result);
        }
    }
}