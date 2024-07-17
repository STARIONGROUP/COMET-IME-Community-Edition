// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpecObjectTypesMappingDialogViewModel.cs" company="Starion Group S.A.">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Windows.Input;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    
    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    
    using CDP4Dal;
    using CDP4Dal.Operations;
    
    using ReactiveUI;

    using ReqIFSharp;

    /// <summary>
    /// The user-interface to map the <see cref="SpecObjectType"/> to <see cref="ParameterizedCategoryRule"/>s
    /// </summary>
    [DialogViewModelExport("SpecObjectTypesMappingDialogViewModel", "The dialog used to map the Reqif SpecObjectType in order to create Requirements.")]
    public class SpecObjectTypesMappingDialogViewModel : ReqIfMappingDialogViewModelBase
    {
        #region fields
        /// <summary>
        /// Backing field for <see cref="CanGoNext"/>
        /// </summary>
        private bool canGoNext;

        /// <summary>
        /// The <see cref="SpecType"/> map
        /// </summary>
        private Dictionary<SpecObjectType, SpecObjectTypeMap> specTypeMap;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="SpecObjectTypesMappingDialogViewModel"/> class.
        /// Used by MEF.
        /// </summary>
        public SpecObjectTypesMappingDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecObjectTypesMappingDialogViewModel"/> class.
        /// </summary>
        public SpecObjectTypesMappingDialogViewModel(IReadOnlyCollection<SpecType> specTypes, IReadOnlyDictionary<DatatypeDefinition, DatatypeDefinitionMap> datatypeDefMap, IReadOnlyDictionary<SpecObjectType, SpecObjectTypeMap> specTypeMap, Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, string lang)
            : base(iteration, session, thingDialogNavigationService, lang)
        {
            this.BackCommand = ReactiveCommandCreator.Create(this.ExecuteBackCommand);

            var canExecuteGoNext = this.WhenAnyValue(x => x.CanGoNext);
            this.NextCommand = ReactiveCommandCreator.Create(this.ExecuteNextCommand, canExecuteGoNext);

            this.CreateCommand = ReactiveCommandCreator.Create(this.ExecuteCreateCategoryTypeCommand);

            this.SpecTypes = new ReactiveList<SpecObjectTypeRowViewModel>();

            foreach (var specType in specTypes.OfType<SpecObjectType>())
            {
                var row = new SpecObjectTypeRowViewModel(specType, this.IterationClone, datatypeDefMap, this.UpdateCanGoNext);
                this.SpecTypes.Add(row);
            }

            if (specTypeMap != null)
            {
                foreach (var pair in specTypeMap)
                {
                    var row = this.SpecTypes.SingleOrDefault(x => x.Identifiable.Identifier == pair.Key.Identifier);

                    if (row is null)
                    {
                        continue;
                    }

                    row.SelectedRules = new ReactiveList<ParameterizedCategoryRule>(row.PossibleRules.Where(x => pair.Value.Rules?.FirstOrDefault(r => r.Iid == x.Iid) != null));
                    row.SelectedCategories = new ReactiveList<CategoryComboBoxItemViewModel>(row.PossibleCategories.Where(x => pair.Value.Categories?.FirstOrDefault(r => r.Iid == x.Category.Iid) != null));
                    
                    foreach (var attributeDefinitionMap in pair.Value.AttributeDefinitionMap)
                    {
                        var attRow = this.SpecTypes.SelectMany(x => x.AttributeDefinitions).Single(x => x.Identifiable.Identifier == attributeDefinitionMap.AttributeDefinition.Identifier);
                        attRow.AttributeDefinitionMapKind = attributeDefinitionMap.MapKind;
                    }
                }
            }

            this.UpdateCanGoNext();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets a value indicating whether the <see cref="NextCommand"/> is enabled
        /// </summary>
        public bool CanGoNext
        {
            get { return this.canGoNext; }
            private set { this.RaiseAndSetIfChanged(ref this.canGoNext, value); }
        }

        /// <summary>
        /// Gets the <see cref="SpecElementWithAttributes"/>s to map
        /// </summary>
        public ReactiveList<SpecObjectTypeRowViewModel> SpecTypes { get; private set; }

        /// <summary>
        /// Gets the back <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> BackCommand { get; private set; } 

        /// <summary>
        /// Gets the "next" <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> NextCommand { get; private set; }
        #endregion

        /// <summary>
        /// Execute the <see cref="ICommand"/> to create a <see cref="ParameterType"/>
        /// </summary>
        /// <remarks>
        /// A new transaction is created to allow a <see cref="ParameterType"/> to be created "on the fly"
        /// </remarks>
        private void ExecuteCreateCategoryTypeCommand()
        {
            var siteDirectory = this.Session.RetrieveSiteDirectory();
            var transactionContext = TransactionContextResolver.ResolveContext(siteDirectory);

            var category = new Category();

            var categoryTransaction = new ThingTransaction(transactionContext);
            this.AddContainedThingToTransaction(categoryTransaction, category);

            var result = this.ThingDialogNavigationService.Navigate(category, categoryTransaction, this.Session, true, ThingDialogKind.Create, this.ThingDialogNavigationService);

            if (!result.HasValue || !result.Value)
            {
                return;
            }

            foreach (var row in this.SpecTypes)
            {
                row.PopulatePossibleCategories();
            }
        }

        /// <summary>
        /// Update the <see cref="CanGoNext"/> property
        /// </summary>
        private void UpdateCanGoNext()
        {
            this.CanGoNext = this.SpecTypes.Count == 0 ||  this.SpecTypes.All(x => x.IsMapped);
        }

        /// <summary>
        /// Executes the <see cref="BackCommand"/>
        /// </summary>
        private void ExecuteBackCommand()
        {
            this.SetMaps();
            this.DialogResult = new RequirementTypeMappingDialogResult(this.specTypeMap, false, true);
        }

        /// <summary>
        /// Executes the <see cref="NextCommand"/>
        /// </summary>
        private void ExecuteNextCommand()
        {
            this.SetMaps();
            this.DialogResult = new RequirementTypeMappingDialogResult(this.specTypeMap, true, true);
        }

        /// <summary>
        /// Sets the maps
        /// </summary>
        private void SetMaps()
        {
            this.specTypeMap = new Dictionary<SpecObjectType, SpecObjectTypeMap>();
            foreach (var specTypeRow in this.SpecTypes)
            {
                var attributes = new List<AttributeDefinitionMap>();
                foreach (var attDefinition in specTypeRow.AttributeDefinitions)
                {
                    var attributeMap = new AttributeDefinitionMap(attDefinition.Identifiable, attDefinition.AttributeDefinitionMapKind);
                    attributes.Add(attributeMap);
                }

                var map = new SpecObjectTypeMap(specTypeRow.Identifiable, specTypeRow.SelectedRules, specTypeRow.SelectedCategories.Select(x => x.Category), attributes, !specTypeRow.IsGroup);
                this.specTypeMap.Add(specTypeRow.Identifiable, map);
            }
        }

        /// <summary>
        /// Executes the cancel <see cref="ICommand"/>
        /// </summary>
        protected override void ExecuteCancelCommand()
        {
            this.DialogResult = new RequirementTypeMappingDialogResult(null, null, false);
        }
    }
}