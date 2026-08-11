// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SelectableThingRowViewModel.cs" company="Starion Group S.A.">
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

namespace CDP4VandV.ViewModels.Rows
{
    using CDP4Common.CommonData;

    using ReactiveUI;

    /// <summary>
    /// A tick-box row for choosing one <see cref="Thing"/>, used for the option and actual-finite-state pickers on the
    /// Coverage tab.
    /// </summary>
    /// <remarks>
    /// This exists instead of binding a DevExpress multi-select editor's <c>EditValue</c> directly to a typed
    /// collection: that binding hands back a loosely-typed list and throws when it is assigned to a strongly-typed
    /// property. A plain list of tick-box rows has no such conversion, and is directly unit-testable.
    /// </remarks>
    public class SelectableThingRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="IsSelected"/>
        /// </summary>
        private bool isSelected;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectableThingRowViewModel"/> class.
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/> being offered.</param>
        /// <param name="display">The text shown next to the tick box.</param>
        /// <param name="isSelected">Whether the thing starts out selected.</param>
        public SelectableThingRowViewModel(Thing thing, string display, bool isSelected = false)
        {
            this.Thing = thing;
            this.Display = display;
            this.isSelected = isSelected;
        }

        /// <summary>
        /// Gets the <see cref="Thing"/> this row represents.
        /// </summary>
        public Thing Thing { get; }

        /// <summary>
        /// Gets the text shown next to the tick box.
        /// </summary>
        public string Display { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the thing is selected.
        /// </summary>
        public bool IsSelected
        {
            get => this.isSelected;
            set => this.RaiseAndSetIfChanged(ref this.isSelected, value);
        }
    }
}
