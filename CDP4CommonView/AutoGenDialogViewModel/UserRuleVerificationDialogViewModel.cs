// -------------------------------------------------------------------------------------------------
// <copyright file="UserRuleVerificationDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA System S.A.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Operations;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="UserRuleVerification"/>
    /// </summary>
    public partial class UserRuleVerificationDialogViewModel : RuleVerificationDialogViewModel<UserRuleVerification>
    {
        /// <summary>
        /// Backing field for <see cref="SelectedRule"/>
        /// </summary>
        private Rule selectedRule;


        /// <summary>
        /// Initializes a new instance of the <see cref="UserRuleVerificationDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public UserRuleVerificationDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRuleVerificationDialogViewModel"/> class
        /// </summary>
        /// <param name="userRuleVerification">
        /// The <see cref="UserRuleVerification"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="DialogViewModelBase{T}"/> is the root of all <see cref="DialogViewModelBase{T}"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="DialogViewModelBase{T}"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/> that is used to navigate to a dialog of a specific <see cref="Thing"/>.
        /// </param>
        /// <param name="container">
        /// The <see cref="Thing"/> that contains the created <see cref="Thing"/> in this Dialog
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public UserRuleVerificationDialogViewModel(UserRuleVerification userRuleVerification, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(userRuleVerification, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as RuleVerificationList;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type RuleVerificationList",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the SelectedRule
        /// </summary>
        public virtual Rule SelectedRule
        {
            get { return this.selectedRule; }
            set { this.RaiseAndSetIfChanged(ref this.selectedRule, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="Rule"/>s for <see cref="SelectedRule"/>
        /// </summary>
        public ReactiveList<Rule> PossibleRule { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedRule"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedRuleCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            var canExecuteInspectSelectedRuleCommand = this.WhenAny(vm => vm.SelectedRule, v => v.Value != null);
            this.InspectSelectedRuleCommand = ReactiveCommand.Create(canExecuteInspectSelectedRuleCommand);
            this.InspectSelectedRuleCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedRule));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.Rule = this.SelectedRule;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleRule = new ReactiveList<Rule>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.SelectedRule = this.Thing.Rule;
            this.PopulatePossibleRule();
        }

        /// <summary>
        /// Populates the <see cref="PossibleRule"/> property
        /// </summary>
        protected virtual void PopulatePossibleRule()
        {
            this.PossibleRule.Clear();
        }
    }
}
