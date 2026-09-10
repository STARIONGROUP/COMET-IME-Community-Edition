// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVTestExtensions.cs" company="Starion Group S.A.">
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

namespace CDP4Requirements.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    /// <summary>
    /// Shared helpers for the V&amp;V test fixtures. The attribute-seeding logic used to be re-written as a private
    /// <c>SetAttribute</c> in every fixture, so a change to how a V&amp;V attribute is modelled meant editing a dozen
    /// identical copies.
    /// </summary>
    public static class VandVTestExtensions
    {
        /// <summary>
        /// Adds a text <see cref="SimpleParameterValue"/> to a <see cref="Requirement"/>, or replaces the value when the
        /// requirement already carries that parameter type.
        /// </summary>
        /// <param name="requirement">The requirement the attribute is set on.</param>
        /// <param name="parameterTypeShortName">The parameter type short-name, e.g. <c>vnv_stage</c>.</param>
        /// <param name="value">The value to store.</param>
        /// <param name="cache">The assembler cache the created things are registered in.</param>
        /// <param name="uri">The data-source uri the created things are stamped with.</param>
        /// <returns>The <see cref="SimpleParameterValue"/> that now carries the value.</returns>
        public static SimpleParameterValue SetVandVAttribute(this Requirement requirement, string parameterTypeShortName, string value, ConcurrentDictionary<CacheKey, Lazy<Thing>> cache, Uri uri)
        {
            var existing = requirement.ParameterValue.FirstOrDefault(x => x.ParameterType != null && x.ParameterType.ShortName == parameterTypeShortName);

            if (existing != null)
            {
                existing.Value = new ValueArray<string>([value]);

                return existing;
            }

            var simpleParameterValue = new SimpleParameterValue(Guid.NewGuid(), cache, uri)
            {
                ParameterType = new TextParameterType(Guid.NewGuid(), cache, uri) { ShortName = parameterTypeShortName, Name = parameterTypeShortName },
                Value = new ValueArray<string>([value])
            };

            requirement.ParameterValue.Add(simpleParameterValue);

            return simpleParameterValue;
        }
    }
}
