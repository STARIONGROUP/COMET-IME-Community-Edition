﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropertyGridViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4PropertyGrid.Tests
{
    using System.Reactive.Concurrency;
    using System.Threading;
    using System.Windows;

    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Navigation.Events;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using CDP4PropertyGrid.ViewModels;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class PropertyGridViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            if (Application.Current == null)
            {
                new Application();
            }

            this.messageBus = new CDPMessageBus();
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
        }

        [Test]
        public void VerifyThatPropertiesArePopulated()
        {
            var person = new Person();
            var vm = new PropertyGridViewModel(person, this.session.Object);

            Assert.IsNotNull(vm.Thing);
            Assert.IsNotNull(vm.Caption);
            Assert.IsNotNull(vm.ToolTip);
        }

        [Test]
        public void VerifyThatThingChangeOccursWhenSelectedThingChanges()
        {
            var person = new Person();
            var vm = new PropertyGridViewModel(person, this.session.Object);

            var expectedPerson = new Person();

            this.messageBus.SendMessage(new SelectedThingChangedEvent(expectedPerson, this.session.Object));

            Assert.That(vm.Thing, Is.EqualTo(expectedPerson));
        }
    }
}
