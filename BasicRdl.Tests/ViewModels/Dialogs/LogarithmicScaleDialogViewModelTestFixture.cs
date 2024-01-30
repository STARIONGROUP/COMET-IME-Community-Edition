// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogarithmicScaleDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace BasicRdl.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Common.SiteDirectoryData;

    using CDP4CommonView;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    using LogarithmicScaleDialogViewModel = BasicRdl.ViewModels.LogarithmicScaleDialogViewModel;

    [TestFixture]
    internal class LogarithmicScaleDialogViewModelTestFixture
    {
        private LogarithmicScaleDialogViewModel viewmodel;
        private LogarithmicScale logarithmicScale;
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
            var referenceQuantityValue = new ScaleReferenceQuantityValue(Guid.NewGuid(), null, null);
            this.rdl = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null) { Name = "testRDL", ShortName = "test" };
            this.rdl.Unit.Add(simpleUnit);
            this.rdl.Scale.Add(testScale);
            var svd1 = new ScaleValueDefinition(Guid.NewGuid(), null, null) { Name = "ReferenceSVD", ShortName = "RSVD" };
            var svd2 = new ScaleValueDefinition(Guid.NewGuid(), null, null) { Name = "DependentSVD", ShortName = "DSVD" };
            this.logarithmicScale = new LogarithmicScale(Guid.NewGuid(), null, null) { Name = "logarithmicScale", ShortName = "dqk" };
            referenceQuantityValue.Scale = this.logarithmicScale;

            this.logarithmicScale.ValueDefinition.Add(svd1);
            this.logarithmicScale.ValueDefinition.Add(svd2);
            this.logarithmicScale.ReferenceQuantityValue.Add(referenceQuantityValue);
            this.rdl.ParameterType.Add(new SimpleQuantityKind { Name = "testSQK", ShortName = "tSQK" });
            this.siteDir.SiteReferenceDataLibrary.Add(this.rdl);

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            this.transaction = new ThingTransaction(transactionContext, null);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new HashSet<ReferenceDataLibrary>(this.siteDir.SiteReferenceDataLibrary));

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            this.session.Setup(x => x.CDPMessageBus).Returns(new CDPMessageBus());
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());

            this.viewmodel = new LogarithmicScaleDialogViewModel(this.logarithmicScale, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.navigation.Object);
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            Assert.AreEqual(this.viewmodel.Name, this.logarithmicScale.Name);
            Assert.AreEqual(this.viewmodel.ShortName, this.logarithmicScale.ShortName);
            Assert.AreEqual(this.viewmodel.IsDeprecated, this.logarithmicScale.IsDeprecated);
            Assert.AreEqual(this.viewmodel.MappingToReferenceScale, this.logarithmicScale.MappingToReferenceScale);
            Assert.AreEqual(this.viewmodel.IsMaximumInclusive, this.logarithmicScale.IsMaximumInclusive);
            Assert.AreEqual(this.viewmodel.IsMinimumInclusive, this.logarithmicScale.IsMinimumInclusive);
            Assert.AreEqual(this.viewmodel.MaximumPermissibleValue, this.logarithmicScale.MaximumPermissibleValue);
            Assert.AreEqual(this.viewmodel.MinimumPermissibleValue, this.logarithmicScale.MinimumPermissibleValue);
            Assert.AreEqual(this.viewmodel.NegativeValueConnotation, this.logarithmicScale.NegativeValueConnotation);
            Assert.AreEqual(this.viewmodel.PositiveValueConnotation, this.logarithmicScale.PositiveValueConnotation);
            Assert.IsTrue(this.viewmodel.PossibleUnit.Any());
        }

        [Test]
        public void VerifyUpdateOkCanExecute()
        {
            Assert.IsNull(this.viewmodel.SelectedUnit);
            Assert.IsFalse(((ICommand)this.viewmodel.OkCommand).CanExecute(null));

            this.viewmodel.SelectedUnit = this.viewmodel.PossibleUnit.First();
            Assert.IsTrue(((ICommand)this.viewmodel.OkCommand).CanExecute(null));
        }

        [Test]
        public void VerifValueDefinitionCommands()
        {
            Assert.IsTrue(((ICommand)this.viewmodel.CreateValueDefinitionCommand).CanExecute(null));
            Assert.IsFalse(((ICommand)this.viewmodel.InspectValueDefinitionCommand).CanExecute(null));
            Assert.IsFalse(((ICommand)this.viewmodel.EditValueDefinitionCommand).CanExecute(null));
            Assert.IsFalse(((ICommand)this.viewmodel.DeleteValueDefinitionCommand).CanExecute(null));

            this.viewmodel.SelectedValueDefinition = this.viewmodel.ValueDefinition.First();

            Assert.IsTrue(((ICommand)this.viewmodel.InspectValueDefinitionCommand).CanExecute(null));
            Assert.IsTrue(((ICommand)this.viewmodel.EditValueDefinitionCommand).CanExecute(null));
            Assert.IsTrue(((ICommand)this.viewmodel.DeleteValueDefinitionCommand).CanExecute(null));
        }

        [Test]
        public void VerifMappingToReferenceScaleCommands()
        {
            Assert.IsTrue(((ICommand)this.viewmodel.CreateMappingToReferenceScaleCommand).CanExecute(null));
            Assert.IsFalse(((ICommand)this.viewmodel.InspectMappingToReferenceScaleCommand).CanExecute(null));
            Assert.IsFalse(((ICommand)this.viewmodel.EditMappingToReferenceScaleCommand).CanExecute(null));
            Assert.IsFalse(((ICommand)this.viewmodel.DeleteMappingToReferenceScaleCommand).CanExecute(null));

            var mtrs = new MappingToReferenceScale(Guid.NewGuid(), null, null);
            var mtrsr = new MappingToReferenceScaleRowViewModel(mtrs, this.session.Object, null);
            this.viewmodel.MappingToReferenceScale.Add(mtrsr);
            this.viewmodel.SelectedMappingToReferenceScale = this.viewmodel.MappingToReferenceScale.First();
            this.viewmodel.SelectedMappingToReferenceScale = this.viewmodel.MappingToReferenceScale.First();

            Assert.IsTrue(((ICommand)this.viewmodel.InspectMappingToReferenceScaleCommand).CanExecute(null));
            Assert.IsTrue(((ICommand)this.viewmodel.EditMappingToReferenceScaleCommand).CanExecute(null));
            Assert.IsTrue(((ICommand)this.viewmodel.DeleteMappingToReferenceScaleCommand).CanExecute(null));

            this.viewmodel.ValueDefinition.Clear();
            Assert.IsFalse(((ICommand)this.viewmodel.CreateMappingToReferenceScaleCommand).CanExecute(null));
        }

        [Test]
        public void VerifReferenceQuantityKindCommands()
        {
            Assert.IsFalse(((ICommand)this.viewmodel.CreateReferenceQuantityValueCommand).CanExecute(null));
            Assert.IsFalse(((ICommand)this.viewmodel.InspectReferenceQuantityValueCommand).CanExecute(null));
            Assert.IsFalse(((ICommand)this.viewmodel.EditReferenceQuantityValueCommand).CanExecute(null));
            Assert.IsFalse(((ICommand)this.viewmodel.DeleteReferenceQuantityValueCommand).CanExecute(null));

            this.viewmodel.SelectedReferenceQuantityValue = this.viewmodel.ReferenceQuantityValue.First();

            Assert.IsTrue(((ICommand)this.viewmodel.InspectReferenceQuantityValueCommand).CanExecute(null));
            Assert.IsTrue(((ICommand)this.viewmodel.EditReferenceQuantityValueCommand).CanExecute(null));
            Assert.IsTrue(((ICommand)this.viewmodel.DeleteReferenceQuantityValueCommand).CanExecute(null));
            Assert.IsFalse(((ICommand)this.viewmodel.CreateReferenceQuantityValueCommand).CanExecute(null));

            this.viewmodel.ReferenceQuantityValue.Clear();
            Assert.IsTrue(((ICommand)this.viewmodel.CreateReferenceQuantityValueCommand).CanExecute(null));
        }

        [Test]
        public async Task VerifyInspectValueDefinition()
        {
            var vm = this.viewmodel;
            Assert.IsNull(vm.SelectedValueDefinition);

            vm.SelectedValueDefinition = vm.ValueDefinition.First();
            Assert.IsTrue(((ICommand)vm.InspectValueDefinitionCommand).CanExecute(null));
            await vm.InspectValueDefinitionCommand.Execute();
            this.navigation.Verify(x => x.Navigate(It.IsAny<ScaleValueDefinition>(), It.IsAny<ThingTransaction>(), this.session.Object, false, ThingDialogKind.Inspect, this.navigation.Object, It.IsAny<Thing>(), null));
        }

        [Test]
        public async Task VerifyInspectMappingToReferenceScale()
        {
            var vm = this.viewmodel;
            Assert.IsNull(vm.SelectedMappingToReferenceScale);

            var mtrs = new MappingToReferenceScale(Guid.NewGuid(), null, null);
            var mtrsr = new MappingToReferenceScaleRowViewModel(mtrs, this.session.Object, null);
            vm.MappingToReferenceScale.Add(mtrsr);
            vm.SelectedMappingToReferenceScale = vm.MappingToReferenceScale.First();
            Assert.IsTrue(((ICommand)vm.InspectMappingToReferenceScaleCommand).CanExecute(null));
            await vm.InspectMappingToReferenceScaleCommand.Execute();
            this.navigation.Verify(x => x.Navigate(It.IsAny<MappingToReferenceScale>(), It.IsAny<ThingTransaction>(), this.session.Object, false, ThingDialogKind.Inspect, this.navigation.Object, It.IsAny<Thing>(), null));
        }

        [Test]
        public async Task VerifyInspectReferenceQuantityKindValue()
        {
            var vm = this.viewmodel;
            this.viewmodel.SelectedReferenceQuantityValue = this.viewmodel.ReferenceQuantityValue.First();
            Assert.IsTrue(((ICommand)vm.InspectReferenceQuantityValueCommand).CanExecute(null));
            await vm.InspectReferenceQuantityValueCommand.Execute();
            this.navigation.Verify(x => x.Navigate(It.IsAny<ScaleReferenceQuantityValue>(), It.IsAny<ThingTransaction>(), this.session.Object, false, ThingDialogKind.Inspect, this.navigation.Object, It.IsAny<Thing>(), null));
        }

        [Test]
        public async Task VerifyInspectReferenceQuantityKind()
        {
            var vm = this.viewmodel;
            this.viewmodel.SelectedReferenceQuantityKind = this.viewmodel.PossibleReferenceQuantityKind.First();
            Assert.IsTrue(((ICommand)vm.InspectSelectedReferenceQuantityKindCommand).CanExecute(null));
            await vm.InspectSelectedReferenceQuantityKindCommand.Execute();
            this.navigation.Verify(x => x.Navigate(It.IsAny<QuantityKind>(), It.IsAny<ThingTransaction>(), this.session.Object, false, ThingDialogKind.Inspect, this.navigation.Object, It.IsAny<Thing>(), null));
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            var dialogViewModel = new LogarithmicScaleDialogViewModel();
            Assert.IsFalse(dialogViewModel.IsReadOnly);
        }
    }
}
