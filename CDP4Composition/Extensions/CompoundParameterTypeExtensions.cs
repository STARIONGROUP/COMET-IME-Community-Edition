// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompoundParameterTypeExtensions.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Extensions
{
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// The purpose of these <see cref="CompoundParameterTypeExtensions"/> is to add functionality to <see cref="CompoundParameterType"/> instances
    /// </summary>
    public static class CompoundParameterTypeExtensions
    {
        /// <summary>
        /// Queries the <see cref="ParameterTypeComponent"/> at the specified index
        /// </summary>
        /// <param name="compoundParameterType">
        /// The <see cref="CompoundParameterType"/> that contains the <see cref="ParameterTypeComponent"/>
        /// </param>
        /// <param name="componentIndex">
        /// The index of the <see cref="ParameterTypeComponent"/>
        /// </param>
        /// <returns>
        /// The <see cref="ParameterTypeComponent"/> at <paramref name="componentIndex"/>, or null when the
        /// <see cref="CompoundParameterType"/> does not contain a component at that index
        /// </returns>
        /// <remarks>
        /// A <see cref="CompoundParameterType"/> that could not be resolved from the cache - for instance because the
        /// <see cref="ReferenceDataLibrary"/> that contains it is not (yet) available on the client - has no components
        /// at all, while the rows that represent its values still address them by index. Indexing the component list
        /// directly then terminates the operation with an exception, see issue #1457.
        /// </remarks>
        public static ParameterTypeComponent QueryComponent(this CompoundParameterType compoundParameterType, int componentIndex)
        {
            if (componentIndex < 0 || componentIndex >= compoundParameterType.Component.Count)
            {
                return null;
            }

            return compoundParameterType.Component[componentIndex];
        }
    }
}
