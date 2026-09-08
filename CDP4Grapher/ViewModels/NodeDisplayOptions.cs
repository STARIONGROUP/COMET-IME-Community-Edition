// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NodeDisplayOptions.cs" company="Starion Group S.A.">
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

namespace CDP4Grapher.ViewModels
{
    using CDP4Common.CommonData;

    /// <summary>
    /// The choice of which lines a node box on the traceability diagram shows, so the boxes can be kept small. The long
    /// <see cref="DefinedThing.Definition"/> content is capped by <see cref="DefinitionMaxLength"/> for the same reason.
    /// </summary>
    public class NodeDisplayOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="ClassKind"/> line is shown
        /// </summary>
        public bool ShowClassKind { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the short name line is shown
        /// </summary>
        public bool ShowShortName { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the name line is shown
        /// </summary>
        public bool ShowName { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the definition line is shown
        /// </summary>
        public bool ShowDefinition { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of characters of the definition that is shown, beyond which it is truncated
        /// with an ellipsis, to keep the boxes from growing too large
        /// </summary>
        public int DefinitionMaxLength { get; set; } = 100;
    }
}
