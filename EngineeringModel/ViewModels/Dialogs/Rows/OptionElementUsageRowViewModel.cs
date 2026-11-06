// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OptionElementUsageRowViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of CDP4-COMET-IME Community Edition.
//    The CDP4-COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels.Dialogs.Rows
{
    using CDP4Common.EngineeringModelData;

    using ReactiveUI;

    /// <summary>
    /// Row of the <see cref="Option"/> dialog that states whether an <see cref="ElementUsage"/> is included in that <see cref="Option"/>.
    /// </summary>
    public class OptionElementUsageRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="IsIncluded"/>.
        /// </summary>
        private bool isIncluded;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionElementUsageRowViewModel"/> class.
        /// </summary>
        /// <param name="elementUsage">The represented <see cref="ElementUsage"/>.</param>
        /// <param name="option">The <see cref="Option"/> the inclusion is computed for.</param>
        /// <param name="isEditable">Whether the user may change the inclusion of the <paramref name="elementUsage"/>.</param>
        public OptionElementUsageRowViewModel(ElementUsage elementUsage, Option option, bool isEditable)
        {
            this.Thing = elementUsage;
            this.IsEditable = isEditable;
            this.WasIncluded = this.IsIncludedIn(option);
            this.isIncluded = this.WasIncluded;
        }

        /// <summary>
        /// Gets the represented <see cref="ElementUsage"/>.
        /// </summary>
        public ElementUsage Thing { get; }

        /// <summary>
        /// Gets the name and short name of the <see cref="ElementDefinition"/> that contains the <see cref="ElementUsage"/>.
        /// The short name is shown since, unlike the name, it is unique in the <see cref="Iteration"/>.
        /// </summary>
        public string ElementDefinitionDisplayName => $"{((ElementDefinition)this.Thing.Container).Name} ({((ElementDefinition)this.Thing.Container).ShortName})";

        /// <summary>
        /// Gets the name of the <see cref="ElementUsage"/>.
        /// </summary>
        public string Name => this.Thing.Name;

        /// <summary>
        /// Gets the short name of the <see cref="ElementUsage"/>.
        /// </summary>
        public string ShortName => this.Thing.ShortName;

        /// <summary>
        /// Gets the short name of the owner of the <see cref="ElementUsage"/>.
        /// </summary>
        public string OwnerShortName => this.Thing.Owner?.ShortName;

        /// <summary>
        /// Gets a value indicating whether the user may change <see cref="IsIncluded"/>.
        /// </summary>
        public bool IsEditable { get; }

        /// <summary>
        /// Gets a value indicating whether <see cref="IsIncluded"/> differs from the persisted inclusion of the <see cref="ElementUsage"/>.
        /// </summary>
        public bool HasChanged => this.IsIncluded != this.WasIncluded;

        /// <summary>
        /// Gets a value indicating whether the <see cref="ElementUsage"/> is included in the <see cref="Option"/> as currently persisted.
        /// </summary>
        private bool WasIncluded { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="ElementUsage"/> is included in the <see cref="Option"/>.
        /// </summary>
        public bool IsIncluded
        {
            get => this.isIncluded;
            set => this.RaiseAndSetIfChanged(ref this.isIncluded, value);
        }

        /// <summary>
        /// Computes whether the <see cref="ElementUsage"/> is included in the given <see cref="Option"/> as currently persisted.
        /// </summary>
        /// <param name="option">The <see cref="Option"/> to check.</param>
        /// <returns><c>true</c> when the <paramref name="option"/> is not excluded from the <see cref="ElementUsage"/>.</returns>
        public bool IsIncludedIn(Option option) => this.Thing.ExcludeOption.TrueForAll(x => x.Iid != option.Iid);

        /// <summary>
        /// Restores <see cref="IsIncluded"/> to the persisted inclusion of the <see cref="ElementUsage"/>.
        /// </summary>
        public void Reset() => this.IsIncluded = this.WasIncluded;
    }
}
