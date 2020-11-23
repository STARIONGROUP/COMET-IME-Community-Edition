// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleQuantityKindDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;

    using BasicRdl.ViewModels;

    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using Microsoft.Practices.ServiceLocation;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    internal class SimpleQuantityKindDialogViewModelTestFixture
    {
        private SimpleQuantityKindDialogViewModel viewmodel;
        private SimpleQuantityKind simpleQuantityKind;
        private SiteDirectory siteDir;
        private ThingTransaction transaction;
        private Mock<ISession> session;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IThingDialogNavigationService> navigation;
        private Mock<IPermissionService> permissionService;
        private SiteReferenceDataLibrary rdl;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigation = new Mock<IThingDialogNavigationService>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>()).Returns(this.navigation.Object);
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.session = new Mock<ISession>();
            var person = new Person(Guid.NewGuid(), null, null) { Container = this.siteDir };
            this.session.Setup(x => x.ActivePerson).Returns(person);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), null, null);
            this.siteDir.Person.Add(person);
            this.rdl = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null) { Name = "testRDL", ShortName = "test" };
            this.simpleQuantityKind = new SimpleQuantityKind(Guid.NewGuid(), null, null) { Name = "simpleQuantityKind", ShortName = "cat" };
            var cat = new Category(Guid.NewGuid(), null, null) { Name = "category1", ShortName = "cat1" };
            cat.PermissibleClass.Add(ClassKind.SimpleQuantityKind);
            this.rdl.DefinedCategory.Add(cat);
            this.siteDir.SiteReferenceDataLibrary.Add(this.rdl);

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            this.transaction = new ThingTransaction(transactionContext, null);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new HashSet<ReferenceDataLibrary>(this.siteDir.SiteReferenceDataLibrary));
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());

            this.viewmodel = new SimpleQuantityKindDialogViewModel(this.simpleQuantityKind, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, null, null);
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            Assert.AreEqual(this.viewmodel.Name, this.simpleQuantityKind.Name);
            Assert.AreEqual(this.viewmodel.ShortName, this.simpleQuantityKind.ShortName);
            Assert.AreEqual(this.viewmodel.IsDeprecated, this.simpleQuantityKind.IsDeprecated);
            Assert.AreEqual(this.viewmodel.Symbol, this.simpleQuantityKind.Symbol);
            Assert.AreEqual(this.viewmodel.QuantityDimensionSymbol, this.simpleQuantityKind.QuantityDimensionSymbol);
            Assert.IsNotEmpty(this.viewmodel.PossibleCategory);
            Assert.IsNotEmpty(this.viewmodel.PossibleContainer);
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
            Assert.IsFalse(this.viewmodel.OkCommand.CanExecute(null));
            Assert.IsEmpty(this.viewmodel.PossibleScale);
            var testScale = new RatioScale();
            this.viewmodel.PossibleScale.Add(testScale);
            Assert.IsFalse(this.viewmodel.OkCommand.CanExecute(null));
            Assert.IsNull(this.viewmodel.SelectedDefaultScale);
            this.viewmodel.SelectedDefaultScale = testScale;
            Assert.IsTrue(this.viewmodel.OkCommand.CanExecute(null));
        }

        [Test]
        public void VerifThatUpdatingContainerPopulatesPossiblePossibleScales()
        {
            Assert.IsEmpty(this.viewmodel.PossiblePossibleScale);
            this.viewmodel.Container = null;
            var rdl = this.siteDir.SiteReferenceDataLibrary.First();
            var testScale = new LogarithmicScale();
            rdl.Scale.Add(testScale);

            this.viewmodel.Container = rdl;
            Assert.IsNotEmpty(this.viewmodel.PossiblePossibleScale);
        }

        [Test]
        public void VerifyInspectSelectedDefaultScale()
        {
            var scale1 = new LogarithmicScale(Guid.NewGuid(), null, null);
            var vm = new SimpleQuantityKindDialogViewModel(this.simpleQuantityKind, this.transaction, this.session.Object, true, ThingDialogKind.Inspect, this.navigation.Object, this.rdl);
            Assert.IsFalse(vm.InspectSelectedScaleCommand.CanExecute(null));
            vm.SelectedDefaultScale = scale1;
            Assert.IsTrue(vm.InspectSelectedScaleCommand.CanExecute(null));
            vm.InspectSelectedScaleCommand.Execute(null);
            this.navigation.Verify(x => x.Navigate(It.IsAny<MeasurementScale>(), It.IsAny<ThingTransaction>(), this.session.Object, false, ThingDialogKind.Inspect, this.navigation.Object, It.IsAny<Thing>(), null));
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            Assert.DoesNotThrow(() => new SimpleQuantityKindDialogViewModel());
        }
    }
}