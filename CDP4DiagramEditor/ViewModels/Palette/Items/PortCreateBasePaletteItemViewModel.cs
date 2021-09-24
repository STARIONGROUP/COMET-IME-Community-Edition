// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PortCreateBasePaletteItemViewModel.cs" company="RHEA System S.A.">
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

namespace CDP4DiagramEditor.ViewModels.Palette
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;

    using CDP4CommonView.Diagram;
    using CDP4CommonView.Diagram.ViewModels;

    using DevExpress.Xpf.Diagram;

    /// <summary>
    /// Base class for all port creation palette items
    /// </summary>
    public abstract class PortCreateBasePaletteItemViewModel : PaletteItemBaseViewModel
    {
        /// <summary>
        /// Creates a port of a certain kind
        /// </summary>
        /// <param name="kind">The <see cref="InterfaceEndKind"/> of the port</param>
        /// <returns>Empty task</returns>
        protected async Task CreatePort(InterfaceEndKind kind)
        {
            // TODO: Replace with proper EU
            var depictedThing = new ElementUsage { Name = "WhyNot", ShortName = "WhyNot" };

            if (this.editorViewModel.SelectedItem is DiagramContentItem { Content: PortContainerDiagramContentItem container } target)
            {
                var row = this.editorViewModel.ThingDiagramItems.SingleOrDefault(x => x.DiagramThing.DepictedThing == depictedThing);

                if (row != null)
                {
                    return;
                }

                var block = new DiagramPort(Guid.NewGuid(), this.editorViewModel.Thing.Cache, new Uri(this.editorViewModel.Session.DataSourceUri))
                {
                    DepictedThing = depictedThing,
                    Name = depictedThing.UserFriendlyName,
                };

                var bound = new Bounds(Guid.NewGuid(), this.editorViewModel.Thing.Cache, new Uri(this.editorViewModel.Session.DataSourceUri))
                {
                    X = (float)target.Position.X,
                    Y = (float)target.Position.Y,
                    Height = (float)target.ActualHeight,
                    Width = (float)target.ActualWidth
                };

                DiagramPort port = new DiagramPort();
                block.Bounds.Add(bound);
                var diagramItem = new DiagramPortViewModel(block, this.editorViewModel.Session, this.editorViewModel, kind);
                container.PortCollection.Add(diagramItem);

                this.editorViewModel.ThingDiagramItems.Add(diagramItem);
                this.editorViewModel.UpdateIsDirty();
            }
        }
    }
}
