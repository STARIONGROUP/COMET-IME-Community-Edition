// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterValueSetBaseDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA S.A.
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
    using CDP4Common.Types;
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;
    using System.Reactive;

    /// <summary>
    /// dialog-view-model class representing a <see cref="ParameterValueSetBase"/>
    /// </summary>
    public abstract partial class ParameterValueSetBaseDialogViewModel<T> : DialogViewModelBase<T> where T : ParameterValueSetBase
    {
        /// <summary>
        /// Backing field for <see cref="ValueSwitch"/>
        /// </summary>
        private ParameterSwitchKind valueSwitch;

        /// <summary>
        /// Backing field for <see cref="SelectedActualState"/>
        /// </summary>
        private ActualFiniteState selectedActualState;

        /// <summary>
        /// Backing field for <see cref="SelectedActualOption"/>
        /// </summary>
        private Option selectedActualOption;


        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterValueSetBaseDialogViewModel{T}"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        protected ParameterValueSetBaseDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterValueSetBaseDialogViewModel{T}"/> class
        /// </summary>
        /// <param name="parameterValueSetBase">
        /// The <see cref="ParameterValueSetBase"/> that is the subject of the current view-model. This is the object
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
        protected ParameterValueSetBaseDialogViewModel(T parameterValueSetBase, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(parameterValueSetBase, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }

        /// <summary>
        /// Gets or sets the ValueSwitch
        /// </summary>
        public virtual ParameterSwitchKind ValueSwitch
        {
            get { return this.valueSwitch; }
            set { this.RaiseAndSetIfChanged(ref this.valueSwitch, value); }
        }

        /// <summary>
        /// Backing field for Published
        /// </summary>
        public ReactiveList<PrimitiveRow<string>> published;

        /// <summary>
        /// Gets or sets the Published
        /// </summary>
        public ReactiveList<PrimitiveRow<string>> Published
        {
            get { return this.published; }
            set { this.RaiseAndSetIfChanged(ref this.published, value); }
        }

        /// <summary>
        /// Backing field for Formula
        /// </summary>
        public ReactiveList<PrimitiveRow<string>> formula;

        /// <summary>
        /// Gets or sets the Formula
        /// </summary>
        public ReactiveList<PrimitiveRow<string>> Formula
        {
            get { return this.formula; }
            set { this.RaiseAndSetIfChanged(ref this.formula, value); }
        }

        /// <summary>
        /// Backing field for Computed
        /// </summary>
        public ReactiveList<PrimitiveRow<string>> computed;

        /// <summary>
        /// Gets or sets the Computed
        /// </summary>
        public ReactiveList<PrimitiveRow<string>> Computed
        {
            get { return this.computed; }
            set { this.RaiseAndSetIfChanged(ref this.computed, value); }
        }

        /// <summary>
        /// Backing field for Manual
        /// </summary>
        public ReactiveList<PrimitiveRow<string>> manual;

        /// <summary>
        /// Gets or sets the Manual
        /// </summary>
        public ReactiveList<PrimitiveRow<string>> Manual
        {
            get { return this.manual; }
            set { this.RaiseAndSetIfChanged(ref this.manual, value); }
        }

        /// <summary>
        /// Backing field for Reference
        /// </summary>
        public ReactiveList<PrimitiveRow<string>> reference;

        /// <summary>
        /// Gets or sets the Reference
        /// </summary>
        public ReactiveList<PrimitiveRow<string>> Reference
        {
            get { return this.reference; }
            set { this.RaiseAndSetIfChanged(ref this.reference, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedActualState
        /// </summary>
        public virtual ActualFiniteState SelectedActualState
        {
            get { return this.selectedActualState; }
            set { this.RaiseAndSetIfChanged(ref this.selectedActualState, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="ActualFiniteState"/>s for <see cref="SelectedActualState"/>
        /// </summary>
        public ReactiveList<ActualFiniteState> PossibleActualState { get; protected set; }

        /// <summary>
        /// Gets or sets the SelectedActualOption
        /// </summary>
        public virtual Option SelectedActualOption
        {
            get { return this.selectedActualOption; }
            set { this.RaiseAndSetIfChanged(ref this.selectedActualOption, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="Option"/>s for <see cref="SelectedActualOption"/>
        /// </summary>
        public ReactiveList<Option> PossibleActualOption { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedActualState"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedActualStateCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedActualOption"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedActualOptionCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            var canExecuteInspectSelectedActualStateCommand = this.WhenAny(vm => vm.SelectedActualState, v => v.Value != null);
            this.InspectSelectedActualStateCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedActualStateCommand);
            this.InspectSelectedActualStateCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedActualState));
            var canExecuteInspectSelectedActualOptionCommand = this.WhenAny(vm => vm.SelectedActualOption, v => v.Value != null);
            this.InspectSelectedActualOptionCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedActualOptionCommand);
            this.InspectSelectedActualOptionCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedActualOption));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.ValueSwitch = this.ValueSwitch;
            clone.Published = new ValueArray<string>(this.Published.OrderBy(x => x.Index).Select(x => x.Value), this.Thing);
 
            clone.Formula = new ValueArray<string>(this.Formula.OrderBy(x => x.Index).Select(x => x.Value), this.Thing);
 
            clone.Computed = new ValueArray<string>(this.Computed.OrderBy(x => x.Index).Select(x => x.Value), this.Thing);
 
            clone.Manual = new ValueArray<string>(this.Manual.OrderBy(x => x.Index).Select(x => x.Value), this.Thing);
 
            clone.Reference = new ValueArray<string>(this.Reference.OrderBy(x => x.Index).Select(x => x.Value), this.Thing);
 
            clone.ActualState = this.SelectedActualState;
            clone.ActualOption = this.SelectedActualOption;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.Published = new ReactiveList<PrimitiveRow<string>>();
            this.Formula = new ReactiveList<PrimitiveRow<string>>();
            this.Computed = new ReactiveList<PrimitiveRow<string>>();
            this.Manual = new ReactiveList<PrimitiveRow<string>>();
            this.Reference = new ReactiveList<PrimitiveRow<string>>();
            this.PossibleActualState = new ReactiveList<ActualFiniteState>();
            this.PossibleActualOption = new ReactiveList<Option>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.ValueSwitch = this.Thing.ValueSwitch;
            this.PopulatePublished();
            this.PopulateFormula();
            this.PopulateComputed();
            this.PopulateManual();
            this.PopulateReference();
            this.SelectedActualState = this.Thing.ActualState;
            this.PopulatePossibleActualState();
            this.SelectedActualOption = this.Thing.ActualOption;
            this.PopulatePossibleActualOption();
        }

        /// <summary>
        /// Populates the <see cref="Published"/> property
        /// </summary>
        protected virtual void PopulatePublished()
        {
            this.Published.Clear();
            foreach(var value in this.Thing.Published)
            {
                this.Published.Add(new PrimitiveRow<string> { Index = this.Published.Count, Value = value });
            }
        }

        /// <summary>
        /// Populates the <see cref="Formula"/> property
        /// </summary>
        protected virtual void PopulateFormula()
        {
            this.Formula.Clear();
            foreach(var value in this.Thing.Formula)
            {
                this.Formula.Add(new PrimitiveRow<string> { Index = this.Formula.Count, Value = value });
            }
        }

        /// <summary>
        /// Populates the <see cref="Computed"/> property
        /// </summary>
        protected virtual void PopulateComputed()
        {
            this.Computed.Clear();
            foreach(var value in this.Thing.Computed)
            {
                this.Computed.Add(new PrimitiveRow<string> { Index = this.Computed.Count, Value = value });
            }
        }

        /// <summary>
        /// Populates the <see cref="Manual"/> property
        /// </summary>
        protected virtual void PopulateManual()
        {
            this.Manual.Clear();
            foreach(var value in this.Thing.Manual)
            {
                this.Manual.Add(new PrimitiveRow<string> { Index = this.Manual.Count, Value = value });
            }
        }

        /// <summary>
        /// Populates the <see cref="Reference"/> property
        /// </summary>
        protected virtual void PopulateReference()
        {
            this.Reference.Clear();
            foreach(var value in this.Thing.Reference)
            {
                this.Reference.Add(new PrimitiveRow<string> { Index = this.Reference.Count, Value = value });
            }
        }

        /// <summary>
        /// Populates the <see cref="PossibleActualState"/> property
        /// </summary>
        protected virtual void PopulatePossibleActualState()
        {
            this.PossibleActualState.Clear();
        }

        /// <summary>
        /// Populates the <see cref="PossibleActualOption"/> property
        /// </summary>
        protected virtual void PopulatePossibleActualOption()
        {
            this.PossibleActualOption.Clear();
        }
    }
}
