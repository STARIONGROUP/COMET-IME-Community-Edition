// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BulkParticipantCreationResult.cs" company="Starion Group S.A.">
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

namespace CDP4SiteDirectory.ViewModels
{
    using System.Collections.Generic;

    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Navigation;

    /// <summary>
    /// The result of the <see cref="BulkParticipantCreationDialogViewModel"/>, carrying the set of
    /// <see cref="BulkParticipantRowViewModel"/>s for which a <see cref="Participant"/> shall be created.
    /// </summary>
    public class BulkParticipantCreationResult : BaseDialogResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BulkParticipantCreationResult"/> class.
        /// </summary>
        /// <param name="result">
        /// A value indicating whether the result is positive or negative.
        /// </param>
        /// <param name="participants">
        /// The selected <see cref="BulkParticipantRowViewModel"/>s for which a <see cref="Participant"/> shall be created.
        /// </param>
        public BulkParticipantCreationResult(bool? result, IEnumerable<BulkParticipantRowViewModel> participants)
            : base(result)
        {
            this.Participants = participants;
        }

        /// <summary>
        /// Gets the selected <see cref="BulkParticipantRowViewModel"/>s for which a <see cref="Participant"/> shall be created.
        /// </summary>
        public IEnumerable<BulkParticipantRowViewModel> Participants { get; private set; }
    }
}
