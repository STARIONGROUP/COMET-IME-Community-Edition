// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleUnitDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace BasicRdl.Tests.ViewModels.Dialogs
{
    using System;
    using System.Reactive.Concurrency;
    using System.Windows.Input;

    using BasicRdl.ViewModels;

    using CDP4Common.MetaInfo;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Operations;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="SimpleUnitDialogViewModel"/>
    /// </summary>
    [TestFixture]
    public class SimpleUnitDialogViewModelTestFixture
    {
        private ThingTransaction transaction;
        private Mock<IThingDialogNavigationService> dialogService;
        private Mock<ISession> session;
        private Mock<IServiceLocator> serviceLocator;
        private SiteDirectory siteDir;
        private SiteReferenceDataLibrary genericSiteReferenceDataLibrary;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.dialogService = new Mock<IThingDialogNavigationService>();

            this.session = new Mock<ISession>();
            
            var person = new Person(Guid.NewGuid(), null, null) { Container = this.siteDir };
            this.session.Setup(x => x.ActivePerson).Returns(person);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), null, null);
            this.siteDir.Person.Add(person);
            this.genericSiteReferenceDataLibrary = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null) { Name = "Generic RDL", ShortName = "GENRDL" };
            this.siteDir.SiteReferenceDataLibrary.Add(this.genericSiteReferenceDataLibrary);

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            this.transaction = new ThingTransaction(transactionContext, null);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerififyThatInvalidContainerThrowsException()
        {
            var simpleUnit = new SimpleUnit(Guid.NewGuid(), null, null);
            Assert.Throws<ArgumentException>(() => new SimpleUnitDialogViewModel(simpleUnit,this.transaction, this.session.Object, true, ThingDialogKind.Inspect, this.dialogService.Object, this.siteDir));
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var shortname = "new shortname";
            var name = "new name";
            var isdeprecated = true;

            var simpleUnit = new SimpleUnit(Guid.NewGuid(), null, null);
            var vm = new SimpleUnitDialogViewModel(simpleUnit, this.transaction, this.session.Object, true, ThingDialogKind.Inspect, this.dialogService.Object, this.genericSiteReferenceDataLibrary);

            Assert.IsFalse(((ICommand)vm.OkCommand).CanExecute(null));

            vm.ShortName = shortname;
            vm.Name = name;
            vm.IsDeprecated = isdeprecated;
            vm.Container = this.genericSiteReferenceDataLibrary;

            Assert.IsTrue(((ICommand)vm.OkCommand).CanExecute(null));
        }

       
        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            var dialogViewModel = new SimpleUnitDialogViewModel();
            Assert.IsFalse(dialogViewModel.IsDeprecated);
        }
    }
}
