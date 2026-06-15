// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CitationDialogViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Windows.Input;

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
    /// Suite of tests for the <see cref="CitationDialogViewModel"/>
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
        private SiteReferenceDataLibrary siteRdl;
        private ModelReferenceDataLibrary modelRdl;
        private ReferenceSource alphaSource;
        private ReferenceSource betaSource;
        private ReferenceSource gammaSource;

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
            this.transaction = new ThingTransaction(transactionContext);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            this.session.Setup(x => x.CDPMessageBus).Returns(new CDPMessageBus());
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());

            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.siteRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, null) { Name = "testRDL", ShortName = "test" };
            this.alphaSource = new ReferenceSource(Guid.NewGuid(), this.cache, null) { Name = "Alpha", ShortName = "alpha" };
            this.betaSource = new ReferenceSource(Guid.NewGuid(), this.cache, null) { Name = "Beta", ShortName = "beta" };
            this.siteRdl.ReferenceSource.Add(this.alphaSource);
            this.siteRdl.ReferenceSource.Add(this.betaSource);
            this.modelRdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.cache, null) { Name = "modelRDL", ShortName = "model", RequiredRdl = this.siteRdl };
            this.gammaSource = new ReferenceSource(Guid.NewGuid(), this.cache, null) { Name = "Gamma", ShortName = "gamma" };
            this.modelRdl.ReferenceSource.Add(this.gammaSource);
            this.siteDirectory.SiteReferenceDataLibrary.Add(this.siteRdl);
        }

        /// <summary>
        /// Basic method to test creating an empty <see cref="CitationDialogViewModel"/>
        /// </summary>
        [Test]
        public void VerifyCreateNewEmptyCitationDialogViewModel()
        {
            this.viewmodel = new CitationDialogViewModel();
            Assert.IsNotNull(this.viewmodel);
        }

        /// <summary>
        /// Basic method to test creating a <see cref="CitationDialogViewModel"/>
        /// </summary>
        [Test]
        public void VerifyCreateNewCitationDialogViewModel()
        {
            this.viewmodel = new CitationDialogViewModel(this.citation, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.navigation.Object, null, new[] { this.siteRdl });
            Assert.IsNotNull(this.viewmodel);
        }
        
        /// <summary>
        /// Verifies that the properties of the dialog-view-model are set from the <see cref="Citation"/> it represents.
        /// </summary>
        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            this.viewmodel = new CitationDialogViewModel(this.citation, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.navigation.Object, null, new[] { this.siteRdl });

            Assert.AreEqual(this.citation.Location, this.viewmodel.Location);
            Assert.AreEqual(this.citation.IsAdaptation, this.viewmodel.IsAdaptation);
            Assert.AreEqual(this.citation.Remark, this.viewmodel.Remark);
            Assert.AreEqual(this.citation.ShortName, this.viewmodel.ShortName);
            Assert.AreEqual(this.citation.Source, this.viewmodel.SelectedSource);
        }

        /// <summary>
        /// Verifies that the <see cref="ReferenceSource"/>s defined directly in an RDL in the chain of containers
        /// are available as possible sources. This is the scenario where a Citation is created on the Definition of
        /// a ReferenceSource, which always worked because the containing RDL holds at least that source.
        /// </summary>
        [Test]
        public void VerifyThatPossibleSourceContainsSourcesFromRdlDirectlyInChain()
        {
            this.viewmodel = new CitationDialogViewModel(this.citation, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.navigation.Object, null, new[] { this.siteRdl });

            CollectionAssert.AreEquivalent(new[] { this.alphaSource, this.betaSource }, this.viewmodel.PossibleSource);
        }

        /// <summary>
        /// Verifies that the <see cref="ReferenceSource"/>s defined in the required (chained) RDLs are available as
        /// possible sources. This is the regression scenario: a Citation created on the Definition of a Glossary term
        /// whose Glossary lives in a model RDL that does not itself define any reference source. The dropdown used to
        /// be empty because only the directly-contained sources were considered.
        /// </summary>
        [Test]
        public void VerifyThatPossibleSourceContainsSourcesFromRequiredRdls()
        {
            this.viewmodel = new CitationDialogViewModel(this.citation, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.navigation.Object, null, new[] { this.modelRdl });

            CollectionAssert.AreEquivalent(new[] { this.gammaSource, this.alphaSource, this.betaSource }, this.viewmodel.PossibleSource);
        }

        /// <summary>
        /// Verifies that the possible sources do not contain duplicates when both an RDL and its required RDL are
        /// present in the chain of containers.
        /// </summary>
        [Test]
        public void VerifyThatPossibleSourceDoesNotContainDuplicates()
        {
            this.viewmodel = new CitationDialogViewModel(this.citation, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.navigation.Object, null, new Thing[] { this.modelRdl, this.siteRdl });

            Assert.AreEqual(3, this.viewmodel.PossibleSource.Count);
            CollectionAssert.AreEquivalent(new[] { this.gammaSource, this.alphaSource, this.betaSource }, this.viewmodel.PossibleSource);
        }

        /// <summary>
        /// Verifies that the possible sources are ordered by <see cref="ReferenceSource.Name"/>.
        /// </summary>
        [Test]
        public void VerifyThatPossibleSourceIsOrderedByName()
        {
            this.viewmodel = new CitationDialogViewModel(this.citation, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.navigation.Object, null, new[] { this.modelRdl });

            CollectionAssert.AreEqual(new[] { this.alphaSource, this.betaSource, this.gammaSource }, this.viewmodel.PossibleSource);
        }

        /// <summary>
        /// Verifies that the possible sources are empty when there is no RDL in the chain of containers.
        /// </summary>
        [Test]
        public void VerifyThatPossibleSourceIsEmptyWithoutRdlInChain()
        {
            this.viewmodel = new CitationDialogViewModel(this.citation, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.navigation.Object);

            CollectionAssert.IsEmpty(this.viewmodel.PossibleSource);
        }

        /// <summary>
        /// Verifies that the <see cref="CitationDialogViewModel.InspectSelectedSourceCommand"/> can only be executed
        /// when a source is selected.
        /// </summary>
        [Test]
        public void VerifyThatInspectSelectedSourceCommandCanOnlyExecuteWhenSourceIsSelected()
        {
            this.viewmodel = new CitationDialogViewModel(this.citation, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.navigation.Object, null, [this.siteRdl]);

            this.viewmodel.SelectedSource = this.alphaSource;
            Assert.IsTrue(((ICommand)this.viewmodel.InspectSelectedSourceCommand).CanExecute(null));

            this.viewmodel.SelectedSource = null;
            Assert.IsFalse(((ICommand)this.viewmodel.InspectSelectedSourceCommand).CanExecute(null));
        }

        /// <summary>
        /// Verifies that the transaction is updated with the values of the dialog-view-model.
        /// </summary>
        [Test]
        public void VerifyThatUpdateTransactionSetsTheSource()
        {
            var definitionTransaction = this.SetupTransactionRootedOn(new Definition(Guid.NewGuid(), this.cache, null), out var definitionClone);

            this.viewmodel = new CitationDialogViewModel(this.citation, definitionTransaction, this.session.Object, false, ThingDialogKind.Create, this.navigation.Object, definitionClone, [this.siteRdl])
            {
                Location = "new location",
                IsAdaptation = false,
                Remark = "new remark",
                ShortName = "NEWCIT",
                SelectedSource = this.betaSource
            };

            this.viewmodel.OkCommand.Execute().Subscribe();

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Never());
            var clone = (Citation)definitionTransaction.AddedThing.Single();

            Assert.AreEqual("new location", clone.Location);
            Assert.IsFalse(clone.IsAdaptation);
            Assert.AreEqual("new remark", clone.Remark);
            Assert.AreEqual("NEWCIT", clone.ShortName);
            Assert.AreEqual(this.betaSource, clone.Source);
        }

        /// <summary>
        /// Verifies that a <see cref="Definition"/> container is accepted.
        /// </summary>
        [Test]
        public void VerifyThatDefinitionContainerDoesNotThrow()
        {
            this.SetupTransactionRootedOn(new Definition(Guid.NewGuid(), this.cache, null), out _);

            Assert.DoesNotThrow(() => { });
        }

        /// <summary>
        /// Creates a <see cref="ThingTransaction"/> whose root is the clone of the provided <paramref name="container"/>,
        /// registering the original in the cache. This is required so that a Create dialog can build its sub-transaction
        /// against a container that is known to the parent transaction.
        /// </summary>
        /// <param name="container">The original container <see cref="Thing"/>.</param>
        /// <param name="containerClone">The clone of the <paramref name="container"/> that roots the transaction.</param>
        /// <returns>The <see cref="ThingTransaction"/> rooted on the container clone.</returns>
        private ThingTransaction SetupTransactionRootedOn(Thing container, out Thing containerClone)
        {
            containerClone = container.Clone(false);
            this.cache.TryAdd(new CacheKey(container.Iid, null), new Lazy<Thing>(() => container));
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDirectory);
            return new ThingTransaction(transactionContext, containerClone);
        }
    }
}
