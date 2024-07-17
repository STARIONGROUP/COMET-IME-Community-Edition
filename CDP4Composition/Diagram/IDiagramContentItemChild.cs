﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDiagramContentItemChild.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2021 Starion Group S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Nathanael Smiechowski.
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Diagram
{
    /// <summary>
    /// Defines the interface for view models that implement a <see cref="DiagramContentItemChildString"/>
    /// </summary>
    public interface IDiagramContentItemChild
    {
        /// <summary>
        /// The textual representation of the <see cref="IDiagramContentItemChild"/>
        /// </summary>
        string DiagramContentItemChildString { get; }
    }
}
