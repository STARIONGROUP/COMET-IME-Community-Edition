// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PersonPermissionRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests.RoleBrowser
{
    using System;
    using System.Collections.Concurrent;
    using System.Reactive.Concurrency;
    using CDP4Common.CommonData;
    using CDP4Common.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using CDP4SiteDirectory.ViewModels;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    [TestFixture]
    internal class PersonPermissionRowViewModelTestFixture
    {
        private Mock<IThingDialogNavigationService> thingDialogNavigation;
        private Mock<IPermissionService> permissionService;
        private Mock<ISession> session;
        private SiteDirectory siteDir;
        private PersonRole personRole;
        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> cache; 
        private readonly Uri uri = new Uri("http://test.com");

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.thingDialogNavigation = new Mock<IThingDialogNavigationService>();
            this.permissionService = new Mock<IPermissionService>();
            this.session = new Mock<ISession>();
            this.siteDir = new SiteDirectory(Guid.NewGuid(), null, this.uri);
            this.personRole = new PersonRole(Guid.NewGuid(), null, this.uri) { Name = "aa" };
            this.siteDir.PersonRole.Add(this.personRole);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString());
            this.cache = new ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);

            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void TestRow()
        {
            var permission = new PersonPermission(Guid.NewGuid(), this.cache, this.uri)
            {
                ObjectClass = ClassKind.Alias,
                AccessRight = PersonAccessRightKind.MODIFY_IF_PARTICIPANT
            };

            this.personRole.PersonPermission.Add(permission);
            this.cache.TryAdd(new Tuple<Guid, Guid?>(permission.Iid, null), new Lazy<Thing>(() => permission));

            var row = new PersonPermissionRowViewModel(permission, this.session.Object, null);

            Assert.AreEqual(row.ObjectClass, permission.ObjectClass);
            Assert.AreEqual(row.AccessRight, permission.AccessRight);

            Assert.IsNotNullOrEmpty(row.Name);
            Assert.IsNotNull(row.ShortName);
            Assert.IsFalse(row.IsReadOnly);

            row.AccessRight = PersonAccessRightKind.NONE;
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }
    }
}