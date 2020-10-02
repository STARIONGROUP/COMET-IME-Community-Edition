// -------------------------------------------------------------------------------------------------
// <copyright file="SpecRelationTypeMappingDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using CDP4Common.EngineeringModelData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using ReactiveUI;
    using ReqIFSharp;

    [DialogViewModelExport("SpecRelationTypeMappingDialogViewModel", "The dialog used to map the Reqif SpecRelationType to categories and rules.")]
    public class SpecRelationTypeMappingDialogViewModel : ReqIfMappingDialogViewModelBase
    {
        /// <summary>
        /// The result of the mapping
        /// </summary>
        private Dictionary<SpecRelationType, SpecRelationTypeMap> map;

        /// <summary>
        /// Backing field for <see cref="CanOk"/>
        /// </summary>
        private bool canOk;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecRelationTypeMappingDialogViewModel"/> class
        /// Used by MEF.
        /// </summary>
        public SpecRelationTypeMappingDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecRelationTypeMappingDialogViewModel"/> class
        /// </summary>
        /// <param name="specRelationTypes"></param>
        /// <param name="specRelationTypeMap"></param>
        /// <param name="datatypeDefMap"></param>
        /// <param name="iteration"></param>
        /// <param name="session"></param>
        /// <param name="thingDialogNavigationService"></param>
        /// <param name="lang"></param>
        public SpecRelationTypeMappingDialogViewModel(IEnumerable<SpecRelationType> specRelationTypes, IReadOnlyDictionary<SpecRelationType, SpecRelationTypeMap> specRelationTypeMap, IReadOnlyDictionary<DatatypeDefinition, DatatypeDefinitionMap> datatypeDefMap, Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, string lang)
            : base(iteration, session, thingDialogNavigationService, lang)
        {
            this.BackCommand = ReactiveCommand.Create();
            this.BackCommand.Subscribe(_ => this.ExecuteBackCommand());

            var canExecuteOk = this.WhenAnyValue(x => x.CanOk);

            this.OkCommand = ReactiveCommand.Create(canExecuteOk);
            this.OkCommand.Subscribe(_ => this.ExecuteOkCommand());

            this.CreateCategoryCommand = ReactiveCommand.Create();
            this.CreateCategoryCommand.Subscribe(_ => this.ExecuteCreateCategoryCommand());

            this.CreateBinaryRealationshipRuleCommand = ReactiveCommand.Create();
            this.CreateBinaryRealationshipRuleCommand.Subscribe(_ => this.ExecuteCreateBinaryRelationshipRuleCommand()); 

            this.SpecTypes = new ReactiveList<SpecRelationMappingRowViewModel>();

            foreach (var specRelationType in specRelationTypes)
            {
                var row = new SpecRelationMappingRowViewModel(specRelationType, iteration, datatypeDefMap, this.UpdateCanOk);
                this.SpecTypes.Add(row);
            }

            if (specRelationTypeMap != null)
            {
                this.PopulateRelationTypeMapProperties(specRelationTypeMap);
            }

            this.UpdateCanOk();
        }

        /// <summary>
        /// Populates the <see cref="SpecTypes"/> row properties
        /// </summary>
        /// <param name="specRelationTypeMap">The Relationship Type Map collection</param>
        public void PopulateRelationTypeMapProperties(IReadOnlyDictionary<SpecRelationType, SpecRelationTypeMap> specRelationTypeMap)
        {
            foreach (var pair in specRelationTypeMap)
            {
                var row = this.SpecTypes.SingleOrDefault(x => x.Identifiable.Identifier == pair.Key.Identifier);

                if (row is null)
                {
                    continue;
                }

                row.SelectedRules = new ReactiveList<ParameterizedCategoryRule>(row.PossibleRules.Where(x => pair.Value.Rules?.FirstOrDefault(r => r.Iid == x.Iid) != null));
                row.SelectedCategories = new ReactiveList<CategoryComboBoxItemViewModel>(row.PossibleCategories.Where(x => pair.Value.Categories?.FirstOrDefault(r => r.Iid == x.Category.Iid) != null));
                row.SelectedBinaryRelationshipRules = new ReactiveList<BinaryRelationshipRule>(row.PossibleBinaryRelationshipRules.Where(r => pair.Value.BinaryRelationshipRules?.FirstOrDefault(x => x.Iid == r.Iid) != null));
                
                foreach (var attributeDefinitionMap in pair.Value.AttributeDefinitionMap)
                {
                    var attRow = this.SpecTypes.SelectMany(x => x.AttributeDefinitions).Single(x => x.Identifiable.Identifier == attributeDefinitionMap.AttributeDefinition.Identifier);
                    attRow.AttributeDefinitionMapKind = attributeDefinitionMap.MapKind;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="OkCommand"/> is enabled
        /// </summary>
        public bool CanOk
        {
            get { return this.canOk; }
            private set { this.RaiseAndSetIfChanged(ref this.canOk, value); }
        }

        /// <summary>
        /// Gets the back <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<object> BackCommand { get; private set; } 

        /// <summary>
        /// Gets the "next" <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<object> OkCommand { get; private set; }

        /// <summary>
        /// Gets the create <see cref="Category"/> command
        /// </summary>
        public ReactiveCommand<object> CreateCategoryCommand { get; private set; }

        /// <summary>
        /// Gets the create <see cref="BinaryRelationshipRule"/> command
        /// </summary>
        public ReactiveCommand<object> CreateBinaryRealationshipRuleCommand { get; private set; }

        /// <summary>
        /// Executes the <see cref="BackCommand"/>
        /// </summary>
        private void ExecuteBackCommand()
        {
            this.PopulateMap();
            this.DialogResult = new RelationshipMappingDialogResult(this.map, false, true);
        }

        /// <summary>
        /// Update the <see cref="CanOk"/> property
        /// </summary>
        private void UpdateCanOk()
        {
            this.CanOk = !this.SpecTypes.Any() || this.SpecTypes.All(x => x.IsMapped);
        }

        /// <summary>
        /// Executes the <see cref="OkCommand"/>
        /// </summary>
        private void ExecuteOkCommand()
        {
            this.PopulateMap();
            this.DialogResult = new RelationshipMappingDialogResult(this.map, true, true);
        }

        /// <summary>
        /// Gets the mapping rows
        /// </summary>
        public ReactiveList<SpecRelationMappingRowViewModel> SpecTypes { get; private set; }

        /// <summary>
        /// Populate the result of the mapping from the row data
        /// </summary>
        private void PopulateMap()
        {
            this.map = new Dictionary<SpecRelationType, SpecRelationTypeMap>();
            foreach (var row in this.SpecTypes)
            {
                var attributes = new List<AttributeDefinitionMap>();
                foreach (var attDefinition in row.AttributeDefinitions)
                {
                    var attributeMap = new AttributeDefinitionMap(attDefinition.Identifiable, attDefinition.AttributeDefinitionMapKind);
                    attributes.Add(attributeMap);
                }

                var mapping = new SpecRelationTypeMap(row.Identifiable, row.SelectedRules, row.SelectedCategories.Select(x => x.Category), attributes, row.SelectedBinaryRelationshipRules);
                this.map.Add(row.Identifiable, mapping);
            }
        }

        #region Create BinaryRelationshipRules/Categories Region
        
        /// <summary>
        /// Execute the <see cref="ICommand"/> to create a <see cref="Category"/>
        /// </summary>
        private void ExecuteCreateCategoryCommand()
        {
            var category = new Category();

            var siteDirectory = this.Session.RetrieveSiteDirectory();
            var transactionContext = TransactionContextResolver.ResolveContext(siteDirectory);
            var categoryTransaction = new ThingTransaction(transactionContext);

            var res = this.ThingDialogNavigationService.Navigate(category, categoryTransaction, this.Session, true, ThingDialogKind.Create, this.ThingDialogNavigationService);
            if (res != null && res.Value)
            {
                foreach (var binaryRelationshipRuleMappingRowViewModel in this.SpecTypes)
                {
                    binaryRelationshipRuleMappingRowViewModel.PopulatePossibleCategories();
                }
            }
        }

        /// <summary>
        /// Execute the <see cref="ICommand"/> to create a <see cref="Category"/>
        /// </summary>
        private void ExecuteCreateBinaryRelationshipRuleCommand()
        {
            var rule = new BinaryRelationshipRule();

            var siteDirectory = this.Session.RetrieveSiteDirectory();
            var transactionContext = TransactionContextResolver.ResolveContext(siteDirectory);
            var thingTransaction = new ThingTransaction(transactionContext);

            var res = this.ThingDialogNavigationService.Navigate(rule, thingTransaction, this.Session, true, ThingDialogKind.Create, this.ThingDialogNavigationService);

            // refresh parameter type list and set the mapping to the new parameter type
            if (res != null && res.Value)
            {
                foreach (var binaryRelationshipRuleMappingRowViewModel in this.SpecTypes)
                {
                    binaryRelationshipRuleMappingRowViewModel.PopulatePossibleRules();
                }
            }
        }

        #endregion

        /// <summary>
        /// Executes the cancel <see cref="ICommand"/>
        /// </summary>
        protected override void ExecuteCancelCommand()
        {
            this.DialogResult = new RelationshipMappingDialogResult(null, null, false);
        }
    }
}