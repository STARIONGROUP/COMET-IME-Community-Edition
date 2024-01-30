// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypeComponentRowViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace BasicRdl.Tests.ViewModels.Dialogs.Rows
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;

    using BasicRdl.ViewModels;

    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class ParameterTypeComponentRowViewModelTestFixture
    {
        private CompoundParameterType compoundPt;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Mock<ISession> session;
        private readonly Uri uri = new Uri("http://www.rheagroup.com");
        private ParameterTypeComponent parameterTypeComponent;
        private SiteDirectory siteDir;
        private SiteReferenceDataLibrary srdl;
        private Category cat;
        private BooleanParameterType bpt;
        private CompoundParameterType cpt;
        private MeasurementScale scale1;
        private MeasurementScale scale2;
        private MeasurementScale scale3;
        private SimpleQuantityKind qt;
        private Mock<IPermissionService> permissionService;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.permissionService = new Mock<IPermissionService>();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.session = new Mock<ISession>();
            var person = new Person(Guid.NewGuid(), null, null) { Container = this.siteDir };
            this.session.Setup(x => x.ActivePerson).Returns(person);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, null);
            this.siteDir.Person.Add(person);
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, null) { Name = "testRDL", ShortName = "test" };
            this.compoundPt = new CompoundParameterType { Name = "parameterType", ShortName = "cat" };
            this.cat = new Category(Guid.NewGuid(), this.cache, null) { Name = "category1", ShortName = "cat1" };
            this.cat.PermissibleClass.Add(ClassKind.CompoundParameterType);
            this.srdl.DefinedCategory.Add(cat);
            this.siteDir.SiteReferenceDataLibrary.Add(this.srdl);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new HashSet<ReferenceDataLibrary>(this.siteDir.SiteReferenceDataLibrary));
            this.bpt = new BooleanParameterType(Guid.NewGuid(), this.cache, null);
            this.cpt = new CompoundParameterType(Guid.NewGuid(), this.cache, null);

            this.srdl.ParameterType.Add(this.bpt);
            this.srdl.ParameterType.Add(this.cpt);
            this.qt = new SimpleQuantityKind(Guid.NewGuid(), this.cache, null);
            this.srdl.ParameterType.Add(this.qt);

            this.scale1 = new OrdinalScale(Guid.NewGuid(), this.cache, null) {Name = "scale1"};
            this.srdl.Scale.Add(this.scale1);
            this.qt.PossibleScale.Add(this.scale1);

            this.scale2 = new OrdinalScale(Guid.NewGuid(), this.cache, null) {Name = "scale2"};
            this.srdl.Scale.Add(this.scale2);
            this.qt.PossibleScale.Add(this.scale2);

            this.scale3 = new OrdinalScale(Guid.NewGuid(), this.cache, null) {Name = "scale2"};
            this.srdl.Scale.Add(this.scale3);
            this.qt.PossibleScale.Add(this.scale3);

            this.cache.TryAdd(new CacheKey(this.srdl.Iid, null), new Lazy<Thing>(() => this.srdl));
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            this.session.Setup(x => x.CDPMessageBus).Returns(new CDPMessageBus());
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
            this.parameterTypeComponent = new ParameterTypeComponent();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext);
            var viewmodel = new CompoundParameterTypeDialogViewModel(this.compoundPt, transaction, this.session.Object, true, ThingDialogKind.Create, null);
            var parameterTypeComponentVm = new ParameterTypeComponentRowViewModel(this.parameterTypeComponent, this.session.Object, viewmodel);
            Assert.That(parameterTypeComponentVm.IsReadOnly, Is.False);
            Assert.That(parameterTypeComponentVm.SelectedFilter, Is.Null);
            Assert.That(parameterTypeComponentVm.Coordinates, Is.Null);
            Assert.That(parameterTypeComponentVm.PossibleParameterType.Count, Is.EqualTo(2));
            Assert.That(parameterTypeComponentVm.PossibleScale.Count, Is.EqualTo(0));
        }

        [Test]
        public void VerifyThatFilterSelectsCorrectPossibleParameterType()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext);
            var viewmodel = new CompoundParameterTypeDialogViewModel(this.compoundPt, transaction, this.session.Object, true, ThingDialogKind.Create, null);
            var parameterTypeComponentVm = new ParameterTypeComponentRowViewModel(this.parameterTypeComponent, this.session.Object, viewmodel);
            parameterTypeComponentVm.SelectedFilter = nameof(SimpleQuantityKind);
            Assert.That(parameterTypeComponentVm.PossibleParameterType[0] is SimpleQuantityKind, Is.True);
        }

        [Test]
        public void VerifyThatPossibleParameterTypesArePopulatedCorrectly()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext);
            var viewmodel = new CompoundParameterTypeDialogViewModel(this.compoundPt, transaction, this.session.Object, true, ThingDialogKind.Create, null);
            var parameterTypeComponentVm = new ParameterTypeComponentRowViewModel(this.parameterTypeComponent, this.session.Object, viewmodel);
            Assert.That(parameterTypeComponentVm.PossibleParameterType.Count, Is.EqualTo(2));
        }

        [Test]
        public void VerifyThatPossibleParameterTypesArePopulatedAgainWhenFilterIsEmpty()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext);
            var viewmodel = new CompoundParameterTypeDialogViewModel(this.compoundPt, transaction, this.session.Object, true, ThingDialogKind.Create, null);
            var parameterTypeComponentVm = new ParameterTypeComponentRowViewModel(this.parameterTypeComponent, this.session.Object, viewmodel);
            parameterTypeComponentVm.SelectedFilter = nameof(SimpleQuantityKind);
            Assert.That(parameterTypeComponentVm.PossibleParameterType[0] is SimpleQuantityKind, Is.True);
            Assert.That(parameterTypeComponentVm.PossibleParameterType.Count, Is.EqualTo(1));
            parameterTypeComponentVm.SelectedFilter = null;
            Assert.That(parameterTypeComponentVm.PossibleParameterType.Count, Is.EqualTo(2));
        }

        [Test]
        public void VerifyThatPossibleMeasurementScalesArePopulatedCorrectly()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext);
            var viewmodel = new CompoundParameterTypeDialogViewModel(this.compoundPt, transaction, this.session.Object, true, ThingDialogKind.Create, null);

            var parameterTypeComponentVm = new ParameterTypeComponentRowViewModel(this.parameterTypeComponent, this.session.Object, viewmodel);
            parameterTypeComponentVm.ParameterType = this.qt;

            Assert.That(parameterTypeComponentVm.PossibleScale.Count, Is.EqualTo(3));
        }

        [Test]
        public void VerifyThatSelectedMeasurementScaleIsPopulatedCorrectlyWithDefaultScale()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext);
            var viewmodel = new CompoundParameterTypeDialogViewModel(this.compoundPt, transaction, this.session.Object, true, ThingDialogKind.Create, null);

            this.parameterTypeComponent.ParameterType = this.qt;
            this.qt.DefaultScale = this.scale2;

            var parameterTypeComponentVm = new ParameterTypeComponentRowViewModel(this.parameterTypeComponent, this.session.Object, viewmodel);

            Assert.That(parameterTypeComponentVm.Scale, Is.EqualTo(this.scale2));
        }

        [Test]
        public void VerifyThatSelectedMeasurementScaleIsPopulatedCorrectlyWithoutDefaultScale()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext);
            var viewmodel = new CompoundParameterTypeDialogViewModel(this.compoundPt, transaction, this.session.Object, true, ThingDialogKind.Create, null);

            this.parameterTypeComponent.ParameterType = this.qt;

            var parameterTypeComponentVm = new ParameterTypeComponentRowViewModel(this.parameterTypeComponent, this.session.Object, viewmodel);

            Assert.That(parameterTypeComponentVm.Scale, Is.EqualTo(this.scale1));
        }

        [Test]
        public void VerifyThatSelectedMeasurementScaleIsPopulatedCorrectlyForExistingParameterComponentType()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext);
            var viewmodel = new CompoundParameterTypeDialogViewModel(this.compoundPt, transaction, this.session.Object, true, ThingDialogKind.Create, null);

            this.parameterTypeComponent.ParameterType = this.qt;
            this.parameterTypeComponent.Scale = this.scale3;

            var parameterTypeComponentVm = new ParameterTypeComponentRowViewModel(this.parameterTypeComponent, this.session.Object, viewmodel);

            Assert.That(parameterTypeComponentVm.Scale, Is.EqualTo(this.scale3));
        }

        [Test]
        public void VerifyThatShortNameIsValidValue()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext);
            var viewmodel = new CompoundParameterTypeDialogViewModel(this.compoundPt, transaction, this.session.Object, true, ThingDialogKind.Create, null);
            var parameterTypeComponent = new ParameterTypeComponent() { ShortName = "Acc" };
            var parameterTypeComponentVm = new ParameterTypeComponentRowViewModel(parameterTypeComponent, this.session.Object, viewmodel);

            var newValue = parameterTypeComponentVm["ShortName"]; //Normally gets called from the UI
            Assert.IsFalse(parameterTypeComponentVm.HasError);

            parameterTypeComponentVm.ShortName = string.Empty;
            newValue = parameterTypeComponentVm["ShortName"]; //Normally gets called from the UI
            Assert.IsTrue(parameterTypeComponentVm.HasError);
        }
    }
}
