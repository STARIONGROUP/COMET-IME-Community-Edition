// -------------------------------------------------------------------------------------------------
// <copyright file="CategoryComboBoxItemViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// The view-model for combo-box items
    /// </summary>
    public class CategoryComboBoxItemViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryComboBoxItemViewModel"/> class
        /// </summary>
        /// <param name="category">The <see cref="Category"/></param>
        /// <param name="isEnabled">A value indicating whether this is enabled</param>
        public CategoryComboBoxItemViewModel(Category category, bool isEnabled)
        {
            this.Category = category;
            this.IsEnabled = isEnabled;
        }

        /// <summary>
        /// Gets the <see cref="Category"/> associated
        /// </summary>
        public Category Category { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="CategoryComboBoxItemViewModel"/> should be enabled
        /// </summary>
        public bool IsEnabled { get; private set; }
    }
}