// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionCreatePaletteViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4DiagramEditor.Tests.Palette
{
    using System;
    using System.Collections.Concurrent;

    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.Types;

    using CDP4CommonView.Diagram;

    using CDP4Composition.Diagram;
    using CDP4Composition.Mvvm.Behaviours;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using CDP4DiagramEditor.ViewModels.Palette;

    using DevExpress.Xpf.Diagram;

    using Microsoft.Practices.ServiceLocation;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class ElementDefinitionCreatePaletteViewModelTestFixture
    {
        [SetUp]
        public void Setup()
        {
            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingCreator>()).Returns(Mock.Of<IThingCreator>());

            this.session = new Mock<ISession>();
            this.assembler = new Assembler(this.uri);
            this.permissionService = new Mock<IPermissionService>();
            this.mockExtendedDiagramBehavior = new Mock<IExtendedDiagramOrgChartBehavior>();
            this.mockDiagramBehavior = new Mock<ICdp4DiagramBehavior>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.pluginSettingsService = new Mock<IPluginSettingsService>();
            this.dropinfo = new Mock<IDiagramDropInfo>();
            this.cache = this.assembler.Cache;

            this.editor = new Mock<IDiagramEditorViewModel>();

            this.selection = new ReactiveList<DiagramItem>();
            this.editor.Setup(vm => vm.SelectedItems).Returns(this.selection);
            this.editor.Setup(vm => vm.Session).Returns(this.session.Object);
            this.session.Setup(s => s.Assembler).Returns(this.assembler);
            this.session.Setup(s => s.DataSourceUri).Returns(this.uri.ToString());
            this.diagram = new DiagramCanvas(Guid.NewGuid(), this.cache, this.uri) { Name = "model" };
            this.palette = new DiagramPaletteViewModel(this.diagram, this.editor.Object);
        }

        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IPluginSettingsService> pluginSettingsService;
        private Mock<IDiagramDropInfo> dropinfo;
        private Mock<IExtendedDiagramOrgChartBehavior> mockExtendedDiagramBehavior;
        private Mock<ICdp4DiagramBehavior> mockDiagramBehavior;
        private readonly Uri uri = new Uri("http://test.com");
        private Mock<IDiagramEditorViewModel> editor;
        private DiagramPaletteViewModel palette;
        private ElementDefinitionCreatePaletteItemViewModel vm;
        private DiagramCanvas diagram;
        private DiagramObject diagramObject1;
        private DiagramObject diagramObject2;
        private Mock<IServiceLocator> serviceLocator;
        private Assembler assembler;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private ReactiveList<DiagramItem> selection;

        [Test]
        public void VerifyCreateElementDefinition()
        {
            this.vm = new ElementDefinitionCreatePaletteItemViewModel();
            this.vm.BindEditor(this.editor.Object, this.palette);

            Assert.DoesNotThrowAsync(async () => await this.vm.HandleMouseDrop(this.dropinfo.Object));
        }
    }
}
