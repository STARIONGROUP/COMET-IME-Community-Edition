// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScaleValueDefinitionDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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
    using System.Reactive.Concurrency;

    using BasicRdl.ViewModels.Dialogs;

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
    internal class ScaleValueDefinitionDialogViewModelTestFixture
    {
        private ScaleValueDefinitionDialogViewModel viewmodel;
        private ScaleValueDefinition scaleValueDefinition;
        private SiteDirectory siteDir;
        private ThingTransaction transaction;
        private Mock<ISession> session;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IThingDialogNavigationService> navigation;
        private Mock<IPermissionService> permissionService;
        private RatioScale testRatioScale;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigation = new Mock<IThingDialogNavigationService>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>()).Returns(this.navigation.Object);

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);

            this.session = new Mock<ISession>();
            var person = new Person(Guid.NewGuid(), this.cache, null) { Container = this.siteDir };
            this.session.Setup(x => x.ActivePerson).Returns(person);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, null);
            this.siteDir.Person.Add(person);
            var rdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, null) { Name = "testRDL", ShortName = "test" };
            this.scaleValueDefinition = new ScaleValueDefinition();
            this.siteDir.SiteReferenceDataLibrary.Add(rdl);
            this.testRatioScale = new RatioScale(Guid.NewGuid(), this.cache, null) { Name = "ratioScale", ShortName = "dqk" };
            var svd1 = new ScaleValueDefinition(Guid.NewGuid(), this.cache, null) { Name = "ReferenceSVD", ShortName = "RSVD" };
            var svd2 = new ScaleValueDefinition(Guid.NewGuid(), this.cache, null) { Name = "DependentSVD", ShortName = "DSVD" };
            this.testRatioScale.ValueDefinition.Add(svd1);
            this.testRatioScale.ValueDefinition.Add(svd2);

            var clone = this.testRatioScale.Clone(false);
            this.cache.TryAdd(new CacheKey(this.testRatioScale.Iid, null), new Lazy<Thing>(() => this.testRatioScale));

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            this.transaction = new ThingTransaction(transactionContext, clone);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());

            this.viewmodel = new ScaleValueDefinitionDialogViewModel(this.scaleValueDefinition, this.transaction, this.session.Object, false, ThingDialogKind.Create, this.navigation.Object, clone);
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            Assert.AreEqual(this.scaleValueDefinition.Name, this.viewmodel.Name);
            Assert.AreEqual(this.scaleValueDefinition.Name, this.viewmodel.ShortName);
            Assert.IsNull(this.viewmodel.ScaleValueDefinition);
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            var dialogViewModel = new ScaleValueDefinitionDialogViewModel();
            Assert.IsFalse(dialogViewModel.IsReadOnly);
        }
    }
}