// -------------------------------------------------------------------------------------------------
// <copyright file="ISpecTypeRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System.Collections.Generic;
    using CDP4Common.SiteDirectoryData;
    using ReactiveUI;
    using ReqIFSharp;

    /// <summary>
    /// The interface for rows representing a mapping row for <see cref="SpecType"/>
    /// </summary>
    /// <typeparam name="T">A type of <see cref="SpecType"/></typeparam>
    public interface ISpecTypeRowViewModel<out T> : IMappingRowViewModelBase<T> where T : SpecType
    {
        /// <summary>
        /// Gets or sets the selected <see cref="ParameterizedCategoryRule"/>
        /// </summary>
        ReactiveList<ParameterizedCategoryRule> SelectedRules { get; set; }

        /// <summary>
        /// Gets or sets the selected <see cref="CategoryComboBoxItemViewModel"/>
        /// </summary>
        ReactiveList<CategoryComboBoxItemViewModel> SelectedCategories { get; set; }

        /// <summary>
        /// Gets the possible <see cref="ParameterizedCategoryRule"/>
        /// </summary>
        ReactiveList<ParameterizedCategoryRule> PossibleRules { get; }

        /// <summary>
        /// Gets the possible <see cref="CategoryComboBoxItemViewModel"/>
        /// </summary>
        ReactiveList<CategoryComboBoxItemViewModel> PossibleCategories { get; }

        /// <summary>
        /// Gets the <see cref="AttributeDefinitionMappingRowViewModel"/>
        /// </summary>
        ReactiveList<AttributeDefinitionMappingRowViewModel> AttributeDefinitions { get; }
    }
}