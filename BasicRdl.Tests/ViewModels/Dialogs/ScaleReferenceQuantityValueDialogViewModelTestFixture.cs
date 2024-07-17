﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScaleReferenceQuantityValueDialogViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace BasicRdl.Tests.ViewModels
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Windows.Input;

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

    using CommonServiceLocator;

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
            this.session.Setup(x => x.CDPMessageBus).Returns(new CDPMessageBus());
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
        public void VerifyThatOkCommandWorks()
        {
            ((ICommand)this.viewmodel.OkCommand).Execute(default);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
            Assert.IsNull(this.viewmodel.WriteException);
            Assert.IsTrue(this.viewmodel.DialogResult.Value);
        }

        [Test]
        public void VerifyThatExceptionAreCaught()
        {
            this.session.Setup(x => x.Write(It.IsAny<OperationContainer>())).Throws(new Exception("test"));

            ((ICommand)this.viewmodel.OkCommand).Execute(default);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));

            Assert.IsNotNull(this.viewmodel.WriteException);
            Assert.AreEqual("test", this.viewmodel.WriteException.Message);
        }

        [Test]
        public void VerifyUpdateOkCanExecute()
        {
            Assert.IsFalse(((ICommand)this.viewmodel.OkCommand).CanExecute(null));
            Assert.That(this.viewmodel.ScaleReferenceQuantityValue, Is.Null.Or.Empty);
            Assert.IsNotEmpty(this.viewmodel.PossibleScale);
            Assert.IsNull(this.viewmodel.SelectedScale);
            this.viewmodel.ScaleReferenceQuantityValue = "1";
            Assert.IsFalse(((ICommand)this.viewmodel.OkCommand).CanExecute(null));
            this.viewmodel.SelectedScale = this.viewmodel.PossibleScale.First();
            Assert.IsTrue(((ICommand)this.viewmodel.OkCommand).CanExecute(null));
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            var dialogViewModel = new ScaleReferenceQuantityValueDialogViewModel();
            Assert.IsFalse(dialogViewModel.IsReadOnly);
        }
    }
}
