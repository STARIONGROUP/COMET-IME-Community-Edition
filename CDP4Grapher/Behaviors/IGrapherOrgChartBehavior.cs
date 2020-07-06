﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IGrapherOrgChartBehavior.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft,
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

namespace CDP4Grapher.Behaviors
{
    using System.Collections.Generic;

    using CDP4Common.CommonData;

    using DevExpress.Diagram.Core;
    using DevExpress.Xpf.Diagram;

    /// <summary>
    /// Definition of a <see cref="GrapherOrgChartBehavior"/> exposing public properties and methods 
    /// </summary>
    public interface IGrapherOrgChartBehavior
    {
        /// <summary>
        /// Apply the desired layout when all the element have been drawed
        /// </summary>
        void ApplySpecifiedAutoLayout();

        /// <summary>
        /// Export the graph as the specified <see cref="DiagramExportFormat"/>
        /// </summary>
        /// <param name="format">the format to export the diagram to</param>
        void ExportGraph(DiagramExportFormat format);
    }
}
