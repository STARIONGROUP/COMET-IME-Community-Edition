﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleConnectorToolTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4DiagramEditor.Tests.ViewModels.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CommonView.Diagram;

    using CDP4Composition.Diagram;
    using CDP4Composition.Mvvm.Types;

    using CDP4Dal;

    using CDP4DiagramEditor.ViewModels;
    using CDP4DiagramEditor.ViewModels.Tools;

    using CommonServiceLocator;

    using DevExpress.Xpf.Diagram;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class SimpleConnectorToolTestFixture
    {
        private SimpleConnectorTool tool;
        private Mock<ICdp4DiagramBehavior> behavior;
        private Mock<IDiagramEditorViewModel> editor;
        private ElementDefinition beginThing;
        private ElementDefinition endThing;
        private ArchitectureDiagram diagram;
        private Iteration iteration;
        private ArchitectureElement beginElement;
        private ArchitectureElement endElement;
        private ElementDefinitionDiagramContentItemViewModel beginVm;
        private ElementDefinitionDiagramContentItemViewModel endVm;
        private SiteDirectory sitedir;

        private Uri uri;
        private DomainOfExpertise domain;
        private Mock<ISession> session;
        private Assembler assembler;
        private Mock<IServiceLocator> serviceLocator;

        private List<Thing> cache;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.cache = new List<Thing>();

            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.uri = new Uri("http://test.com");
            this.assembler = new Assembler(this.uri, this.messageBus);
            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "TestDoE" };
            this.sitedir.Domain.Add(this.domain);
            this.serviceLocator = new Mock<IServiceLocator>();

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.tool = new SimpleConnectorTool();
            this.editor = new Mock<IDiagramEditorViewModel>();

            this.behavior = new Mock<ICdp4DiagramBehavior>();

            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.diagram = new ArchitectureDiagram(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Container = this.iteration
            };

            this.beginThing = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "1",
                Owner = this.domain
            };

            this.endThing = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "2",
                Owner = this.domain
            };

            this.beginElement = new ArchitectureElement(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                DepictedThing = this.beginThing
            };

            this.endElement = new ArchitectureElement(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                DepictedThing = this.endThing
            };

            this.beginVm = new ElementDefinitionDiagramContentItemViewModel(this.beginElement, this.session.Object, this.editor.Object);
            this.endVm = new ElementDefinitionDiagramContentItemViewModel(this.endElement, this.session.Object, this.editor.Object);

            var thingViewModels = new DisposableReactiveList<IThingDiagramItemViewModel>
            {
                this.beginVm,
                this.endVm
            };

            this.editor.Setup(e => e.Thing).Returns(this.diagram);
            this.session.Setup(s => s.QuerySelectedDomainOfExpertise(It.IsAny<Iteration>())).Returns(this.domain);
            this.session.Setup(s => s.DataSourceUri).Returns(this.uri.ToString());
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
            this.editor.Setup(e => e.Session).Returns(this.session.Object);
            this.editor.Setup(e => e.ConnectorViewModels).Returns(new DisposableReactiveList<IDiagramConnectorViewModel>());
            this.editor.Setup(e => e.ThingDiagramItemViewModels).Returns(thingViewModels);
            this.behavior.Setup(b => b.ViewModel).Returns(this.editor.Object);
        }

        [Test]
        public void VerifyConnectorToolExitsWhenItemsNotSet()
        {
            var connector = new SimpleConnector(this.tool);

            Assert.DoesNotThrowAsync(() => this.tool.ExecuteCreate(connector, this.behavior.Object));
            this.behavior.Verify(x => x.ResetTool(), Times.Once);
        }

        [Test]
        public void VerifyConnectorToolPropertiesWork()
        {
            Assert.AreEqual("Simple Connector Tool", this.tool.ToolName);
            Assert.AreEqual(nameof(SimpleConnectorTool), this.tool.ToolId);
            Assert.IsNotNull(this.tool.GetConnector);
            Assert.IsNotNull(this.tool.GetConnectorViewModel);
        }

        [Test]
        public void VerifyConnectorToolWorksWhenItemsSet()
        {
            var connector = new SimpleConnector(this.tool);

            connector.BeginItem = new DiagramContentItem { Content = this.beginVm };
            connector.EndItem = new DiagramContentItem { Content = this.beginVm };

            Assert.IsTrue(connector.CanDrawFrom(connector.BeginItem as DiagramItem));
            Assert.IsTrue(connector.CanDrawTo(connector.EndItem as DiagramItem));

            Assert.DoesNotThrowAsync(() => this.tool.ExecuteCreate(connector, this.behavior.Object));
            this.behavior.Verify(x => x.ResetTool(), Times.Once);
            this.editor.Verify(e => e.UpdateIsDirty(), Times.Once);
        }
    }
}
