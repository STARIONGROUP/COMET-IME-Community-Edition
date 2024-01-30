// -------------------------------------------------------------------------------------------------
// <copyright file="ParticipantRoleDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using CDP4SiteDirectory.ViewModels;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    internal class ParticipantRoleDialogViewModelTestFixture
    {
        private ParticipantRoleDialogViewModel viewmodel;
        private ParticipantRole participantRole;
        private SiteDirectory siteDir;
        private ThingTransaction transaction;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IServiceLocator> serviceLocator;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private SiteDirectory clone;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.serviceLocator = new Mock<IServiceLocator>();
            var metadataProvider = new MetaDataProvider();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IMetaDataProvider>()).Returns(metadataProvider);
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, null);
            var person = new Person(Guid.NewGuid(), this.cache, null) { Container = this.siteDir };
            this.session.Setup(x => x.ActivePerson).Returns(person);
            this.session.Setup(x => x.IsVersionSupported(It.IsAny<Version>())).Returns(true);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.participantRole = new ParticipantRole { Name = "Participant role", ShortName = "ParticipantParticipantRole" };

            this.cache.TryAdd(new CacheKey(this.siteDir.Iid, null), new Lazy<Thing>(() => this.siteDir));
            this.clone = this.siteDir.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            this.transaction = new ThingTransaction(transactionContext, this.clone);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());

            this.viewmodel = new ParticipantRoleDialogViewModel(this.participantRole, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, this.clone);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            Assert.AreEqual(this.viewmodel.Name, this.participantRole.Name);
            Assert.AreEqual(this.viewmodel.ShortName, this.participantRole.ShortName);
            Assert.AreEqual(this.participantRole.IsDeprecated, this.viewmodel.IsDeprecated);
            Assert.IsTrue(this.viewmodel.IsPersonEditable);
        }

        [Test]
        public async Task VerifyThatOkCommandWorks()
        {
            await this.viewmodel.OkCommand.Execute();

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
            Assert.IsNull(this.viewmodel.WriteException);
            Assert.IsTrue(this.viewmodel.DialogResult.Value);
            Assert.AreEqual(49, this.transaction.AddedThing.Count());
        }

        [Test]
        public async Task VerifyThatExceptionAreCaught()
        {
            this.session.Setup(x => x.Write(It.IsAny<OperationContainer>())).Throws(new Exception("test"));

            ((ICommand)this.viewmodel.OkCommand).Execute(default);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));

            Assert.IsNotNull(this.viewmodel.WriteException);
            Assert.AreEqual("test", this.viewmodel.WriteException.Message);
        }

        [Test]
        public void VerifyThatDefaultConstructorDoesNotThrowException()
        {
            var participantRoleDialogViewModel = new ParticipantRoleDialogViewModel();
            Assert.IsNotNull(participantRoleDialogViewModel);
        }

        [Test]
        public void VerifyThatAllParticipantPermissionsAreSet()
        {
            var vm = new ParticipantRoleDialogViewModel(this.participantRole, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, this.clone);

            Assert.AreEqual(this.participantRole.ParticipantPermission.Count, vm.ParticipantPermission.Count);

            var participantRoleWith10Permissions = this.participantRole.Clone(false);
            participantRoleWith10Permissions.ParticipantPermission.RemoveRange(9, this.participantRole.ParticipantPermission.Count - 10);

            Assert.AreEqual(10, participantRoleWith10Permissions.ParticipantPermission.Count);

            vm = new ParticipantRoleDialogViewModel(participantRoleWith10Permissions, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, this.clone);

            Assert.AreEqual(this.participantRole.ParticipantPermission.Count, vm.ParticipantPermission.Count);

            // Check that if the version is not supported the participant role doesn't have any ParticipantPermision
            this.session.Setup(x => x.IsVersionSupported(It.IsAny<Version>())).Returns(false);
            vm = new ParticipantRoleDialogViewModel(participantRoleWith10Permissions, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, this.clone);
            Assert.AreEqual(0, vm.ParticipantPermission.Count);
        }

        [Test]
        public async Task VerifyParticipantRoleUpdate()
        {
            var vm = new ParticipantRoleDialogViewModel(this.participantRole, this.transaction, this.session.Object, true, ThingDialogKind.Update, null, this.clone);
            Assert.AreEqual(ParticipantAccessRightKind.NONE, vm.ParticipantPermission.First().AccessRight);

            vm.ParticipantPermission.First().AccessRight = ParticipantAccessRightKind.READ;
            await vm.OkCommand.Execute();
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
            this.messageBus.SendObjectChangeEvent(this.participantRole, EventKind.Updated);

            Assert.AreEqual(ParticipantAccessRightKind.READ, vm.ParticipantPermission.First().AccessRight);
        }
    }
}
