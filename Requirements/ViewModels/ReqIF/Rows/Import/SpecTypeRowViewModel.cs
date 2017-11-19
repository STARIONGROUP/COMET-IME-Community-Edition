﻿// -------------------------------------------------------------------------------------------------
// <copyright file="SpecElementRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using ReactiveUI;
    using ReqIFSharp;
    using ReqIFDal;

    /// <summary>
    /// row-view-model to represent a <see cref="SpecElementWithAttributes"/>
    /// </summary>
    public abstract class SpecTypeRowViewModel<T> : MappingRowViewModelBase<T>, ISpecTypeRowViewModel<T> where T : SpecType
    {
        /// <summary>
        /// The <see cref="ParameterType"/> map
        /// </summary>
        private IReadOnlyDictionary<DatatypeDefinition, DatatypeDefinitionMap> parameterTypesMap;

        /// <summary>
        /// Backing field for <see cref="SelectedRules"/>
        /// </summary>
        private ReactiveList<ParameterizedCategoryRule> selectedRules;

        /// <summary>
        /// Backing field for <see cref="SelectedCategories"/>
        /// </summary>
        private ReactiveList<CategoryComboBoxItemViewModel> selectedCategories;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecTypeRowViewModel{T}"/> class
        /// </summary>
        /// <param name="specType">The <see cref="SpecType"/></param>
        /// <param name="iteration">The <see cref="Iteration"/></param>
        /// <param name="parameterTypeMap">The <see cref="DatatypeDefinition"/> map</param>
        /// <param name="refreshCanGoNext">The Call-back method</param>
        protected SpecTypeRowViewModel(T specType, Iteration iteration, IReadOnlyDictionary<DatatypeDefinition, DatatypeDefinitionMap> parameterTypeMap, Action refreshCanGoNext)
            : base(specType)
        {
            this.Iteration = iteration;
            this.parameterTypesMap = parameterTypeMap;
            this.AttributeDefinitions = new ReactiveList<AttributeDefinitionMappingRowViewModel>();
            this.PossibleRules = new ReactiveList<ParameterizedCategoryRule>();
            this.PossibleCategories = new ReactiveList<CategoryComboBoxItemViewModel>();
            this.WhenAnyValue(x => x.IsMapped).Subscribe(_ => refreshCanGoNext());
            this.SetAttributeDefinition();
        }

        /// <summary>
        /// Gets or sets the current <see cref="Iteration"/> context
        /// </summary>
        protected Iteration Iteration { get; set; }

        /// <summary>
        /// Gets the rows representing the <see cref="AttributeDefinition"/>s for this <see cref="SpecType"/>
        /// </summary>
        /// <remarks>
        /// The first row shall represent the <see cref="SpecType"/> followed by potential <see cref="AttributeValue"/>
        /// </remarks>
        public ReactiveList<AttributeDefinitionMappingRowViewModel> AttributeDefinitions { get; private set; }

        /// <summary>
        /// Gets or sets the selected <see cref="ParameterizedCategoryRule"/>
        /// </summary>
        public ReactiveList<ParameterizedCategoryRule> SelectedRules
        {
            get { return this.selectedRules; }
            set { this.RaiseAndSetIfChanged(ref this.selectedRules, value); }
        }

        /// <summary>
        /// Gets or sets the selected <see cref="CategoryComboBoxItemViewModel"/>
        /// </summary>
        public ReactiveList<CategoryComboBoxItemViewModel> SelectedCategories
        {
            get { return this.selectedCategories; }
            set { this.RaiseAndSetIfChanged(ref this.selectedCategories, value); }
        }

        /// <summary>
        /// Gets the possible <see cref="ParameterizedCategoryRule"/>
        /// </summary>
        public ReactiveList<ParameterizedCategoryRule> PossibleRules { get; private set; }

        /// <summary>
        /// Gets the possible <see cref="Category"/>
        /// </summary>
        public ReactiveList<CategoryComboBoxItemViewModel> PossibleCategories { get; private set; }

        /// <summary>
        /// Populates the possible <see cref="ParameterizedCategoryRule"/>
        /// </summary>
        public abstract void PopulatePossibleRules();

        /// <summary>
        /// Populate the possible <see cref="Category"/>
        /// </summary>
        public abstract void PopulatePossibleCategories();

        /// <summary>
        /// Add the <see cref="AttributeDefinition"/>s row for the current <see cref="SpecType"/>
        /// </summary>
        private void SetAttributeDefinition()
        {
            foreach (var attributeDefinition in this.Identifiable.SpecAttributes)
            {
                DatatypeDefinitionMap datatypeMap;
                var parameterType = this.parameterTypesMap.TryGetValue(attributeDefinition.DatatypeDefinition, out datatypeMap) ? datatypeMap.ParameterType : null;

                var row = new AttributeDefinitionMappingRowViewModel(attributeDefinition, parameterType, this.UpdateIsMapped);
                this.AttributeDefinitions.Add(row);
            }
        }

        /// <summary>
        /// Update the <see cref="SpecTypeRowViewModel{T}.IsMapped"/> property of this row
        /// </summary>
        protected override void UpdateIsMapped()
        {
            this.IsMapped = this.AttributeDefinitions.All(x => x.IsMapped) 
                && this.AttributeDefinitions.Count(x => x.AttributeDefinitionMapKind == AttributeDefinitionMapKind.FIRST_DEFINITION) < 2
                && this.AttributeDefinitions.Count(x => x.AttributeDefinitionMapKind == AttributeDefinitionMapKind.SHORTNAME) < 2
                && this.AttributeDefinitions.Count(x => x.AttributeDefinitionMapKind == AttributeDefinitionMapKind.NAME) < 2;
        }
    }
}