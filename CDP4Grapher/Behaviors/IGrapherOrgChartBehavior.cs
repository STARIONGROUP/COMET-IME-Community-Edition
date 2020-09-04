// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IGrapherOrgChartBehavior.cs" company="RHEA System S.A.">
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

namespace CDP4Grapher.Behaviors
{
    using System;

    using CDP4Grapher.Utilities;
    using CDP4Grapher.ViewModels;

    using DevExpress.Diagram.Core;

    /// <summary>
    /// Definition of a <see cref="GrapherOrgChartBehavior"/> exposing public properties and methods 
    /// </summary>
    public interface IGrapherOrgChartBehavior
    {
        /// <summary>
        /// Export the graph as the specified <see cref="DiagramExportFormat"/>
        /// </summary>
        /// <param name="format">the format to export the diagram to</param>
        void ExportGraph(DiagramExportFormat format);

        /// <summary>
        /// Apply the desired layout specified
        /// </summary>
        /// <param name="layout">the <see cref="LayoutEnumeration"/> layout to apply </param>
        /// <param name="direction">the value holding the direction of the layout</param>
        /// <typeparam name="T">The devexpress enum type needed by the layouts Fugiyama, TipOver, Tree and Mind map </typeparam>
        void ApplySpecifiedLayout<T>(LayoutEnumeration layout, T direction) where T : Enum;

        /// <summary>
        /// Apply the desired layout specified
        /// <param name="layout">the <see cref="LayoutEnumeration"/> layout to apply </param>
        /// </summary>
        void ApplySpecifiedLayout(LayoutEnumeration layout);
        
        /// <summary>
        /// Applies the saved layout from <see cref="CurrentLayout"/>
        /// </summary>
        void ApplyPreviousLayout();

        /// <summary>
        /// Isolate the Element under the mouse if any and display only its children element and itself
        /// </summary>
        /// <returns>An assert whether isolation is on</returns>
        bool Isolate();
        
        /// <summary>
        /// Exits the isolation
        /// </summary>
        void ExitIsolation();
    }
}
