// -------------------------------------------------------------------------------------------------
// <copyright file="SubSystemConfigViewModel.cs" company="RHEA System S.A.">
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

    public class SubSystemConfigViewModel : ReactiveObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubSystemConfigViewModel"/> class
        /// </summary>
        /// <param name="usedCategory">The possible <see cref="Category"/></param>
        /// <param name="validateMainForm">The form validator</param>
        /// <param name="remove">The action to remove this view-model</param>
        public SubSystemConfigViewModel(IReadOnlyList<Category> usedCategory, Action validateMainForm, Action<SubSystemConfigViewModel> remove)
        {
            this.SubSystemDefinitions = new CategorySelectionViewModel(usedCategory, validateMainForm);
            this.SubSystemElementDefinition = new CategorySelectionViewModel(usedCategory, validateMainForm);

            this.RemoveSubSystemDefinitionCommand = ReactiveCommandCreator.Create(() => remove(this));
        }

        /// <summary>
        /// Gets the command that remove a new sub-system definition
        /// </summary>
        public ReactiveCommand<Unit, Unit> RemoveSubSystemDefinitionCommand { get; private set; }

        /// <summary>
        /// Gets the view-model of the sub-system definition 
        /// </summary>
        /// <remarks>
        /// this is the category definition that the <see cref="ElementDefinition"/> to represent the sub-system
        /// </remarks>
        public CategorySelectionViewModel SubSystemDefinitions { get; private set; }

        /// <summary>
        /// Gets the view-model of the sub-system element definition
        /// </summary>
        public CategorySelectionViewModel SubSystemElementDefinition { get; private set; }

        /// <summary>
        /// Asserts whether the form is valid
        /// </summary>
        /// <returns>True if it is</returns>
        public bool IsFormValid()
        {
            return this.SubSystemElementDefinition.SelectedCategories != null
                   && this.SubSystemElementDefinition.SelectedCategories.Count > 0 
                   && this.SubSystemDefinitions.SelectedCategories != null
                   && this.SubSystemDefinitions.SelectedCategories.Count > 0;
        }
    }
}
