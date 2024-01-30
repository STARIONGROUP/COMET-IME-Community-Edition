// -------------------------------------------------------------------------------------------------
// <copyright file="CopyConfirmationDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4CommonView.ViewModels;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="CopyConfirmationDialogViewModel"/> class
    /// </summary>
    [TestFixture]
    public class CopyConfirmationDialogViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Uri uri = new Uri("http://www.rheagroup.com");
        private Assembler assembler;
        private ElementDefinition elementDefinition;
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.assembler = new Assembler(this.uri, this.messageBus);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);

            this.elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri);
        }

        [Test]
        public async Task VerifyThatDialogCanBeConstructed()
        {
            var copyableThings = new List<Thing> { this.elementDefinition };

            var dialog = new CopyConfirmationDialogViewModel(copyableThings, new Dictionary<Thing, string>());

            Assert.AreEqual("Copy Confirmation", dialog.Title);

            Assert.IsTrue(((ICommand)dialog.ProceedCommand).CanExecute(null));
            Assert.IsTrue(((ICommand)dialog.CancelCommand).CanExecute(null));

            Assert.IsNotEmpty(dialog.CopyPermissionDetails);
            Assert.AreEqual("A partial copy will be performed.", dialog.CopyPermissionMessage);
        }

        [Test]
        public async Task VerifyThatIfNoThingsToCopyTheProceedCommandIsDisabled()
        {
            var copyableThings = new List<Thing>();

            var dialog = new CopyConfirmationDialogViewModel(copyableThings, new Dictionary<Thing, string>());

            Assert.IsFalse(((ICommand)dialog.ProceedCommand).CanExecute(null));
            Assert.IsEmpty(dialog.CopyPermissionDetails);
            Assert.AreEqual("The copy operation cannot be performed.", dialog.CopyPermissionMessage);
        }

        [Test]
        public async Task VerifyThatTheDialogResultIsPositiveWhenTheProceedCommandIsExecuted()
        {
            var copyableThings = new List<Thing> { this.elementDefinition };

            var dialog = new CopyConfirmationDialogViewModel(copyableThings, new Dictionary<Thing, string>());

            await dialog.ProceedCommand.Execute();

            Assert.IsTrue((bool)dialog.DialogResult.Result);
        }

        [Test]
        public async Task VerifyThatTheDialogResultIsNegativeWhenTheCancelCommandIsExecuted()
        {
            var copyableThings = new List<Thing> { this.elementDefinition };

            var dialog = new CopyConfirmationDialogViewModel(copyableThings, new Dictionary<Thing, string>());

            await dialog.CancelCommand.Execute();

            Assert.IsFalse((bool)dialog.DialogResult.Result);
        }
    }
}
