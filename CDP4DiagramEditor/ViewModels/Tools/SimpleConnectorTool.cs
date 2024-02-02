// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleConnectorTool.cs" company="RHEA System S.A.">
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
    using System.Threading.Tasks;

    using CDP4Common.DiagramData;

    using CDP4CommonView.Diagram;

    using CDP4Composition.Diagram;

    using DevExpress.Diagram.Core;
    using DevExpress.Xpf.Diagram;

    using NLog;

    /// <summary>
    /// A connector tool to create Element Usages
    /// </summary>
    public class SimpleConnectorTool : ConnectorTool, IConnectorTool
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        protected static Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Gets the tool name
        /// </summary>
        public override string ToolName
        {
            get { return "Simple Connector Tool"; }
        }

        /// <summary>
        /// Gets the tool Id.
        /// </summary>
        public override string ToolId
        {
            get { return nameof(SimpleConnectorTool); }
        }

        /// <summary>
        /// Gets the type of connector to be created
        /// </summary>
        public IDiagramConnectorViewModel GetConnectorViewModel
        {
            get { return new SimpleEdgeViewModel(null, null, null); }
        }

        /// <summary>
        /// Gets the type of <see cref="DiagramConnector" /> to be created
        /// </summary>
        public DiagramConnector GetConnector
        {
            get { return new SimpleConnector(this); }
        }

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
            var beginItemContent = ((DiagramContentItem) connector.BeginItem)?.Content as ThingDiagramContentItemViewModel;
            var endItemContent = ((DiagramContentItem) connector.EndItem)?.Content as ThingDiagramContentItemViewModel;

            if (beginItemContent == null || endItemContent == null)
            {
                // connector was drawn with either the source or target missing or incorrect
                behavior.ResetTool();
                return;
            }

            try
            {
                CreateConnector((DiagramObject) beginItemContent.DiagramThing, (DiagramObject) endItemContent.DiagramThing, behavior);
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
        /// Create a <see cref="DiagramEdge" /> from a <see cref="SimpleConnector" />
        /// </summary>
        /// <param name="source">The <see cref="DiagramObject" /> source</param>
        /// <param name="target">The <see cref="DiagramObject" /> target</param>
        /// <param name="behavior">The diagram bahavior</param>
        public static void CreateConnector(DiagramObject source, DiagramObject target, ICdp4DiagramBehavior behavior)
        {
            var edge = new DiagramEdge(Guid.NewGuid(), behavior.ViewModel.Thing.Cache, new Uri(behavior.ViewModel.Session.DataSourceUri))
            {
                Source = source,
                Target = target,
                DepictedThing = null,
                Name = source.Name
            };

            var connectorItem = new SimpleEdgeViewModel(edge, behavior.ViewModel.Session, behavior.ViewModel);
            behavior.ViewModel.ConnectorViewModels.Add(connectorItem);

            behavior.ViewModel.UpdateIsDirty();
        }
    }
}
