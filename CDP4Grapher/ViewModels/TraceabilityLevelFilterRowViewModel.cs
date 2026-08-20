// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TraceabilityLevelFilterRowViewModel.cs" company="Starion Group S.A.">
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
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;

    using CDP4Grapher.Helpers;

    using ReactiveUI;

    /// <summary>
    /// Represents one per-level override row of the relationship filter of the traceability diagram: at the given
    /// <see cref="Level"/> only relationships that are a member of one of the <see cref="SelectedCategories"/> are
    /// followed, instead of the default filter.
    /// </summary>
    public class TraceabilityLevelFilterRowViewModel : ReactiveObject
    {
        /// <summary>
        /// The callback that notifies the owning panel that this row changed
        /// </summary>
        private readonly Action onChanged;

        /// <summary>
        /// Backing field for <see cref="Level"/>
        /// </summary>
        private int level = 1;

        /// <summary>
        /// Backing field for <see cref="SelectedCategories"/>
        /// </summary>
        private List<Category> selectedCategories = new List<Category>();

        /// <summary>
        /// Backing field for <see cref="SelectedDirection"/>
        /// </summary>
        private TraceabilityDirectionOption selectedDirection = TraceabilityDirectionOption.All[0];

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceabilityLevelFilterRowViewModel"/> class
        /// </summary>
        /// <param name="possibleCategories">
        /// The <see cref="Category"/>s the user may pick from
        /// </param>
        /// <param name="onChanged">
        /// The callback that notifies the owning panel that this row changed
        /// </param>
        public TraceabilityLevelFilterRowViewModel(ReactiveList<Category> possibleCategories, Action onChanged)
        {
            this.PossibleCategories = possibleCategories;
            this.onChanged = onChanged;
        }

        /// <summary>
        /// Gets the <see cref="Category"/>s the user may pick from
        /// </summary>
        public ReactiveList<Category> PossibleCategories { get; }

        /// <summary>
        /// Gets or sets the one-based level at which this override applies
        /// </summary>
        public int Level
        {
            get => this.level;
            set
            {
                this.RaiseAndSetIfChanged(ref this.level, value);
                this.onChanged?.Invoke();
            }
        }

        /// <summary>
        /// Gets or sets how relationship arrows are followed at this level; the first
        /// <see cref="TraceabilityDirectionOption"/> stands for the natural direction of the expansion
        /// </summary>
        public TraceabilityDirectionOption SelectedDirection
        {
            get => this.selectedDirection;
            set
            {
                this.RaiseAndSetIfChanged(ref this.selectedDirection, value ?? TraceabilityDirectionOption.All[0]);
                this.onChanged?.Invoke();
            }
        }

        /// <summary>
        /// Gets how relationship arrows are followed at this level, or null for the natural direction of the expansion
        /// </summary>
        public RelationshipTraversalDirection? Direction => this.SelectedDirection.Direction;

        /// <summary>
        /// Gets a value indicating whether this row constrains its level at all: it must have a valid level and either
        /// a category filter or a direction override
        /// </summary>
        public bool IsActive => this.Level >= 1 && (this.SelectedCategories.Any() || this.Direction != null);

        /// <summary>
        /// Gets or sets the <see cref="Category"/>s that a relationship must be a member of to be followed at this
        /// level
        /// </summary>
        public List<Category> SelectedCategories
        {
            get => this.selectedCategories;
            set
            {
                this.RaiseAndSetIfChanged(ref this.selectedCategories, value ?? new List<Category>());
                this.onChanged?.Invoke();
            }
        }
    }
}
