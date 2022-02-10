// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InterfaceConnectorTool.cs" company="RHEA System S.A.">
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
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;

    using CDP4CommonView.Diagram;

    using CDP4Composition.Diagram;
    using CDP4Composition.Services;

    using DevExpress.Diagram.Core;
    using DevExpress.Xpf.Diagram;

    using Microsoft.Practices.ServiceLocation;

    using NLog;

    /// <summary>
    /// A connector tool to create Interfaces
    /// </summary>
    public class InterfaceConnectorTool : ConnectorTool, IConnectorTool
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        protected static Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The backing field for <see cref="ThingCreator" />
        /// </summary>
        private IThingCreator thingCreator;

        /// <summary>
        /// Gets the tool name
        /// </summary>
        public override string ToolName
        {
            get { return "Interface Tool"; }
        }

        /// <summary>
        /// Gets the tool Id.
        /// </summary>
        public override string ToolId
        {
            get { return nameof(InterfaceConnectorTool); }
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
        public IDiagramConnectorViewModel GetConnectorViewModel => new InterfaceEdgeViewModel(null, null, null);

        /// <summary>
        /// Gets the type of <see cref="DiagramConnector"/> to be created
        /// </summary>
        public DiagramConnector GetConnector => new InterfaceConnector(this);

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
            var beginItemContent = connector.BeginItem?.DataContext as PortDiagramContentItemViewModel;
            var endItemContent = connector.EndItem?.DataContext as PortDiagramContentItemViewModel;

            if (beginItemContent == null || endItemContent == null)
            {
                // connector was drawn with either the source or target missing or incorrect
                behavior.ResetTool();
                return;
            }

            try
            {
                var relationship = await this.ThingCreator.CreateAndGetInterface(endItemContent.Thing as ElementUsage, beginItemContent.Thing as ElementUsage, (Iteration)behavior.ViewModel.Thing.Container, behavior.ViewModel.Session.QuerySelectedDomainOfExpertise((Iteration)behavior.ViewModel.Thing.Container), behavior.ViewModel.Session);
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
        /// <param name="relationship">The <see cref="BinaryRelationship" /> representing the interface</param>
        /// <param name="source">The <see cref="DiagramObject" /> source</param>
        /// <param name="target">The <see cref="DiagramObject" /> target</param>
        /// <param name="behavior">The diagram bahavior</param>
        public static void CreateConnector(BinaryRelationship relationship, DiagramPort source, DiagramPort target, ICdp4DiagramBehavior behavior)
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

            connectorItem = new InterfaceEdgeViewModel(edge, behavior.ViewModel.Session, behavior.ViewModel);
            behavior.ViewModel.ConnectorViewModels.Add(connectorItem);

            behavior.ViewModel.UpdateIsDirty();
        }

        /// <summary>
        /// Checks whether a <see cref="Thing"/> is an Interface End
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/> to check</param>
        /// <returns>True if it is an <see cref="ElementUsage"/> with an InterfaceEnd kind set to non-null</returns>
        public static bool IsThingAnInterfaceEnd(Thing thing)
        {
            if (thing is ElementUsage usage)
            {
                if (usage.InterfaceEnd != InterfaceEndKind.NONE)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
