// -------------------------------------------------------------------------------------------------
// <copyright file="MassBudgetParameterConfigViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Budget.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;

    using Config;
    using ReactiveUI;
    using Services;

    /// <summary>
    /// The view-model used to setup configuration for the budget view
    /// </summary>
    public class MassBudgetParameterConfigViewModel : BudgetParameterConfigViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MassBudgetParameterConfigViewModel"/> class
        /// </summary>
        /// <param name="possibleParameterTypes">The possible <see cref="QuantityKind"/></param>
        /// <param name="validateMainForm">The action to validate the main form</param>
        /// <param name="possibleCategories">The possible <see cref="Category"/></param>
        public MassBudgetParameterConfigViewModel(IReadOnlyList<QuantityKind> possibleParameterTypes, Action validateMainForm, IReadOnlyList<Category> possibleCategories) : base(validateMainForm)
        {
            this.PossibleCategories = possibleCategories;
            this.PossibleParameterTypes = possibleParameterTypes;
            this.DryMassConfig = new ParameterConfigViewModel(possibleParameterTypes, validateMainForm);
            this.ExtraMassContributions = new ReactiveList<ExtraMassContributionConfigurationViewModel>();

            object NoOp(object param) => param;

            this.AddExtraMassContributionCommand = ReactiveCommand.Create<object, object>(NoOp);
            this.AddExtraMassContributionCommand.Subscribe(_ => this.ExtraMassContributions.Add(new ExtraMassContributionConfigurationViewModel(possibleCategories, validateMainForm, this.RemoveExtraMassConfigViewModel)));
        }

        /// <summary>
        /// Gets the possible categories
        /// </summary>
        public IReadOnlyList<Category> PossibleCategories { get; }

        /// <summary>
        /// Gets the possible parameter-types
        /// </summary>
        public IReadOnlyList<QuantityKind> PossibleParameterTypes { get; }

        /// <summary>
        /// Gets the view-moedl to setup the dry mass
        /// </summary>
        public ParameterConfigViewModel DryMassConfig { get; }

        /// <summary>
        /// Gets the <see cref="BudgetKind"/> associated with this view-model
        /// </summary>
        public override BudgetKind BudgetKind => BudgetKind.Mass;

        /// <summary>
        /// Gets the command to add an extra mass contribution
        /// </summary>
        public ReactiveCommand<object, object> AddExtraMassContributionCommand { get; private set; }

        /// <summary>
        /// Gets the list of <see cref="ExtraMassContributionConfigurationViewModel"/>
        /// </summary>
        public ReactiveList<ExtraMassContributionConfigurationViewModel> ExtraMassContributions { get; private set; }

        /// <summary>
        /// Asserts whether the view-model is valid
        /// </summary>
        /// <returns>True if it is</returns>
        public override bool IsFormValid()
        {
            return this.DryMassConfig.SelectedParameterType != null && this.ExtraMassContributions.All(x => x.SelectedCategories != null && x.SelectedCategories.Count > 0 && x.SelectedParameter != null);
        }

        /// <summary>
        /// Removes the <see cref="ExtraMassContributionConfigurationViewModel"/>
        /// </summary>
        /// <param name="vm">The <see cref="ExtraMassContributionConfigurationViewModel"/> to remove</param>
        private void RemoveExtraMassConfigViewModel(ExtraMassContributionConfigurationViewModel vm)
        {
            this.ExtraMassContributions.Remove(vm);
        }

        /// <summary>
        /// Add an extra contribution from an existing configuration file
        /// </summary>
        /// <param name="extraMassConf">The existing confiuration</param>
        public void AddExtraContributionFromExistingConf(IReadOnlyList<ExtraMassContributionConfiguration> extraMassConf)
        {
            if (extraMassConf == null)
            {
                return;
            }

            foreach (var extraMassContributionConfiguration in extraMassConf)
            {
                var vm = new ExtraMassContributionConfigurationViewModel(this.PossibleCategories, this.ValidateMainForm, this.RemoveExtraMassConfigViewModel);
                vm.SelectedCategories = new ReactiveList<Category>(this.PossibleCategories.Where(x => extraMassContributionConfiguration.ContributionCategories.Select(y => y.Iid).Contains(x.Iid)));
                vm.SelectedParameter = this.PossibleParameterTypes.FirstOrDefault(x => x.Iid == extraMassContributionConfiguration.MassParameterType.Iid);
                vm.SelectedMarginParameter = extraMassContributionConfiguration.MarginParameterType != null
                    ? this.PossibleParameterTypes.FirstOrDefault(x => x.Iid == extraMassContributionConfiguration.MarginParameterType.Iid)
                    : null;
                this.ExtraMassContributions.Add(vm);
            }
        }
    }
}
