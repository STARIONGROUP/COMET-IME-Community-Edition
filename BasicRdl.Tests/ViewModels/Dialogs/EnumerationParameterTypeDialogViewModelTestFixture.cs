// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnumerationParameterTypeDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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
    using System.Linq;
    using System.Reactive.Concurrency;

    using BasicRdl.ViewModels;

    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Common.SiteDirectoryData;
    
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
 
    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;
 
    using Microsoft.Practices.ServiceLocation;
 
    using Moq;
 
    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    internal class EnumerationParameterTypeDialogViewModelTestFixture
    {
        private EnumerationParameterTypeDialogViewModel viewmodel;
        private EnumerationParameterType enumerationParameterType;
        private SiteDirectory siteDir;
        private ThingTransaction transaction;
        private Mock<ISession> session;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IThingDialogNavigationService> navigation;
        private Mock<IPermissionService> permissionService;
        private ConcurrentDictionary<Guid, Lazy<Thing>> cache;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigation = new Mock<IThingDialogNavigationService>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>()).Returns(this.navigation.Object);

            this.session = new Mock<ISession>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            var person = new Person(Guid.NewGuid(), null, null) { Container = this.siteDir };
            this.session.Setup(x => x.ActivePerson).Returns(person);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), null, null);
            this.siteDir.Person.Add(person);
            var rdl = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null) { Name = "testRDL", ShortName = "test" };
            this.enumerationParameterType = new EnumerationParameterType(Guid.NewGuid(), null, null) { Name = "enumerationParameterType", ShortName = "cat" };
            var testValueDefinition = new EnumerationValueDefinition{ Name = "definition1", ShortName = "def" };
            this.enumerationParameterType.ValueDefinition.Add(testValueDefinition);

            this.siteDir.SiteReferenceDataLibrary.Add(rdl);

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            this.transaction = new ThingTransaction(transactionContext, null);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new HashSet<ReferenceDataLibrary>(this.siteDir.SiteReferenceDataLibrary));

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());

            this.viewmodel = new EnumerationParameterTypeDialogViewModel(this.enumerationParameterType, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.navigation.Object);
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            Assert.AreEqual(this.viewmodel.Name, this.enumerationParameterType.Name);
            Assert.AreEqual(this.viewmodel.ShortName, this.enumerationParameterType.ShortName);
            Assert.AreEqual(this.viewmodel.IsDeprecated, this.enumerationParameterType.IsDeprecated);
            Assert.AreEqual(this.viewmodel.Symbol, this.enumerationParameterType.Symbol);
            Assert.IsNotEmpty(this.viewmodel.ValueDefinition);
            Assert.IsNotEmpty(this.viewmodel.PossibleContainer);
        }

        [Test]
        public void VerifyUpdateOkCanExecute()
        {
            Assert.AreEqual(1, this.viewmodel.ValueDefinition.Count);
            Assert.IsTrue(this.viewmodel.OkCommand.CanExecute(null));
            this.viewmodel.ValueDefinition.Clear();
            Assert.IsFalse(this.viewmodel.OkCommand.CanExecute(null));
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
            var dialogViewModel = new EnumerationParameterTypeDialogViewModel();
            Assert.IsFalse(dialogViewModel.IsDeprecated);
        }

        [Test]
        public void VerifValueDefinitionCommands()
        {
            Assert.IsTrue(this.viewmodel.CreateValueDefinitionCommand.CanExecute(null));
            Assert.IsFalse(this.viewmodel.InspectValueDefinitionCommand.CanExecute(null));
            Assert.IsFalse(this.viewmodel.EditValueDefinitionCommand.CanExecute(null));
            Assert.IsFalse(this.viewmodel.DeleteValueDefinitionCommand.CanExecute(null));
            Assert.IsFalse(this.viewmodel.MoveUpValueDefinitionCommand.CanExecute(null));
            Assert.IsFalse(this.viewmodel.MoveDownValueDefinitionCommand.CanExecute(null));

            this.viewmodel.SelectedValueDefinition = this.viewmodel.ValueDefinition.First();

            Assert.IsTrue(this.viewmodel.InspectValueDefinitionCommand.CanExecute(null));
            Assert.IsTrue(this.viewmodel.EditValueDefinitionCommand.CanExecute(null));
            Assert.IsTrue(this.viewmodel.DeleteValueDefinitionCommand.CanExecute(null));

            Assert.IsTrue(this.viewmodel.MoveUpValueDefinitionCommand.CanExecute(null));
            Assert.IsTrue(this.viewmodel.MoveDownValueDefinitionCommand.CanExecute(null));
        }

        [Test]
        public void VerifyInspectValueDefinition()
        {
            this.viewmodel.SelectedValueDefinition = this.viewmodel.ValueDefinition.First();
            Assert.IsTrue(this.viewmodel.InspectValueDefinitionCommand.CanExecute(null));
            this.viewmodel.InspectValueDefinitionCommand.Execute(null);
            this.navigation.Verify(x => x.Navigate(It.IsAny<EnumerationValueDefinition>(), It.IsAny<ThingTransaction>(), this.session.Object, false, ThingDialogKind.Inspect, this.navigation.Object, It.IsAny<Thing>(), null));
        }
    }
}