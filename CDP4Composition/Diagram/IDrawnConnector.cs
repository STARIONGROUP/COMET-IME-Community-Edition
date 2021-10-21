// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDrawnConnector.cs" company="RHEA System S.A.">
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

namespace CDP4Composition.Diagram
{
    using DevExpress.Xpf.Diagram;

    /// <summary>
    /// Interface connecting all diagram drawn connectors.
    /// </summary>
    public interface IDrawnConnector
    {
        /// <summary>
        /// Gets the tool that drew the connector
        /// </summary>
        IConnectorTool Tool { get; }

        /// <summary>
        /// Checks whether the provided diagramItem can be used to draw from for this connector
        /// </summary>
        /// <param name="item">The diagramitem</param>
        /// <returns>True if allowed</returns>
        bool CanDrawFrom(DiagramItem item);

        /// <summary>
        /// Checks whether the provided diagramItem can be used to draw to for this connector
        /// </summary>
        /// <param name="item">The diagramitem</param>
        /// <returns>True if allowed</returns>
        bool CanDrawTo(DiagramItem item);
    }
}
