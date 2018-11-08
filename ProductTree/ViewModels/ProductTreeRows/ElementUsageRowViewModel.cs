// ------------------------------------------------------------------------------------------------
// <copyright file="ElementUsageRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4ProductTree.ViewModels
{
    using CDP4Composition.Events;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4ProductTree.Comparers;
    using ReactiveUI;

    /// <summary>
    /// A row-view-model class representing a <see cref="ElementUsage"/>
    /// </summary>
    public class ElementUsageRowViewModel : CDP4CommonView.ElementUsageRowViewModel, IParameterRowContainer
    {
        #region Fields
        /// <summary>
        /// The <see cref="IComparer{T}"/>
        /// </summary>
        public static readonly IComparer<IRowViewModelBase<Thing>> ChildRowComparer = new ElementUsageChildRowComparer();

        /// <summary>
        /// The listener cache associated with a <see cref="ElementUsage"/>
        /// </summary>
        private readonly Dictionary<ElementUsage, IDisposable> elementUsageListenerCache = new Dictionary<ElementUsage, IDisposable>(); 

        /// <summary>
        /// The selected <see cref="Option"/> for the browser this row is contained in
        /// </summary>
        public readonly Option Option;

        /// <summary>
        /// A cache for all <see cref="ParameterBase"/>
        /// </summary>
        private Dictionary<ParameterBase, IRowViewModelBase<ParameterOrOverrideBase>> parameterOrOverrideBaseCache;

        /// <summary>
        /// A cache that associates a <see cref="ParameterBase"/> with its parent row in the tree-view
        /// </summary>
        private Dictionary<ParameterBase, ParameterGroup> parameterOrOverrideContainerMap;

        /// <summary>
        /// A list of all rows representing all <see cref="ParameterGroup"/> contained by this <see cref="CDP4Common.EngineeringModelData.ElementDefinition"/>
        /// </summary>
        private Dictionary<ParameterGroup, ParameterGroupRowViewModel> parameterGroupCache;

        /// <summary>
        /// A parameter group - parameter group container mapping
        /// </summary>
        private Dictionary<ParameterGroup, ParameterGroup> parameterGroupContainment;

        /// <summary>
        /// Backing field for <see cref="ModelCode"/>
        /// </summary>
        private string modelCode;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ElementUsageRowViewModel"/> class
        /// </summary>
        /// <param name="elementUsage">
        /// The <see cref="ElementUsage"/> associated with this row
        /// </param>
        /// <param name="option">
        /// The selected <see cref="Option"/> for the browser this row is contained in
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="containerRow">
        /// The <see cref="ElementDefinitionRowViewModel"/> parent row.
        /// </param>
        public ElementUsageRowViewModel(ElementUsage elementUsage, Option option, ISession session, IRowViewModelBase<Thing> containerRow)
            : base(elementUsage, session, containerRow)
        {
            this.Option = option;
            this.parameterOrOverrideBaseCache = new Dictionary<ParameterBase, IRowViewModelBase<ParameterOrOverrideBase>>();
            this.parameterOrOverrideContainerMap = new Dictionary<ParameterBase, ParameterGroup>();
            this.parameterGroupCache = new Dictionary<ParameterGroup, ParameterGroupRowViewModel>();
            this.parameterGroupContainment = new Dictionary<ParameterGroup, ParameterGroup>();

            this.UpdateElementDefinitionProperties();
            this.UpdateProperties();
            this.UpdateTooltip();
        }
        #endregion

        /// <summary>
        /// Gets the model-code
        /// </summary>
        public string ModelCode
        {
            get { return this.modelCode; }
            private set { this.RaiseAndSetIfChanged(ref this.modelCode, value); }
        }

        #region IParameterRowContainer public methods

        /// <summary>
        /// Update the row containment associated to a <see cref="ParameterBase"/>
        /// </summary>
        /// <param name="parameterBase">The <see cref="ParameterBase"/></param>
        public void UpdateParameterBasePosition(ParameterBase parameterBase)
        {
            var oldContainer = this.parameterOrOverrideContainerMap[parameterBase];
            var newContainer = parameterBase.Group;
            var associatedRow = this.parameterOrOverrideBaseCache[parameterBase];

            if (newContainer != null && oldContainer == null)
            {
                this.ContainedRows.Remove(associatedRow);
                this.parameterGroupCache[newContainer].ContainedRows.SortedInsert(associatedRow, ParameterGroupRowViewModel.ChildRowComparer);
                this.parameterOrOverrideContainerMap[parameterBase] = newContainer;
            }
            else if (newContainer == null && oldContainer != null)
            {
                this.parameterGroupCache[oldContainer].ContainedRows.Remove(associatedRow);
                this.ContainedRows.SortedInsert(associatedRow, ChildRowComparer);
                this.parameterOrOverrideContainerMap[parameterBase] = null;
            }
            else if (newContainer != null && oldContainer != null && newContainer != oldContainer)
            {
                this.parameterGroupCache[oldContainer].ContainedRows.Remove(associatedRow);
                this.parameterGroupCache[newContainer].ContainedRows.SortedInsert(associatedRow, ParameterGroupRowViewModel.ChildRowComparer);
                this.parameterOrOverrideContainerMap[parameterBase] = newContainer;
            }
        }

        /// <summary>
        /// Update the row containment associated to a <see cref="ParameterGroup"/>
        /// </summary>
        /// <param name="parameterGroup">The <see cref="ParameterGroup"/></param>
        public void UpdateParameterGroupPosition(ParameterGroup parameterGroup)
        {
            var oldContainer = this.parameterGroupContainment[parameterGroup];
            var newContainer = parameterGroup.ContainingGroup;
            var associatedRow = this.parameterGroupCache[parameterGroup];

            if (newContainer != null && oldContainer == null)
            {
                this.ContainedRows.Remove(associatedRow);
                this.parameterGroupCache[newContainer].ContainedRows.SortedInsert(associatedRow, ParameterGroupRowViewModel.ChildRowComparer);
                this.parameterGroupContainment[parameterGroup] = newContainer;
            }
            else if (newContainer == null && oldContainer != null)
            {
                this.parameterGroupCache[oldContainer].ContainedRows.Remove(associatedRow);
                this.ContainedRows.SortedInsert(associatedRow, ChildRowComparer);
                this.parameterGroupContainment[parameterGroup] = null;
            }
            else if (newContainer != null && oldContainer != null && newContainer != oldContainer)
            {
                this.parameterGroupCache[oldContainer].ContainedRows.Remove(associatedRow);
                this.parameterGroupCache[newContainer].ContainedRows.SortedInsert(associatedRow, ParameterGroupRowViewModel.ChildRowComparer);
                this.parameterGroupContainment[parameterGroup] = newContainer;
            }
        }
        #endregion

        #region Row Base
        /// <summary>
        /// Initializes the subscriptions
        /// </summary>
        protected override void InitializeSubscriptions()
        {
            base.InitializeSubscriptions();
            var elementDefListener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing.ElementDefinition)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => this.UpdateElementDefinitionProperties());
            this.Disposables.Add(elementDefListener);

            var highlightSubscription = CDPMessageBus.Current.Listen<HighlightByCategoryEvent>(this.Thing.ElementDefinition)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.HighlightEventHandler());
            this.Disposables.Add(highlightSubscription);

            var highlightUsageSubscription = CDPMessageBus.Current.Listen<ElementUsageHighlightEvent>(this.Thing.ElementDefinition)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.HighlightEventHandler());
            this.Disposables.Add(highlightSubscription);
        }

        /// <summary>
        /// Update the tooltip
        /// </summary>
        protected override void UpdateTooltip()
        {
            this.Tooltip = string.Join(Environment.NewLine, this.Thing.Category.Union(this.Thing.ElementDefinition.Category).OrderBy(x => x.ShortName).Select(x => x.ShortName));
        }

        /// <summary>
        /// The object changed event handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
            this.UpdateTooltip();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// a value indicating whether the class is being disposed of
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            foreach (var parameterOverride in this.parameterOrOverrideBaseCache)
            {
                parameterOverride.Value.Dispose();
            }

            foreach (var group in this.parameterGroupCache)
            {
                group.Value.Dispose();
            }

            foreach (var disposable in this.elementUsageListenerCache.Values)
            {
                disposable.Dispose();
            }

            this.elementUsageListenerCache.Clear();
        }
        #endregion

        /// <summary>
        /// Update the properties related to this <see cref="ElementUsage"/>
        /// </summary>
        private void UpdateProperties()
        {
            this.Name = this.Thing.Name + " : " + this.ElementDefinition.Name;
            this.ShortName = this.Thing.ShortName + " : " + this.ElementDefinition.ShortName;
            this.ModelCode = this.Thing.ModelCode();

            this.UpdateOwnerNameAndShortName();

            this.PopulateParameterOrOverride();
        }

        /// <summary>
        /// Updates the <see cref="OwnerName"/> and <see cref="OwnerShortName"/> properties of the current view-model
        /// </summary>
        private void UpdateOwnerNameAndShortName()
        {
            if (this.Owner != null)
            {
                this.OwnerName = this.Owner.Name;
                this.OwnerShortName = this.Owner.ShortName;
            }
        }

        /// <summary>
        /// Handles this <see cref="ElementUsageRowViewModel.ElementDefinition"/> updates
        /// </summary>
        private void UpdateElementDefinitionProperties()
        {
            this.PopulateElementUsages();
            this.PopulateParameterGroups();
            this.PopulateParameterOrOverride();
        }

        /// <summary>
        /// Populate the <see cref="ElementUsage"/> property
        /// </summary>
        private void PopulateElementUsages()
        {
            var existingUsages = this.ContainedRows.OfType<ElementUsageRowViewModel>().Select(x => x.Thing).ToList();
            var currentUsages = this.Thing.ElementDefinition.ContainedElement;

            var newUsages = currentUsages.Except(existingUsages);
            var deletedUsaged = existingUsages.Except(currentUsages);

            foreach (var usage in newUsages)
            {
                this.AddElementUsage(usage);
            }

            foreach (var elementUsage in deletedUsaged)
            {
                this.RemoveElementUsage(elementUsage);
            }
        }

        /// <summary>
        /// Add a row representing a <see cref="ElementUsage"/>
        /// </summary>
        /// <param name="elementUsage">The <see cref="ElementUsage"/> to add</param>
        private void AddElementUsage(ElementUsage elementUsage)
        {
            if (!this.elementUsageListenerCache.ContainsKey(elementUsage))
            {
                var listener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(elementUsage)
                    .Where(
                        objectChange =>
                            objectChange.EventKind == EventKind.Updated &&
                            objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => this.UpdateOptionDependentElementUsage((ElementUsage)x.ChangedThing));
                this.elementUsageListenerCache.Add(elementUsage, listener);
            }

            // avoid duplicate and filter on Option
            if (this.ContainedRows.Any(x => x.Thing == elementUsage) || elementUsage.ExcludeOption.Contains(this.Option))
            {
                return;
            }

            var row = new ElementUsageRowViewModel(elementUsage, this.Option, this.Session, this);

            this.ContainedRows.SortedInsert(row, ChildRowComparer);
        }

        /// <summary>
        /// Remove the row associated to a <see cref="ElementUsage"/> that was deleted
        /// </summary>
        /// <param name="elementUsage">The <see cref="ElementUsage"/></param>
        private void RemoveElementUsage(ElementUsage elementUsage)
        {
            var row = this.ContainedRows.SingleOrDefault(x => x.Thing == elementUsage);
            if (row != null)
            {
                row.Dispose();
                this.ContainedRows.Remove(row);
            }

            this.elementUsageListenerCache.Remove(elementUsage);
        }

        /// <summary>
        /// Add or remove a row associated to an <see cref="ElementUsage"/> depending on its <see cref="Option"/>
        /// </summary>
        /// <param name="elementUsage">The <see cref="ElementUsage"/> to update</param>
        private void UpdateOptionDependentElementUsage(ElementUsage elementUsage)
        {
            if (elementUsage.ExcludeOption.Contains(this.Option))
            {
                var row = this.ContainedRows.SingleOrDefault(x => x.Thing == elementUsage);
                if (row != null)
                {
                    row.Dispose();
                    this.ContainedRows.Remove(row);
                }

                return;
            }

            this.AddElementUsage(elementUsage);
        }

        /// <summary>
        /// Populates the Parameter group rows
        /// </summary>
        private void PopulateParameterGroups()
        {
            var definedGroups = this.Thing.ElementDefinition.ParameterGroup;

            // remove deleted groups
            var oldgroup = this.parameterGroupCache.Keys.Except(definedGroups).ToList();
            foreach (var group in oldgroup)
            {
                if (group.ContainingGroup == null)
                {
                    this.ContainedRows.Remove(this.parameterGroupCache[group]);
                }
                else
                {
                    this.parameterGroupCache[group.ContainingGroup].ContainedRows.Remove(
                        this.parameterGroupCache[group]);
                }

                this.parameterGroupCache[group].Dispose();
                this.parameterGroupCache.Remove(group);
                this.parameterGroupContainment.Remove(group);
            }

            // create new group rows
            var newgroup = definedGroups.Except(this.parameterGroupCache.Keys).ToList();
            foreach (var group in newgroup)
            {
                var row = new ParameterGroupRowViewModel(group, this.Session, this);
                this.parameterGroupCache.Add(group, row);
                this.parameterGroupContainment.Add(group, group.ContainingGroup);
            }

            // add the new group in the right position in the tree
            foreach (var group in newgroup)
            {
                if (group.ContainingGroup == null)
                {
                    this.ContainedRows.SortedInsert(this.parameterGroupCache[group], ChildRowComparer);
                }
                else
                {
                    var container = this.parameterGroupCache[group.ContainingGroup];
                    container.ContainedRows.SortedInsert(this.parameterGroupCache[group], ParameterGroupRowViewModel.ChildRowComparer);
                }
            }
        }

        /// <summary>
        /// Populate the <see cref="Parameter"/>s for this row
        /// </summary>
        private void PopulateParameterOrOverride()
        {
            var definedParametersOverride = this.Thing.ParameterOverride;

            // parameters not overriden
            var overridenParameters = definedParametersOverride.Select(x => x.Parameter);
            var definedParameter = this.Thing.ElementDefinition.Parameter.Except(overridenParameters);

            var definedParameterOrOverride = new List<ParameterBase>(definedParametersOverride);
            definedParameterOrOverride.AddRange(definedParameter);

            // remove deleted ParameterOrOverride
            var oldParameterOrOverride = this.parameterOrOverrideBaseCache.Keys.Except(definedParameterOrOverride).ToList();
            foreach (var parameterOrOverride in oldParameterOrOverride)
            {
                var group = this.parameterOrOverrideContainerMap[parameterOrOverride];
                if (group == null)
                {
                    this.ContainedRows.Remove(this.parameterOrOverrideBaseCache[parameterOrOverride]);
                }
                else
                {
                    this.parameterGroupCache[group].ContainedRows.Remove(this.parameterOrOverrideBaseCache[parameterOrOverride]);
                }

                this.parameterOrOverrideBaseCache[parameterOrOverride].Dispose();
                this.parameterOrOverrideBaseCache.Remove(parameterOrOverride);
                this.parameterOrOverrideContainerMap.Remove(parameterOrOverride);
            }

            // create new ParameterOrOverride rows
            var newparameterOrOverrides = definedParameterOrOverride.Except(this.parameterOrOverrideBaseCache.Keys).ToList();
            foreach (var parameterOrOverride in newparameterOrOverrides)
            {
                IRowViewModelBase<ParameterOrOverrideBase> row;

                var parameterOverride = parameterOrOverride as ParameterOverride;
                if (parameterOverride != null)
                {
                    row = new ParameterOverrideRowViewModel(parameterOverride, this.Option, this.Session, this);
                }
                else
                {
                    row = new ParameterRowViewModel((Parameter)parameterOrOverride, this.Option, this.Session, this);
                }

                this.parameterOrOverrideBaseCache.Add(parameterOrOverride, row);
                this.parameterOrOverrideContainerMap.Add(parameterOrOverride, parameterOrOverride.Group);
            }

            // add the new ParameterOrOverride in the right position in the tree
            foreach (var parameterOrOverride in newparameterOrOverrides)
            {
                if (parameterOrOverride.Group == null)
                {
                    this.ContainedRows.SortedInsert(this.parameterOrOverrideBaseCache[parameterOrOverride], ChildRowComparer);
                }
                else
                {
                    var container = this.parameterGroupCache[parameterOrOverride.Group];
                    container.ContainedRows.SortedInsert(this.parameterOrOverrideBaseCache[parameterOrOverride], ParameterGroupRowViewModel.ChildRowComparer);
                }
            }
        }
    }
}