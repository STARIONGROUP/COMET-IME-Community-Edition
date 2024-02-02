// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImplicationCreatePaletteItemViewModel.cs" company="RHEA System S.A.">
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

namespace CDP4DiagramEditor.ViewModels.Palette
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Diagram;
    using CDP4Composition.Services;

    using CDP4DiagramEditor.Helpers;
    using CDP4DiagramEditor.ViewModels.Tools;

    using DevExpress.Xpf.Diagram;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    /// Base view model for implication create items
    /// </summary>
    public class ImplicationCreatePaletteItemViewModel<TTool> : ConnectorCreatePaletteItemBaseViewModel where TTool : ImplicationConnectorTool, IConnectorTool, new()
    {
        /// <summary>
        /// The backing field for <see cref="ThingCreator" />
        /// </summary>
        private IThingCreator thingCreator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImplicationCreatePaletteItemViewModel" /> class.
        /// </summary>
        public ImplicationCreatePaletteItemViewModel(ImplicationKind kind)
        {
            this.ImplicationKind = kind;
            this.ConnectorTool = new TTool();
        }

        /// <summary>
        /// Gets the <see cref="ImplicationKind"/>
        /// </summary>
        public ImplicationKind ImplicationKind { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="IThingCreator" /> that is used to create different <see cref="Things" />.
        /// </summary>
        public IThingCreator ThingCreator
        {
            get { return this.thingCreator ??= ServiceLocator.Current.GetInstance<IThingCreator>(); }
            set { this.thingCreator = value; }
        }

        /// <summary>
        /// Executes the command of this <see cref="IPaletteItemViewModel" />
        /// </summary>
        /// <returns>An empty task</returns>
        public override async Task ExecuteAsyncCommand()
        {
            var selectionList = this.editorViewModel.SelectedItems.OfType<DiagramContentItem>().ToList();

            // if selection is two things between which constraints canbe created, do it without need to drag
            if (selectionList.Count == 2)
            {
                var beginItemContent = selectionList.First().Content as ElementDefinitionDiagramContentItemViewModel;
                var endItemContent = selectionList.Last().Content as ElementDefinitionDiagramContentItemViewModel;

                if (beginItemContent == null || endItemContent == null)
                {
                    return;
                }

                try
                {
                    var iteration = (Iteration)this.editorViewModel.Thing.Container;
                    var createCategory = DiagramRDLHelper.GetOrAddImplicationCategory(iteration, this.ImplicationKind, out var category, out var rdlClone);

                    var relationship = await this.ThingCreator.CreateAndGetConstraint(beginItemContent.Thing as ElementDefinition, endItemContent.Thing as ElementDefinition, category, createCategory ? rdlClone : null, iteration, this.editorViewModel.Session.QuerySelectedDomainOfExpertise(iteration), this.editorViewModel.Session);
                    ImplicationConnectorTool.CreateConnector(relationship, (DiagramObject)beginItemContent.DiagramThing, (DiagramObject)endItemContent.DiagramThing, this.editorViewModel.Behavior);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message);
                }

                return;
            }

            // activate tool
            this.editorViewModel.ActivateConnectorTool<TTool>(this);
        }
    }
}
