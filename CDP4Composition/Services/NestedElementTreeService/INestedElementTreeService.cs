// --------------------------------------------------------------------------------------------------------------------
// <copyright file="INestedElementTreeService.cs" company="RHEA System S.A.">
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

namespace CDP4Composition.Services.NestedElementTreeService
{
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Definition of the <see cref="INestedElementTreeService"/> used to wrap the NestedElementTreeGenerator functionality
    /// </summary>
    public interface INestedElementTreeService
    {
        /// <summary>
        /// Get <see cref="NestedElement.ShortName"/> for an <see cref="ElementDefinition"/>.
        /// For the <see cref="Iteration"/>'s TopElement this is equal to the param <see cref="ElementDefinition"/>'s ShortName, that ShortName can be returned immediately.
        /// Other <see cref="ElementDefinition"/>s  shall return the value null.
        /// </summary>
        /// <param name="elementDefinition">The <see cref="ElementDefinition"/></param>
        /// <param name="option">The <see cref="Option"/></param>
        /// <returns>The <see cref="NestedElement.ShortName"/> if found, otherwise null</returns>
        string GetNestedElementPath(ElementDefinition elementDefinition, Option option);

        /// <summary>
        /// Get <see cref="NestedElement.ShortName"/> for an <see cref="ElementUsage"/> 
        /// </summary>
        /// <param name="elementUsage">The <see cref="ElementUsage"/></param>
        /// <param name="option">The <see cref="Option"/></param>
        /// <returns>The <see cref="NestedElement.ShortName"/> if found, otherwise null</returns>
        string GetNestedElementPath(ElementUsage elementUsage, Option option);

        /// <summary>
        /// Get <see cref="NestedParameter.Path"/> for a <see cref="ParameterBase"/> 
        /// </summary>
        /// <param name="parameterBase">The <see cref="ParameterBase"/></param>
        /// <param name="option">The <see cref="Option"/></param>
        /// <returns>The <see cref="NestedParameter.Path"/> if found, otherwise null</returns>
        string GetNestedParameterPath(ParameterBase parameterBase, Option option);

        /// <summary>
        /// Get <see cref="NestedParameter.Path"/> for a <see cref="ParameterBase"/> 
        /// </summary>
        /// <param name="parameterBase">The <see cref="ParameterBase"/></param>
        /// <param name="option">The <see cref="Option"/></param>
        /// <param name="actualFiniteState">Get the <see cref="NestedParameter.Path"/> from a specific <see cref="ActualFiniteState"/></param>
        /// <returns>The <see cref="NestedParameter.Path"/> if found, otherwise null</returns>
        string GetNestedParameterPath(ParameterBase parameterBase, Option option, ActualFiniteState actualFiniteState);
    }
}
