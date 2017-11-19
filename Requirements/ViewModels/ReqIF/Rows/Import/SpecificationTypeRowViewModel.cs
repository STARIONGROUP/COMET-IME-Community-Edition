// -------------------------------------------------------------------------------------------------
// <copyright file="SpecificationTypeRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using ReactiveUI;
    using ReqIFSharp;

    /// <summary>
    /// The <see cref="SpecificationType"/> mapping row
    /// </summary>
    public class SpecificationTypeRowViewModel : SpecTypeRowViewModel<SpecificationType>
    {
        public SpecificationTypeRowViewModel(
            SpecificationType specType, 
            Iteration iteration,
            IReadOnlyDictionary<DatatypeDefinition, DatatypeDefinitionMap> parameterTypeMap,
            Action refreshCanGoNext
            )
            : base(specType, iteration, parameterTypeMap, refreshCanGoNext)
        {
            this.PopulatePossibleRules();
            this.WhenAnyValue(x => x.SelectedRules).Subscribe(x => this.PopulatePossibleCategories());
        }

        /// <summary>
        /// Popuolates the possible <see cref="Category"/>
        /// </summary>
        public override void PopulatePossibleCategories()
        {
            this.PossibleCategories.Clear();

            var model = (EngineeringModel)this.Iteration.Container;
            var rdls = new List<ReferenceDataLibrary>();

            var requiredRdl = model.EngineeringModelSetup.RequiredRdl.Single();
            rdls.Add(requiredRdl);
            rdls.AddRange(requiredRdl.GetRequiredRdls());

            var categories = rdls.SelectMany(x => x.DefinedCategory.Where(cat => cat.PermissibleClass.Contains(ClassKind.RequirementsSpecification))).OrderBy(x => x.Name);

            // compute categories from selectedRule if any
            var categoriesFromRules = new List<Category>();
            if (this.SelectedRules != null && this.SelectedRules.Any())
            {
                categoriesFromRules.AddRange(this.SelectedRules.Select(r => r.Category).OrderBy(x => x.Name));
            }

            this.PossibleCategories.AddRange(categoriesFromRules.Select(x => new CategoryComboBoxItemViewModel(x, false)));
            this.PossibleCategories.AddRange(categories.Except(categoriesFromRules).Select(x => new CategoryComboBoxItemViewModel(x, true)));

            var previousSelection = this.SelectedCategories != null ? this.PossibleCategories.Where(x => this.SelectedCategories.Select(s => s.Category).Contains(x.Category)) : new CategoryComboBoxItemViewModel[] {};
            this.SelectedCategories = new ReactiveList<CategoryComboBoxItemViewModel>(this.PossibleCategories.Where(x => !x.IsEnabled).Union(previousSelection));
        }

        /// <summary>
        /// Populates the possible <see cref="ParameterizedCategoryRule"/>
        /// </summary>
        public override void PopulatePossibleRules()
        {
            var model = (EngineeringModel)this.Iteration.Container;
            var rdls = new List<ReferenceDataLibrary>();

            var requiredRdl = model.EngineeringModelSetup.RequiredRdl.Single();
            rdls.Add(requiredRdl);
            rdls.AddRange(requiredRdl.GetRequiredRdls());

            var rules = rdls.SelectMany(x => x.Rule.OfType<ParameterizedCategoryRule>().Where(r => r.Category.PermissibleClass.Contains(ClassKind.RequirementsSpecification))).OrderBy(x => x.Name);

            var oldRules = this.PossibleRules.Except(rules);
            var newRules = rules.Except(this.PossibleRules);

            foreach (var parameterizedCategoryRule in oldRules)
            {
                this.PossibleRules.Remove(parameterizedCategoryRule);
            }

            foreach (var parameterizedCategoryRule in newRules)
            {
                this.PossibleRules.Add(parameterizedCategoryRule);
            }
        }
    }
}