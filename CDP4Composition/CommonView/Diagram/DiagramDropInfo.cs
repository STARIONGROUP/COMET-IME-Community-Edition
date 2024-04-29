// -------------------------------------------------------------------------------------------------
// <copyright file="DiagramDropInfo.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2021 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Diagram
{
    using System.Windows;

    using CDP4Composition.DragDrop;

    using DevExpress.Xpf.Diagram;

    /// <summary>
    /// Extended class specific to diagrams
    /// </summary>
    public class DiagramDropInfo : DropInfo, IDiagramDropInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramDropInfo"/> class.
        /// </summary>
        /// <param name="sender">
        /// The sender of the drag event.
        /// </param>
        /// <param name="e">
        /// The drag event
        /// </param>
        public DiagramDropInfo(object sender, DragEventArgs e) : base(sender, e)
        {
            // In case of a DiagramContentItem, the sender is the Diagram Editor,
            // so we need to check e.Source here.
            if (e.Source is DiagramContentItem diagramContentItem)
            {
                this.SetTargetItem(diagramContentItem);
            }
        }

        /// <summary>
        /// Sets the <see cref="DropInfo.TargetItem"/> to the object that the mouse is over.
        /// </summary>
        /// <typeparam name="T">The type of object to be set as TargetItem</typeparam>
        /// <param name="diagramContentItem">
        /// The <typeparamref name="T"/> that the mouse is moving over
        /// </param>
        private void SetTargetItem<T>(T diagramContentItem)
        {
            this.TargetItem = diagramContentItem;
        }

        /// <summary>
        /// Gets or sets the drop point relatively to a diagram control
        /// </summary>
        public Point DiagramDropPoint { get; set; }
    }
}
