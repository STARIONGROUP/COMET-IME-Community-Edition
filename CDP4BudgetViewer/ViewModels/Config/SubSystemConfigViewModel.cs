// -------------------------------------------------------------------------------------------------
// <copyright file="SubSystemConfigViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------


namespace CDP4Budget.ViewModels
{
    using System;
    using System.Collections.Generic;
    using CDP4Common.SiteDirectoryData;
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
            object NoOp(object param) => param;

            this.SubSystemDefinitions = new CategorySelectionViewModel(usedCategory, validateMainForm);
            this.SubSystemElementDefinition = new CategorySelectionViewModel(usedCategory, validateMainForm);

            this.RemoveSubSystemDefinitionCommand = ReactiveCommand.Create<object, object>(NoOp);
            this.RemoveSubSystemDefinitionCommand.Subscribe(x => remove(this));
        }

        /// <summary>
        /// Gets the command that remove a new sub-system definition
        /// </summary>
        public ReactiveCommand<object, object> RemoveSubSystemDefinitionCommand { get; private set; }

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
