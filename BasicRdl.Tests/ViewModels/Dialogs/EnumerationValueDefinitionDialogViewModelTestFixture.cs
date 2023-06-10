// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnumerationValueDefinitionDialogViewModleTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2023 RHEA System S.A.
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

namespace BasicRdl.Tests.ViewModels.Dialogs
{
    using System;
    using System.Reactive.Concurrency;

    using BasicRdl.ViewModels;

    using CDP4Common.MetaInfo;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    internal class EnumerationValueDefinitionDialogViewModleTestFixture
    {
        private Uri uri = new Uri("http://test.com");
        private DerivedUnit derivedUnit = null;
        private SiteDirectory siteDir;
        private SiteReferenceDataLibrary siteRdl;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> navigation;
        private ThingTransaction transaction;
        private EnumerationValueDefinitionDialogViewModel viewModel;
        private EnumerationValueDefinition testEnumerationValueDefinition;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.navigation = new Mock<IThingDialogNavigationService>();
            this.permissionService = new Mock<IPermissionService>();
            this.session = new Mock<ISession>();
            
            this.siteDir = new SiteDirectory(Guid.NewGuid(), null, this.uri);
            this.siteRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), null, this.uri);
            this.siteDir.SiteReferenceDataLibrary.Add(this.siteRdl);

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            this.transaction = new ThingTransaction(transactionContext, null);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.testEnumerationValueDefinition = new EnumerationValueDefinition(Guid.NewGuid(), null, null);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());

            this.viewModel = new EnumerationValueDefinitionDialogViewModel(this.testEnumerationValueDefinition, this.transaction, this.session.Object, true,
    ThingDialogKind.Create, this.navigation.Object, this.derivedUnit);
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            Assert.AreEqual(this.viewModel.Name, this.testEnumerationValueDefinition.Name);
            Assert.AreEqual(this.viewModel.ShortName, this.testEnumerationValueDefinition.ShortName);
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            var dialogViewModel = new EnumerationValueDefinitionDialogViewModel();
            Assert.IsNotNull(dialogViewModel.IsReadOnly);
        }
    }
}