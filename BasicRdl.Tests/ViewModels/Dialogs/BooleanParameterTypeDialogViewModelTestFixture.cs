// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BooleanParameterTypeDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    internal class BooleanParameterTypeDialogViewModelTestFixture
    {
        private BooleanParameterTypeDialogViewModel viewmodel;
        private BooleanParameterType booleanParameterType;
        private SiteDirectory siteDir;
        private ThingTransaction transaction;
        private Mock<ISession> session;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IThingDialogNavigationService> navigation;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Mock<IPermissionService> permissionService;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigation = new Mock<IThingDialogNavigationService>();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>()).Returns(this.navigation.Object);
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.session = new Mock<ISession>();
            var person = new Person(Guid.NewGuid(), null, null) { Container = this.siteDir };
            this.session.Setup(x => x.ActivePerson).Returns(person);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, null);
            this.siteDir.Person.Add(person);
            var rdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, null) { Name = "testRDL", ShortName = "test" };
            this.booleanParameterType = new BooleanParameterType() { Name = "booleanParameterType", ShortName = "cat" };
            var cat = new Category(Guid.NewGuid(), this.cache, null) { Name = "category1", ShortName = "cat1" };
            rdl.DefinedCategory.Add(cat);
            this.siteDir.SiteReferenceDataLibrary.Add(rdl);

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            this.transaction = new ThingTransaction(transactionContext, null);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new HashSet<ReferenceDataLibrary>(this.siteDir.SiteReferenceDataLibrary));
            this.cache.TryAdd(new CacheKey(rdl.Iid, null), new Lazy<Thing>(() => rdl));
            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());

            this.viewmodel = new BooleanParameterTypeDialogViewModel(this.booleanParameterType, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, null, null);
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            Assert.AreEqual(this.viewmodel.Name, this.booleanParameterType.Name);
            Assert.AreEqual(this.viewmodel.ShortName, this.booleanParameterType.ShortName);
            Assert.AreEqual(this.viewmodel.IsDeprecated, this.booleanParameterType.IsDeprecated);
            Assert.AreEqual(this.viewmodel.Symbol, this.booleanParameterType.Symbol);
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
        public void VerifyThatParameterlessContructorExists()
        {
            var dialogViewModel = new BooleanParameterTypeDialogViewModel();
            Assert.IsFalse(dialogViewModel.IsDeprecated);
        }
    }
}