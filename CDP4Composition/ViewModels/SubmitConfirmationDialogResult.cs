// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SubmitConfirmationDialogResult.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.ViewModels
{
    using System.Collections.Generic;

    using CDP4Common.CommonData;

    using CDP4Composition.Navigation;

    /// <summary>
    /// The purpose of the <see cref="SubmitConfirmationDialogResult"/> is to return a value
    /// that specifies that the parameter sheet changes shall be submitted, and the change
    /// SubmitMessage.
    /// </summary>
    public class SubmitConfirmationDialogResult : BaseDialogResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubmitConfirmationDialogResult"/> class.
        /// </summary>
        /// <param name="result">
        /// The result (true or false)
        /// </param>
        /// <param name="submitMessage">
        /// The SubmitMessage that describes the changes that were made..
        /// </param>
        /// <param name="clones">
        /// The the <see cref="Thing"/> clones that need to be submitted.
        /// </param>
        public SubmitConfirmationDialogResult(bool? result, string submitMessage, IEnumerable<Thing> clones)
            : base(result)
        {
            this.SubmitMessage = submitMessage;
            this.Clones = clones;
        }

        /// <summary>
        /// Gets the SubmitMessage that describes the changes that were made.
        /// </summary>
        public string SubmitMessage { get; private set; }

        /// <summary>
        /// Gets the <see cref="Thing"/> clones that need to be submitted.
        /// </summary>
        public IEnumerable<Thing> Clones { get; private set; }
    }
}
