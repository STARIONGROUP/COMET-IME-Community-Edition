// -------------------------------------------------------------------------------------------------
// <copyright file="CyclicRatioScaleDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace BasicRdl.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using BasicRdl.ViewModels;
    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4CommonView;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Permission;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    [TestFixture]
    internal class CyclicRatioScaleDialogViewModelTestFixture
    {
        private BasicRdl.ViewModels.CyclicRatioScaleDialogViewModel viewmodel;
        private CyclicRatioScale cyclicRatioScale;
        private SiteDirectory siteDir;
        private ThingTransaction transaction;
        private Mock<ISession> session;
        private Mock<IThingDialogNavigationService> navigation;
        private Mock<IPermissionService> permissionService;
        private SiteReferenceDataLibrary rdl;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.navigation = new Mock<IThingDialogNavigationService>();
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
            this.cyclicRatioScale = new CyclicRatioScale(Guid.NewGuid(), null, null) { Name = "ratioScale", ShortName = "dqk", Modulus = "modulus"};
            this.cyclicRatioScale.ValueDefinition.Add(svd1);
            this.cyclicRatioScale.ValueDefinition.Add(svd2);
            this.rdl.ParameterType.Add(new SimpleQuantityKind { Name = "testSQK", ShortName = "tSQK" });
            this.siteDir.SiteReferenceDataLibrary.Add(this.rdl);

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            this.transaction = new ThingTransaction(transactionContext);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new HashSet<ReferenceDataLibrary>(this.siteDir.SiteReferenceDataLibrary));

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());

            this.viewmodel = new BasicRdl.ViewModels.CyclicRatioScaleDialogViewModel(this.cyclicRatioScale, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.navigation.Object);
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            Assert.AreEqual(this.viewmodel.Name, this.cyclicRatioScale.Name);
            Assert.AreEqual(this.viewmodel.ShortName, this.cyclicRatioScale.ShortName);
            Assert.AreEqual(this.viewmodel.IsDeprecated, this.cyclicRatioScale.IsDeprecated);
            Assert.AreEqual(this.viewmodel.MappingToReferenceScale, this.cyclicRatioScale.MappingToReferenceScale);
            Assert.AreEqual(this.viewmodel.IsMaximumInclusive, this.cyclicRatioScale.IsMaximumInclusive);
            Assert.AreEqual(this.viewmodel.IsMinimumInclusive, this.cyclicRatioScale.IsMinimumInclusive);
            Assert.AreEqual(this.viewmodel.MaximumPermissibleValue, this.cyclicRatioScale.MaximumPermissibleValue);
            Assert.AreEqual(this.viewmodel.MinimumPermissibleValue, this.cyclicRatioScale.MinimumPermissibleValue);
            Assert.AreEqual(this.viewmodel.NegativeValueConnotation, this.cyclicRatioScale.NegativeValueConnotation);
            Assert.AreEqual(this.viewmodel.PositiveValueConnotation, this.cyclicRatioScale.PositiveValueConnotation);
            Assert.AreEqual(this.viewmodel.Modulus, this.cyclicRatioScale.Modulus);
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
            var mtrsr = new MappingToReferenceScaleRowViewModel(mtrs, this.session.Object, null);
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
            var mtrsr = new MappingToReferenceScaleRowViewModel(mtrs, this.session.Object, null);
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
            var vm = new BasicRdl.ViewModels.CyclicRatioScaleDialogViewModel(this.cyclicRatioScale, this.transaction, this.session.Object, true, ThingDialogKind.Inspect, this.navigation.Object, this.rdl, null);
            Assert.IsFalse(vm.CreateAliasCommand.CanExecute(null));
            Assert.IsFalse(vm.CreateDefinitionCommand.CanExecute(null));
            Assert.IsFalse(vm.CreateHyperLinkCommand.CanExecute(null));
            Assert.IsFalse(vm.CreateMappingToReferenceScaleCommand.CanExecute(null));
            Assert.IsFalse(vm.CreateValueDefinitionCommand.CanExecute(null));

            vm.SelectedValueDefinition = this.viewmodel.ValueDefinition.First();
            var mtrs = new MappingToReferenceScale(Guid.NewGuid(), null, null);
            var mtrsr = new MappingToReferenceScaleRowViewModel(mtrs, this.session.Object, null);
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