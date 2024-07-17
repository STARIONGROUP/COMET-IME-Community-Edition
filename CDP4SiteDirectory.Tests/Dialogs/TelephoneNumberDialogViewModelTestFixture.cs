// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TelephoneNumberDialogViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4SiteDirectory.Tests.Dialogs
{
    using System;
    using System.Collections.Concurrent;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

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

    using CDP4SiteDirectory.ViewModels;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    internal class TelephoneNumberDialogViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IThingDialogNavigationService> navigation;
        private Mock<IPermissionService> permissionService;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private SiteDirectory siteDirectory;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
            this.navigation = new Mock<IThingDialogNavigationService>();
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();

            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), this.cache, null);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [TearDown]
        public void Teardown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public async Task VerifyThatSetDefaultWorks()
        {
            var person = new Person();
            var phone = new TelephoneNumber();

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDirectory);
            var transaction = new ThingTransaction(transactionContext, person);

            var vm = new TelephoneNumberDialogViewModel(phone, transaction, this.session.Object, false, ThingDialogKind.Create, this.navigation.Object, person);

            vm.IsDefault = true;
            await vm.OkCommand.Execute();

            Assert.AreSame(person.DefaultTelephoneNumber, phone);
        }
    }
}
