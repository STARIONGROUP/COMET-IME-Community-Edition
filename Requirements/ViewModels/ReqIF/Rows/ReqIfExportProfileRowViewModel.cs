// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReqIfExportProfileRowViewModel.cs" company="Starion Group S.A.">
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

namespace CDP4Requirements.ViewModels
{
    using CDP4Requirements.ReqIFDal;

    /// <summary>
    /// A selectable export-format row that pairs a <see cref="ReqIfExportProfile"/> with a human-readable name for
    /// display in the export dialog.
    /// </summary>
    public class ReqIfExportProfileRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReqIfExportProfileRowViewModel"/> class
        /// </summary>
        /// <param name="profile">The <see cref="ReqIfExportProfile"/> represented</param>
        /// <param name="displayName">The human-readable name shown to the user</param>
        public ReqIfExportProfileRowViewModel(ReqIfExportProfile profile, string displayName)
        {
            this.Profile = profile;
            this.DisplayName = displayName;
        }

        /// <summary>
        /// Gets the <see cref="ReqIfExportProfile"/> represented by this row
        /// </summary>
        public ReqIfExportProfile Profile { get; }

        /// <summary>
        /// Gets the human-readable name shown to the user
        /// </summary>
        public string DisplayName { get; }
    }
}
