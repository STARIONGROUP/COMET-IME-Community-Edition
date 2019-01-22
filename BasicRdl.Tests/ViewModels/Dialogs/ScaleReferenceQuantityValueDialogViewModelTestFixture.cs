// -------------------------------------------------------------------------------------------------
// <copyright file="ScaleReferenceQuantityValueDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace BasicRdl.Tests.ViewModels
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reactive.Concurrency;
    using BasicRdl.ViewModels.Dialogs;
    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Common.Types;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.DAL;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    [TestFixture]
    internal class ScaleReferenceQuantityValueDialogViewModelTestFixture
    {
        private ScaleReferenceQuantityValueDialogViewModel viewmodel;
        private ScaleReferenceQuantityValue scaleReferenceQuantityValue;
        private SiteDirectory siteDir;
        private ThingTransaction transaction;
        private Mock<ISession> session;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IThingDialogNavigationService> navigation;


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

            this.session = new Mock<ISession>();
            var person = new Person(Guid.NewGuid(), this.cache, null) { Container = this.siteDir };
            this.session.Setup(x => x.ActivePerson).Returns(person);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, null);
            this.siteDir.Person.Add(person);
            var rdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, null) { Name = "testRDL", ShortName = "test" };
            this.scaleReferenceQuantityValue = new ScaleReferenceQuantityValue();
            var testLogarithmicScale = new LogarithmicScale(Guid.NewGuid(), this.cache, null) { Name = "Test Derived QK", ShortName = "tdqk" };
            var simpleQuantityKind = new SimpleQuantityKind();
            rdl.ParameterType.Add(simpleQuantityKind);
            rdl.Scale.Add(testLogarithmicScale);
            this.siteDir.SiteReferenceDataLibrary.Add(rdl);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            var chainOfContainers = new[] { rdl };

            var clone = testLogarithmicScale.Clone(false);
            this.cache.TryAdd(new CacheKey(testLogarithmicScale.Iid, null), new Lazy<Thing>(() => testLogarithmicScale));

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            this.transaction = new ThingTransaction(transactionContext, clone);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());

            this.viewmodel = new ScaleReferenceQuantityValueDialogViewModel(this.scaleReferenceQuantityValue, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, clone, chainOfContainers);
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            Assert.AreEqual(this.viewmodel.ScaleReferenceQuantityValue, this.scaleReferenceQuantityValue.Value);
            Assert.AreEqual(this.viewmodel.SelectedScale, this.scaleReferenceQuantityValue.Scale);
            Assert.IsNotEmpty(this.viewmodel.PossibleScale);
        }

        [Test]
        public async void VerifyThatOkCommandWorks()
        {
            this.viewmodel.OkCommand.Execute(null);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
            Assert.IsNull(this.viewmodel.WriteException);
            Assert.IsTrue(this.viewmodel.DialogResult.Value);
        }

        [Test]
        public async void VerifyThatExceptionAreCaught()
        {
            this.session.Setup(x => x.Write(It.IsAny<OperationContainer>())).Throws(new Exception("test"));

            this.viewmodel.OkCommand.Execute(null);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));

            Assert.IsNotNull(this.viewmodel.WriteException);
            Assert.AreEqual("test", this.viewmodel.WriteException.Message);
        }

        [Test]
        public void VerifyUpdateOkCanExecute()
        {
            Assert.IsFalse(this.viewmodel.OkCommand.CanExecute(null));
            Assert.IsNullOrEmpty(this.viewmodel.ScaleReferenceQuantityValue);
            Assert.IsNotEmpty(this.viewmodel.PossibleScale);
            Assert.IsNull(this.viewmodel.SelectedScale);
            this.viewmodel.ScaleReferenceQuantityValue = "1";
            Assert.IsFalse(this.viewmodel.OkCommand.CanExecute(null));
            this.viewmodel.SelectedScale = this.viewmodel.PossibleScale.First();
            Assert.IsTrue(this.viewmodel.OkCommand.CanExecute(null));
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            var dialogViewModel = new ScaleReferenceQuantityValueDialogViewModel();
            Assert.IsFalse(dialogViewModel.IsReadOnly);
        }
    }
}