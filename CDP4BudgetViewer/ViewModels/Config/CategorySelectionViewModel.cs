// -------------------------------------------------------------------------------------------------
// <copyright file="CategorySelectionViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Budget.ViewModels
{
    using System;
    using System.Collections.Generic;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;

    using ReactiveUI;

    /// <summary>
    /// The view-model used to setup configuration for the budget view
    /// </summary>
    public class CategorySelectionViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="SelectedCategories"/>
        /// </summary>
        private ReactiveList<Category> selectedCategories;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategorySelectionViewModel"/> class
        /// </summary>
        /// <param name="possibleCategories">The possible <see cref="Category"/></param>
        /// <param name="validateMainForm">The main form validator that is triggered on any changes in this view-model</param>
        public CategorySelectionViewModel(IEnumerable<Category> possibleCategories, Action validateMainForm)
        {
            this.PossibleCategories = new ReactiveList<Category>(possibleCategories);
            this.WhenAnyValue(x => x.SelectedCategories).Subscribe(_ => validateMainForm());
        }

        /// <summary>
        /// Gets or sets the selected <see cref="Category"/>
        /// </summary>
        public ReactiveList<Category> SelectedCategories
        {
            get { return this.selectedCategories; }
            set { this.RaiseAndSetIfChanged(ref this.selectedCategories, value); }
        }

        /// <summary>
        /// Gets the possible <see cref="Category"/> to select
        /// </summary>
        public ReactiveList<Category> PossibleCategories { get; private set; }
    }
}
