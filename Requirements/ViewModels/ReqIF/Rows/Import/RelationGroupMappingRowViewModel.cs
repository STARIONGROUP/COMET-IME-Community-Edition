// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationGroupMappingRowViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;

    using ReactiveUI;

    using ReqIFSharp;

    /// <summary>
    /// The row-view-model for the <see cref="RelationGroupType"/> mapping
    /// </summary>
    public class RelationGroupMappingRowViewModel : SpecTypeRowViewModel<RelationGroupType>
    {
        /// <summary>
        /// Backing field for <see cref="SelectedBinaryRelationshipRules"/>
        /// </summary>
        private ReactiveList<BinaryRelationshipRule> selectedBinaryRelationshipRules;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationGroupMappingRowViewModel"/> class
        /// </summary>
        /// <param name="specRelationType">The <see cref="RelationGroupType"/></param>
        /// <param name="iteration">The <see cref="Iteration"/></param>
        /// <param name="parameterTypeMap">The <see cref="DatatypeDefinition"/> map</param>
        /// <param name="refreshCanGoNext">The call-back method</param>
        public RelationGroupMappingRowViewModel(RelationGroupType specRelationType, Iteration iteration, IReadOnlyDictionary<DatatypeDefinition, DatatypeDefinitionMap> parameterTypeMap, Action refreshCanGoNext)
            : base(specRelationType, iteration, parameterTypeMap, refreshCanGoNext)
        {
            this.PossibleBinaryRelationshipRules = new ReactiveList<BinaryRelationshipRule>();
            this.PopulatePossibleRules();
            this.WhenAnyValue(x => x.SelectedRules).Subscribe(x => this.PopulatePossibleCategories());
            this.WhenAnyValue(x => x.SelectedBinaryRelationshipRules).Subscribe(x => this.PopulatePossibleCategories());
        }

        /// <summary>
        /// Gets or sets the selected <see cref="BinaryRelationshipRule"/>s
        /// </summary>
        public ReactiveList<BinaryRelationshipRule> SelectedBinaryRelationshipRules
        {
            get { return this.selectedBinaryRelationshipRules; }
            set { this.RaiseAndSetIfChanged(ref this.selectedBinaryRelationshipRules, value); }
        }

        /// <summary>
        /// Gets the <see cref="ParameterType"/>
        /// </summary>
        public ReactiveList<BinaryRelationshipRule> PossibleBinaryRelationshipRules { get; private set; }

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

            var categories = rdls.SelectMany(x => x.DefinedCategory.Where(cat => cat.PermissibleClass.Contains(ClassKind.BinaryRelationship))).OrderBy(x => x.Name);

            // compute categories from selectedRule if any
            var categoriesFromRules = new List<Category>();
            if (this.SelectedRules != null && this.SelectedRules.Any())
            {
                categoriesFromRules.AddRange(this.SelectedRules.Select(r => r.Category));
            }

            if (this.SelectedBinaryRelationshipRules != null && this.SelectedBinaryRelationshipRules.Any())
            {
                categoriesFromRules.AddRange(this.SelectedBinaryRelationshipRules.Select(r => r.RelationshipCategory));
            }

            categoriesFromRules = categoriesFromRules.OrderBy(x => x.Name).ToList();

            this.PossibleCategories.AddRange(categoriesFromRules.Select(x => new CategoryComboBoxItemViewModel(x, false)));
            this.PossibleCategories.AddRange(categories.Except(categoriesFromRules).Select(x => new CategoryComboBoxItemViewModel(x, true)));

            var previousSelection = this.SelectedCategories != null ? this.PossibleCategories.Where(x => this.SelectedCategories.Select(s => s.Category).Contains(x.Category)) : new CategoryComboBoxItemViewModel[] { };
            this.SelectedCategories = new ReactiveList<CategoryComboBoxItemViewModel>(this.PossibleCategories.Where(x => !x.IsEnabled).Union(previousSelection));
        }

        /// <summary>
        /// Populates the possible <see cref="ParameterizedCategoryRule"/>
        /// </summary>
        public override void PopulatePossibleRules()
        {
            this.PossibleRules.Clear();
            this.PossibleBinaryRelationshipRules.Clear();

            var model = (EngineeringModel)this.Iteration.Container;
            var rdls = new List<ReferenceDataLibrary>();

            var requiredRdl = model.EngineeringModelSetup.RequiredRdl.Single();
            rdls.Add(requiredRdl);
            rdls.AddRange(requiredRdl.GetRequiredRdls());

            var rules = rdls.SelectMany(x => x.Rule.OfType<ParameterizedCategoryRule>().Where(r => r.Category.PermissibleClass.Contains(ClassKind.BinaryRelationship))).OrderBy(x => x.Name);
            var binaryRules = rdls.SelectMany(x => x.Rule.OfType<BinaryRelationshipRule>().Where(r => r.RelationshipCategory.PermissibleClass.Contains(ClassKind.BinaryRelationship))).OrderBy(x => x.Name);

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

            var oldBinaryRules = this.PossibleBinaryRelationshipRules.Except(binaryRules);
            var newBinaryRules = binaryRules.Except(this.PossibleBinaryRelationshipRules);
            foreach (var parameterizedCategoryRule in oldBinaryRules)
            {
                this.PossibleBinaryRelationshipRules.Remove(parameterizedCategoryRule);
            }

            foreach (var parameterizedCategoryRule in newBinaryRules)
            {
                this.PossibleBinaryRelationshipRules.Add(parameterizedCategoryRule);
            }
        }
    }
}