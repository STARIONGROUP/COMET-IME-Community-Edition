// -------------------------------------------------------------------------------------------------
// <copyright file="ElementUsageRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Events;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Services;
    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// The row class representing an <see cref="ElementUsage"/>
    /// </summary>
    public class ElementUsageRowViewModel : ElementBaseRowViewModel<ElementUsage>, IDropTarget
    {
        /// <summary>
        /// Backing field for the <see cref="AllOptions"/> property.
        /// </summary>
        private ReactiveList<Option> allOptions;

        /// <summary>
        /// Backing field for the <see cref="ExcludedOptions"/> property.
        /// </summary>
        private ReactiveList<Option> excludedOptions;

        /// <summary>
        /// Backing field for the <see cref="SelectedOptions"/> property.
        /// </summary>
        private ReactiveList<Option> selectedOptions;

        /// <summary>
        /// Backing field for the option selection Tooltip.
        /// </summary>
        private string optionToolTip;

        /// <summary>
        /// Backing field for <see cref="HasExcludes"/> 
        /// </summary>
        private bool? hasExcludes;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ElementUsageRowViewModel"/> class
        /// </summary>
        /// <param name="elementUsage">The associated <see cref="ElementUsage"/></param>
        /// <param name="currentExpertise">The active <see cref="DomainOfExpertise"/></param>
        /// <param name="session">The associated <see cref="ISession"/></param>
        /// <param name="containerViewModel">The container view-model</param>
        public ElementUsageRowViewModel(ElementUsage elementUsage, DomainOfExpertise currentExpertise, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(elementUsage, currentExpertise, session, containerViewModel)
        {
            this.AllOptions = new ReactiveList<Option>();
            this.ExcludedOptions = new ReactiveList<Option>();
            this.SelectedOptions = new ReactiveList<Option>();

            this.WhenAnyValue(vm => vm.SelectedOptions).Subscribe(_ => this.ExcludedOptions = new ReactiveList<Option>(this.AllOptions.Except(this.SelectedOptions)));

            this.WhenAnyValue(vm => vm.ExcludedOptions).Subscribe(_ =>
            {
                if (!this.SelectedOptions.Any())
                {
                    this.HasExcludes = null;
                    this.OptionToolTip = "This ElementUsage is not used in any option.";
                }
                else
                {
                    this.HasExcludes = this.ExcludedOptions.Any();

                    if (this.HasExcludes.Value)
                    {
                        var excludedOptionNames = string.Join("\n", this.ExcludedOptions.Select(o => o.Name));

                        this.OptionToolTip = string.Format("This ElementUsage is excluded from options:\n\r{0}", excludedOptionNames);
                    }
                    else if (!this.HasExcludes.Value)
                    {
                        this.OptionToolTip = "This ElementUsage is used in all options.";
                    }
                }
            });

            this.WhenAnyValue(vm => vm.ExcludedOptions).Skip(1).Subscribe(_ => this.SendUpdateExcludedOptionOperation());

            this.UpdateOptionLists();
            this.PopulateParameterGroups();
            this.UpdateProperties();
        }
        
        /// <summary>
        /// Gets or sets the <see cref="ReactiveList{T}"/> of all <see cref="Option"/>s
        /// in this iteration.
        /// </summary>
        public ReactiveList<Option> AllOptions
        {
            get { return this.allOptions; }
            set { this.RaiseAndSetIfChanged(ref this.allOptions, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="ReactiveList{T}"/> of excluded <see cref="Option"/>s of this <see cref="ElementUsage"/>.
        /// </summary>
        public ReactiveList<Option> ExcludedOptions
        {
            get { return this.excludedOptions; }
            set { this.RaiseAndSetIfChanged(ref this.excludedOptions, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="HasExcludes"/>. Null if <see cref="ElementUsage"/> is in no options.
        /// </summary>
        public override bool? HasExcludes
        {
            get { return this.hasExcludes; }
            set { this.RaiseAndSetIfChanged(ref this.hasExcludes, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="OptionToolTip"/>
        /// </summary>
        public string OptionToolTip
        {
            get { return this.optionToolTip; }
            set { this.RaiseAndSetIfChanged(ref this.optionToolTip, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="ReactiveList{T}"/> of selected <see cref="Option"/>s of this <see cref="ElementUsage"/>.
        /// </summary>
        public ReactiveList<Option> SelectedOptions
        {
            get { return this.selectedOptions; }
            set { this.RaiseAndSetIfChanged(ref this.selectedOptions, value); }
        }

        public new IEnumerable<Category> Category => this.Thing.Category.Union(this.Thing.ElementDefinition.Category).Distinct();

        /// <summary>
        /// Update the children rows of the current row
        /// </summary>
        public void UpdateChildren()
        {
            this.UpdateProperties();
        }
        
        /// <summary>
        /// Initializes the subscriptions
        /// </summary>
        protected override void InitializeSubscriptions()
        {
            base.InitializeSubscriptions();
            var elementDefListener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing.ElementDefinition)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => this.ElementDefinitionObjectChangedHandler());

            var highlightSubscription = CDPMessageBus.Current.Listen<ElementUsageHighlightEvent>(this.Thing.ElementDefinition)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.HighlightEventHandler());
            this.Disposables.Add(highlightSubscription);

            var optionAddListener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(Option))
                    .Where(objectChange => objectChange.EventKind == EventKind.Added && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache && objectChange.ChangedThing.Container == this.Thing.Container.Container)
                    .Select(x => x.ChangedThing as Option)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateOptionLists());

            var optionRemoveListener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(Option))
                    .Where(objectChange => objectChange.EventKind == EventKind.Removed && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache && objectChange.ChangedThing.Container == this.Thing.Container.Container)
                    .Select(x => x.ChangedThing as Option)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateOptionLists());

            this.Disposables.Add(optionAddListener);
            this.Disposables.Add(optionRemoveListener);
            this.Disposables.Add(elementDefListener);
        }
        
        /// <summary>
        /// The object changed event handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }
        
        /// <summary>
        /// Update this row upon a <see cref="ObjectChangedEvent"/> on this <see cref="ElementUsage"/>
        /// </summary>
        private void UpdateProperties()
        {
            this.UpdateThingStatus();
            this.UpdateOptionLists();
            this.PopulateParameters();
        }

        /// <summary>
        /// Update this row upon a <see cref="ObjectChangedEvent"/> on this <see cref="ElementUsage.ElementDefinition"/>
        /// </summary>
        private void ElementDefinitionObjectChangedHandler()
        {
            this.PopulateParameterGroups();
            this.PopulateParameters();
        }

        /// <summary>
        /// Populate the <see cref="ParameterGroup"/>s
        /// </summary>
        private void PopulateParameterGroups()
        {
            this.PopulateParameterGroups(this.Thing.ElementDefinition);
        }

        /// <summary>
        /// Populates the <see cref="ParameterBase"/>s
        /// </summary>
        protected override void PopulateParameters()
        {
            var parameterOrOveride = new List<ParameterOrOverrideBase>(this.Thing.ParameterOverride);
            var overridenParameter = this.Thing.ParameterOverride.Select(ov => ov.Parameter).ToList();
            parameterOrOveride.AddRange(this.Thing.ElementDefinition.Parameter.Except(overridenParameter).ToList());

            // Populate Subscription
            var definedParameterOverrideWithSubscription =
                parameterOrOveride.Where(x => x.ParameterSubscription.Any(s => s.Owner == this.currentDomain)).ToList();

            var definedSubscription =
                definedParameterOverrideWithSubscription.Select(
                    x => x.ParameterSubscription.Single(s => s.Owner == this.currentDomain)).ToList();

            var currentSubscription = this.parameterBaseCache.Keys.OfType<ParameterSubscription>().ToList();
            
            // deleted Parameter Subscription
            var deletedSubscription = currentSubscription.Except(definedSubscription).ToList();
            this.RemoveParameterBase(deletedSubscription);

            // added Parameter Subscription
            var addedSubscription = definedSubscription.Except(currentSubscription).ToList();
            this.AddParameterBase(addedSubscription);

            // Populate Parameters Or Overrides
            var definedParameterOrOverrides = parameterOrOveride.Except(definedParameterOverrideWithSubscription).ToList();
            var currentParameterOrOverride = this.parameterBaseCache.Keys.OfType<ParameterOrOverrideBase>().ToList();

            var deletedParameterOrOverride = currentParameterOrOverride.Except(definedParameterOrOverrides).ToList();
            this.RemoveParameterBase(deletedParameterOrOverride);

            var addedParameterOrOverride = definedParameterOrOverrides.Except(currentParameterOrOverride).ToList();
            this.AddParameterBase(addedParameterOrOverride);
        }

        /// <summary>
        /// Update the <see cref="ThingStatus"/> property
        /// </summary>
        protected override void UpdateThingStatus()
        {
            this.ThingStatus = new ThingStatus(this.Thing);
        }

        /// <summary>
        /// Handles the <see cref="ObjectChangedEvent"/> for added and removed <see cref="Option"/>s
        /// </summary>
        private void UpdateOptionLists()
        {
            this.AllOptions = new ReactiveList<Option>(((Iteration)this.Thing.Container.Container).Option);

            this.ExcludedOptions = new ReactiveList<Option>(this.Thing.ExcludeOption);
            this.SelectedOptions = new ReactiveList<Option>(((Iteration)this.Thing.Container.Container).Option.Except(this.Thing.ExcludeOption));
        }

        /// <summary>
        /// Send the update operation for this <see cref="ElementUsage"/> related to its excluded <see cref="Option"/>
        /// </summary>
        private void SendUpdateExcludedOptionOperation()
        {
            if (this.ExcludedOptions.OrderBy(x => x.ShortName).SequenceEqual(this.Thing.ExcludeOption.OrderBy(x => x.ShortName)))
            {
                return;
            }


            var clone = this.Thing.Clone(false);
            clone.ExcludeOption = new List<Option>(this.ExcludedOptions);

            var transactionContext = TransactionContextResolver.ResolveContext(this.Thing);
            var transaction = new ThingTransaction(transactionContext);
            transaction.CreateOrUpdate(clone);

            this.DalWrite(transaction);
        }
        
        /// <summary>
        /// Updates the current drag state.
        /// </summary>
        /// <param name="dropInfo">
        ///  Information about the drag operation.
        /// </param>
        /// <remarks>
        /// To allow a drop at the current drag position, the <see cref="DropInfo.Effects"/> property on 
        /// <paramref name="dropInfo"/> should be set to a value other than <see cref="DragDropEffects.None"/>
        /// and <see cref="DropInfo.Payload"/> should be set to a non-null value.
        /// </remarks>
        public void DragOver(IDropInfo dropInfo)
        {
            var category = dropInfo.Payload as Category;
            if (category != null)
            {
                this.DragOver(dropInfo, category);
                return;
            }

            dropInfo.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// Performs the drop operation
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drop operation.
        /// </param>
        public async Task Drop(IDropInfo dropInfo)
        {
            var category = dropInfo.Payload as Category;
            if (category != null)
            {
                this.Drop(dropInfo, category);
            }
        }
    }
}