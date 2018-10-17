// -------------------------------------------------------------------------------------------------
// <copyright file="BudgetConfigViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Budget.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using Config;
    using ConfigFile;
    using ReactiveUI;
    using Services;

    /// <summary>
    /// The view-model used to setup configuration for the budget view
    /// </summary>
    public class BudgetConfigViewModel : DialogViewModelBase
    {
        #region backing fields
        /// <summary>
        /// Backing field for <see cref="SelectedElements"/>
        /// </summary>
        private ReactiveList<ElementDefinition> selectedElements;

        /// <summary>
        /// The used <see cref="Category"/>
        /// </summary>
        private List<Category> usedCategories;

        /// <summary>
        /// Backing field for <see cref="NumberOfElement"/>
        /// </summary>
        private QuantityKind numberOfElement;

        /// <summary>
        /// Backing field for <see cref="SystemLevel"/>
        /// </summary>
        private EnumerationParameterType systemLevel;

        /// <summary>
        /// Backing field for <see cref="OverAllMargin"/>
        /// </summary>
        private float overAllMargin;

        /// <summary>
        /// Backing field for <see cref="PossibleSystemLevelEnum"/>
        /// </summary>
        private List<EnumerationValueDefinition> possibleSystemLevelEnum;

        /// <summary>
        /// Backing field for <see cref="SelectedSubSystemEnum"/>
        /// </summary>
        private EnumerationValueDefinition selectedSubSystemEnum;

        /// <summary>
        /// Backing field for <see cref="SelectedEquipmentEnum"/>
        /// </summary>
        private EnumerationValueDefinition selectedEquipmentEnum;

        /// <summary>
        /// Backing field for <see cref="SelectedBudgetKind"/>
        /// </summary>
        private BudgetKind selectedBudgetKind;

        /// <summary>
        /// Backing field for <see cref="BudgetParameterConfig"/>
        /// </summary>
        private BudgetParameterConfigViewModelBase budgetParameterConfig;

        /// <summary>
        /// Backing field for <see cref="CanOk"/>
        /// </summary>
        private bool canOk;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="BudgetConfigViewModel"/> class
        /// </summary>
        /// <param name="iteration">The current iteration</param>
        /// <param name="currentConfig">An existing <see cref="BudgetConfig"/></param>
        public BudgetConfigViewModel(Iteration iteration, BudgetConfig currentConfig)
        {
            this.PopulateRdl(iteration);
            this.InitializeCommand();

            if (currentConfig != null)
            {
                this.LoadExistingConfiguration(currentConfig);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iteration"></param>
        /// <param name="dto"></param>
        public BudgetConfigViewModel(Iteration iteration, BudgetConfigDto dto)
        {
            this.PopulateRdl(iteration);
            this.InitializeCommand();

            var budgetconfig = dto.ToBudgetConfig(this.PossibleParameterTypes, this.PossibleSystemLevelParameterTypes, this.usedCategories);
            this.LoadExistingConfiguration(budgetconfig);
        }

        #region view fields
        /// <summary>
        /// Gets or sets the root <see cref="ElementDefinition"/>s to generate the budget for
        /// </summary>
        public ReactiveList<ElementDefinition> SelectedElements
        {
            get { return this.selectedElements; }
            set { this.RaiseAndSetIfChanged(ref this.selectedElements, value); }
        }

        /// <summary>
        /// Gets or sets the root <see cref="ElementDefinition"/> to generate the budget for
        /// </summary>
        public QuantityKind NumberOfElement
        {
            get { return this.numberOfElement; }
            set { this.RaiseAndSetIfChanged(ref this.numberOfElement, value); }
        }

        /// <summary>
        /// Gets or sets the root <see cref="ElementDefinition"/> to generate the budget for
        /// </summary>
        public BudgetKind SelectedBudgetKind
        {
            get { return this.selectedBudgetKind; }
            set { this.RaiseAndSetIfChanged(ref this.selectedBudgetKind, value); }
        }

        /// <summary>
        /// Gets or sets the root <see cref="ElementDefinition"/> to generate the budget for
        /// </summary>
        public EnumerationParameterType SystemLevel
        {
            get { return this.systemLevel; }
            set { this.RaiseAndSetIfChanged(ref this.systemLevel, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="EnumerationValueDefinition"/> that matches the sub-system level
        /// </summary>
        public EnumerationValueDefinition SelectedSubSystemEnum
        {
            get { return this.selectedSubSystemEnum; }
            set { this.RaiseAndSetIfChanged(ref this.selectedSubSystemEnum, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="EnumerationValueDefinition"/> that matches the equipment level
        /// </summary>
        public EnumerationValueDefinition SelectedEquipmentEnum
        {
            get { return this.selectedEquipmentEnum; }
            set { this.RaiseAndSetIfChanged(ref this.selectedEquipmentEnum, value); }
        }

        /// <summary>
        /// Get or sets the overall margin
        /// </summary>
        public float OverAllMargin
        {
            get { return this.overAllMargin; }
            set { this.RaiseAndSetIfChanged(ref this.overAllMargin, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="OkCommand"/> can be executed
        /// </summary>
        public bool CanOk
        {
            get { return this.canOk; }
            private set { this.RaiseAndSetIfChanged(ref this.canOk, value); }
        }

        /// <summary>
        /// Gets all available <see cref="ElementDefinition"/>
        /// </summary>
        public ReactiveList<ElementDefinition> PossibleElementDefinitions { get; private set; }

        /// <summary>
        /// Gets the view-model used to define the sub-systems
        /// </summary>
        public ReactiveList<SubSystemConfigViewModel> SubSystemDefinitions { get; private set; }

        /// <summary>
        /// Gets the possible <see cref="EnumerationValueDefinition"/> to use to define the system-levels
        /// </summary>
        public List<EnumerationValueDefinition> PossibleSystemLevelEnum
        {
            get { return this.possibleSystemLevelEnum; }
            private set { this.RaiseAndSetIfChanged(ref this.possibleSystemLevelEnum, value); }
        }

        /// <summary>
        /// Gets the view-model to set the mass budget parameters
        /// </summary>
        public BudgetParameterConfigViewModelBase BudgetParameterConfig
        {
            get { return this.budgetParameterConfig; }
            private set { this.RaiseAndSetIfChanged(ref this.budgetParameterConfig, value); }
        }

        /// <summary>
        /// Gets the possible list of <see cref="ParameterType"/> to use
        /// </summary>
        public ReactiveList<QuantityKind> PossibleParameterTypes { get; private set; }

        /// <summary>
        /// Gets the possible <see cref="EnumerationParameterType"/> to use
        /// </summary>
        public ReactiveList<EnumerationParameterType> PossibleSystemLevelParameterTypes { get; private set; }

        /// <summary>
        /// Gets the command that add a new sub-system definition
        /// </summary>
        public ReactiveCommand<object> AddSubSystemDefinitionCommand { get; private set; }

        /// <summary>
        /// Gets the OK command
        /// </summary>
        public ReactiveCommand<object> OkCommand { get; private set; }

        /// <summary>
        /// Gets the Cancel command
        /// </summary>
        public ReactiveCommand<object> CancelCommand { get; private set; }
        #endregion

        /// <summary>
        /// Execute the <see cref="AddSubSystemDefinitionCommand"/>
        /// </summary>
        private void ExecuteAddSubSystemDefinition()
        {
            this.SubSystemDefinitions.Add(new SubSystemConfigViewModel(this.usedCategories, this.UpdateCanOk, vm => this.SubSystemDefinitions.Remove(vm)));
        }

        /// <summary>
        /// Executes the <see cref="OkCommand"/>
        /// </summary>
        private void ExecuteOkCommand()
        {
            var subSysDefinitions = new List<SubSystemDefinition>();
            foreach (var ssDef in this.SubSystemDefinitions)
            {
                var subSysCat = new SubSystemDefinition(ssDef.SubSystemDefinitions.SelectedCategories, ssDef.SubSystemElementDefinition.SelectedCategories);
                if (subSysDefinitions.Contains(subSysCat))
                {
                    continue;
                }

                subSysDefinitions.Add(subSysCat);
            }

            BudgetParameterConfigBase parameterConfig;
            switch (this.SelectedBudgetKind)
            {
                case BudgetKind.Mass:
                    var massBudgetVm = (MassBudgetParameterConfigViewModel)this.BudgetParameterConfig;
                    var drymassConfig = new BudgetParameterMarginPair(massBudgetVm.DryMassConfig.SelectedParameterType, massBudgetVm.DryMassConfig.SelectedMarginParameterType);
                    var extraConf = new List<ExtraMassContributionConfiguration>();
                    foreach (var extraMassContributionConfigurationViewModel in massBudgetVm.ExtraMassContributions)
                    {
                        extraConf.Add(new ExtraMassContributionConfiguration(extraMassContributionConfigurationViewModel.SelectedCategories, extraMassContributionConfigurationViewModel.SelectedParameter, extraMassContributionConfigurationViewModel.SelectedMarginParameter));
                    }

                    parameterConfig = new MassBudgetParameterConfig(drymassConfig, extraConf);
                    break;
                case BudgetKind.Cost:
                    var costBudgetVm = (CostBudgetParameterConfigViewModel)this.BudgetParameterConfig;
                    parameterConfig = new CostBudgetParameterConfig(new BudgetParameterMarginPair(costBudgetVm.GenericConfig.SelectedParameterType, costBudgetVm.GenericConfig.SelectedMarginParameterType));
                    break;
                //case BudgetKind.Power:
                //    var powerBudgetVm = (PowerBudgetParameterConfigViewModel)this.BudgetParameterConfig;
                //    parameterConfig = new PowerBudgetParameterConfig(new BudgetParameterMarginPair(powerBudgetVm.GenericConfig.SelectedParameterType, powerBudgetVm.GenericConfig.SelectedMarginParameterType));
                //    break;
                default:
                    throw new NotImplementedException($"Case {this.SelectedBudgetKind} not implemented");
            }

            var config = new BudgetConfig(this.SelectedElements, subSysDefinitions, parameterConfig, this.NumberOfElement, this.SystemLevel, this.SelectedSubSystemEnum, this.SelectedEquipmentEnum);
            this.DialogResult = new BudgetConfigDialogResult(true, config);
        }

        /// <summary>
        /// Add validation constraint trigger
        /// </summary>
        private void AddValidationConstraintTrigger()
        {
            this.SelectedElements = new ReactiveList<ElementDefinition>();
            this.WhenAnyValue(x => x.SelectedElements, x => x.SelectedBudgetKind, x => x.SystemLevel, x => x.SelectedSubSystemEnum, x => x.SelectedEquipmentEnum).Subscribe(_ => this.UpdateCanOk());

            this.SubSystemDefinitions.CountChanged.Subscribe(_ => this.UpdateCanOk());
        }

        /// <summary>
        /// Update <see cref="CanOk"/>
        /// </summary>
        private void UpdateCanOk()
        {
            this.CanOk = this.BudgetParameterConfig != null
                         && this.BudgetParameterConfig.IsFormValid() 
                         && this.SubSystemDefinitions.All(x => x.IsFormValid())
                         && this.SelectedElements != null && this.SelectedElements.Count > 0
                         && (this.SystemLevel == null || this.SelectedEquipmentEnum != null && this.SelectedSubSystemEnum != null);
        }

        /// <summary>
        /// Executes the <see cref="CancelCommand"/>
        /// </summary>
        private void ExecuteCancelCommand()
        {
            this.DialogResult = new BudgetConfigDialogResult(false, null);
        }

        /// <summary>
        /// Populates the reference-data
        /// </summary>
        /// <param name="iteration">The current <see cref="Iteration"/></param>
        private void PopulateRdl(Iteration iteration)
        {
            var elementBases = new List<ElementBase>(iteration.Element);
            elementBases.AddRange(iteration.Element.SelectMany(x => x.ContainedElement));

            this.usedCategories = new List<Category>(elementBases.SelectMany(x => x.Category).Distinct().OrderBy(x => x.Name));

            var usedParameterTypes = iteration.Element.SelectMany(x => x.Parameter).Select(x => x.ParameterType).Distinct().OrderBy(x => x.Name);
            this.PossibleParameterTypes = new ReactiveList<QuantityKind>(usedParameterTypes.OfType<QuantityKind>());
            this.PossibleSystemLevelParameterTypes = new ReactiveList<EnumerationParameterType>(this.PossibleParameterTypes.OfType<EnumerationParameterType>());
            this.SubSystemDefinitions = new ReactiveList<SubSystemConfigViewModel>();
            this.PossibleElementDefinitions = new ReactiveList<ElementDefinition>(iteration.Element.OrderBy(x => x.Name));
        }

        /// <summary>
        /// Load the current <see cref="BudgetConfig"/> into the current view
        /// </summary>
        /// <param name="configuration">The existing configuration</param>
        private void LoadExistingConfiguration(BudgetConfig configuration)
        {
            this.SelectedElements = new ReactiveList<ElementDefinition>(this.PossibleElementDefinitions.Where(x => configuration.Elements != null && configuration.Elements.Any(y => y.Iid == x.Iid)));
            this.NumberOfElement = this.PossibleParameterTypes.SingleOrDefault(x => configuration.NumberOfElementParameterType != null && x.Iid == configuration.NumberOfElementParameterType.Iid);
            this.SystemLevel = this.PossibleSystemLevelParameterTypes.SingleOrDefault(x => configuration.SystemLevelToUse != null && x.Iid == configuration.SystemLevelToUse.Iid);
            this.SelectedSubSystemEnum = this.PossibleSystemLevelEnum != null && this.PossibleSystemLevelEnum.Count > 0 && configuration.SubSystemLevelEnum != null
                ? this.PossibleSystemLevelEnum.SingleOrDefault(x => x.Iid == configuration.SubSystemLevelEnum.Iid)
                : null;

            this.SelectedEquipmentEnum = this.PossibleSystemLevelEnum != null && this.PossibleSystemLevelEnum.Count > 0 && configuration.EquipmentLevelEnum != null
                ? this.PossibleSystemLevelEnum.SingleOrDefault(x => x.Iid == configuration.EquipmentLevelEnum.Iid)
                : null;

            var massBudgetConfig = configuration.BudgetParameterConfig as MassBudgetParameterConfig;
            if (massBudgetConfig != null)
            {
                this.SelectedBudgetKind = BudgetKind.Mass;
                var vm = new MassBudgetParameterConfigViewModel(this.PossibleParameterTypes, this.UpdateCanOk, this.usedCategories);
                vm.DryMassConfig.SelectedParameterType = this.PossibleParameterTypes.FirstOrDefault(x => x.Iid == massBudgetConfig.DryMassTuple.MainParameterType.Iid);
                vm.DryMassConfig.SelectedMarginParameterType = massBudgetConfig.DryMassTuple.MarginParameterType != null
                    ? this.PossibleParameterTypes.FirstOrDefault(x => x.Iid == massBudgetConfig.DryMassTuple.MarginParameterType.Iid)
                    : null;

                vm.AddExtraContributionFromExistingConf(massBudgetConfig.ExtraMassContributionConfigurations);
                this.BudgetParameterConfig = vm;
            }

            //var powerBudgetConfig = configuration.BudgetParameterConfig as PowerBudgetParameterConfig;
            //if (powerBudgetConfig != null)
            //{
            //    this.SelectedBudgetKind = BudgetKind.Power;
            //}

            var costBudgetConfig = configuration.BudgetParameterConfig as CostBudgetParameterConfig;
            if (costBudgetConfig != null)
            {
                this.SelectedBudgetKind = BudgetKind.Cost;
            }

            foreach (var subSystemDefinition in configuration.SubSystemDefinition)
            {
                var subsysrow = new SubSystemConfigViewModel(this.usedCategories, this.UpdateCanOk, vm => this.SubSystemDefinitions.Remove(vm));
                subsysrow.SubSystemDefinitions.SelectedCategories = new ReactiveList<Category>(subsysrow.SubSystemDefinitions.PossibleCategories.Where(x => subSystemDefinition.Categories.Select(y => y.Iid).Contains(x.Iid)));
                subsysrow.SubSystemElementDefinition.SelectedCategories = new ReactiveList<Category>(subsysrow.SubSystemElementDefinition.PossibleCategories.Where(x => subSystemDefinition.ElementCategories.Select(y => y.Iid).Contains(x.Iid)));
                this.SubSystemDefinitions.Add(subsysrow);
            }
        }

        /// <summary>
        /// Initializes the view-model
        /// </summary>
        private void InitializeCommand()
        {
            this.AddSubSystemDefinitionCommand = ReactiveCommand.Create();
            this.AddSubSystemDefinitionCommand.Subscribe(_ => this.ExecuteAddSubSystemDefinition());

            this.OkCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanOk));
            this.OkCommand.Subscribe(_ => this.ExecuteOkCommand());

            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(_ => this.ExecuteCancelCommand());

            this.WhenAnyValue(vm => vm.SelectedBudgetKind).Subscribe(
                x =>
                {
                    switch (x)
                    {
                        case BudgetKind.Mass:
                            this.BudgetParameterConfig = new MassBudgetParameterConfigViewModel(this.PossibleParameterTypes, this.UpdateCanOk, this.usedCategories);
                            break;
                        case BudgetKind.Cost:
                            this.BudgetParameterConfig = new CostBudgetParameterConfigViewModel(this.PossibleParameterTypes, this.UpdateCanOk);
                            break;
                        //case BudgetKind.Power:
                        //    this.BudgetParameterConfig = new PowerBudgetParameterConfigViewModel(this.PossibleParameterTypes, this.UpdateCanOk);
                        //    break;
                        default:
                            throw new NotImplementedException($"{x} is not implemented");
                    }
                });

            this.WhenAnyValue(vm => vm.SystemLevel).Subscribe(
                x =>
                {
                    this.PossibleSystemLevelEnum = x == null ? new List<EnumerationValueDefinition>() : new List<EnumerationValueDefinition>(x.ValueDefinition);
                });

            this.Subscriptions.Add(this.AddSubSystemDefinitionCommand);
            this.AddValidationConstraintTrigger();
        }
    }
}
