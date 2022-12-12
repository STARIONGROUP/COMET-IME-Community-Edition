// -------------------------------------------------------------------------------------------------
// <copyright file="ExtraMassContributionConfigurationViewModel.cs" company="RHEA System S.A.">
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
            object NoOp(object param) => param;

            this.PossibleCategories = new ReactiveList<Category>(possibleCategories);
            this.RemoveExtraMassContributionCommand = ReactiveCommand.Create<object, object>(NoOp);
            this.RemoveExtraMassContributionCommand.Subscribe(_ => removeAction(this));

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
        public ReactiveCommand<object, object> RemoveExtraMassContributionCommand { get; }
    }
}
