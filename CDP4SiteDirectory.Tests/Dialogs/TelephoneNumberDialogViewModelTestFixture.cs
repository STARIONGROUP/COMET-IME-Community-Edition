// -------------------------------------------------------------------------------------------------
// <copyright file="TelephoneNumberDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests.Dialogs
{
    using System;
    using System.Collections.Concurrent;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Common.Types;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.DAL;
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

        [SetUp]
        public void Setup()
        {
            this.navigation = new Mock<IThingDialogNavigationService>();
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();


            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), this.cache, null);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [TearDown]
        public void Teardown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
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