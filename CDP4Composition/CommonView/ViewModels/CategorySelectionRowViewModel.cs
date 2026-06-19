// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategorySelectionRowViewModel.cs" company="Starion Group S.A.">
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

namespace CDP4CommonView.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.SiteDirectoryData;

    using ReactiveUI;

    /// <summary>
    /// A row-view-model that represents a single <see cref="Category"/> in the category selection grid shown when
    /// assigning <see cref="Category"/>s to a <see cref="CDP4Common.CommonData.ICategorizableThing"/>. It exposes the
    /// <see cref="Category"/>'s short name, name, super categories, container <see cref="ReferenceDataLibrary"/> and
    /// first <see cref="Definition"/> (GitHub issues #72 and #1043) and a <see cref="IsSelected"/> flag driving the
    /// check box.
    /// </summary>
    public class CategorySelectionRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="IsSelected"/>.
        /// </summary>
        private bool isSelected;

        /// <summary>
        /// Backing field for <see cref="IsReadOnly"/>.
        /// </summary>
        private bool isReadOnly;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategorySelectionRowViewModel"/> class.
        /// </summary>
        /// <param name="category">
        /// The <see cref="Category"/> that is represented by this row-view-model.
        /// </param>
        public CategorySelectionRowViewModel(Category category)
        {
            this.Category = category ?? throw new ArgumentNullException(nameof(category));
            this.SuperCategory = category.SuperCategory.Count == 0 ? string.Empty : string.Join(", ", category.SuperCategory.Select(x => x.ShortName));
            this.ContainerRdl = category.Container is ReferenceDataLibrary containerRdl ? containerRdl.ShortName : string.Empty;
            this.Definition = category.Definition.FirstOrDefault()?.Content ?? string.Empty;
        }

        /// <summary>
        /// Gets the <see cref="Category"/> represented by this row-view-model.
        /// </summary>
        public Category Category { get; private set; }

        /// <summary>
        /// Gets the name of the <see cref="Category"/>.
        /// </summary>
        public string Name => this.Category.Name;

        /// <summary>
        /// Gets the short name of the <see cref="Category"/>.
        /// </summary>
        public string ShortName => this.Category.ShortName;

        /// <summary>
        /// Gets the comma-separated short names of the super categories of the <see cref="Category"/>.
        /// </summary>
        public string SuperCategory { get; private set; }

        /// <summary>
        /// Gets the short name of the container <see cref="ReferenceDataLibrary"/> of the <see cref="Category"/>.
        /// </summary>
        public string ContainerRdl { get; private set; }

        /// <summary>
        /// Gets the content of the first <see cref="CDP4Common.CommonData.Definition"/> of the <see cref="Category"/>.
        /// </summary>
        public string Definition { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the represented <see cref="Category"/> is deprecated.
        /// </summary>
        public bool IsDeprecated => this.Category.IsDeprecated;

        /// <summary>
        /// Gets or sets a value indicating whether the represented <see cref="Category"/> is assigned to the
        /// categorizable thing.
        /// </summary>
        public bool IsSelected
        {
            get => this.isSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref this.isSelected, value);
                this.RaisePropertyChanged(nameof(this.IsSelectable));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the category selection grid is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get => this.isReadOnly;
            set
            {
                this.RaiseAndSetIfChanged(ref this.isReadOnly, value);
                this.RaisePropertyChanged(nameof(this.IsSelectable));
            }
        }

        /// <summary>
        /// Gets a value indicating whether the check box of this row can be toggled. A deprecated <see cref="Category"/>
        /// that is not already assigned cannot be selected (but an assigned one can still be de-selected), and nothing
        /// can be toggled while the grid is read-only.
        /// </summary>
        public bool IsSelectable => !this.IsReadOnly && (!this.IsDeprecated || this.IsSelected);

        /// <summary>
        /// Orders the supplied <paramref name="rows"/> showing the selected (assigned) <see cref="Category"/>s first and
        /// then the remaining ones in alphabetical order of their <see cref="Name"/> (GitHub issues #72 and #1043).
        /// </summary>
        /// <param name="rows">The rows to order.</param>
        /// <returns>The ordered rows.</returns>
        public static IEnumerable<CategorySelectionRowViewModel> OrderBySelectionThenName(IEnumerable<CategorySelectionRowViewModel> rows)
        {
            return (rows ?? Enumerable.Empty<CategorySelectionRowViewModel>())
                .OrderByDescending(row => row.IsSelected)
                .ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase);
        }
    }
}
