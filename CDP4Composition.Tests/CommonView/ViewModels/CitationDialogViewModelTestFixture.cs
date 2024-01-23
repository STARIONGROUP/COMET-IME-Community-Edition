// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CitationDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4CommonView.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Reactive.Concurrency;

    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4CommonView.ViewModels;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Operations;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="CitationDialogViewModelTestFixture"/>
    /// </summary>
    [TestFixture]
    public class CitationDialogViewModelTestFixture
    {
        private CitationDialogViewModel viewmodel;
        private Citation citation;
        private ReferenceSource referenceSource;
        private ThingTransaction transaction;
        private Mock<ISession> session;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IThingDialogNavigationService> navigation;
        private SiteDirectory siteDirectory;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private SiteReferenceDataLibrary[] chainOfContainers;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigation = new Mock<IThingDialogNavigationService>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>()).Returns(this.navigation.Object);
            this.session = new Mock<ISession>();
            this.referenceSource = new ReferenceSource(Guid.NewGuid(), null, null) { Name = "Referencesource", ShortName = "RSO", IsDeprecated = true, };
            this.citation = new Citation(Guid.NewGuid(), null, null) { ShortName = "CIT", Location = "location", IsAdaptation = true, Remark = "remark" };
            this.citation.Source = this.referenceSource;
            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), null, null);

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDirectory);
            this.transaction = new ThingTransaction(transactionContext, null);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            this.session.Setup(x => x.CDPMessageBus).Returns(new CDPMessageBus());
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());

            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            var rdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, null) { Name = "testRDL", ShortName = "test" };
            var simpleQuantityKind = new SimpleQuantityKind();
            var testDerivedQuantityKind = new DerivedQuantityKind(Guid.NewGuid(), this.cache, null) { Name = "Test Derived QK", ShortName = "tdqk" };
            rdl.ParameterType.Add(simpleQuantityKind);
            rdl.ParameterType.Add(testDerivedQuantityKind);
            this.chainOfContainers = new[] { rdl };
        }

        /// <summary>
        /// Basic method to test creating an empty <see cref="CitationDialogViewModelTestFixture"/>
        /// </summary>
        [Test]
        public void VerifyCreateNewEmptyAliasDialogViewModel()
        {
            this.viewmodel = new CitationDialogViewModel();
            Assert.IsNotNull(this.viewmodel);
        }

        /// <summary>
        /// Basic method to test creating a <see cref="CitationDialogViewModelTestFixture"/>
        /// </summary>
        [Test]
        public void VerifyCreateNewAliasDialogViewModel()
        {
            this.viewmodel = new CitationDialogViewModel(this.citation, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.navigation.Object, null, this.chainOfContainers);
            Assert.IsNotNull(this.viewmodel);
        }
    }
}
