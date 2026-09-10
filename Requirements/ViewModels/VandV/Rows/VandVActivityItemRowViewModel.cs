// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVActivityItemRowViewModel.cs" company="Starion Group S.A.">
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

namespace CDP4Requirements.ViewModels.Rows
{
    using CDP4Requirements.Rdl;
    using CDP4Requirements.Services;

    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// One read-only line in the activity dialog's V&amp;V Items tab: an item the activity performs, the requirement
    /// that item verifies, and where it stands. A plain projection rather than a
    /// <see cref="CDP4Composition.Mvvm.RowViewModelBase{T}"/>, because the dialog only shows it, and a row
    /// view-model would bring message-bus subscriptions the dialog would have to dispose.
    /// </summary>
    public class VandVActivityItemRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VandVActivityItemRowViewModel"/> class.
        /// </summary>
        /// <param name="item">The V&amp;V item performed by the activity.</param>
        /// <param name="covered">The requirement the item verifies, resolved once by the caller.</param>
        /// <param name="activity">The activity performing the item, which the dialog already knows.</param>
        public VandVActivityItemRowViewModel(Requirement item, Requirement covered, Requirement activity)
        {
            this.ShortName = item.ShortName;
            this.Name = item.Name;
            this.Requirement = covered == null ? string.Empty : $"{covered.ShortName}: {covered.Name}";
            this.Status = VandVActivityQuery.EffectiveAttribute(item, activity, VandVParameter.Status);
            this.Compliance = VandVCloseOut.QueryCompliance(item);
            this.CloseOut = VandVCloseOut.IsClosed(item) ? "Closed" : "Open";
        }

        /// <summary>
        /// Gets the short-name of the V&amp;V item.
        /// </summary>
        public string ShortName { get; }

        /// <summary>
        /// Gets the name of the V&amp;V item.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the requirement the item verifies or validates.
        /// </summary>
        public string Requirement { get; }

        /// <summary>
        /// Gets the execution status, the activity's when the item states none of its own.
        /// </summary>
        public string Status { get; }

        /// <summary>
        /// Gets the compliance status, which is always the item's own judgement.
        /// </summary>
        public string Compliance { get; }

        /// <summary>
        /// Gets whether the item has been closed out.
        /// </summary>
        public string CloseOut { get; }
    }
}
