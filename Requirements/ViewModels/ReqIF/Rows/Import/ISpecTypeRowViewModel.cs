// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISpecTypeRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
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

namespace CDP4Requirements.ViewModels
{
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;

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