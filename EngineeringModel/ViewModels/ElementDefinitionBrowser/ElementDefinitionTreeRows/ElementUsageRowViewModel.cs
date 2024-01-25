// --------------------------------------------------------------------------------------------------------------------

// <copyright file="ElementUsageRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Builders;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Events;
    using CDP4Composition.MessageBus;
    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;

    using CDP4EngineeringModel.Services;

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
        /// Backing field for <see cref="DisplayCategory"/>
        /// </summary>
        private string displayCategory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementUsageRowViewModel"/> class
        /// </summary>
        /// <param name="elementUsage">The associated <see cref="ElementUsage"/></param>
        /// <param name="currentExpertise">The active <see cref="DomainOfExpertise"/></param>
        /// <param name="session">The associated <see cref="ISession"/></param>
        /// <param name="containerViewModel">The container view-model</param>
        /// <param name="obfuscationService">The obfuscation service</param>
        public ElementUsageRowViewModel(
            ElementUsage elementUsage,
            DomainOfExpertise currentExpertise,
            ISession session,
            IViewModelBase<Thing> containerViewModel,
            IObfuscationService obfuscationService)
            : base(elementUsage, currentExpertise, session, containerViewModel, obfuscationService)
        {
            this.AllOptions = new ReactiveList<Option>();
            this.ExcludedOptions = new ReactiveList<Option>();
            this.SelectedOptions = new ReactiveList<Option>();

            this.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(this.SelectedOptions))
                {
                    this.ExcludedOptions = new ReactiveList<Option>(this.AllOptions.Except(this.SelectedOptions));
                }

                if (e.PropertyName == nameof(this.ExcludedOptions))
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

                            this.OptionToolTip = $"This ElementUsage is excluded from options:\n\r{excludedOptionNames}";
                        }
                        else if (!this.HasExcludes.Value)
                        {
                            this.OptionToolTip = "This ElementUsage is used in all options.";
                        }
                    }

                    this.SendUpdateExcludedOptionOperation();
                }
            };

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
            get => this.allOptions;
            set => this.RaiseAndSetIfChanged(ref this.allOptions, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="ReactiveList{T}"/> of excluded <see cref="Option"/>s of this <see cref="ElementUsage"/>.
        /// </summary>
        public ReactiveList<Option> ExcludedOptions
        {
            get => this.excludedOptions;
            set => this.RaiseAndSetIfChanged(ref this.excludedOptions, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="HasExcludes"/>. Null if <see cref="ElementUsage"/> is in no options.
        /// </summary>
        public override bool? HasExcludes
        {
            get => this.hasExcludes;
            set => this.RaiseAndSetIfChanged(ref this.hasExcludes, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="OptionToolTip"/>
        /// </summary>
        public string OptionToolTip
        {
            get => this.optionToolTip;
            set => this.RaiseAndSetIfChanged(ref this.optionToolTip, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="ReactiveList{T}"/> of selected <see cref="Option"/>s of this <see cref="ElementUsage"/>.
        /// </summary>
        public ReactiveList<Option> SelectedOptions
        {
            get => this.selectedOptions;
            set => this.RaiseAndSetIfChanged(ref this.selectedOptions, value);
        }

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> containing all applied categories for this <see cref="ElementUsage"/>
        /// and its according <see cref="ElementDefinition"/>.
        /// </summary>
        public new IEnumerable<Category> Category
        {
            get
            {
                if (this.Thing != null)
                {
                    return this.Thing.Category.Union(this.Thing.ElementDefinition.Category).Distinct();
                }

                return new List<Category>();
            }
        }

        /// <summary>
        /// Gets or sets the Categories in display format
        /// </summary>
        public string DisplayCategory
        {
            get => this.displayCategory;
            set => this.RaiseAndSetIfChanged(ref this.displayCategory, value);
        }

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

            Func<ObjectChangedEvent, bool> optionAddDiscriminator =
                objectChange =>
                    objectChange.EventKind == EventKind.Added
                    && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache
                    && objectChange.ChangedThing.TopContainer == this.Thing.TopContainer;

            Action<ObjectChangedEvent> optionAddAction = x => this.UpdateOptionLists();

            Func<ObjectChangedEvent, bool> optionRemoveDiscriminator =
                objectChange =>
                    objectChange.EventKind == EventKind.Removed
                    && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache
                    && objectChange.ChangedThing.TopContainer == this.Thing.Container;

            Action<ObjectChangedEvent> optionRemoveAction = x => this.UpdateOptionLists();

            Func<ObjectChangedEvent, bool> elementDefinitionDiscriminator = objectChange => objectChange.EventKind == EventKind.Updated;
            Action<ObjectChangedEvent> elementDefinitionAction = x => this.ElementDefinitionObjectChangedHandler();

            if (this.AllowMessageBusSubscriptions)
            {
                var elementDefListener = this.CDPMessageBus.Listen<ObjectChangedEvent>(this.Thing.ElementDefinition)
                    .Where(elementDefinitionDiscriminator)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(elementDefinitionAction);

                var highlightSubscription = this.CDPMessageBus.Listen<ElementUsageHighlightEvent>(this.Thing.ElementDefinition)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.HighlightEventHandler());

                this.Disposables.Add(highlightSubscription);

                var optionAddListener =
                    this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(Option))
                        .Where(optionAddDiscriminator)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(optionAddAction);

                var optionRemoveListener =
                    this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(Option))
                        .Where(optionRemoveDiscriminator)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(optionRemoveAction);

                this.Disposables.Add(optionAddListener);
                this.Disposables.Add(optionRemoveListener);
                this.Disposables.Add(elementDefListener);
            }
            else
            {
                var highlightObserver = this.CDPMessageBus.Listen<ElementUsageHighlightEvent>();

                this.Disposables.Add(
                    this.MessageBusHandler.GetHandler<ElementUsageHighlightEvent>()
                        .RegisterEventHandler(highlightObserver, new MessageBusEventHandlerSubscription<ElementUsageHighlightEvent>(x => x.ElementDefinition.Equals(this.Thing.ElementDefinition), _ => this.HighlightEventHandler())));

                var optionObserver = this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(Option));
                this.Disposables.Add(this.MessageBusHandler.GetHandler<ObjectChangedEvent>().RegisterEventHandler(optionObserver, new ObjectChangedMessageBusEventHandlerSubscription(typeof(Option), optionAddDiscriminator, optionAddAction)));

                this.Disposables.Add(this.MessageBusHandler.GetHandler<ObjectChangedEvent>().RegisterEventHandler(optionObserver, new ObjectChangedMessageBusEventHandlerSubscription(typeof(Option), optionRemoveDiscriminator, optionRemoveAction)));

                var elementDefinitionObserver = this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(ElementDefinition));

                this.Disposables.Add(this.MessageBusHandler.GetHandler<ObjectChangedEvent>().RegisterEventHandler(elementDefinitionObserver, new ObjectChangedMessageBusEventHandlerSubscription(this.Thing.ElementDefinition, elementDefinitionDiscriminator, elementDefinitionAction)));
            }

            this.WhenAnyValue(x => x.ExcludedOptions).Subscribe(_ => this.UpdateDetails());
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
            this.UpdateCategories();
        }

        /// <summary>
        /// Update this row upon a <see cref="ObjectChangedEvent"/> on this <see cref="ElementUsage.ElementDefinition"/>
        /// </summary>
        private void ElementDefinitionObjectChangedHandler()
        {
            this.PopulateParameterGroups();
            this.PopulateParameters();
            this.UpdateTooltip();
            this.UpdateCategories();
            this.UpdateModelCode();
        }

        /// <summary>
        /// Updates the view model categories with 
        /// </summary>
        private void UpdateCategories()
        {
            this.DisplayCategory = new CategoryStringBuilder()
                .AddCategories("EU", this.Thing.Category)
                .AddCategories("ED", this.Thing.ElementDefinition.Category)
                .Build();
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
                definedParameterOverrideWithSubscription.Select(x => x.ParameterSubscription.Single(s => s.Owner == this.currentDomain)).ToList();

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
        /// Update this <see cref="Tooltip"/> with extra information.
        /// </summary>
        protected override void UpdateDetails()
        {
            base.UpdateDetails();

            var sb = new StringBuilder(this.Details);

            if (this.ExcludedOptions != null)
            {
                sb.AppendLine();

                if (this.ExcludedOptions.Count == 0)
                {
                    sb.AppendLine($"Excluded Options: -");
                }
                else
                {
                    sb.AppendLine($"Excluded Options: {string.Join("; ", this.ExcludedOptions.Select(x => x.Name))}");
                }
            }

            if (this.AllOptions != null)
            {
                if (this.AllOptions.Count == 0)
                {
                    sb.AppendLine($"Included Options: -");
                }
                else
                {
                    sb.AppendLine($"Excluded Options: {string.Join("; ", this.AllOptions.Except(this.ExcludedOptions).Select(x => x.Name))}");
                }
            }

            this.Details = sb.ToString();
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
            if (dropInfo.Payload is Category category)
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
            if (dropInfo.Payload is Category category)
            {
                this.Drop(dropInfo, category);
            }
        }
    }
}
