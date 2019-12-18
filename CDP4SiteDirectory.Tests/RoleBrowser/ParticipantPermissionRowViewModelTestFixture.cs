// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParticipantPermissionRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests.RoleBrowser
{
    using System;
    using System.Collections.Concurrent;
    using System.Reactive.Concurrency;
    using CDP4Common.CommonData;
    using CDP4Common.Types;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using CDP4SiteDirectory.ViewModels;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    [TestFixture]
    internal class ParticipantPermissionRowViewModelTestFixture
    {
        private Mock<IThingDialogNavigationService> thingDialogNavigation;
        private Mock<IPermissionService> permissionService;
        private Mock<ISession> session;
        private SiteDirectory siteDir;
        private ParticipantRole role;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache; 
        private readonly Uri uri = new Uri("http://test.com");

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.thingDialogNavigation = new Mock<IThingDialogNavigationService>();
            this.permissionService = new Mock<IPermissionService>();
            this.session = new Mock<ISession>();
            this.siteDir = new SiteDirectory(Guid.NewGuid(), null, this.uri);
            this.role = new ParticipantRole(Guid.NewGuid(), null, this.uri) { Name = "aa" };
            this.siteDir.ParticipantRole.Add(this.role);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString());
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
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
            var permission = new ParticipantPermission(Guid.NewGuid(), this.cache, this.uri)
            {
                ObjectClass = ClassKind.Alias,
                AccessRight = ParticipantAccessRightKind.MODIFY
            };

            this.role.ParticipantPermission.Add(permission);
            this.cache.TryAdd(new CacheKey(permission.Iid, null), new Lazy<Thing>(() => permission));

            var row = new ParticipantPermissionRowViewModel(permission, this.session.Object, null);

            Assert.AreEqual(row.ObjectClass, permission.ObjectClass);
            Assert.AreEqual(row.AccessRight, permission.AccessRight);
            
            Assert.That(row.Name, Is.Not.Null.Or.Empty);
            Assert.IsNotNull(row.ShortName);
        }
    }
}