// -------------------------------------------------------------------------------------------------
// <copyright file="ExtraMassContributionConfigurationViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2023 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Budget.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Reactive;

    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;

    using ReactiveUI;

    /// <summary>
    /// The view-model to setup the extra mass contribution
    /// </summary>
    public class ExtraMassContributionConfigurationViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="SelectedCategories"/>
        /// </summary>
        private ReactiveList<Category> selectedCategories;

        /// <summary>
        /// Backing field for <see cref="SelectedParameter"/>
        /// </summary>
        private QuantityKind selectedParameter;

        /// <summary>
        /// Backing field for <see cref="SelectedMarginParameter"/>
        /// </summary>
        private QuantityKind selectedMarginParameter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtraMassContributionConfigurationViewModel"/> class
        /// </summary>
        /// <param name="possibleCategories">The possible <see cref="Category"/></param>
        /// <param name="validateForm">The form validator action</param>
        /// <param name="removeAction">The action to remove the current view-model</param>
        public ExtraMassContributionConfigurationViewModel(IReadOnlyList<Category> possibleCategories, Action validateForm, Action<ExtraMassContributionConfigurationViewModel> removeAction)
        {
            this.PossibleCategories = new ReactiveList<Category>(possibleCategories);
            this.RemoveExtraMassContributionCommand = ReactiveCommandCreator.Create(() => removeAction(this));

            this.WhenAnyValue(x => x.SelectedCategories, x => x.SelectedParameter).Subscribe(x => validateForm());
        }

        /// <summary>
        /// Gets the possible <see cref="Category"/>
        /// </summary>
        public ReactiveList<Category> PossibleCategories { get; }

        /// <summary>
        /// Gets or sets the selected <see cref="Category"/>
        /// </summary>
        public ReactiveList<Category> SelectedCategories
        {
            get { return this.selectedCategories; }
            set { this.RaiseAndSetIfChanged(ref this.selectedCategories, value); }
        }

        /// <summary>
        /// Gets or sets the selected parameter
        /// </summary>
        public QuantityKind SelectedParameter
        {
            get { return this.selectedParameter; }
            set { this.RaiseAndSetIfChanged(ref this.selectedParameter, value); }
        }

        /// <summary>
        /// Gets or sets the selected margin-parameter
        /// </summary>
        public QuantityKind SelectedMarginParameter
        {
            get { return this.selectedMarginParameter; }
            set { this.RaiseAndSetIfChanged(ref this.selectedMarginParameter, value); }
        }

        /// <summary>
        /// Gets the command to remove an extra mass contribution
        /// </summary>
        public ReactiveCommand<Unit, Unit> RemoveExtraMassContributionCommand { get; }
    }
}
