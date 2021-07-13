// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagramPortShapeBehavior.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
//            Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Diagram.Behaviors
{
    using System;
    using System.Windows;

    using CDP4CommonView.Diagram.ViewModels;
    using CDP4CommonView.Diagram.Views;

    using DevExpress.Diagram.Core;
    using DevExpress.Mvvm.UI.Interactivity;

    /// <summary>
    /// The purpose of the <see cref="DiagramPortShapeBehavior"/> is to update the position
    /// and to set the correct orientation of the connection point of the attached <see cref="DiagramPortShape"/>
    /// </summary>
    public class DiagramPortShapeBehavior : Behavior<DiagramPortShape>
    {
        /// <summary>
        /// Gets or sets the viewModel <see cref="IDiagramPortViewModel"/> of the attached view <see cref="DiagramPortShape"/>
        /// </summary>
        private IDiagramPortViewModel viewModel;

        /// <summary>
        /// The on Attached event Handler
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            this.viewModel = (IDiagramPortViewModel) this.AssociatedObject.DataContext;
            this.viewModel.WhenPositionIsUpdated += this.WhenPositionIsUpdated;
            this.DeterminePortConnectorRotation();
        }

        /// <summary>
        /// Event handler of the event firing when Position property has changed
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The arguments</param>
        private void WhenPositionIsUpdated(object sender, EventArgs e)
        {
            this.AssociatedObject.Position = this.viewModel.Position;
        }

        /// <summary>
        /// the on detaching event handler
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.viewModel.WhenPositionIsUpdated -= this.WhenPositionIsUpdated;
        }

        /// <summary>
        /// Method use to set the correction orientation of the associate object connection point based on which side of the container it belongs
        /// </summary>
        public void DeterminePortConnectorRotation()
        {
            this.AssociatedObject.Position = this.viewModel.Position;

            switch (this.viewModel.PortContainerShapeSide)
            {
                case PortContainerShapeSide.Top:
                    this.AssociatedObject.ConnectionPoints = new DiagramPointCollection(new[] { new Point(0.5, 0) });
                    break;
                case PortContainerShapeSide.Left:
                    this.AssociatedObject.ConnectionPoints = new DiagramPointCollection(new[] { new Point(0, 0.5) });
                    break;
                case PortContainerShapeSide.Right:
                    this.AssociatedObject.ConnectionPoints = new DiagramPointCollection(new[] { new Point(1, 0.5) });
                    break;
                case PortContainerShapeSide.Bottom:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
