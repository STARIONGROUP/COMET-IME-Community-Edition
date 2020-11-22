// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypeComponentRowViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace BasicRdl.Tests.ViewModels.Dialogs.Rows
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using BasicRdl.ViewModels;
    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Common.Types;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Dal;
    using CDP4Dal.DAL;
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
        private ParameterTypeComponent parameterType;
        private SiteDirectory siteDir;
        private SiteReferenceDataLibrary srdl;
        private Category cat;
        private BooleanParameterType bpt;
        private CompoundParameterType cpt;
        private MeasurementScale scale;
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

            this.scale = new OrdinalScale(Guid.NewGuid(), this.cache, null);
            this.srdl.Scale.Add(this.scale);
            this.qt.PossibleScale.Add(this.scale);

            this.cache.TryAdd(new CacheKey(this.srdl.Iid, null), new Lazy<Thing>(() => this.srdl));
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
            this.parameterType = new ParameterTypeComponent();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext);
            var viewmodel = new CompoundParameterTypeDialogViewModel(this.compoundPt, transaction, this.session.Object, true, ThingDialogKind.Create, null);
            var parameterTypeComponent = new ParameterTypeComponentRowViewModel(this.parameterType, this.session.Object, viewmodel);
            Assert.False(parameterTypeComponent.IsReadOnly);
            Assert.Null(parameterTypeComponent.SelectedFilter);
            Assert.Null(parameterTypeComponent.Coordinates);
            Assert.AreEqual(parameterTypeComponent.PossibleParameterType.Count, 2);
            Assert.AreEqual(parameterTypeComponent.PossibleScale.Count, 0);
        }

        [Test]
        public void VerifyThatFilterSelectsCorrectPossibleParameterType()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext);
            var viewmodel = new CompoundParameterTypeDialogViewModel(this.compoundPt, transaction, this.session.Object, true, ThingDialogKind.Create, null);
            var parameterTypeComponent = new ParameterTypeComponentRowViewModel(this.parameterType, this.session.Object, viewmodel);
            parameterTypeComponent.SelectedFilter = typeof(SimpleQuantityKind).Name;
            Assert.True(parameterTypeComponent.PossibleParameterType[0] is SimpleQuantityKind);
        }

        [Test]
        public void VerifyThatPossibleParameterTypesArePopulatedCorrectly()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext);
            var viewmodel = new CompoundParameterTypeDialogViewModel(this.compoundPt, transaction, this.session.Object, true, ThingDialogKind.Create, null);
            var parameterTypeComponent = new ParameterTypeComponentRowViewModel(this.parameterType, this.session.Object, viewmodel);
            Assert.True(parameterTypeComponent.PossibleParameterType.Count.Equals(2));
        }

        [Test]
        public void VerifyThatPossibleParameterTypesArePopulatedAgainWhenFilterIsEmpty()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext);
            var viewmodel = new CompoundParameterTypeDialogViewModel(this.compoundPt, transaction, this.session.Object, true, ThingDialogKind.Create, null);
            var parameterTypeComponent = new ParameterTypeComponentRowViewModel(this.parameterType, this.session.Object, viewmodel);
            parameterTypeComponent.SelectedFilter = typeof(SimpleQuantityKind).Name;
            Assert.True(parameterTypeComponent.PossibleParameterType[0] is SimpleQuantityKind);
            Assert.True(parameterTypeComponent.PossibleParameterType.Count.Equals(1));
            parameterTypeComponent.SelectedFilter = null;
            Assert.True(parameterTypeComponent.PossibleParameterType.Count.Equals(2));
        }

    }
}
