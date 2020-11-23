// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DerivedQuantityKindDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski
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

namespace BasicRdl.Tests.ViewModels
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Threading.Tasks;

    using BasicRdl.ViewModels;

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
    internal class DerivedQuantityKindDialogViewModelTestFixture
    {
        private DerivedQuantityKindDialogViewModel viewmodel;
        private DerivedQuantityKind derivedQuantityKind;
        private SiteDirectory siteDir;
        private ThingTransaction transaction;
        private Mock<ISession> session;
        private Mock<IThingDialogNavigationService> navigation;
        private Mock<IPermissionService> permissionService;
        private SiteReferenceDataLibrary rdl;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.navigation = new Mock<IThingDialogNavigationService>();

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            var person = new Person(Guid.NewGuid(), this.cache, null) { Container = this.siteDir };
            this.session.Setup(x => x.ActivePerson).Returns(person);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, null);
            this.siteDir.Person.Add(person);
            var testScale = new LogarithmicScale();
            this.rdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, null) { Name = "testRDL", ShortName = "test" };
            this.rdl.Scale.Add(testScale);
            var qkf = new QuantityKindFactor();
            this.derivedQuantityKind = new DerivedQuantityKind { Name = "derivedQuantityKind", ShortName = "dqk" };
            this.derivedQuantityKind.QuantityKindFactor.Add(qkf);
            this.rdl.ParameterType.Add(new SimpleQuantityKind { Name = "testSQK", ShortName = "tSQK" });
            this.siteDir.SiteReferenceDataLibrary.Add(this.rdl);

            this.cache.TryAdd(new CacheKey(this.rdl.Iid, null), new Lazy<Thing>(() => this.rdl));

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            this.transaction = new ThingTransaction(transactionContext, null);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new HashSet<ReferenceDataLibrary>(this.siteDir.SiteReferenceDataLibrary));

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());

            this.viewmodel = new DerivedQuantityKindDialogViewModel(this.derivedQuantityKind, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.navigation.Object, null);
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            Assert.AreEqual(this.viewmodel.Name, this.derivedQuantityKind.Name);
            Assert.AreEqual(this.viewmodel.ShortName, this.derivedQuantityKind.ShortName);
            Assert.AreEqual(this.viewmodel.IsDeprecated, this.derivedQuantityKind.IsDeprecated);
            Assert.AreEqual(this.viewmodel.Symbol, this.derivedQuantityKind.Symbol);
            Assert.AreEqual(this.viewmodel.QuantityDimensionSymbol, this.derivedQuantityKind.QuantityDimensionSymbol);
            Assert.AreEqual(this.viewmodel.SelectedDefaultScale, this.derivedQuantityKind.DefaultScale);
            Assert.IsNotEmpty(this.viewmodel.PossibleContainer);
        }

        [Test]
        public async Task VerifyThatOkCommandWorks()
        {
            this.viewmodel.OkCommand.Execute(null);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
            Assert.IsNull(this.viewmodel.WriteException);
            Assert.IsTrue(this.viewmodel.DialogResult.Value);
        }

        [Test]
        public async Task VerifyThatExceptionAreCaught()
        {
            this.session.Setup(x => x.Write(It.IsAny<OperationContainer>())).Throws(new Exception("test"));

            this.viewmodel.OkCommand.Execute(null);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));

            Assert.IsNotNull(this.viewmodel.WriteException);
            Assert.AreEqual("test", this.viewmodel.WriteException.Message);
        }

        [Test]
        public void VerifyDialogValidation()
        {
            Assert.AreEqual(0, this.viewmodel.ValidationErrors.Count);
            Assert.That(this.viewmodel["Symbol"], Is.Not.Null.Or.Not.Empty);

            this.viewmodel.Symbol = "something";
            Assert.That(this.viewmodel["Symbol"], Is.Null.Or.Empty);
        }

        [Test]
        public void VerifyUpdateOkCanExecute()
        {
            Assert.IsNotEmpty(this.viewmodel.QuantityKindFactor);
            Assert.IsNull(this.viewmodel.SelectedDefaultScale);
            Assert.IsFalse(this.viewmodel.OkCommand.CanExecute(null));
            this.viewmodel.SelectedDefaultScale = new RatioScale();
            Assert.IsTrue(this.viewmodel.OkCommand.CanExecute(null));
        }

        [Test]
        public void VerifThatUpdatingContainerPopulatesPossiblePossibleScales()
        {
            this.viewmodel.PossiblePossibleScale.Clear();
            Assert.IsEmpty(this.viewmodel.PossiblePossibleScale);
            this.viewmodel.Container = null;
            var rdl = this.siteDir.SiteReferenceDataLibrary.First();
            var testScale = new LogarithmicScale();
            rdl.Scale.Add(testScale);

            this.viewmodel.Container = rdl;
            Assert.IsNotEmpty(this.viewmodel.PossiblePossibleScale);
        }

        [Test]
        public void VerifQuantityKindFactorCommands()
        {
            Assert.IsTrue(this.viewmodel.CreateQuantityKindFactorCommand.CanExecute(null));
            Assert.IsFalse(this.viewmodel.InspectQuantityKindFactorCommand.CanExecute(null));
            Assert.IsFalse(this.viewmodel.EditQuantityKindFactorCommand.CanExecute(null));
            Assert.IsFalse(this.viewmodel.DeleteQuantityKindFactorCommand.CanExecute(null));
            Assert.IsFalse(this.viewmodel.MoveUpQuantityKindFactorCommand.CanExecute(null));
            Assert.IsFalse(this.viewmodel.MoveDownQuantityKindFactorCommand.CanExecute(null));

            this.viewmodel.SelectedQuantityKindFactor = this.viewmodel.QuantityKindFactor.First();

            Assert.IsTrue(this.viewmodel.InspectQuantityKindFactorCommand.CanExecute(null));
            Assert.IsTrue(this.viewmodel.EditQuantityKindFactorCommand.CanExecute(null));
            Assert.IsTrue(this.viewmodel.DeleteQuantityKindFactorCommand.CanExecute(null));

            Assert.IsTrue(this.viewmodel.MoveUpQuantityKindFactorCommand.CanExecute(null));
            Assert.IsTrue(this.viewmodel.MoveDownQuantityKindFactorCommand.CanExecute(null));
        }

        [Test]
        public void VerifyInspectQuantityKindFactor()
        {
            var vm = new DerivedQuantityKindDialogViewModel(this.derivedQuantityKind, this.transaction, this.session.Object, true, ThingDialogKind.Inspect, this.navigation.Object, this.rdl);
            Assert.IsNull(vm.SelectedQuantityKindFactor);

            vm.SelectedQuantityKindFactor = vm.QuantityKindFactor.First();
            Assert.IsTrue(vm.InspectQuantityKindFactorCommand.CanExecute(null));
            vm.InspectQuantityKindFactorCommand.Execute(null);
            this.navigation.Verify(x => x.Navigate(It.IsAny<QuantityKindFactor>(), It.IsAny<ThingTransaction>(), this.session.Object, false, ThingDialogKind.Inspect, this.navigation.Object, It.IsAny<Thing>(), null));
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            var dialogViewModel = new DerivedQuantityKindDialogViewModel();
            Assert.IsFalse(dialogViewModel.IsDeprecated);
        }

        [Test]
        public void VerifyInspectSelectedDefaultScale()
        {
            var scale1 = new LogarithmicScale(Guid.NewGuid(), null, null);
            var vm = new DerivedQuantityKindDialogViewModel(this.derivedQuantityKind, this.transaction, this.session.Object, true, ThingDialogKind.Inspect, this.navigation.Object, this.rdl);
            Assert.IsFalse(vm.InspectSelectedScaleCommand.CanExecute(null));
            vm.SelectedDefaultScale = scale1;
            Assert.IsTrue(vm.InspectSelectedScaleCommand.CanExecute(null));
            vm.InspectSelectedScaleCommand.Execute(null);
            this.navigation.Verify(x => x.Navigate(It.IsAny<MeasurementScale>(), It.IsAny<ThingTransaction>(), this.session.Object, false, ThingDialogKind.Inspect, this.navigation.Object, It.IsAny<Thing>(), null));
        }
    }
}