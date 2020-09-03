// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GrapherOrgChartBehaviorTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4Grapher.Tests.Behaviors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Threading;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;

    using CDP4Grapher.Behaviors;
    using CDP4Grapher.Tests.Data;
    using CDP4Grapher.Utilities;
    using CDP4Grapher.ViewModels;
    using CDP4Grapher.Views;

    using DevExpress.Diagram.Core;
    using DevExpress.Diagram.Core.Layout;
    using DevExpress.Xpf.Diagram;

    using Microsoft.Practices.ServiceLocation;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    using LayoutDirection = DevExpress.Diagram.Core.Layout.LayoutDirection;

    [TestFixture, Apartment(ApartmentState.STA)]
    public class GrapherOrgChartBehaviorTestFixture : GrapherBaseTestData
    {
        private GrapherOrgChartBehavior behavior;
        private Mock<IOpenSaveFileDialogService> saveFileDialog;
        private List<GraphElementViewModel> elementViewModels;
        private Mock<IGrapherViewModel> grapherViewModel;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IHaveContextMenu> contextMenu;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            
            this.elementViewModels = new List<GraphElementViewModel>()
            {
                new GraphElementViewModel(new NestedElement(Guid.NewGuid(), this.Assembler.Cache, this.Uri)
                {
                    RootElement = this.ElementDefinition1,
                    ElementUsage = new OrderedItemList<ElementUsage>(this.Option) { this.ElementUsage1 }
                }, this.Option),

                new GraphElementViewModel(new NestedElement(Guid.NewGuid(), this.Assembler.Cache, this.Uri)
                {
                    RootElement = this.ElementDefinition1,
                    ElementUsage = new OrderedItemList<ElementUsage>(this.Option) { this.ElementUsage1, this.ElementUsage2 }
                }, this.Option)
            };

            this.contextMenu = new Mock<IHaveContextMenu>();
            this.behavior = new GrapherOrgChartBehavior();
            this.grapherViewModel = new Mock<IGrapherViewModel>();
            this.grapherViewModel.Setup(x => x.GraphElements).Returns(new ReactiveList<GraphElementViewModel>(this.elementViewModels));
            this.grapherViewModel.Setup(x => x.Behavior).Returns(this.behavior);
            this.grapherViewModel.Setup(x => x.Isolate(It.IsAny<GraphElementViewModel>()));
            this.grapherViewModel.Setup(x => x.ExitIsolation());
            this.grapherViewModel.Setup(x => x.DiagramContextMenuViewModel).Returns(this.contextMenu.Object);
            this.saveFileDialog = new Mock<IOpenSaveFileDialogService>();
            this.saveFileDialog.Setup(x => x.GetSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns(string.Empty);
            this.serviceLocator = new Mock<IServiceLocator>();
            this.serviceLocator.Setup(x => x.GetInstance<IOpenSaveFileDialogService>()).Returns(this.saveFileDialog.Object);
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
        }

        [TearDown]
        public void Destroy()
        {
            this.behavior?.Detach();
        }

        [Test]
        public void VerifyProperties()
        {
            var diagramControl = new GrapherDiagramControl() { DataContext = this.grapherViewModel.Object };
            this.behavior.Attach(diagramControl);
            Assert.IsTrue(this.behavior.CurrentLayout != default);
            Assert.AreSame(this.behavior.AssociatedObject, diagramControl);
        }
        
        [Test]
        public void VerifyExport()
        {
            this.behavior.Attach(new GrapherDiagramControl());
            this.behavior.ExportGraph(DiagramExportFormat.JPEG);
            this.saveFileDialog.Verify(x => x.GetSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
        }

        [Test]
        public void VerifyApplyPreviousLayout()
        {
            this.behavior.Attach(new GrapherDiagramControl());
            this.behavior.ApplyPreviousLayout();
            Assert.AreEqual(this.behavior.CurrentLayout.layout, this.behavior.CurrentLayout.layout);
            Assert.AreEqual(this.behavior.CurrentLayout.direction, this.behavior.CurrentLayout.direction);
        }

        [Test]
        public void VerifyApplyLayoutWithoutAnyDirection()
        {
            this.behavior.Attach(new GrapherDiagramControl());
            this.behavior.ApplySpecifiedLayout(LayoutEnumeration.Circular);
            Assert.AreEqual(this.behavior.CurrentLayout.layout, LayoutEnumeration.Circular);
            Assert.AreEqual(this.behavior.CurrentLayout.direction, null);
            this.behavior.ApplySpecifiedLayout(LayoutEnumeration.OrganisationalChart);
            Assert.AreEqual(this.behavior.CurrentLayout.layout, LayoutEnumeration.OrganisationalChart);
            Assert.AreEqual(this.behavior.CurrentLayout.direction, null);

            Assert.Throws(typeof(ArgumentOutOfRangeException), () => this.behavior.ApplySpecifiedLayout(LayoutEnumeration.TipOver));
        }

        [Test]
        public void VerifyApplyLayout()
        {
            this.behavior.Attach(new GrapherDiagramControl());
            this.behavior.ApplySpecifiedLayout(LayoutEnumeration.MindMap, OrientationKind.Vertical);
            Assert.AreEqual(this.behavior.CurrentLayout.layout, LayoutEnumeration.MindMap);
            Assert.AreEqual(this.behavior.CurrentLayout.direction, OrientationKind.Vertical);

            this.behavior.ApplySpecifiedLayout(LayoutEnumeration.TipOver, TipOverDirection.RightToLeft);
            Assert.AreEqual(this.behavior.CurrentLayout.layout, LayoutEnumeration.TipOver);
            Assert.AreEqual(this.behavior.CurrentLayout.direction, TipOverDirection.RightToLeft);

            this.behavior.ApplySpecifiedLayout(LayoutEnumeration.Fugiyama, Direction.Right);
            Assert.AreEqual(this.behavior.CurrentLayout.layout, LayoutEnumeration.Fugiyama);
            Assert.AreEqual(this.behavior.CurrentLayout.direction, Direction.Right);

            this.behavior.ApplySpecifiedLayout(LayoutEnumeration.TreeView, LayoutDirection.BottomToTop);
            Assert.AreEqual(this.behavior.CurrentLayout.layout, LayoutEnumeration.TreeView);
            Assert.AreEqual(this.behavior.CurrentLayout.direction, LayoutDirection.BottomToTop);

            Assert.Throws(typeof(ArgumentOutOfRangeException), () => this.behavior.ApplySpecifiedLayout(LayoutEnumeration.TipOver, Direction.Right));
        }

        [Test]
        public void VerifyItemsChanged()
        {
            var diagramControl = new GrapherDiagramControl() { DataContext = this.grapherViewModel.Object };
            this.behavior.Attach(diagramControl);

            foreach (var elementViewModel in this.elementViewModels)
            {
                this.behavior.ItemsChanged(null, new DiagramItemsChangedEventArgs(diagramControl, new DiagramContentItem() { Content = elementViewModel }, ItemsChangedAction.Added));
            }
        }

        [Test]
        public void VerifyIsolate()
        {
            this.behavior.Attach(new GrapherDiagramControl() { DataContext = this.grapherViewModel.Object });
            Assert.IsFalse(this.behavior.Isolate());
            this.contextMenu.Setup(x => x.HoveredElement).Returns(this.elementViewModels.Last());
            Assert.IsTrue(this.behavior.Isolate());
            this.grapherViewModel.Verify(x => x.Isolate(It.IsAny<GraphElementViewModel>()), Times.Once);
            this.behavior.ExitIsolation();
            this.grapherViewModel.Verify(x => x.ExitIsolation(), Times.Once);
        }
    }
}
