﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParticipantPermissionRowViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4SiteDirectory.Tests.RoleBrowser
{
    using System;
    using System.Collections.Concurrent;
    using System.Reactive.Concurrency;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

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
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.messageBus = new CDPMessageBus();
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
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
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
