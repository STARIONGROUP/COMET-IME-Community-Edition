// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BinaryRelationshipConnectorTool.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
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

namespace CDP4DiagramEditor.ViewModels.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CommonView.Diagram;
    using CDP4CommonView.Diagram.Views;

    using CDP4Composition.Diagram;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Services;

    using DevExpress.Diagram.Core;
    using DevExpress.Xpf.Diagram;

    using Microsoft.Practices.ServiceLocation;

    using NLog;

    using DiagramShape = CDP4Common.DiagramData.DiagramShape;

    /// <summary>
    /// A connector tool to create Element Usages
    /// </summary>
    public class BinaryRelationshipConnectorTool : ConnectorTool, IConnectorTool
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        protected static Logger Logger;

        /// <summary>
        /// The backing field for <see cref="ThingCreator" />
        /// </summary>
        private IThingCreator thingCreator;

        /// <summary>
        /// Gets the tool name
        /// </summary>
        public override string ToolName
        {
            get { return "Binary Relationship Tool"; }
        }

        /// <summary>
        /// Gets the tool Id.
        /// </summary>
        public override string ToolId
        {
            get { return nameof(BinaryRelationshipConnectorTool); }
        }

        /// <summary>
        /// Gets or sets the <see cref="IThingCreator" /> that is used to create different <see cref="Things" />.
        /// </summary>
        public IThingCreator ThingCreator
        {
            get { return this.thingCreator ??= ServiceLocator.Current.GetInstance<IThingCreator>(); }
            set { this.thingCreator = value; }
        }

        /// <summary>
        /// Gets the type of connector to be created
        /// </summary>
        public IDiagramConnectorViewModel GetConnectorViewModel => new BinaryRelationshipEdgeViewModel(null, null, null);


        /// <summary>
        /// Gets the type of <see cref="DiagramConnector"/> to be created
        /// </summary>
        public DiagramConnector GetConnector => new BinaryRelationshipConnector(this);

        /// <summary>
        /// Gets the dummy connector that was used in the creation method
        /// </summary>
        public DiagramConnector DummyConnector { get; private set; }

        /// <summary>
        /// Executes the creation of the objects conveyed by the tool
        /// </summary>
        /// <param name="connector">The supplied temp connector</param>
        /// <param name="behavior">The behavior</param>
        public async Task ExecuteCreate(DiagramConnector connector, ICdp4DiagramBehavior behavior)
        {
            this.DummyConnector = connector;

            // misuse connector.CustomStyleId to carry the Category to apply to the binary relationship
            var beginItemContent = ((DiagramContentItem)connector.BeginItem)?.Content as ThingDiagramContentItemViewModel;
            var endItemContent = ((DiagramContentItem)connector.EndItem)?.Content as ThingDiagramContentItemViewModel;

            if (beginItemContent == null || endItemContent == null)
            {
                // connector was drawn with either the source or target missing or incorrect
                // remove the dummy connector
                behavior.ViewModel.RemoveDiagramThingItem(connector);
                behavior.ResetTool();
                return;
            }

            var contextMenuItems = this.GetContextMenuItems(connector, behavior, beginItemContent, endItemContent);

            behavior.ViewModel.ShowDropContextMenuOptions(contextMenuItems);
        }

        /// <summary>
        /// A callback to initiate once Category has been picked from the context menu
        /// </summary>
        /// <param name="connector">The supplied temp connector</param>
        /// <param name="behavior">The behavior</param>
        /// <param name="beginItemContent">The begin item content</param>
        /// <param name="endItemContent">The end item content</param>
        /// <returns>An empty task</returns>
        private async Task CreateCallback(DiagramConnector connector, ICdp4DiagramBehavior behavior, ThingDiagramContentItemViewModel beginItemContent, ThingDiagramContentItemViewModel endItemContent)
        {
            try
            {
                var usage = await this.ThingCreator.CreateAndGetBinaryRelationship(endItemContent.Thing, beginItemContent.Thing, connector.CustomStyleId as Category, (Iteration)behavior.ViewModel.Thing.Container, behavior.ViewModel.Session.QuerySelectedDomainOfExpertise((Iteration)behavior.ViewModel.Thing.Container), behavior.ViewModel.Session);
                CreateConnector(usage, (DiagramShape)beginItemContent.DiagramThing, (DiagramShape)endItemContent.DiagramThing, behavior);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
            finally
            {
                behavior.ResetTool();
            }
        }

        /// <summary>
        /// Create a <see cref="DiagramEdge" /> from a <see cref="BinaryRelationship" />
        /// </summary>
        /// <param name="relationship">The <see cref="BinaryRelationship" /></param>
        /// <param name="source">The <see cref="DiagramShape" /> source</param>
        /// <param name="target">The <see cref="DiagramShape" /> target</param>
        /// <param name="behavior">The diagram bahavior</param>
        public static void CreateConnector(BinaryRelationship relationship, DiagramShape source, DiagramShape target, ICdp4DiagramBehavior behavior)
        {
            var connectorItem = behavior.ViewModel.ConnectorViewModels.SingleOrDefault(x => x.Thing == relationship);

            if (connectorItem != null)
            {
                return;
            }

            var edge = new DiagramEdge(Guid.NewGuid(), relationship.Cache, new Uri(behavior.ViewModel.Session.DataSourceUri))
            {
                Source = source,
                Target = target,
                DepictedThing = relationship,
                Name = source.Name
            };

            connectorItem = new BinaryRelationshipEdgeViewModel(edge, behavior.ViewModel.Session, behavior.ViewModel);
            behavior.ViewModel.ConnectorViewModels.Add(connectorItem);

            behavior.ViewModel.UpdateIsDirty();
        }

        /// <summary>
        /// Gets all necessary ContextMenu items
        /// </summary>
        /// <param name="connector">The supplied temp connector</param>
        /// <param name="behavior">The behavior</param>
        /// <param name="beginItemContent">The begin item content</param>
        /// <param name="endItemContent">The end item content</param>
        /// <returns>A <see cref="List{T}"/> of type <see cref="ContextMenuItemViewModel"/></returns>
        private List<ContextMenuItemViewModel> GetContextMenuItems(DiagramConnector connector, ICdp4DiagramBehavior behavior, ThingDiagramContentItemViewModel beginItemContent, ThingDiagramContentItemViewModel endItemContent)
        {
            var categories = behavior.ViewModel.Session
                .GetEngineeringModelRdlChain(behavior.ViewModel.Thing.GetContainerOfType<EngineeringModel>())
                .SelectMany(rdl => rdl.DefinedCategory.Where(c => c.PermissibleClass.Contains(ClassKind.BinaryRelationship)));

            var contextMenuItems = categories
                .Select(s =>
                    new ContextMenuItemViewModel(s.Name, string.Empty, async t =>
                    {
                        connector.CustomStyleId = t;
                        await this.CreateCallback(connector, behavior, beginItemContent, endItemContent);
                    }, s, true))
                .ToList();

            contextMenuItems.Insert(0,
                new ContextMenuItemViewModel(
                    "(No Category)",
                    string.Empty,
                    async _ =>
                    {
                        connector.CustomStyleId = null;
                        await this.CreateCallback(connector, behavior, beginItemContent, endItemContent);
                    },
                    true
                ));

            return contextMenuItems;
        }
    }
}
