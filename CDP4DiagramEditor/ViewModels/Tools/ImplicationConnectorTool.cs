// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImplicationConnectorTool.cs" company="RHEA System S.A.">
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

namespace CDP4DiagramEditor.ViewModels.Tools
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;

    using CDP4CommonView.Diagram;

    using CDP4Composition.Diagram;
    using CDP4Composition.Services;

    using CDP4DiagramEditor.Helpers;

    using DevExpress.Diagram.Core;
    using DevExpress.Xpf.Diagram;

    using Microsoft.Practices.ServiceLocation;

    using NLog;

    /// <summary>
    /// A connector tool to create Implications
    /// </summary>
    public abstract class ImplicationConnectorTool : ConnectorTool, IConnectorTool
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
            get { return "Implication Tool"; }
        }

        /// <summary>
        /// Gets the tool Id.
        /// </summary>
        public override string ToolId
        {
            get { return nameof(ConstraintConnectorTool); }
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
        public IDiagramConnectorViewModel GetConnectorViewModel => new ImplicationEdgeViewModel(null, null, null);

        /// <summary>
        /// Gets the type of <see cref="DiagramConnector"/> to be created
        /// </summary>
        public DiagramConnector GetConnector => new ImplicationConnector(this);

        /// <summary>
        /// Gets the dummy connector that was used in the creation method
        /// </summary>
        public DiagramConnector DummyConnector { get; private set; }

        /// <summary>
        /// Executes the creation of the objects conveyed by the tool
        /// </summary>
        /// <param name="connector">The temporary connector</param>
        /// <param name="behavior">The behavior</param>
        public abstract Task ExecuteCreate(DiagramConnector connector, ICdp4DiagramBehavior behavior);

        /// <summary>
        /// Executes the creation of the objects conveyed by the tool
        /// </summary>
        /// <param name="connector">The supplied temp connector</param>
        /// <param name="behavior">The behavior</param>
        protected async Task ExecuteCreate(DiagramConnector connector, ICdp4DiagramBehavior behavior, ImplicationKind kind)
        {
            this.DummyConnector = connector;
            var beginItemContent = ((DiagramContentItem)connector.BeginItem)?.Content as ElementDefinitionDiagramContentItemViewModel;
            var endItemContent = ((DiagramContentItem)connector.EndItem)?.Content as ElementDefinitionDiagramContentItemViewModel;

            if (beginItemContent == null || endItemContent == null)
            {
                behavior.ResetTool();
                return;
            }

            try
            {
                var iteration = (Iteration)behavior.ViewModel.Thing.Container;
                var createCategory = DiagramRDLHelper.GetOrAddImplicationCategory(iteration, kind, out var category, out var rdlClone);

                var relationship = await this.ThingCreator.CreateAndGetConstraint(beginItemContent.Thing as ElementDefinition, endItemContent.Thing as ElementDefinition, category, createCategory ? rdlClone : null, iteration, behavior.ViewModel.Session.QuerySelectedDomainOfExpertise(iteration), behavior.ViewModel.Session);
                CreateConnector(relationship, (DiagramObject)beginItemContent.DiagramThing, (DiagramObject)endItemContent.DiagramThing, behavior);
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
        /// <param name="source">The <see cref="DiagramObject" /> source</param>
        /// <param name="target">The <see cref="DiagramObject" /> target</param>
        /// <param name="behavior">The diagram bahavior</param>
        public static void CreateConnector(BinaryRelationship relationship, DiagramObject source, DiagramObject target, ICdp4DiagramBehavior behavior)
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

            connectorItem = new ImplicationEdgeViewModel(edge, behavior.ViewModel.Session, behavior.ViewModel);
            behavior.ViewModel.ConnectorViewModels.Add(connectorItem);

            behavior.ViewModel.UpdateIsDirty();
        }
    }
}
