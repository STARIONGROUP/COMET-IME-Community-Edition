// -------------------------------------------------------------------------------------------------
// <copyright file="EngineeringModelSetupDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Threading.Tasks;
    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Common.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4EngineeringModel.ViewModels;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    /// <summary>
    /// suite of tests for the <see cref="EngineeringModelSetupDialogViewModel"/> class.
    /// </summary>
    [TestFixture]
    public class EngineeringModelSetupDialogViewModelTestFixture
    {
        /// <summary>
        /// a write exception message
        /// </summary>
        private const string ExceptionMessage = "A write exception";

        /// <summary>
        /// The view model under test
        /// </summary>
        private EngineeringModelSetupDialogViewModel viewModel;

        /// <summary>
        /// A mock of the session.
        /// </summary>
        private Mock<ISession> session;

        /// <summary>
        /// A mocked <see cref="ISession"/> that throws an exception on Write
        /// </summary>
        private Mock<ISession> sessionThatThrowsWriteException;

        /// <summary>
        /// The uri.
        /// </summary>
        private Uri uri;

        /// <summary>
        /// The <see cref="SiteDirectory"/> that is the container of the <see cref="EngineeringModelSetup"/> that is to be created
        /// or edited using the <see cref="EngineeringModelSetupDialogViewModel"/>
        /// </summary>
        private SiteDirectory siteDirectory;

        /// <summary>
        /// The unique ID of the source <see cref="EngineeringModelSetup"/>
        /// </summary>
        private EngineeringModelSetup sourceEngineeringModelSetup;
        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> cache;
        private SiteDirectory siteDirClone;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.cache = new ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>>();

            this.uri = new Uri("http://test.com");
            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            var sourceEngineeringModelSetupIid = Guid.NewGuid();
            this.sourceEngineeringModelSetup = new EngineeringModelSetup(sourceEngineeringModelSetupIid, this.cache, this.uri);
            this.siteDirectory.Model.Add(sourceEngineeringModelSetup);
            
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.Write(It.IsAny<OperationContainer>())).Returns(Task.FromResult("some result"));
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDirectory);

            this.sessionThatThrowsWriteException = new Mock<ISession>();
            this.sessionThatThrowsWriteException.Setup(x => x.Write(It.IsAny<OperationContainer>())).Throws(new Exception(ExceptionMessage));
            this.sessionThatThrowsWriteException.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDirectory);

            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.siteDirectory.Iid, null), new Lazy<Thing>(() => this.siteDirectory));

            this.siteDirClone = this.siteDirectory.Clone(false);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());

            this.sessionThatThrowsWriteException.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.sessionThatThrowsWriteException.Setup(x => x.Dal).Returns(dal.Object);
        }

        [TearDown]
        public void TearDown()
        {
            this.siteDirectory = null;
            this.uri = null;
            this.viewModel = null;
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatConstructorSetsPropertiesOfNewEngineeringModelSetup()
        {
            // The EngineeringModelSetup that is to be created
            var engineeringModelSetup = new EngineeringModelSetup();
            engineeringModelSetup.SourceEngineeringModelSetupIid = this.sourceEngineeringModelSetup.Iid;

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDirectory);
            var transaction = new ThingTransaction(transactionContext, this.siteDirClone);

            this.viewModel = new EngineeringModelSetupDialogViewModel(engineeringModelSetup, transaction, this.session.Object, true, ThingDialogKind.Create, null, this.siteDirClone);

            Assert.AreEqual(engineeringModelSetup.ShortName, this.viewModel.ShortName);
            Assert.AreEqual(engineeringModelSetup.Name, this.viewModel.Name);
            Assert.IsNotNull(engineeringModelSetup.SourceEngineeringModelSetupIid);
            Assert.AreEqual(this.sourceEngineeringModelSetup.Iid, engineeringModelSetup.SourceEngineeringModelSetupIid);
            Assert.AreEqual(default(StudyPhaseKind), this.viewModel.StudyPhase);
            Assert.Contains(this.sourceEngineeringModelSetup, this.viewModel.PossibleSourceEngineeringModelSetup);
        }

        [Test]
        public void VerifyThatWhenExceptionIsNotNullHasExceptionReturnsTrue()
        {
            var engineeringModelSetup = new EngineeringModelSetup();
            engineeringModelSetup.SourceEngineeringModelSetupIid = this.sourceEngineeringModelSetup.Iid;

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDirectory);
            var transaction = new ThingTransaction(transactionContext, this.siteDirClone);

            this.viewModel = new EngineeringModelSetupDialogViewModel(engineeringModelSetup, transaction, this.session.Object, true, ThingDialogKind.Create, null, this.siteDirClone);
            Assert.IsFalse(this.viewModel.HasException);

            var exception = new Exception(ExceptionMessage);
            this.viewModel.WriteException = exception;

            Assert.IsTrue(this.viewModel.HasException);
            Assert.AreEqual(ExceptionMessage, this.viewModel.WriteException.Message);
        }

        [Test]
        public void VerifyThatWriteExecutesOnTheSessionAndThatPropertiesAreSetFromViewModel()
        {
            var engineeringModelSetup = new EngineeringModelSetup();
            engineeringModelSetup.SourceEngineeringModelSetupIid = this.sourceEngineeringModelSetup.Iid;

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDirectory);
            var transaction = new ThingTransaction(transactionContext, this.siteDirClone);

            this.viewModel = new EngineeringModelSetupDialogViewModel(engineeringModelSetup, transaction, this.session.Object, true, ThingDialogKind.Create, null, this.siteDirClone);

            Assert.IsNotNullOrEmpty(this.viewModel["Name"]);
            Assert.IsFalse(this.viewModel.OkCommand.CanExecute(null));

            var newShortName = "updatedshortname";
            var newName = "updated name";
            var newStudyPhase = StudyPhaseKind.DESIGN_SESSION_PHASE;
            var newSourceId = Guid.NewGuid();

            this.viewModel.ShortName = newShortName;
            this.viewModel.Name = newName;
            this.viewModel.StudyPhase = newStudyPhase;
            var newSourceModel = new EngineeringModelSetup(newSourceId, null, this.uri);
            this.viewModel.SourceEngineeringModelSetup = newSourceModel;

            this.viewModel.OkCommand.Execute(null);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
            Assert.IsNull(this.viewModel.WriteException);
            Assert.IsTrue(this.viewModel.DialogResult.Value);

            var clone = (EngineeringModelSetup)transaction.GetClone(engineeringModelSetup);

            Assert.AreEqual(newShortName, clone.ShortName);
            Assert.AreEqual(newName, clone.Name);
            Assert.AreEqual(newStudyPhase, clone.StudyPhase);
            Assert.AreEqual(newSourceId, clone.SourceEngineeringModelSetupIid);
        }

        [Test]
        public void VerifyThatWriteExceptionCanBeSetFromExecuteOkCommand()
        {
            var engineeringModelSetup = new EngineeringModelSetup();
            engineeringModelSetup.SourceEngineeringModelSetupIid = this.sourceEngineeringModelSetup.Iid;

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDirectory);
            var transaction = new ThingTransaction(transactionContext, this.siteDirClone);

            this.viewModel = new EngineeringModelSetupDialogViewModel(engineeringModelSetup, transaction, this.sessionThatThrowsWriteException.Object, true, ThingDialogKind.Create, null, this.siteDirClone);

            this.viewModel.OkCommand.Execute(null);

            Assert.IsTrue(this.viewModel.HasException);

            Assert.IsNull(this.viewModel.DialogResult);
        }

        [Test]
        public void VerifyThatCancelDoesNotRecordAnyChanges()
        {
            var shortname = "shortname";
            var name = "name";
            var studyPhase = StudyPhaseKind.PREPARATION_PHASE;

            var engineeringModelSetup = new EngineeringModelSetup();
            engineeringModelSetup.ShortName = shortname;
            engineeringModelSetup.Name = name;
            engineeringModelSetup.StudyPhase = studyPhase;
            engineeringModelSetup.SourceEngineeringModelSetupIid = this.sourceEngineeringModelSetup.Iid;

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDirectory);
            var transaction = new ThingTransaction(transactionContext, this.siteDirClone);

            this.viewModel = new EngineeringModelSetupDialogViewModel(engineeringModelSetup, transaction, this.session.Object, true, ThingDialogKind.Create, null, this.siteDirClone);

            var newShortName = "updatedshortname";
            var newName = "updated name";
            var newStudyPhase = StudyPhaseKind.DESIGN_SESSION_PHASE;
            var newSourceId = Guid.NewGuid();
            var newSourceModel = new EngineeringModelSetup(newSourceId, this.cache, this.uri);

            this.viewModel.ShortName = newShortName;
            this.viewModel.Name = newName;
            this.viewModel.StudyPhase = newStudyPhase;
            this.viewModel.SourceEngineeringModelSetup = newSourceModel;

            this.viewModel.CancelCommand.Execute(null);


            Assert.AreEqual(shortname, engineeringModelSetup.ShortName);
            Assert.AreEqual(name, engineeringModelSetup.Name);
            Assert.AreEqual(studyPhase, engineeringModelSetup.StudyPhase);
            Assert.IsNotNull(this.viewModel.SourceEngineeringModelSetupIid);
            Assert.IsFalse(this.viewModel.IsOriginal);
            Assert.IsFalse(this.viewModel.IsNonEditableFieldReadOnly);
            Assert.AreEqual(this.sourceEngineeringModelSetup.Iid, engineeringModelSetup.SourceEngineeringModelSetupIid);

            Assert.IsFalse(this.viewModel.DialogResult.Value);
        }

        [Test]
        public void VerifyOkCommandWithoutSourceModel()
        {
            var engineeringModelSetup = new EngineeringModelSetup();

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDirectory);
            var transaction = new ThingTransaction(transactionContext, this.siteDirClone);
            this.viewModel = new EngineeringModelSetupDialogViewModel(engineeringModelSetup, transaction, this.session.Object, true, ThingDialogKind.Create, null, this.siteDirClone);

            Assert.IsNotNullOrEmpty(this.viewModel["Name"]);
            Assert.IsFalse(this.viewModel.OkCommand.CanExecute(null));

            var newShortName = "EMShortname";
            var newName = "EMName";
            this.viewModel.ShortName = newShortName;
            this.viewModel.Name = newName;
            this.viewModel.StudyPhase = StudyPhaseKind.DESIGN_SESSION_PHASE;
            this.viewModel.SourceEngineeringModelSetup = null;
            var srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, null) { Name = "testRDL", ShortName = "test" };
            this.viewModel.SelectedSiteReferenceDataLibrary = srdl;

            this.viewModel.OkCommand.Execute(null);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
            Assert.IsNull(this.viewModel.WriteException);
            Assert.IsTrue(this.viewModel.DialogResult.Value);

            var clone = (EngineeringModelSetup)transaction.GetClone(engineeringModelSetup);

            Assert.AreEqual(newShortName, clone.ShortName);
            Assert.AreEqual(newName, clone.Name);
            Assert.AreEqual(StudyPhaseKind.DESIGN_SESSION_PHASE, clone.StudyPhase);
            Assert.IsNull(clone.SourceEngineeringModelSetupIid);
            Assert.AreEqual(1, clone.RequiredRdl.Count);
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            var dialogViewModel = new EngineeringModelSetupDialogViewModel();
            Assert.IsNotNull(dialogViewModel.IsReadOnly);
        }

        [Test]
        public void VerifyThatActiveDomainIsCorrectlyPopulated()
        {
            var domain1 = new DomainOfExpertise(Guid.NewGuid(), null, this.uri);
            var domain2 = new DomainOfExpertise(Guid.NewGuid(), null, this.uri); 
            var domain3 = new DomainOfExpertise(Guid.NewGuid(), null, this.uri);

            this.siteDirClone.Domain.Add(domain1);
            this.siteDirClone.Domain.Add(domain2);
            this.siteDirClone.Domain.Add(domain3);

            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            engineeringModelSetup.ActiveDomain.Add(domain1);

            this.cache.TryAdd(new Tuple<Guid, Guid?>(engineeringModelSetup.Iid, null), new Lazy<Thing>(() => engineeringModelSetup));

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDirectory);
            var transaction = new ThingTransaction(transactionContext, this.siteDirClone);

            this.viewModel = new EngineeringModelSetupDialogViewModel(engineeringModelSetup.Clone(false), transaction, this.session.Object, true, ThingDialogKind.Update, null, this.siteDirClone);

            Assert.AreEqual(3, this.viewModel.PossibleActiveDomain.Count);
            Assert.AreEqual(1, this.viewModel.ActiveDomain.Count);
        }
    }
}
