// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnitFactorDialogViewModleTestFixture.cs" company="RHEA System S.A.">
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
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using System.Windows.Input;

    using BasicRdl.ViewModels.Dialogs;

    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    internal class UnitFactorDialogViewModleTestFixture
    {
        private Uri uri = new Uri("http://test.com");
        private DerivedUnit derivedUnit;
        private SimpleUnit unit;
        private SiteDirectory siteDir;
        private SiteReferenceDataLibrary siteRdl;

        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> navigation;
        private ThingTransaction transaction;
        
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private DerivedUnit clone;
    
        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.navigation = new Mock<IThingDialogNavigationService>();
            this.permissionService = new Mock<IPermissionService>();
            this.session = new Mock<ISession>();
            
            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.siteRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri);
            this.derivedUnit = new DerivedUnit(Guid.NewGuid(), this.cache, this.uri);
            this.unit = new SimpleUnit(Guid.NewGuid(), this.cache, this.uri);

            this.siteRdl.Unit.Add(this.unit);

            this.siteDir.SiteReferenceDataLibrary.Add(this.siteRdl);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.cache.TryAdd(new CacheKey(this.derivedUnit.Iid, null), new Lazy<Thing>(() => this.derivedUnit));
            this.clone = this.derivedUnit.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            this.transaction = new ThingTransaction(transactionContext, this.clone);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [Test]
        public void VerifyThatUpdateOkCanExecuteWorks()
        {
            var vm = new UnitFactorDialogViewModel(new UnitFactor(), this.transaction, this.session.Object, true,
                ThingDialogKind.Create, this.navigation.Object, this.clone, new List<Thing> {this.siteRdl});

            Assert.IsNotEmpty(vm.PossibleUnit);
            Assert.IsFalse(((ICommand)vm.OkCommand).CanExecute(null));

            vm.SelectedUnit = this.unit;
            Assert.IsTrue(((ICommand)vm.OkCommand).CanExecute(null));
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            Assert.DoesNotThrow(() => new UnitFactorDialogViewModel());
        }
    }
}