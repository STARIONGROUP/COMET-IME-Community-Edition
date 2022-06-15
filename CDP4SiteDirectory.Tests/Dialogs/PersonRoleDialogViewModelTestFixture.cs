// -------------------------------------------------------------------------------------------------
// <copyright file="PersonRoleDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests.Dialogs
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading.Tasks;
    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Common.Types;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    using CDP4SiteDirectory.ViewModels;
    using CommonServiceLocator;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    internal class PersonRoleDialogViewModelTestFixture
    {
        private PersonRoleDialogViewModel viewmodel;
        private PersonRole personRole;
        private SiteDirectory siteDir;
        private ThingTransaction transaction;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private SiteDirectory clone;
        private Mock<IServiceLocator> serviceLocator;

        [SetUp]
        public void Setup()
        {
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            var uri = new Uri("http://www.rheagroup.com");
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.serviceLocator = new Mock<IServiceLocator>();
            var metadataProvider = new MetaDataProvider();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IMetaDataProvider>()).Returns(metadataProvider);
            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, uri);
            var person = new Person(Guid.NewGuid(), this.cache, uri) { Container = this.siteDir };
            this.session.Setup(x => x.ActivePerson).Returns(person);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.IsVersionSupported(It.IsAny<Version>())).Returns(true);
            this.personRole = new PersonRole { Name = "Person role", ShortName = "personRole", IDalUri = uri };
            this.siteDir.PersonRole.Add(this.personRole);
            this.cache.TryAdd(new CacheKey(this.siteDir.Iid, null), new Lazy<Thing>(() => this.siteDir));
            this.clone = this.siteDir.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            this.transaction = new ThingTransaction(transactionContext, this.clone);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());

            this.viewmodel = new PersonRoleDialogViewModel(this.personRole, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, this.clone);
        }

        [TearDown]
        public void Teardown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            Assert.AreEqual(this.viewmodel.Name, this.personRole.Name);
            Assert.AreEqual(this.viewmodel.ShortName, this.personRole.ShortName);
            Assert.AreEqual(this.personRole.IsDeprecated, this.viewmodel.IsDeprecated);
        }

        [Test]
        public async Task VerifyThatOkCommandWorks()
        {
            this.viewmodel.OkCommand.Execute(null);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
            Assert.IsNull(this.viewmodel.WriteException);
            Assert.IsTrue(this.viewmodel.DialogResult.Value);
            Assert.AreEqual(18, this.transaction.AddedThing.Count());
        }

        [Test]
        public async Task VerifyThatExceptionAreCaught()
        {
            this.session.Setup(x => x.Write(It.IsAny<OperationContainer>())).Throws(new Exception("test"));

            this.viewmodel.OkCommand.Execute(null);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));

            Assert.IsNotNull(this.viewmodel.WriteException);
            Assert.AreEqual("test", this.viewmodel.WriteException.Message);
        }

        [Test]
        public void VerifyThatDefaultConstructorDoesNotThrowException()
        {
            var personRoleDialogViewModel = new PersonRoleDialogViewModel();
            Assert.IsNotNull(personRoleDialogViewModel);
        }

        [Test]
        public void VerifyPersonRoleUpdate()
        {
            var vm = new PersonRoleDialogViewModel(this.personRole, this.transaction, this.session.Object, true, ThingDialogKind.Update, null, this.clone);
            Assert.AreEqual(PersonAccessRightKind.NONE, vm.PersonPermission.First().AccessRight);

            vm.PersonPermission.First().AccessRight = PersonAccessRightKind.READ;
            vm.OkCommand.Execute(null);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
            CDPMessageBus.Current.SendObjectChangeEvent(this.personRole, EventKind.Updated);

            Assert.AreEqual(PersonAccessRightKind.READ, vm.PersonPermission.First().AccessRight);
        }

        [Test]
        public void VerifyThatAllPersonPermissionsAreSet()
        {
            var vm = new PersonRoleDialogViewModel(this.personRole, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, this.clone);

            Assert.AreEqual(this.personRole.PersonPermission.Count, vm.PersonPermission.Count);

            var personRoleWith5Permissions = this.personRole.Clone(false);
            personRoleWith5Permissions.PersonPermission.RemoveRange(5, this.personRole.PersonPermission.Count - 5);

            Assert.AreEqual(5, personRoleWith5Permissions.PersonPermission.Count);

            vm = new PersonRoleDialogViewModel(personRoleWith5Permissions, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, this.clone);

            Assert.AreEqual(this.personRole.PersonPermission.Count, vm.PersonPermission.Count);

            // Check that if the version is not supported the person role doesn't have any PersonPermission
            this.session.Setup(x => x.IsVersionSupported(It.IsAny<Version>())).Returns(false);
            vm = new PersonRoleDialogViewModel(personRoleWith5Permissions, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, this.clone);
            Assert.AreEqual(0, vm.PersonPermission.Count);
        }
    }
}