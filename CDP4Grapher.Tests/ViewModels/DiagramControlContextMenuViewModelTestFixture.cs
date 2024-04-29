﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagramControlContextMenuViewModelTestFixture.cs" company="Starion Group S.A.">
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
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using CDP4Grapher.Behaviors;
    using CDP4Grapher.Utilities;
    using CDP4Grapher.ViewModels;

    using DevExpress.Diagram.Core;
    using DevExpress.Diagram.Core.Layout;
    using DevExpress.Xpf.Bars;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="DiagramControlContextMenuViewModel"/> class.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class DiagramControlContextMenuViewModelTestFixture
    {
        private Mock<IGrapherOrgChartBehavior> behavior;
        private Option option;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();

            this.option = new Option()
            {
                Name = "test"
            };

            this.behavior = new Mock<IGrapherOrgChartBehavior>();
            this.behavior.Setup(x => x.ExportGraph(It.IsAny<DiagramExportFormat>()));
            this.behavior.Setup(x => x.ApplySpecifiedLayout(It.IsAny<LayoutEnumeration>()));
            this.behavior.Setup(x => x.ApplySpecifiedLayout(It.IsAny<LayoutEnumeration>(), It.IsAny<Enum>()));
        }

        [Test]
        public void VerifyProperties()
        {
            var vm = new DiagramControlContextMenuViewModel { Behavior = this.behavior.Object };
            Assert.IsNotEmpty(vm.ContextMenu);
            Assert.AreEqual(5, vm.ContextMenu.Count);
            Assert.AreEqual(6, vm.ContextMenu.OfType<BarSubItem>().SelectMany(x => x.Items).Count());
            Assert.AreEqual(12, vm.ContextMenu.OfType<BarSubItem>().SelectMany(x => x.Items.OfType<BarSubItem>().SelectMany(s => s.Items)).Count());
            Assert.IsNotNull(vm.Behavior);

            Assert.IsNotNull(vm.IsolateCommand);

            Assert.IsNotNull(vm.ApplyTreeViewLayoutRightToLeft);
            Assert.IsNotNull(vm.ApplyTreeViewLayoutLeftToRight);
            Assert.IsNotNull(vm.ApplyTreeViewLayoutBottomToTop);
            Assert.IsNotNull(vm.ApplyTreeViewLayoutTopToBottom);

            Assert.IsNotNull(vm.ApplyFugiyamaLayoutRightToLeft);
            Assert.IsNotNull(vm.ApplyFugiyamaLayoutLeftToRight);
            Assert.IsNotNull(vm.ApplyFugiyamaLayoutTopToBottom);
            Assert.IsNotNull(vm.ApplyFugiyamaLayoutBottomToTop);

            Assert.IsNotNull(vm.ApplyTipOverLayoutLeftToRight);
            Assert.IsNotNull(vm.ApplyTipOverLayoutRightToLeft);

            Assert.IsNotNull(vm.ApplyMindMapLayoutBottomToTop);
            Assert.IsNotNull(vm.ApplyMindMapLayoutLeftToRight);

            Assert.IsNotNull(vm.ApplyCircularLayout);
            Assert.IsNotNull(vm.ApplyOrganisationalChartLayout);

            Assert.IsNull(vm.HoveredElement);
        }

        [Test]
        public async Task VerifyExportCommand()
        {
            var vm = new DiagramControlContextMenuViewModel { Behavior = this.behavior.Object };
            Assert.IsTrue(vm.ContextMenu.Any());
            Assert.IsTrue(((ICommand)vm.ExportGraphAsPdf).CanExecute(null));
            Assert.IsTrue(((ICommand)vm.ExportGraphAsJpg).CanExecute(null));
            await vm.ExportGraphAsJpg.Execute();
            await vm.ExportGraphAsPdf.Execute();
            this.behavior.Verify(x => x.ExportGraph(DiagramExportFormat.JPEG), Times.Once);
            this.behavior.Verify(x => x.ExportGraph(DiagramExportFormat.PDF), Times.Once);
        }

        [Test]
        public async Task VerifyApplyLayout()
        {
            var vm = new DiagramControlContextMenuViewModel { Behavior = this.behavior.Object };

            await vm.ApplyTreeViewLayoutRightToLeft.Execute();
            this.behavior.Verify(x => x.ApplySpecifiedLayout(LayoutEnumeration.TreeView, LayoutDirection.RightToLeft), Times.Once);
            await vm.ApplyFugiyamaLayoutBottomToTop.Execute();
            this.behavior.Verify(x => x.ApplySpecifiedLayout(LayoutEnumeration.Fugiyama, Direction.Up), Times.Once);
            await vm.ApplyCircularLayout.Execute();
            this.behavior.Verify(x => x.ApplySpecifiedLayout(LayoutEnumeration.Circular), Times.Once);
        }

        [Test]
        public async Task VerifyIsolation()
        {
            this.behavior.Setup(x => x.ExitIsolation());
            this.behavior.Setup(x => x.Isolate()).Returns(false);
            var vm = new DiagramControlContextMenuViewModel { Behavior = this.behavior.Object };

            vm.HoveredElement = null;
            Assert.IsFalse(((ICommand)vm.IsolateCommand).CanExecute(null));

            vm.HoveredElement = new GraphElementViewModel(new NestedElement()
            {
                RootElement = new ElementDefinition(),
                ElementUsage =
                {
                    new ElementUsage()
                    {
                        Container = new ElementDefinition() { Owner = new DomainOfExpertise() },
                        Owner = new DomainOfExpertise(), ElementDefinition = new ElementDefinition()
                    }
                }
            }, this.messageBus);

            Assert.IsTrue(((ICommand)vm.IsolateCommand).CanExecute(null));
            vm.HoveredElement = null;
            Assert.IsFalse(((ICommand)vm.IsolateCommand).CanExecute(null));

            Assert.IsFalse(vm.CanExitIsolation);
            await vm.IsolateCommand.Execute();
            Assert.IsFalse(vm.CanExitIsolation);
            this.behavior.Setup(x => x.Isolate()).Returns(true);
            await vm.IsolateCommand.Execute();
            Assert.IsTrue(vm.CanExitIsolation);
            await vm.IsolateCommand.Execute();
            Assert.IsTrue(vm.CanExitIsolation);
            await vm.ExitIsolationCommand.Execute();
            Assert.IsFalse(vm.CanExitIsolation);
            Assert.IsFalse(((ICommand)vm.ExitIsolationCommand).CanExecute(null));

            this.behavior.Verify(x => x.Isolate(), Times.Exactly(3));
            this.behavior.Verify(x => x.ExitIsolation(), Times.Once);
        }
    }
}
