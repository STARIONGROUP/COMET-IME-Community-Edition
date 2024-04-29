// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Cdp4DiagramOrgChartBehaviorTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4Composition.Tests.Diagram
{
    using System.Threading;

    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CommonView.Diagram;

    using CDP4Composition.Diagram;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Services;

    using CDP4Dal;

    using DevExpress.Xpf.Diagram;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="Cdp4DiagramOrgChartBehavior"/> class
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class Cdp4DiagramOrgChartBehaviorTestFixture
    {
        private DiagramContentItem diagramContentItem;
        private DiagramObject diagramObject;
        private ElementDefinition elementDefinition;
        private Parameter parameter1;
        private Parameter parameter2;
        private ISession session;
        private Cdp4DiagramOrgChartBehavior cdp4DiagramOrgChartBehavior;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IDropTarget> dropTarget;
        private Mock<IIDropTarget> iDropTarget;
        private Mock<IDiagramDropInfo> diagramDropInfo;
        private DiagramControl diagramControl;

        [SetUp]
        public void SetUp()
        {
            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingCreator>()).Returns(Mock.Of<IThingCreator>());

            this.diagramControl = new DiagramControl();

            this.elementDefinition = new ElementDefinition();
            this.parameter1 = new Parameter { ParameterType = new SimpleQuantityKind() };
            this.parameter2 = new Parameter { ParameterType = new SimpleQuantityKind() };
            this.elementDefinition.Parameter.Add(this.parameter1);
            this.elementDefinition.Parameter.Add(this.parameter2);

            this.diagramObject = new DiagramObject
            {
                DepictedThing = this.elementDefinition
            };

            this.session = Mock.Of<ISession>();

            this.diagramContentItem = new DiagramContentItem();

            this.cdp4DiagramOrgChartBehavior = new Cdp4DiagramOrgChartBehavior();

            this.diagramDropInfo = new Mock<IDiagramDropInfo>();
            this.dropTarget = new Mock<IDropTarget>();
            this.iDropTarget = new Mock<IIDropTarget>();
            this.iDropTarget.Setup(x => x.DropTarget).Returns(this.dropTarget.Object);
        }

        [Test]
        public void VerifyThatHandleDropWorksForHandledIDropTarget()
        {
            this.diagramContentItem.Content = this.dropTarget.Object;
            this.diagramDropInfo.Setup(x => x.Handled).Returns(true);

            Assert.IsTrue(this.cdp4DiagramOrgChartBehavior.HandleDrop(this.diagramContentItem, this.diagramDropInfo.Object));

            this.dropTarget.Verify(x => x.Drop(this.diagramDropInfo.Object), Times.Once);
        }

        [Test]
        public void VerifyThatHandleDropWorksForUnhandledIDropTarget()
        {
            this.diagramContentItem.Content = this.dropTarget.Object;
            this.diagramDropInfo.Setup(x => x.Handled).Returns(false);

            Assert.IsFalse(this.cdp4DiagramOrgChartBehavior.HandleDrop(this.diagramContentItem, this.diagramDropInfo.Object));

            this.dropTarget.Verify(x => x.Drop(this.diagramDropInfo.Object), Times.Once);
        }

        [Test]
        public void VerifyThatHandleDropWorksForHandledIIDropTarget()
        {
            this.diagramContentItem.Content = this.iDropTarget.Object;
            this.diagramDropInfo.Setup(x => x.Handled).Returns(true);

            Assert.IsTrue(this.cdp4DiagramOrgChartBehavior.HandleDrop(this.diagramContentItem, this.diagramDropInfo.Object));

            this.dropTarget.Verify(x => x.Drop(this.diagramDropInfo.Object), Times.Once);
        }

        [Test]
        public void VerifyThatHandleDropWorksForUnhandledIIDropTarget()
        {
            this.diagramContentItem.Content = this.iDropTarget.Object;
            this.diagramDropInfo.Setup(x => x.Handled).Returns(false);

            Assert.IsFalse(this.cdp4DiagramOrgChartBehavior.HandleDrop(this.diagramContentItem, this.diagramDropInfo.Object));

            this.dropTarget.Verify(x => x.Drop(this.diagramDropInfo.Object), Times.Once);
        }

        [Test]
        public void VerifyThatHandleDropWorksForUnsupportedObject()
        {
            this.diagramContentItem.Content = "";
            Assert.IsFalse(this.cdp4DiagramOrgChartBehavior.HandleDrop(this.diagramContentItem, this.diagramDropInfo.Object));

            this.dropTarget.Verify(x => x.Drop(this.diagramDropInfo.Object), Times.Never);
        }

        [Test]
        public void VerifyThatHandleDropWorksForDiagramControl()
        {
            this.diagramControl.DataContext = this.dropTarget.Object;
            this.cdp4DiagramOrgChartBehavior.Attach(this.diagramControl);
            this.diagramDropInfo.Setup(x => x.Handled).Returns(true);

            Assert.IsTrue(this.cdp4DiagramOrgChartBehavior.HandleDrop(this.diagramContentItem, this.diagramDropInfo.Object));

            this.dropTarget.Verify(x => x.Drop(this.diagramDropInfo.Object), Times.Once);
        }

        [Test]
        public void VerifyThatHandleDropWorksForDiagramControlWithUnhandledEvent()
        {
            this.diagramControl.DataContext = this.dropTarget.Object;
            this.cdp4DiagramOrgChartBehavior.Attach(this.diagramControl);
            this.diagramDropInfo.Setup(x => x.Handled).Returns(false);

            Assert.IsFalse(this.cdp4DiagramOrgChartBehavior.HandleDrop(this.diagramContentItem, this.diagramDropInfo.Object));

            this.dropTarget.Verify(x => x.Drop(this.diagramDropInfo.Object), Times.Once);
        }

        [Test]
        public void VerifyThatHandleDropWorksForDiagramControlWithUnsupportedDataContext()
        {
            this.diagramControl.DataContext = "";
            this.cdp4DiagramOrgChartBehavior.Attach(this.diagramControl);

            Assert.IsFalse(this.cdp4DiagramOrgChartBehavior.HandleDrop(this.diagramContentItem, this.diagramDropInfo.Object));

            this.dropTarget.Verify(x => x.Drop(this.diagramDropInfo.Object), Times.Never);
        }

        [Test]
        public void VerifyThatHandleDragOverWorksForHandledIDropTarget()
        {
            this.diagramContentItem.Content = this.dropTarget.Object;
            this.diagramDropInfo.Setup(x => x.Handled).Returns(true);

            Assert.IsTrue(this.cdp4DiagramOrgChartBehavior.HandleDragOver(this.diagramContentItem, this.diagramDropInfo.Object));

            this.dropTarget.Verify(x => x.DragOver(this.diagramDropInfo.Object), Times.Once);
        }

        [Test]
        public void VerifyThatHandleDragOverWorksForUnhandledIDropTarget()
        {
            this.diagramContentItem.Content = this.dropTarget.Object;
            this.diagramDropInfo.Setup(x => x.Handled).Returns(false);

            Assert.IsFalse(this.cdp4DiagramOrgChartBehavior.HandleDragOver(this.diagramContentItem, this.diagramDropInfo.Object));

            this.dropTarget.Verify(x => x.DragOver(this.diagramDropInfo.Object), Times.Once);
        }

        [Test]
        public void VerifyThatHandleDragOverWorksForHandledIIDropTarget()
        {
            this.diagramContentItem.Content = this.iDropTarget.Object;
            this.diagramDropInfo.Setup(x => x.Handled).Returns(true);

            Assert.IsTrue(this.cdp4DiagramOrgChartBehavior.HandleDragOver(this.diagramContentItem, this.diagramDropInfo.Object));

            this.dropTarget.Verify(x => x.DragOver(this.diagramDropInfo.Object), Times.Once);
        }

        [Test]
        public void VerifyThatHandleDragOverWorksForUnhandledIIDropTarget()
        {
            this.diagramContentItem.Content = this.iDropTarget.Object;
            this.diagramDropInfo.Setup(x => x.Handled).Returns(false);

            Assert.IsFalse(this.cdp4DiagramOrgChartBehavior.HandleDragOver(this.diagramContentItem, this.diagramDropInfo.Object));

            this.dropTarget.Verify(x => x.DragOver(this.diagramDropInfo.Object), Times.Once);
        }

        [Test]
        public void VerifyThatHandleDragOverWorksForUnsupportedObject()
        {
            this.diagramContentItem.Content = "";
            Assert.IsFalse(this.cdp4DiagramOrgChartBehavior.HandleDragOver(this.diagramContentItem, this.diagramDropInfo.Object));

            this.dropTarget.Verify(x => x.DragOver(this.diagramDropInfo.Object), Times.Never);
        }

        [Test]
        public void VerifyThatHandleDragOverWorksForDiagramControl()
        {
            this.diagramControl.DataContext = this.dropTarget.Object;
            this.cdp4DiagramOrgChartBehavior.Attach(this.diagramControl);
            this.diagramDropInfo.Setup(x => x.Handled).Returns(true);

            Assert.IsTrue(this.cdp4DiagramOrgChartBehavior.HandleDragOver(this.diagramContentItem, this.diagramDropInfo.Object));

            this.dropTarget.Verify(x => x.DragOver(this.diagramDropInfo.Object), Times.Once);
        }

        [Test]
        public void VerifyThatHandleDragOverWorksForDiagramControlWithUnhandledEvent()
        {
            this.diagramControl.DataContext = this.dropTarget.Object;
            this.cdp4DiagramOrgChartBehavior.Attach(this.diagramControl);
            this.diagramDropInfo.Setup(x => x.Handled).Returns(false);

            Assert.IsFalse(this.cdp4DiagramOrgChartBehavior.HandleDragOver(this.diagramContentItem, this.diagramDropInfo.Object));

            this.dropTarget.Verify(x => x.DragOver(this.diagramDropInfo.Object), Times.Once);
        }

        [Test]
        public void VerifyThatHandleDragOverWorksForDiagramControlWithUnsupportedDataContext()
        {
            this.diagramControl.DataContext = "";
            this.cdp4DiagramOrgChartBehavior.Attach(this.diagramControl);

            Assert.IsFalse(this.cdp4DiagramOrgChartBehavior.HandleDragOver(this.diagramContentItem, this.diagramDropInfo.Object));

            this.dropTarget.Verify(x => x.DragOver(this.diagramDropInfo.Object), Times.Never);
        }


    }
}
