﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GrapherViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4Grapher.Tests.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Threading;
    using System.Windows;

    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal.Events;

    using CDP4Grapher.Tests.Data;
    using CDP4Grapher.ViewModels;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="GrapherViewModel"/> class.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class GrapherViewModelTestFixture : GrapherBaseTestData
    {
        private Mock<IThingDialogNavigationService> thingNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IPluginSettingsService> pluginSettingService;
        private Mock<IHaveContextMenu> diagramControlContextMenu;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.thingNavigationService = new Mock<IThingDialogNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.pluginSettingService = new Mock<IPluginSettingsService>();
            this.diagramControlContextMenu = new Mock<IHaveContextMenu>();
        }

        [Test]
        public void VerifyProperties()
        {
            var vm = new GrapherViewModel(this.Option, this.Session.Object, this.thingNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, this.pluginSettingService.Object)
            {
                DiagramContextMenuViewModel = this.diagramControlContextMenu.Object
            };

            Assert.IsNotEmpty(vm.GraphElements);
            Assert.IsNotNull(vm.DiagramContextMenuViewModel);

            Assert.IsTrue(vm.GraphElements.All(x =>
                x.NestedElementElement.Iid == this.TopElement.Iid ||
                x.NestedElementElement.Iid == this.ElementUsage1.Iid ||
                x.NestedElementElement.Iid == this.ElementUsage2.Iid ||
                x.NestedElementElement.Iid == this.ElementUsage3.Iid
            ));

            Assert.IsNotNull(vm.CurrentModel);
            Assert.IsNotNull(vm.CurrentIteration);
            Assert.IsNotNull(vm.CurrentOption);
            Assert.IsNull(vm.SelectedElementModelCode);
            Assert.IsNull(vm.SelectedElement);
        }

        [Test]
        public void VerifySubscription()
        {
            var vm = new GrapherViewModel(this.Option, this.Session.Object, this.thingNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, this.pluginSettingService.Object);

            this.EngineeringModelSetup.Name = "updatedShortName";
            this.MessageBus.SendObjectChangeEvent(this.EngineeringModelSetup, EventKind.Updated);
            Assert.AreEqual(this.EngineeringModelSetup.Name, vm.CurrentModel);

            this.Iteration.TopElement = this.ElementDefinition1;
            this.MessageBus.SendObjectChangeEvent(this.Iteration, EventKind.Updated);
            Assert.AreEqual(this.Iteration.TopElement, (vm.Thing.Container as Iteration)?.TopElement);

            this.IterationSetup.Description = "updatedDescription";
            this.MessageBus.SendObjectChangeEvent(this.IterationSetup, EventKind.Updated);
            Assert.AreEqual(this.IterationSetup, (vm.Thing.Container as Iteration)?.IterationSetup);

            this.IterationSetup.Description = "updatedDescription";
            this.MessageBus.SendObjectChangeEvent(this.IterationSetup, EventKind.Updated);
            Assert.AreEqual(this.IterationSetup, (vm.Thing.Container as Iteration)?.IterationSetup);

            this.Option.Name = "updatedName";
            this.MessageBus.SendObjectChangeEvent(this.Option, EventKind.Updated);
            Assert.AreEqual(this.Option.Name, vm.Thing.Name);
        }

        [Test]
        public void VerifyIsolate()
        {
            var vm = new GrapherViewModel(this.Option, this.Session.Object, this.thingNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, this.pluginSettingService.Object);
            Assert.AreEqual(1, vm.GraphElements.Count);
            var newTrunk = vm.GraphElements.FirstOrDefault();
            Assert.Throws<InvalidOperationException>(() => vm.Isolate(newTrunk));
            Assert.AreEqual(1, vm.GraphElements.Count);
        }

        [Test]
        public void VerifyExitIsolation()
        {
            var vm = new GrapherViewModel(this.Option, this.Session.Object, this.thingNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, this.pluginSettingService.Object);
            Assert.AreEqual(1, vm.GraphElements.Count);
            vm.ExitIsolation();
            Assert.AreEqual(1, vm.GraphElements.Count);
        }

        [Test]
        public void SetsSelectedElementAndSelectedElementPath()
        {
            var vm = new GrapherViewModel(this.Option, this.Session.Object, this.thingNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, this.pluginSettingService.Object);
            Assert.IsNull(vm.SelectedElementModelCode);
            Assert.IsNull(vm.SelectedElement);
            vm.SetsSelectedElementAndSelectedElementPath(vm.GraphElements.FirstOrDefault());
            Assert.IsNotNull(vm.SelectedElementModelCode);
            Assert.IsNotNull(vm.SelectedElement);
        }

        [Test]
        public void VerifyGrapherWithNoTopElement()
        {
            this.Iteration.TopElement = null;
            var vm = new GrapherViewModel(this.Option, this.Session.Object, this.thingNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, this.pluginSettingService.Object);

            Assert.Multiple(() =>
            {
                Assert.That(vm.GrapherVisibility, Is.EqualTo(Visibility.Collapsed));
                Assert.That(vm.GrapherPlaceholderVisibility, Is.EqualTo(Visibility.Visible));
            });
        }
    }
}
