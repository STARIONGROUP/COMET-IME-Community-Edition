// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagramControlContextMenuViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace CDP4Grapher.Tests.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Threading;

    using CDP4Grapher.Behaviors;
    using CDP4Grapher.Utilities;
    using CDP4Grapher.ViewModels;

    using DevExpress.Diagram.Core;
    using DevExpress.Diagram.Core.Layout;
    using DevExpress.Xpf.Bars;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture, Apartment(ApartmentState.STA)]
    public class DiagramControlContextMenuViewModelTestFixture
    {
        private Mock<IGrapherOrgChartBehavior> behavior;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.behavior = new Mock<IGrapherOrgChartBehavior>();
            this.behavior.Setup(x => x.ExportGraph(It.IsAny<DiagramExportFormat>()));
            this.behavior.Setup(x => x.ApplySpecifiedLayout(It.IsAny<LayoutEnumeration>()));
            this.behavior.Setup(x => x.ApplySpecifiedLayout(It.IsAny<LayoutEnumeration>(), It.IsAny<Enum>()));
        }

        [Test]
        public void VerifyProperties()
        {
            var vm = new DiagramControlContextMenuViewModel { Behavior = this.behavior.Object};
            Assert.IsNotEmpty(vm.ContextMenu);
            Assert.AreEqual(4, vm.ContextMenu.Count);
            Assert.AreEqual(6, vm.ContextMenu.OfType<BarSubItem>().SelectMany(x => x.Items).Count());
            Assert.AreEqual(12, vm.ContextMenu.OfType<BarSubItem>().SelectMany(x => x.Items.OfType<BarSubItem>().SelectMany(s =>s.Items)).Count());
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
        }
        
        [Test]
        public void VerifyExportCommand()
        {
            var vm = new DiagramControlContextMenuViewModel { Behavior = this.behavior.Object };
            Assert.IsTrue(vm.ContextMenu.Any());
            Assert.IsTrue(vm.ExportGraphAsPdf.CanExecute(null));
            Assert.IsTrue(vm.ExportGraphAsJpg.CanExecute(null));
            vm.ExportGraphAsJpg.Execute(null);
            vm.ExportGraphAsPdf.Execute(null);
            this.behavior.Verify(x => x.ExportGraph(DiagramExportFormat.JPEG), Times.Once);
            this.behavior.Verify(x => x.ExportGraph(DiagramExportFormat.PDF), Times.Once);
        }

        [Test]
        public void VerifyApplyLayout()
        {
            var vm = new DiagramControlContextMenuViewModel { Behavior = this.behavior.Object };

            vm.ApplyTreeViewLayoutRightToLeft.Execute(null);
            this.behavior.Verify(x => x.ApplySpecifiedLayout(LayoutEnumeration.TreeView, LayoutDirection.RightToLeft), Times.Once);
            vm.ApplyFugiyamaLayoutBottomToTop.Execute(null);
            this.behavior.Verify(x => x.ApplySpecifiedLayout(LayoutEnumeration.Fugiyama, Direction.Up), Times.Once);
            vm.ApplyCircularLayout.Execute(null);
            this.behavior.Verify(x => x.ApplySpecifiedLayout(LayoutEnumeration.Circular), Times.Once);
        }

        [Test]
        public void VerifyIsolation()
        {
            var observable = new Mock<IObservable<bool>>();

            this.behavior.Setup(x => x.CanIsolateObservable).Returns(observable.Object);
            var vm = new DiagramControlContextMenuViewModel { Behavior = this.behavior.Object };
            this.behavior.Setup(x => x.ExitIsolation());
            this.behavior.Setup(x => x.Isolate()).Returns(false);
            vm.IsolateCommand.Execute(null);
            Assert.IsTrue(vm.CanIsolate);
            this.behavior.Setup(x => x.Isolate()).Returns(true);
            vm.IsolateCommand.Execute(null);
            Assert.IsFalse(vm.CanIsolate);
            vm.IsolateCommand.Execute(null);
            Assert.IsTrue(vm.CanIsolate);
            this.behavior.Verify(x => x.Isolate(), Times.Exactly(2));
            this.behavior.Verify(x => x.ExitIsolation(), Times.Once);
        }
    }
}
