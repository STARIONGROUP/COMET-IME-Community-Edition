// -------------------------------------------------------------------------------------------------
// <copyright file="SpecializedQuantityKindDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Permission;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    [TestFixture]
    internal class SpecializedQuantityKindDialogViewModelTestFixture
    {
        private SpecializedQuantityKindDialogViewModel viewmodel;
        private SpecializedQuantityKind specializedQuantityKind;
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

            this.siteDir = new SiteDirectory(Guid.NewGuid(), null, null);
            this.siteDir.Person.Add(person);
            this.rdl = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null) { Name = "testRDL", ShortName = "test" };
            this.specializedQuantityKind = new SpecializedQuantityKind(Guid.NewGuid(), null, null) { Name = "specializedQuantityKind", ShortName = "sqk" };
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

            this.viewmodel = new SpecializedQuantityKindDialogViewModel(this.specializedQuantityKind, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, null, null);
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            Assert.AreEqual(this.viewmodel.Name, this.specializedQuantityKind.Name);
            Assert.AreEqual(this.viewmodel.ShortName, this.specializedQuantityKind.ShortName);
            Assert.AreEqual(this.viewmodel.IsDeprecated, this.specializedQuantityKind.IsDeprecated);
            Assert.AreEqual(this.viewmodel.Symbol, this.specializedQuantityKind.Symbol);
            Assert.AreEqual(this.viewmodel.QuantityDimensionSymbol, this.specializedQuantityKind.QuantityDimensionSymbol);
            Assert.AreEqual(this.viewmodel.SelectedGeneral, this.specializedQuantityKind.General);
            Assert.IsNotEmpty(this.viewmodel.PossibleContainer);
            Assert.IsNotEmpty(this.viewmodel.PossibleGeneral);
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
            Assert.IsEmpty(this.viewmodel.PossibleScale);
            Assert.IsFalse(this.viewmodel.OkCommand.CanExecute(null));
            Assert.IsNull(this.viewmodel.SelectedDefaultScale);
            Assert.IsNull(this.viewmodel.SelectedGeneral);

            this.viewmodel.SelectedDefaultScale = new RatioScale();
            Assert.IsFalse(this.viewmodel.OkCommand.CanExecute(null));
            this.viewmodel.SelectedGeneral = new SimpleQuantityKind();
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
        public void VerifThatUpdatingContainerPopulatesPossibleGenerals()
        {
            this.viewmodel.PossibleGeneral.Clear();
            this.viewmodel.Container = null;
            var rdl = this.siteDir.SiteReferenceDataLibrary.First();
            rdl.ParameterType.Add(new SimpleQuantityKind());

            this.viewmodel.Container = rdl;
            Assert.IsNotEmpty(this.viewmodel.PossibleGeneral);
        }

        [Test]
        public void VerifThatUpdatingGeneralUpdatesPossiblePossibleScales()
        {
            Assert.IsNotEmpty(this.viewmodel.PossibleGeneral);
            Assert.IsNull(this.viewmodel.SelectedGeneral);

            var scale1 = new LogarithmicScale(Guid.NewGuid(), null, null);
            var scale2 = new RatioScale(Guid.NewGuid(), null, null);
            this.rdl.Scale.Add(scale1);
            this.rdl.Scale.Add(scale2);

            var generalQuantityKind = this.viewmodel.PossibleGeneral.First();
            Assert.AreEqual(0, generalQuantityKind.PossibleScale.Count);

            generalQuantityKind.PossibleScale.Add(scale1);
            generalQuantityKind.PossibleScale.Add(scale2);
            this.viewmodel.SelectedGeneral = generalQuantityKind;

            Assert.AreEqual(2, this.viewmodel.GeneralizationScale.Count);
            Assert.AreEqual(0, this.viewmodel.PossiblePossibleScale.Count);
        }

        [Test]
        public void VerifyInspectSelectedGeneral()
        {
            var simpleQuantityKind = new SimpleQuantityKind();
            this.rdl.ParameterType.Add(simpleQuantityKind);
            var vm = new SpecializedQuantityKindDialogViewModel(this.specializedQuantityKind, this.transaction, this.session.Object, true, ThingDialogKind.Inspect, this.navigation.Object, this.rdl);
            Assert.IsNull(vm.SelectedGeneral);
            Assert.IsFalse(vm.InspectSelectedGeneralCommand.CanExecute(null));

            vm.SelectedGeneral = simpleQuantityKind;
            Assert.IsTrue(vm.InspectSelectedGeneralCommand.CanExecute(null));
            vm.InspectSelectedGeneralCommand.Execute(null);
            this.navigation.Verify(x => x.Navigate(It.IsAny<SimpleQuantityKind>(), It.IsAny<ThingTransaction>(), this.session.Object,false, ThingDialogKind.Inspect, this.navigation.Object, It.IsAny<Thing>(), null));            
        }

        [Test]
        public void VerifyInspectSelectedDefaultScale()
        {
            var scale1 = new LogarithmicScale(Guid.NewGuid(), null, null);
            var vm = new SpecializedQuantityKindDialogViewModel(this.specializedQuantityKind, this.transaction, this.session.Object, true, ThingDialogKind.Inspect, this.navigation.Object, this.rdl);
            Assert.IsFalse(vm.InspectSelectedDefaultScaleCommand.CanExecute(null));
            vm.SelectedDefaultScale = scale1;
            Assert.IsTrue(vm.InspectSelectedDefaultScaleCommand.CanExecute(null));
            vm.InspectSelectedDefaultScaleCommand.Execute(null);
            this.navigation.Verify(x => x.Navigate(It.IsAny<MeasurementScale>(), It.IsAny<ThingTransaction>(), this.session.Object, false, ThingDialogKind.Inspect, this.navigation.Object, It.IsAny<Thing>(), null));
        }

        [Test]
        public void CoverEmptyConstructor()
        {
            Assert.DoesNotThrow(() => new SpecializedQuantityKindDialogViewModel());
        }
    }
}