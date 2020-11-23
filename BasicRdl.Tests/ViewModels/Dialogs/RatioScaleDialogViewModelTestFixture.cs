// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RatioScaleDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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
    internal class RatioScaleDialogViewModelTestFixture
    {
        private RatioScaleDialogViewModel viewmodel;
        private RatioScale ratioScale;
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
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            var person = new Person(Guid.NewGuid(), null, null) { Container = this.siteDir };
            this.session.Setup(x => x.ActivePerson).Returns(person);

            var simpleUnit = new SimpleUnit(Guid.NewGuid(), null, null);
            this.siteDir = new SiteDirectory(Guid.NewGuid(), null, null);
            this.siteDir.Person.Add(person);
            var testScale = new LogarithmicScale();
            this.rdl = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null) { Name = "testRDL", ShortName = "test" };
            this.rdl.Unit.Add(simpleUnit);
            this.rdl.Scale.Add(testScale);
            var svd1 = new ScaleValueDefinition(Guid.NewGuid(), null, null) { Name = "ReferenceSVD", ShortName = "RSVD" };
            var svd2 = new ScaleValueDefinition(Guid.NewGuid(), null, null) { Name = "DependentSVD", ShortName = "DSVD" };
            this.ratioScale = new RatioScale(Guid.NewGuid(), null, null) { Name = "ratioScale", ShortName = "dqk" };
            this.ratioScale.ValueDefinition.Add(svd1);
            this.ratioScale.ValueDefinition.Add(svd2);
            this.rdl.ParameterType.Add(new SimpleQuantityKind { Name = "testSQK", ShortName = "tSQK" });
            this.siteDir.SiteReferenceDataLibrary.Add(this.rdl);

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            this.transaction = new ThingTransaction(transactionContext, null);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new HashSet<ReferenceDataLibrary>(this.siteDir.SiteReferenceDataLibrary));

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());

            this.viewmodel = new RatioScaleDialogViewModel(this.ratioScale, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.navigation.Object);
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            Assert.AreEqual(this.viewmodel.Name, this.ratioScale.Name);
            Assert.AreEqual(this.viewmodel.ShortName, this.ratioScale.ShortName);
            Assert.AreEqual(this.viewmodel.IsDeprecated, this.ratioScale.IsDeprecated);
            Assert.AreEqual(this.viewmodel.MappingToReferenceScale, this.ratioScale.MappingToReferenceScale);
            Assert.AreEqual(this.viewmodel.IsMaximumInclusive, this.ratioScale.IsMaximumInclusive);
            Assert.AreEqual(this.viewmodel.IsMinimumInclusive, this.ratioScale.IsMinimumInclusive);
            Assert.AreEqual(this.viewmodel.MaximumPermissibleValue, this.ratioScale.MaximumPermissibleValue);
            Assert.AreEqual(this.viewmodel.MinimumPermissibleValue, this.ratioScale.MinimumPermissibleValue);
            Assert.AreEqual(this.viewmodel.NegativeValueConnotation, this.ratioScale.NegativeValueConnotation);
            Assert.AreEqual(this.viewmodel.PositiveValueConnotation, this.ratioScale.PositiveValueConnotation);
            Assert.IsTrue(this.viewmodel.PossibleUnit.Any());
        }

        [Test]
        public void VerifyUpdateOkCanExecute()
        {
            Assert.IsNull(this.viewmodel.SelectedUnit);
            Assert.IsFalse(this.viewmodel.OkCommand.CanExecute(null));

            this.viewmodel.SelectedUnit = this.viewmodel.PossibleUnit.First();
            Assert.IsTrue(this.viewmodel.OkCommand.CanExecute(null));
        }

        [Test]
        public void VerifValueDefinitionCommands()
        {
            Assert.IsTrue(this.viewmodel.CreateValueDefinitionCommand.CanExecute(null));
            Assert.IsFalse(this.viewmodel.InspectValueDefinitionCommand.CanExecute(null));
            Assert.IsFalse(this.viewmodel.EditValueDefinitionCommand.CanExecute(null));
            Assert.IsFalse(this.viewmodel.DeleteValueDefinitionCommand.CanExecute(null));

            this.viewmodel.SelectedValueDefinition = this.viewmodel.ValueDefinition.First();

            Assert.IsTrue(this.viewmodel.InspectValueDefinitionCommand.CanExecute(null));
            Assert.IsTrue(this.viewmodel.EditValueDefinitionCommand.CanExecute(null));
            Assert.IsTrue(this.viewmodel.DeleteValueDefinitionCommand.CanExecute(null));
        }

        [Test]
        public void VerifMappingToReferenceScaleCommands()
        {
            Assert.IsTrue(this.viewmodel.CreateMappingToReferenceScaleCommand.CanExecute(null));
            Assert.IsFalse(this.viewmodel.InspectMappingToReferenceScaleCommand.CanExecute(null));
            Assert.IsFalse(this.viewmodel.EditMappingToReferenceScaleCommand.CanExecute(null));
            Assert.IsFalse(this.viewmodel.DeleteMappingToReferenceScaleCommand.CanExecute(null));

            var mtrs = new MappingToReferenceScale(Guid.NewGuid(), null, null);
            var mtrsr = new CDP4CommonView.MappingToReferenceScaleRowViewModel(mtrs, this.session.Object, null);
            this.viewmodel.MappingToReferenceScale.Add(mtrsr);
            this.viewmodel.SelectedMappingToReferenceScale = this.viewmodel.MappingToReferenceScale.First();

            Assert.IsTrue(this.viewmodel.InspectMappingToReferenceScaleCommand.CanExecute(null));
            Assert.IsTrue(this.viewmodel.EditMappingToReferenceScaleCommand.CanExecute(null));
            Assert.IsTrue(this.viewmodel.DeleteMappingToReferenceScaleCommand.CanExecute(null));

            this.viewmodel.ValueDefinition.Clear();
            Assert.IsFalse(this.viewmodel.CreateMappingToReferenceScaleCommand.CanExecute(null));
        }

        [Test]
        public void VerifyInspectValueDefinition()
        {
            var vm = this.viewmodel; 
            Assert.IsNull(vm.SelectedValueDefinition);

            vm.SelectedValueDefinition = vm.ValueDefinition.First();
            Assert.IsTrue(vm.InspectValueDefinitionCommand.CanExecute(null));
            vm.InspectValueDefinitionCommand.Execute(null);
            this.navigation.Verify(x => x.Navigate(It.IsAny<ScaleValueDefinition>(), It.IsAny<ThingTransaction>(), this.session.Object, false, ThingDialogKind.Inspect, this.navigation.Object, It.IsAny<Thing>(), null));
        }

        [Test]
        public void VerifyInspectMappingToReferenceScale()
        {
            var vm = this.viewmodel;
            Assert.IsNull(vm.SelectedMappingToReferenceScale);

            var mtrs = new MappingToReferenceScale(Guid.NewGuid(), null, null);
            var mtrsr = new CDP4CommonView.MappingToReferenceScaleRowViewModel(mtrs, this.session.Object, null);
            vm.MappingToReferenceScale.Add(mtrsr);
            vm.SelectedMappingToReferenceScale = vm.MappingToReferenceScale.First();
            Assert.IsTrue(vm.InspectMappingToReferenceScaleCommand.CanExecute(null));
            vm.InspectMappingToReferenceScaleCommand.Execute(null);
            this.navigation.Verify(x => x.Navigate(It.IsAny<MappingToReferenceScale>(), It.IsAny<ThingTransaction>(), this.session.Object, false, ThingDialogKind.Inspect, this.navigation.Object, It.IsAny<Thing>(), null));
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            var dialogViewModel = new MappingToReferenceScale();
            Assert.IsNotNull(dialogViewModel.Iid);
        }

        [Test]
        public void VerifyReactiveCommandsCanExecuteOnInspectMode()
        {
            var vm = new RatioScaleDialogViewModel(this.ratioScale, this.transaction, this.session.Object, true, ThingDialogKind.Inspect, this.navigation.Object, this.rdl);
            Assert.IsFalse(vm.CreateAliasCommand.CanExecute(null));
            Assert.IsFalse(vm.CreateDefinitionCommand.CanExecute(null));
            Assert.IsFalse(vm.CreateHyperLinkCommand.CanExecute(null));
            Assert.IsFalse(vm.CreateMappingToReferenceScaleCommand.CanExecute(null));
            Assert.IsFalse(vm.CreateValueDefinitionCommand.CanExecute(null));

            vm.SelectedValueDefinition = this.viewmodel.ValueDefinition.First();
            var mtrs = new MappingToReferenceScale(Guid.NewGuid(), null, null);
            var mtrsr = new CDP4CommonView.MappingToReferenceScaleRowViewModel(mtrs, this.session.Object, null);
            vm.SelectedMappingToReferenceScale = mtrsr;

            Assert.IsTrue(vm.InspectValueDefinitionCommand.CanExecute(null));
            Assert.IsFalse(vm.EditValueDefinitionCommand.CanExecute(null));
            Assert.IsFalse(vm.DeleteValueDefinitionCommand.CanExecute(null));
            Assert.IsTrue(vm.InspectMappingToReferenceScaleCommand.CanExecute(null));
            Assert.IsFalse(vm.EditMappingToReferenceScaleCommand.CanExecute(null));
            Assert.IsFalse(vm.DeleteMappingToReferenceScaleCommand.CanExecute(null));
        }
    }
}