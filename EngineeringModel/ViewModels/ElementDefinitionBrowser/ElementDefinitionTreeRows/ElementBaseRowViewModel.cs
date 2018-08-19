// -------------------------------------------------------------------------------------------------
// <copyright file="ElementBaseRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
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
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Events;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4EngineeringModel.Comparers;
    using ReactiveUI;

    /// <summary>
    /// The Base row class representing an <see cref="ElementBase"/>
    /// </summary>
    /// <typeparam name="T">An <see cref="ElementBase"/> type</typeparam>
    public abstract class ElementBaseRowViewModel<T> : CDP4CommonView.ElementBaseRowViewModel<T>, IElementBaseRowViewModel where T : ElementBase
    {
        /// <summary>
        /// The <see cref="IComparer{T}"/>
        /// </summary>
        public static readonly IComparer<IRowViewModelBase<Thing>> ChildRowComparer = new ElementBaseChildRowComparer(); 

        /// <summary>
        /// A cache for all <see cref="ParameterBase"/>
        /// </summary>
        protected Dictionary<ParameterBase, IRowViewModelBase<ParameterBase>> parameterBaseCache;

        /// <summary>
        /// A cache that associates a <see cref="ParameterBase"/> with its <see cref="ParameterGroup"/> in the tree-view
        /// </summary>
        protected Dictionary<ParameterBase, ParameterGroup> parameterBaseContainerMap;

        /// <summary>
        /// A list of all rows representing all <see cref="ParameterGroup"/> contained by this <see cref="CDP4Common.EngineeringModelData.ElementDefinition"/>
        /// </summary>
        protected Dictionary<ParameterGroup, ParameterGroupRowViewModel> parameterGroupCache;

        /// <summary>
        /// A parameter group - parameter group container mapping
        /// </summary>
        protected Dictionary<ParameterGroup, ParameterGroup> parameterGroupContainment;

        /// <summary>
        /// The cache for the Parameter update's listener
        /// </summary>
        protected Dictionary<ParameterBase, IDisposable> ParameterBaseListener; 

        /// <summary>
        /// The active <see cref="DomainOfExpertise"/>
        /// </summary>
        protected DomainOfExpertise currentDomain;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ElementBaseRowViewModel{T}"/> class
        /// </summary>
        /// <param name="elementBase">The associated <see cref="ElementBase"/></param>
        /// <param name="currentDomain">The active <see cref="DomainOfExpertise"/></param>
        /// <param name="session">The associated <see cref="ISession"/></param>
        /// <param name="containerViewModel">The container view-model</param>
        protected ElementBaseRowViewModel(T elementBase, DomainOfExpertise currentDomain, ISession session,
            IViewModelBase<Thing> containerViewModel)
            : base(elementBase, session, containerViewModel)
        {
            this.parameterBaseCache = new Dictionary<ParameterBase, IRowViewModelBase<ParameterBase>>();
            this.parameterBaseContainerMap = new Dictionary<ParameterBase, ParameterGroup>();
            this.parameterGroupCache = new Dictionary<ParameterGroup, ParameterGroupRowViewModel>();
            this.parameterGroupContainment = new Dictionary<ParameterGroup, ParameterGroup>();
            this.ParameterBaseListener = new Dictionary<ParameterBase, IDisposable>();
            this.currentDomain = currentDomain;
            this.UpdateTooltip();
            this.UpdateOwnerProperties();
            this.WhenAnyValue(vm => vm.Owner).Subscribe(_ => this.UpdateOwnerProperties());
        }
        
        /// <summary>
        /// Gets or sets the <see cref="HasExcludes"/>. Null if <see cref="ElementUsage"/> is in no options.
        /// </summary>
        public virtual bool? HasExcludes
        {
            get { return null; }
            set { /*does nothing, for binding purposes only*/ }
        }

        /// <summary>
        /// Gets the value indicating whether the row is a top element. Property implemented here to fix binding errors.
        /// </summary>
        public virtual bool IsTopElement
        {
            get { return false; }
            set { /*does nothing, for binding purposes only*/ }
        }

        /// <summary>
        /// Gets the active <see cref="DomainOfExpertise"/>
        /// </summary>
        public virtual DomainOfExpertise CurrentDomain
        {
            get { return this.currentDomain; }
        }

        /// <summary>
        /// Gets a value indicating whether the value set editors are active
        /// </summary>
        public bool IsValueSetEditorActive
        {
            get { return false; }
        }
        
        /// <summary>
        /// Update properties of the Owner
        /// </summary>
        public void UpdateOwnerProperties()
        {
            if (this.Owner != null)
            {
                this.OwnerName = this.Owner.Name;
                this.OwnerShortName = this.Owner.ShortName;
            }
        }

        /// <summary>
        /// Update the row containment associated to a <see cref="ParameterBase"/>
        /// </summary>
        /// <param name="parameterBase">The <see cref="ParameterBase"/></param>
        public void UpdateParameterBasePosition(ParameterBase parameterBase)
        {
            try
            {
                var oldContainer = this.parameterBaseContainerMap[parameterBase];
                var newContainer = parameterBase.Group;
                var associatedRow = this.parameterBaseCache[parameterBase];

                if (newContainer != null && oldContainer == null)
                {
                    this.ContainedRows.Remove(associatedRow);
                    this.parameterGroupCache[newContainer].ContainedRows.SortedInsert(associatedRow, ParameterGroupRowViewModel.ChildRowComparer);
                    this.parameterBaseContainerMap[parameterBase] = newContainer;
                }
                else if (newContainer == null && oldContainer != null)
                {
                    this.parameterGroupCache[oldContainer].ContainedRows.Remove(associatedRow);
                    this.ContainedRows.SortedInsert(associatedRow, ChildRowComparer);
                    this.parameterBaseContainerMap[parameterBase] = null;
                }
                else if (newContainer != null && oldContainer != null && newContainer != oldContainer)
                {
                    this.parameterGroupCache[oldContainer].ContainedRows.Remove(associatedRow);
                    this.parameterGroupCache[newContainer].ContainedRows.SortedInsert(associatedRow, ParameterGroupRowViewModel.ChildRowComparer);
                    this.parameterBaseContainerMap[parameterBase] = newContainer;
                }
            }
            catch (Exception exception)
            {
                logger.Error(exception, "A problem occured when executing the UpdateParameterBasePosition method.");
            }            
        }

        /// <summary>
        /// Update the row containment associated to a <see cref="ParameterGroup"/>
        /// </summary>
        /// <param name="parameterGroup">The <see cref="ParameterGroup"/></param>
        public void UpdateParameterGroupPosition(ParameterGroup parameterGroup)
        {
            try
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
            catch (Exception exception)
            {
                logger.Error(exception, "A problem occured when executing the UpdateParameterGroupPosition method.");
            }
        }
        
        /// <summary>
        /// Initializes the subscription for this row
        /// </summary>
        protected override void InitializeSubscriptions()
        {
            base.InitializeSubscriptions();

            var ownerListener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing.Owner)
                                   .Where(objectChange => objectChange.EventKind == EventKind.Updated)
                                   .ObserveOn(RxApp.MainThreadScheduler)
                                   .Subscribe(x => { this.OwnerName = this.Thing.Owner.Name; this.OwnerShortName = this.Thing.Owner.ShortName; });
            this.Disposables.Add(ownerListener);
        }

        /// <summary>
        /// Handles the <see cref="ObjectChangedEvent"/>
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
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
            foreach (var row in this.parameterBaseCache.Values)
            {
                row.Dispose();
            }

            foreach (var groupRow in this.parameterGroupCache.Values)
            {
                groupRow.Dispose();
            }

            foreach (var listener in this.ParameterBaseListener.Values)
            {
                listener.Dispose();
            }
        }
        
        /// <summary>
        /// Populates the Parameter group rows
        /// </summary>
        /// <param name="elementDefinition">
        /// The element Definition.
        /// </param>
        protected void PopulateParameterGroups(ElementDefinition elementDefinition)
        {
            try
            {
                var definedGroups = elementDefinition.ParameterGroup;

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
                        this.parameterGroupCache[group.ContainingGroup].ContainedRows.Remove(this.parameterGroupCache[group]);
                    }

                    this.parameterGroupCache[group].Dispose();
                    this.parameterGroupCache.Remove(group);
                    this.parameterGroupContainment.Remove(group);
                }

                var updatedGroups = this.parameterGroupCache.Keys.Intersect(definedGroups).ToList();

                // create new group rows
                var newgroup = definedGroups.Except(this.parameterGroupCache.Keys).ToList();
                foreach (var group in newgroup)
                {
                    var row = new ParameterGroupRowViewModel(group, this.currentDomain, this.Session, this);
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

                // Check if ContainingGroup for existing group might have been updated
                foreach (var group in updatedGroups)
                {
                    this.UpdateParameterGroupPosition(group);
                }
            }
            catch (Exception exception)
            {
                logger.Error(exception, "A problem occured when executing the PopulateParameterGroups method.");
            }
        }

        /// <summary>
        /// Populate the <see cref="ParameterBase"/> rows for this <see cref="ElementBase{T}"/> row
        /// </summary>
        protected abstract void PopulateParameters();

        /// <summary>
        /// Removes the list of <see cref="ParameterBase"/>
        /// </summary>
        /// <param name="deletedParameterBase">The <see cref="ParameterBase"/>s to remove</param>
        protected void RemoveParameterBase(IEnumerable<ParameterBase> deletedParameterBase)
        {
            try
            {
                foreach (var parameter in deletedParameterBase)
                {
                    IRowViewModelBase<ParameterBase> row;
                    if (!this.parameterBaseCache.TryGetValue(parameter, out row))
                    {
                        continue;
                    }

                    // remove the row from its container node
                    var group = this.parameterBaseContainerMap[parameter];
                    if (group == null)
                    {
                        this.ContainedRows.Remove(row);
                    }
                    else
                    {
                        this.parameterGroupCache[group].ContainedRows.Remove(row);
                    }

                    row.Dispose();
                    this.parameterBaseCache.Remove(parameter);
                    this.parameterBaseContainerMap.Remove(parameter);

                    this.RemoveParameterBaseListener(parameter);
                }
            }
            catch (Exception exception)
            {
                logger.Error(exception, "A problem occured when executing the RemoveParameterBase method.");
            }
        }

        /// <summary>
        /// Adds the list of <see cref="ParameterBase"/>
        /// </summary>
        /// <param name="addedParameterBase">The <see cref="ParameterBase"/>s to add</param>
        protected void AddParameterBase(IEnumerable<ParameterBase> addedParameterBase)
        {
            foreach (var parameterBase in addedParameterBase)
            {
                IRowViewModelBase<ParameterBase> row = null;
                var parameter = parameterBase as Parameter;
                if (parameter != null)
                {
                    row = new ParameterRowViewModel(parameter, this.Session, this, typeof(T) == typeof(ElementUsage));
                    this.AddParameterOrOverrideListener(parameter);
                }

                var parameterOverride = parameterBase as ParameterOverride;
                if (parameterOverride != null)
                {
                    row = new ParameterOverrideRowViewModel(parameterOverride, this.Session, this);
                    this.AddParameterOrOverrideListener(parameterOverride);
                }

                var parameterSubscription = parameterBase as ParameterSubscription;
                if (parameterSubscription != null)
                {
                    var subscribedParameter = parameterSubscription.Container as Parameter;
                    if (subscribedParameter != null)
                    {
                        row = new ParameterSubscriptionRowViewModel(parameterSubscription, this.Session, this, typeof(T) == typeof(ElementUsage));
                    }

                    var subscribedParameterOverride = parameterSubscription.Container as ParameterOverride;
                    if (subscribedParameterOverride != null)
                    {
                        row = new ParameterSubscriptionRowViewModel(parameterSubscription, this.Session, this, false);
                    }
                    
                    this.AddParameterSubscriptionListener(parameterSubscription);
                }

                if (row == null)
                {
                    throw new NotSupportedException("The ParameterBase is neither a Parameter or a Subscription.");
                }

                this.parameterBaseCache.Add(parameterBase, row);

                var group = parameterBase.Group;
                this.parameterBaseContainerMap.Add(parameterBase, group);

                if (group == null)
                {
                    this.ContainedRows.SortedInsert(row, ChildRowComparer);
                }
                else
                {
                    ParameterGroupRowViewModel parameterGroupRowViewModel;
                    if (this.parameterGroupCache.TryGetValue(group, out parameterGroupRowViewModel))
                    {
                        parameterGroupRowViewModel.ContainedRows.SortedInsert(row, ParameterGroupRowViewModel.ChildRowComparer);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the list of <see cref="ParameterBase"/>
        /// </summary>
        /// <param name="updatedParameterBase">The <see cref="ParameterBase"/>s to update</param>
        protected void UpdateParameterBase(IEnumerable<ParameterBase> updatedParameterBase)
        {
            foreach (var parameterBase in updatedParameterBase)
            {
                this.UpdateParameterBasePosition(parameterBase);
            }
        }

        /// <summary>
        /// Add the listener associated to the <see cref="ParameterOrOverrideBase"/>
        /// </summary>
        /// <param name="parameterOrOverride">The <see cref="ParameterOrOverrideBase"/></param>
        private void AddParameterOrOverrideListener(ParameterOrOverrideBase parameterOrOverride)
        {
            if (this.ParameterBaseListener.ContainsKey(parameterOrOverride))
            {
                return;
            }

            var listener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(parameterOrOverride)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.PopulateParameters());
            this.ParameterBaseListener.Add(parameterOrOverride, listener);
        }

        /// <summary>
        /// Add the listener associated to the <see cref="ParameterSubscription"/>
        /// </summary>
        /// <param name="parameterSubscription">The <see cref="ParameterSubscription"/></param>
        private void AddParameterSubscriptionListener(ParameterSubscription parameterSubscription)
        {
            if (this.ParameterBaseListener.ContainsKey(parameterSubscription))
            {
                return;
            }

            var listener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(parameterSubscription)
                .Where(objectChange => objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.PopulateParameters());
            this.ParameterBaseListener.Add(parameterSubscription, listener);
        }

        /// <summary>
        /// Remove the listener associated to the <see cref="ParameterBase"/>
        /// </summary>
        /// <param name="parameterBase">The <see cref="ParameterBase"/></param>
        private void RemoveParameterBaseListener(ParameterBase parameterBase)
        {
            var parameter = parameterBase as Parameter;
            if (parameter != null)
            {
                this.RemoveParameterOrOverrideListener(parameter);
                return;
            }

            var parameterOverride = parameterBase as ParameterOverride;
            if (parameterOverride != null)
            {
                this.RemoveParameterOrOverrideListener(parameterOverride);
            }

            var parameterSubscription = parameterBase as ParameterSubscription;
            if (parameterSubscription != null)
            {
                this.RemoveParameterSubscriptionListener(parameterSubscription);
            }
        }

        /// <summary>
        /// Remove the listener associated to the <see cref="Parameter"/>
        /// </summary>
        /// <param name="parameter">The <see cref="Parameter"/></param>
        private void RemoveParameterOrOverrideListener(Parameter parameter)
        {
            IDisposable disposable;
            if (this.ParameterBaseListener.TryGetValue(parameter, out disposable))
            {
                var elementDef = this.Thing as ElementDefinition;
                if (elementDef == null)
                {
                    var usage = this.Thing as ElementUsage;
                    if (usage == null)
                    {
                        return;
                    }

                    elementDef = usage.ElementDefinition;
                }

                if (!elementDef.Parameter.Contains(parameter))
                {
                    disposable.Dispose();
                    this.ParameterBaseListener.Remove(parameter);   
                }
            }
        }

        /// <summary>
        /// Remove the listener associated to the <see cref="ParameterSubscription"/>
        /// </summary>
        /// <param name="parameterSubscription">The <see cref="ParameterSubscription"/></param>
        private void RemoveParameterSubscriptionListener(ParameterSubscription parameterSubscription)
        {
            IDisposable disposable;
            if(this.ParameterBaseListener.TryGetValue(parameterSubscription, out disposable))
            {
                disposable.Dispose();
                this.ParameterBaseListener.Remove(parameterSubscription);
            }
        }

        /// <summary>
        /// Remove the listener associated to the <see cref="ParameterOverride"/>
        /// </summary>
        /// <param name="parameterOverride">The <see cref="ParameterOverride"/></param>
        private void RemoveParameterOrOverrideListener(ParameterOverride parameterOverride)
        {
            IDisposable disposable;
            if (this.ParameterBaseListener.TryGetValue(parameterOverride, out disposable))
            {
                var usage = this.Thing as ElementUsage;
                if (usage == null)
                {
                    return;
                }

                if (!usage.ParameterOverride.Contains(parameterOverride))
                {
                    disposable.Dispose();
                    this.ParameterBaseListener.Remove(parameterOverride);
                }
            }    
        }

        /// <summary>
        /// Set the <see cref="IDropInfo.Effects"/> when the payload is an <see cref="Category"/>
        /// </summary>
        /// <param name="dropInfo">The <see cref="IDropInfo"/></param>
        /// <param name="category">The <see cref="Category"/> to drop</param>
        protected void DragOver(IDropInfo dropInfo, Category category)
        {
            if (!this.PermissionService.CanWrite(this.Thing) || this.Thing.Category.Contains(category) || !category.PermissibleClass.Contains(this.Thing.ClassKind))
            {
                dropInfo.Effects = DragDropEffects.None;
                return;
            }

            dropInfo.Effects = DragDropEffects.Copy;
        }

        /// <summary>
        /// Handle the drop of a <see cref="Category"/>
        /// </summary>
        /// <param name="dropInfo">The <see cref="IDropInfo"/> containing the payload</param>
        /// <param name="category">The <see cref="Category"/></param>
        protected async Task Drop(IDropInfo dropInfo, Category category)
        {
            var clone = this.Thing.Clone(false);
            clone.Category.Add(category);

            await this.DalWrite(clone, true);
            if (!this.HasError)
            {
                this.ShowConfirmation("Success", $"The Element {this.Thing.Name} was updated successfully", NotificationKind.INFO);
            }
        }
    }
}