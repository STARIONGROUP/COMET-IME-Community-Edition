// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAssemblyLocationLoader.cs" company="RHEA System S.A.">
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

namespace CDP4Composition.Utilities
{
    using System;
    using System.Reflection;

    /// <summary>
    /// The interface that defines members of implementing classes of <see cref="IAssemblyInformationService"/>
    /// </summary>
    public interface IAssemblyInformationService
    {
        /// <summary>
        /// Gets the path of the executing assembly
        /// </summary>
        /// <returns>the path of the assembly</returns>
        string GetLocation();

        /// <summary>
        /// Gets the Version of the executing assembly
        /// </summary>
        /// <returns>The <see cref="Version"/></returns>
        Version GetVersion();

        /// <summary>
        /// Gets the target processor architecture of the executing assembly
        /// </summary>
        /// <returns>The <see cref="ProcessorArchitecture"/></returns>
        ProcessorArchitecture GetProcessorArchitecture();
    }
}
