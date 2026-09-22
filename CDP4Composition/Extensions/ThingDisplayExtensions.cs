// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThingDisplayExtensions.cs" company="Starion Group S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Extension methods that produce a human-readable name for a <see cref="Thing"/>, handling the specific cases
    /// where the SDK <see cref="Thing.UserFriendlyName"/> falls back to its "not implemented" text.
    /// </summary>
    public static class ThingDisplayExtensions
    {
        /// <summary>
        /// Gets a human-readable name for the <paramref name="thing"/>. A <see cref="File"/> carries no name of its own,
        /// so the name of its current <see cref="FileRevision"/> is used; any other <see cref="Thing"/> falls back to its
        /// <see cref="Thing.UserFriendlyName"/>. Add further <c>case</c>s here when other name-less things need a display name.
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/> to represent.</param>
        /// <returns>The display name of the <paramref name="thing"/>.</returns>
        public static string QueryName(this Thing thing)
        {
            switch (thing)
            {
                case null:
                    return string.Empty;
                case File file:
                    return file.CurrentFileRevision?.Name;
                default:
                    return thing.UserFriendlyName;
            }
        }
    }
}
