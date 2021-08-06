// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InterfaceConnectorToolTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Nathanael Smiechowski, Ahmed Ahmed, Simon Wood
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4DiagramEditor.Tests.ViewModels.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CommonView.Diagram;
    using CDP4CommonView.Diagram.Views;

    using CDP4Composition.Diagram;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using CDP4DiagramEditor.ViewModels;
    using CDP4DiagramEditor.ViewModels.Tools;

    using DevExpress.Xpf.Diagram;

    using Microsoft.Practices.ServiceLocation;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class InterfaceConnectorToolTestFixture
    {
        [SetUp]
        public void Setup()
        {
            this.cache = new List<Thing>();

            //RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.uri = new Uri("http://test.com");
            this.assembler = new Assembler(this.uri);
            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "TestDoE" };
            this.sitedir.Domain.Add(this.domain);
            this.serviceLocator = new Mock<IServiceLocator>();

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.tool = new InterfaceConnectorTool();
            this.thingCreator = new Mock<IThingCreator>();
            this.editor = new Mock<IDiagramEditorViewModel>();

            this.tool.ThingCreator = this.thingCreator.Object;
            this.behavior = new Mock<ICdp4DiagramBehavior>();

            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.diagram = new ArchitectureDiagram(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Container = this.iteration
            };

            this.beginThing = new ElementUsage(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "1",
                Owner = this.domain,
                InterfaceEnd = InterfaceEndKind.OUTPUT
            };


            this.endThing = new ElementUsage(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "2",
                Owner = this.domain,
                InterfaceEnd = InterfaceEndKind.INPUT
            };

            this.containerThing = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "2",
                Owner = this.domain
            };

            this.containerElement = new ArchitectureElement(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                DepictedThing = this.containerThing
            };

            this.containerVm = new ElementDefinitionDiagramContentItemViewModel(this.containerElement, this.session.Object, this.editor.Object);

            this.beginElement = new DiagramPort(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                DepictedThing = this.beginThing
            };

            this.endElement = new DiagramPort(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                DepictedThing = this.endThing
            };

            this.beginVm = new PortDiagramContentItemViewModel(this.beginElement, this.containerVm, this.session.Object, this.editor.Object);
            this.endVm = new PortDiagramContentItemViewModel(this.endElement, this.containerVm, this.session.Object, this.editor.Object);

            var thingViewModels = new DisposableReactiveList<IThingDiagramItemViewModel>
            {
                this.beginVm,
                this.endVm
            };

            this.binaryRelationship = new BinaryRelationship(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Source = this.beginThing,
                Target = this.endThing
            };

            this.editor.Setup(e => e.Thing).Returns(this.diagram);
            this.session.Setup(s => s.QuerySelectedDomainOfExpertise(It.IsAny<Iteration>())).Returns(this.domain);
            this.session.Setup(s => s.DataSourceUri).Returns(this.uri.ToString());
            this.editor.Setup(e => e.Session).Returns(this.session.Object);
            this.editor.Setup(e => e.ConnectorViewModels).Returns(new DisposableReactiveList<IDiagramConnectorViewModel>());
            this.editor.Setup(e => e.ThingDiagramItemViewModels).Returns(thingViewModels);
            this.behavior.Setup(b => b.ViewModel).Returns(this.editor.Object);

            this.thingCreator.Setup(c => c.CreateAndGetInterface(It.IsAny<ElementUsage>(), It.IsAny<ElementUsage>(), It.IsAny<Iteration>(), It.IsAny<DomainOfExpertise>(), this.session.Object)).Returns(Task.FromResult(this.binaryRelationship));
        }

        private InterfaceConnectorTool tool;
        private Mock<IThingCreator> thingCreator;
        private Mock<ICdp4DiagramBehavior> behavior;
        private Mock<IDiagramEditorViewModel> editor;
        private ElementUsage beginThing;
        private ElementUsage endThing;
        private ElementUsage usage;
        private ArchitectureDiagram diagram;
        private Iteration iteration;
        private DiagramPort beginElement;
        private DiagramPort endElement;
        private PortDiagramContentItemViewModel beginVm;
        private PortDiagramContentItemViewModel endVm;
        private ElementDefinitionDiagramContentItemViewModel containerVm;
        private SiteDirectory sitedir;

        private Uri uri;
        private DomainOfExpertise domain;
        private Mock<ISession> session;
        private Assembler assembler;
        private Mock<IPermissionService> permissionService;
        private Mock<IServiceLocator> serviceLocator;

        private List<Thing> cache;
        private ElementDefinition containerThing;
        private ArchitectureElement containerElement;
        private BinaryRelationship binaryRelationship;

        [Test]
        public void VerifyConnectorToolExitsWhenItemsNotSet()
        {
            var connector = new ElementUsageConnector(this.tool);

            Assert.DoesNotThrowAsync(() => this.tool.ExecuteCreate(connector, this.behavior.Object));
            this.behavior.Verify(x => x.ResetTool(), Times.Once);
            this.thingCreator.Verify(x => x.CreateAndGetInterface(It.IsAny<ElementUsage>(), It.IsAny<ElementUsage>(), It.IsAny<Iteration>(), It.IsAny<DomainOfExpertise>(), this.session.Object), Times.Never);
        }

        [Test]
        public void VerifyConnectorToolPropertiesWork()
        {
            Assert.AreEqual("Interface Tool", this.tool.ToolName);
            Assert.AreEqual(nameof(InterfaceConnectorTool), this.tool.ToolId);
            Assert.IsNotNull(this.tool.GetConnector);
            Assert.IsNotNull(this.tool.GetConnectorViewModel);
        }

        [Test]
        public void VerifyIsThingAnInterfaceEnd()
        {
            Assert.IsTrue(InterfaceConnectorTool.IsThingAnInterfaceEnd(this.beginThing));
            Assert.IsFalse(InterfaceConnectorTool.IsThingAnInterfaceEnd(new ElementUsage { InterfaceEnd = InterfaceEndKind.NONE}));
            Assert.IsFalse(InterfaceConnectorTool.IsThingAnInterfaceEnd(new ElementDefinition()));
        }

        [Test]
        public void VerifyConnectorToolWorksWhenItemsSet()
        {
            var connector = new InterfaceConnector(this.tool);

            connector.BeginItem = new DiagramPortShape { DataContext = this.beginVm };
            connector.EndItem = new DiagramPortShape { DataContext = this.beginVm };

            Assert.IsTrue(connector.CanDrawFrom(connector.BeginItem as DiagramItem));
            Assert.IsTrue(connector.CanDrawTo(connector.EndItem as DiagramItem));

            Assert.DoesNotThrowAsync(() => this.tool.ExecuteCreate(connector, this.behavior.Object));
            this.behavior.Verify(x => x.ResetTool(), Times.Once);
            this.thingCreator.Verify(x => x.CreateAndGetInterface(It.IsAny<ElementUsage>(), It.IsAny<ElementUsage>(), It.IsAny<Iteration>(), It.IsAny<DomainOfExpertise>(), this.session.Object), Times.Once);

            Assert.DoesNotThrow(() => InterfaceConnectorTool.CreateConnector(this.binaryRelationship, this.beginElement, this.endElement, this.behavior.Object));

            this.editor.Verify(e => e.UpdateIsDirty(), Times.Once);
        }
    }
}
