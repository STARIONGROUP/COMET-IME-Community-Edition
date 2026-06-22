// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterClipboard.cs" company="Starion Group S.A.">
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

namespace CDP4EngineeringModel.Utilities
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// A process-wide clipboard that holds the <see cref="Parameter"/> or <see cref="ParameterGroup"/> that has been copied,
    /// so that it can be pasted into an <see cref="ElementDefinition"/> in any open <see cref="Iteration"/> of the same
    /// <see cref="CDP4Dal.ISession"/>. It is intentionally static, mirroring the way the application uses the WPF
    /// <see cref="System.Windows.Clipboard"/>, so that a copy made in one Element Definitions browser can be pasted in another.
    /// </summary>
    internal static class ParameterClipboard
    {
        /// <summary>
        /// Gets or sets the <see cref="Parameter"/> or <see cref="ParameterGroup"/> that has been copied and is ready to be pasted.
        /// </summary>
        public static Thing CopiedThing { get; set; }
    }
}
