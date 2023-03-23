// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompoundParameterTypeDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace BasicRdl.Tests.ViewModels
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

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
    internal class CompoundParameterTypeDialogViewModelTestFixture
    {
        private CompoundParameterType compoundPt;
        private SiteDirectory siteDir;
        private SiteReferenceDataLibrary srdl;
        private Category cat;
        private BooleanParameterType bpt;
        private CompoundParameterType cpt;
        private SimpleQuantityKind qt;
        private MeasurementScale scale;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService; 
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        
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
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext);
            var viewmodel = new CompoundParameterTypeDialogViewModel(this.compoundPt, transaction, this.session.Object, true, ThingDialogKind.Create, null);

            Assert.AreEqual(viewmodel.Name, this.compoundPt.Name);
            Assert.AreEqual(viewmodel.ShortName, this.compoundPt.ShortName);
            Assert.AreEqual(viewmodel.IsDeprecated, this.compoundPt.IsDeprecated);
            Assert.AreEqual(viewmodel.Symbol, this.compoundPt.Symbol);
            Assert.IsNotEmpty(viewmodel.PossibleContainer);
            Assert.IsNotEmpty(viewmodel.PossibleCategory);
            Assert.AreEqual(2, viewmodel.PossibleParameterType.Count);
        }

        [Test]
        public async Task VerifyThatCreateComponentWork()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext);
            var viewmodel = new CompoundParameterTypeDialogViewModel(this.compoundPt, transaction, this.session.Object, true, ThingDialogKind.Create, null, null);
            Assert.IsEmpty(viewmodel.Component);
            await viewmodel.CreateComponentCommand.Execute();
            Assert.AreEqual(1, viewmodel.Component.Count);

            Assert.IsFalse(((ICommand)viewmodel.OkCommand).CanExecute(null));

            var componentRow = (ParameterTypeComponentRowViewModel)viewmodel.Component.Single();
            componentRow.ParameterType = componentRow.PossibleParameterType.First();

            var newValue = componentRow["ShortName"]; //Normally gets called from the UI
            Assert.IsFalse(((ICommand)viewmodel.OkCommand).CanExecute(null));

            viewmodel.Component.First().ShortName = "FirstComponent";
            newValue = componentRow["ShortName"]; //Normally gets called from the UI
            Assert.IsTrue(((ICommand)viewmodel.OkCommand).CanExecute(null));

            await viewmodel.CreateComponentCommand.Execute();
            viewmodel.Component.Last().ShortName = "SecondComponent";
            newValue = componentRow["ShortName"]; //Normally gets called from the UI
            Assert.AreEqual(2, viewmodel.Component.Count);
            Assert.IsFalse(((ICommand)viewmodel.OkCommand).CanExecute(null));

            var c2Row = (ParameterTypeComponentRowViewModel)viewmodel.Component.Last();
            c2Row.ParameterType = c2Row.PossibleParameterType.Single(x => x is SimpleQuantityKind);
            Assert.IsNotNull(c2Row.Scale);
            Assert.IsTrue(((ICommand)viewmodel.OkCommand).CanExecute(null));
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            Assert.DoesNotThrow(() => new CompoundParameterTypeDialogViewModel());
        }

        [Test]
        public void VerifyThatParameterTypeComponentHasValidShortName()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext);
            var viewmodel = new CompoundParameterTypeDialogViewModel(this.compoundPt, transaction, this.session.Object, true, ThingDialogKind.Create, null);

            var parameterTypeComponent = new ParameterTypeComponent(Guid.NewGuid(), this.cache, null) { ShortName = "Acc", ParameterType = this.bpt, Scale = this.scale };
            var parameterTypeComponentVm = new ParameterTypeComponentRowViewModel(parameterTypeComponent, this.session.Object, viewmodel);
            viewmodel.Component.Add(parameterTypeComponentVm);

            Assert.IsTrue(viewmodel.OkCanExecute);

            parameterTypeComponentVm.ShortName = "";
            var newValue = parameterTypeComponentVm["ShortName"]; //Normally gets called from the UI

            Assert.IsFalse(viewmodel.OkCanExecute);
        }
    }
}