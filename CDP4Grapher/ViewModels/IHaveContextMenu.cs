// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IHaveContextMenu.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
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

namespace CDP4Grapher.ViewModels
{
    using System.Collections.Generic;

    using CDP4Grapher.Behaviors;

    using DevExpress.Xpf.Bars;
    using DevExpress.Xpf.Diagram;

    /// <summary>
    /// Define a class that has a <see cref="List{IBarManagerControllerAction}"/> that can be use to override DiagramControl ContextMenu.
    /// Using <see cref="DiagramControl.CreateContextMenu"/>
    /// </summary>
    public interface IHaveContextMenu
    {
        /// <summary>
        /// Holds the <see cref="BarButtonItem"/> and <see cref="BarSubItem"/> representing an overridable diagram Context Menu
        /// </summary>
        List<IBarManagerControllerAction> ContextMenu { get; }
        
        /// <summary>
        /// Gets or sets the attached behavior
        /// </summary>
        IGrapherOrgChartBehavior Behavior { get; set; }
    }
}
