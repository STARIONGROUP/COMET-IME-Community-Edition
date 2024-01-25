// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementBaseRowViewModel.cs" company="RHEA System S.A.">
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
    using System.Threading.Tasks;

    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Events;
    using CDP4Composition.MessageBus;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4EngineeringModel.Comparers;
    using CDP4EngineeringModel.Services;

    using ReactiveUI;

    /// <summary>
    /// The Base row class representing an <see cref="ElementBase" />
    /// </summary>
    /// <typeparam name="T">An <see cref="ElementBase" /> type</typeparam>
    public abstract class ElementBaseRowViewModel<T> : CDP4CommonView.ElementBaseRowViewModel<T>, IElementBaseRowViewModel, IObfuscatedRowViewModel where T : ElementBase
    {
        /// <summary>
        /// The <see cref="IComparer{T}" />
        /// </summary>
        public static readonly IComparer<IRowViewModelBase<Thing>> ChildRowComparer = new ElementBaseChildRowComparer();

        /// <summary>
        /// The active <see cref="DomainOfExpertise" />
        /// </summary>
        protected DomainOfExpertise currentDomain;

        /// <summary>
        /// Backing field for <see cref="ModelCode" />
        /// </summary>
        private string modelCode;

        /// <summary>
        /// Backing field for <see cref="IsObfuscated" />
        /// </summary>
        private bool isObfuscated;

        /// <summary>
        /// A cache for all <see cref="ParameterBase" />
        /// </summary>
        protected Dictionary<ParameterBase, IRowViewModelBase<ParameterBase>> parameterBaseCache;

        /// <summary>
        /// A cache that associates a <see cref="ParameterBase" /> with its <see cref="ParameterGroup" /> in the tree-view
        /// </summary>
        protected Dictionary<ParameterBase, ParameterGroup> parameterBaseContainerMap;

        /// <summary>
        /// The cache for the Parameter update's listener
        /// </summary>
        protected Dictionary<ParameterBase, IDisposable> ParameterBaseListener;

        /// <summary>
        /// A list of all rows representing all <see cref="ParameterGroup" /> contained by this
        /// <see cref="CDP4Common.EngineeringModelData.ElementDefinition" />
        /// </summary>
        protected Dictionary<ParameterGroup, ParameterGroupRowViewModel> parameterGroupCache;

        /// <summary>
        /// A parameter group - parameter group container mapping
        /// </summary>
        protected Dictionary<ParameterGroup, ParameterGroup> parameterGroupContainment;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementBaseRowViewModel{T}" /> class
        /// </summary>
        /// <param name="elementBase">The associated <see cref="ElementBase" /></param>
        /// <param name="currentDomain">The active <see cref="DomainOfExpertise" /></param>
        /// <param name="session">The associated <see cref="ISession" /></param>
        /// <param name="containerViewModel">The container view-model</param>
        protected ElementBaseRowViewModel(
            T elementBase,
            DomainOfExpertise currentDomain,
            ISession session,
            IViewModelBase<Thing> containerViewModel,
            IObfuscationService obfuscationService)
            : base(elementBase, session, containerViewModel)
        {
            this.parameterBaseCache = new Dictionary<ParameterBase, IRowViewModelBase<ParameterBase>>();
            this.parameterBaseContainerMap = new Dictionary<ParameterBase, ParameterGroup>();
            this.parameterGroupCache = new Dictionary<ParameterGroup, ParameterGroupRowViewModel>();
            this.parameterGroupContainment = new Dictionary<ParameterGroup, ParameterGroup>();
            this.ParameterBaseListener = new Dictionary<ParameterBase, IDisposable>();
            this.ObfuscationService = obfuscationService;

            this.currentDomain = currentDomain;
            this.UpdateOwnerProperties();
            this.UpdateObfuscationProperties();

            this.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(this.Owner))
                {
                    this.UpdateOwnerProperties();
                }
            };

            this.UpdateModelCode();
        }

        /// <summary>
        /// Gets or sets the <see cref="HasExcludes" />. Null if <see cref="ElementUsage" /> is in no options.
        /// </summary>
        public virtual bool? HasExcludes
        {
            get => null;
            set
            {
                /*does nothing, for binding purposes only*/
            }
        }

        /// <summary>
        /// Gets the value indicating whether the row is a top element. Property implemented here to fix binding errors.
        /// </summary>
        public virtual bool IsTopElement
        {
            get => false;
            set
            {
                /*does nothing, for binding purposes only*/
            }
        }

        /// <summary>
        /// Gets the active <see cref="DomainOfExpertise" />
        /// </summary>
        public virtual DomainOfExpertise CurrentDomain => this.currentDomain;

        /// <summary>
        /// Gets a value indicating whether the value set editors are active
        /// </summary>
        public bool IsValueSetEditorActive => false;

        /// <summary>
        /// Gets the mode-code
        /// </summary>
        public string ModelCode
        {
            get => this.modelCode;
            private set => this.RaiseAndSetIfChanged(ref this.modelCode, value);
        }

        /// <summary>
        /// Update the model code property of itself and all contained rows recursively
        /// </summary>
        public void UpdateModelCode()
        {
            this.ModelCode = ((IModelCode)this.Thing).ModelCode();

            foreach (var containedRow in this.ContainedRows)
            {
                var modelCodeRow = containedRow as IHaveModelCode;
                modelCodeRow?.UpdateModelCode();

                if (containedRow is IHaveContainedModelCodes groupRow)
                {
                    groupRow.UpdateModelCode();
                }
            }
        }

        /// <summary>
        /// Gets the applied categories as an <see cref="IEnumerable{Category}" />.
        /// </summary>
        public IEnumerable<Category> Category => this.Thing?.Category;

        /// <summary>
        /// Update the row containment associated to a <see cref="ParameterBase" />
        /// </summary>
        /// <param name="parameterBase">The <see cref="ParameterBase" /></param>
        public void UpdateParameterBasePosition(ParameterBase parameterBase)
        {
            try
            {
                var oldContainer = this.parameterBaseContainerMap[parameterBase];
                var newContainer = parameterBase.Group;
                var associatedRow = this.parameterBaseCache[parameterBase];

                if (newContainer != null && oldContainer == null)
                {
                    this.ContainedRows.RemoveWithoutDispose(associatedRow);
                    this.parameterGroupCache[newContainer].ContainedRows.SortedInsert(associatedRow, ParameterGroupRowViewModel.ChildRowComparer);
                    this.parameterBaseContainerMap[parameterBase] = newContainer;
                }
                else if (newContainer == null && oldContainer != null)
                {
                    this.parameterGroupCache[oldContainer].ContainedRows.RemoveWithoutDispose(associatedRow);
                    this.ContainedRows.SortedInsert(associatedRow, ChildRowComparer);
                    this.parameterBaseContainerMap[parameterBase] = null;
                }
                else if (newContainer != null && oldContainer != null && newContainer != oldContainer)
                {
                    this.parameterGroupCache[oldContainer].ContainedRows.RemoveWithoutDispose(associatedRow);
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
        /// Update the row containment associated to a <see cref="ParameterGroup" />
        /// </summary>
        /// <param name="parameterGroup">The <see cref="ParameterGroup" /></param>
        public void UpdateParameterGroupPosition(ParameterGroup parameterGroup)
        {
            this.UpdateParameterGroupPosition(parameterGroup, false, null);
        }

        /// <summary>
        /// Update the row containment associated to a <see cref="ParameterGroup" />
        /// </summary>
        /// <param name="parameterGroup">The <see cref="ParameterGroup" /></param>
        private void UpdateParameterGroupPosition(ParameterGroup parameterGroup, bool delaySort, HashSet<DisposableReactiveList<IRowViewModelBase<Thing>>> toBeSorted)
        {
            try
            {
                var oldContainer = this.parameterGroupContainment[parameterGroup];
                var newContainer = parameterGroup.ContainingGroup;
                var associatedRow = this.parameterGroupCache[parameterGroup];

                if (newContainer != null && oldContainer == null)
                {
                    this.ContainedRows.RemoveWithoutDispose(associatedRow);

                    if (delaySort)
                    {
                        toBeSorted.Add(this.parameterGroupCache[newContainer].ContainedRows);
                        this.parameterGroupCache[newContainer].ContainedRows.Add(associatedRow);
                    }
                    else
                    {
                        this.parameterGroupCache[newContainer].ContainedRows.SortedInsert(associatedRow, ParameterGroupRowViewModel.ChildRowComparer);
                    }

                    this.parameterGroupContainment[parameterGroup] = newContainer;
                }
                else if (newContainer == null && oldContainer != null)
                {
                    this.parameterGroupCache[oldContainer].ContainedRows.RemoveWithoutDispose(associatedRow);
                    this.ContainedRows.SortedInsert(associatedRow, ChildRowComparer);
                    this.parameterGroupContainment[parameterGroup] = null;
                }
                else if (newContainer != null && oldContainer != null && newContainer != oldContainer)
                {
                    this.parameterGroupCache[oldContainer].ContainedRows.RemoveWithoutDispose(associatedRow);
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
        /// Gets the <see cref="IObfuscationService" />
        /// </summary>
        public IObfuscationService ObfuscationService { get; private set; }

        /// <summary>
        /// Gets or sets the value indicating whether the row is obfuscated
        /// </summary>
        public bool IsObfuscated { get; set; }

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
        /// Update properties of obfuscation
        /// </summary>
        public void UpdateObfuscationProperties()
        {
            this.IsObfuscated = this.ObfuscationService.IsRowObfuscated(this.Thing);
        }

        /// <summary>
        /// Initializes the subscription for this row
        /// </summary>
        protected override void InitializeSubscriptions()
        {
            base.InitializeSubscriptions();

            Func<ObjectChangedEvent, bool> discriminator = objectChange => objectChange.EventKind == EventKind.Updated;
            Action<ObjectChangedEvent> action = x => this.UpdateOwnerProperties();

            if (this.AllowMessageBusSubscriptions)
            {
                var ownerListener = this.CDPMessageBus.Listen<ObjectChangedEvent>(this.Thing.Owner)
                    .Where(discriminator)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(action);

                this.Disposables.Add(ownerListener);
            }
            else
            {
                var ownerObserver = this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(DomainOfExpertise));
                this.Disposables.Add(this.MessageBusHandler.GetHandler<ObjectChangedEvent>().RegisterEventHandler(ownerObserver, new ObjectChangedMessageBusEventHandlerSubscription(this.Thing.Owner, discriminator, action)));
            }
        }

        /// <summary>
        /// Handles the <see cref="ObjectChangedEvent" />
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent" /></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateModelCode();
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
                        this.ContainedRows.RemoveWithoutDispose(this.parameterGroupCache[group]);
                    }
                    else
                    {
                        this.parameterGroupCache[group.ContainingGroup].ContainedRows.RemoveWithoutDispose(this.parameterGroupCache[group]);
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

                var containers = new HashSet<ParameterGroupRowViewModel>();

                // add the new group in the right position in the tree
                foreach (var group in newgroup)
                {
                    if (group.ContainingGroup == null)
                    {
                        this.ContainedRows.Add(this.parameterGroupCache[group]);
                    }
                    else
                    {
                        var container = this.parameterGroupCache[group.ContainingGroup];
                        containers.Add(container);
                        container.ContainedRows.Add(this.parameterGroupCache[group]);
                    }
                }

                // Check if ContainingGroup for existing group might have been updated
                var containedRowLists = new HashSet<DisposableReactiveList<IRowViewModelBase<Thing>>>();

                foreach (var group in updatedGroups)
                {
                    this.UpdateParameterGroupPosition(group, true, containedRowLists);
                }

                foreach (var containedRowList in containedRowLists)
                {
                    containedRowList.Sort(ParameterGroupRowViewModel.ChildRowComparer);
                }

                foreach (var container in containers)
                {
                    container.ContainedRows.Sort(ParameterGroupRowViewModel.ChildRowComparer);
                }

                this.ContainedRows.Sort(ChildRowComparer);
            }
            catch (Exception exception)
            {
                logger.Error(exception, "A problem occured when executing the PopulateParameterGroups method.");
            }
        }

        /// <summary>
        /// Populate the <see cref="ParameterBase" /> rows for this <see cref="ElementBase{T}" /> row
        /// </summary>
        protected abstract void PopulateParameters();

        /// <summary>
        /// Removes the list of <see cref="ParameterBase" />
        /// </summary>
        /// <param name="deletedParameterBase">The <see cref="ParameterBase" />s to remove</param>
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
                        this.ContainedRows.RemoveWithoutDispose(row);
                    }
                    else
                    {
                        this.parameterGroupCache[group].ContainedRows.RemoveWithoutDispose(row);
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
        /// Adds the list of <see cref="ParameterBase" />
        /// </summary>
        /// <param name="addedParameterBase">The <see cref="ParameterBase" />s to add</param>
        protected void AddParameterBase(IEnumerable<ParameterBase> addedParameterBase)
        {
            var groupContainers = new HashSet<DisposableReactiveList<IRowViewModelBase<Thing>>>();

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
                    this.ContainedRows.Add(row);
                }
                else
                {
                    ParameterGroupRowViewModel parameterGroupRowViewModel;

                    if (this.parameterGroupCache.TryGetValue(group, out parameterGroupRowViewModel))
                    {
                        groupContainers.Add(parameterGroupRowViewModel.ContainedRows);
                        parameterGroupRowViewModel.ContainedRows.Add(row);
                    }
                }
            }

            foreach (var groupContainer in groupContainers)
            {
                groupContainer.Sort(ParameterGroupRowViewModel.ChildRowComparer);
            }

            this.ContainedRows.Sort(ChildRowComparer);
        }

        /// <summary>
        /// Updates the list of <see cref="ParameterBase" />
        /// </summary>
        /// <param name="updatedParameterBase">The <see cref="ParameterBase" />s to update</param>
        protected void UpdateParameterBase(IEnumerable<ParameterBase> updatedParameterBase)
        {
            foreach (var parameterBase in updatedParameterBase)
            {
                this.UpdateParameterBasePosition(parameterBase);
            }
        }

        /// <summary>
        /// Add the listener associated to the <see cref="ParameterOrOverrideBase" />
        /// </summary>
        /// <param name="parameterOrOverride">The <see cref="ParameterOrOverrideBase" /></param>
        private void AddParameterOrOverrideListener(ParameterOrOverrideBase parameterOrOverride)
        {
            if (this.ParameterBaseListener.ContainsKey(parameterOrOverride))
            {
                return;
            }

            Func<ObjectChangedEvent, bool> discriminator =
                objectChange => objectChange.EventKind == EventKind.Updated
                                && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber;

            Action<ObjectChangedEvent> action = x => this.PopulateParameters();

            if (this.AllowMessageBusSubscriptions)
            {
                var listener = this.CDPMessageBus.Listen<ObjectChangedEvent>(parameterOrOverride)
                    .Where(discriminator)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(action);

                this.ParameterBaseListener.Add(parameterOrOverride, listener);
            }
            else
            {
                var parameterOrOverrideObserver = this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(ParameterOrOverrideBase));

                this.ParameterBaseListener.Add(
                    parameterOrOverride,
                    this.MessageBusHandler.GetHandler<ObjectChangedEvent>().RegisterEventHandler(parameterOrOverrideObserver, new ObjectChangedMessageBusEventHandlerSubscription(parameterOrOverride, discriminator, action)));
            }
        }

        /// <summary>
        /// Add the listener associated to the <see cref="ParameterSubscription" />
        /// </summary>
        /// <param name="parameterSubscription">The <see cref="ParameterSubscription" /></param>
        private void AddParameterSubscriptionListener(ParameterSubscription parameterSubscription)
        {
            if (this.ParameterBaseListener.ContainsKey(parameterSubscription))
            {
                return;
            }

            Func<ObjectChangedEvent, bool> discriminator = objectChange => objectChange.ChangedThing.RevisionNumber > this.RevisionNumber;
            Action<ObjectChangedEvent> action = x => this.PopulateParameters();

            if (this.AllowMessageBusSubscriptions)
            {
                var listener = this.CDPMessageBus.Listen<ObjectChangedEvent>(parameterSubscription)
                    .Where(discriminator)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(action);

                this.ParameterBaseListener.Add(parameterSubscription, listener);
            }
            else
            {
                var parameterOrOverrideObserver = this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(ParameterSubscription));

                this.ParameterBaseListener.Add(
                    parameterSubscription,
                    this.MessageBusHandler.GetHandler<ObjectChangedEvent>().RegisterEventHandler(parameterOrOverrideObserver, new ObjectChangedMessageBusEventHandlerSubscription(parameterSubscription, discriminator, action)));
            }
        }

        /// <summary>
        /// Remove the listener associated to the <see cref="ParameterBase" />
        /// </summary>
        /// <param name="parameterBase">The <see cref="ParameterBase" /></param>
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
        /// Remove the listener associated to the <see cref="Parameter" />
        /// </summary>
        /// <param name="parameter">The <see cref="Parameter" /></param>
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
        /// Remove the listener associated to the <see cref="ParameterSubscription" />
        /// </summary>
        /// <param name="parameterSubscription">The <see cref="ParameterSubscription" /></param>
        private void RemoveParameterSubscriptionListener(ParameterSubscription parameterSubscription)
        {
            IDisposable disposable;

            if (this.ParameterBaseListener.TryGetValue(parameterSubscription, out disposable))
            {
                disposable.Dispose();
                this.ParameterBaseListener.Remove(parameterSubscription);
            }
        }

        /// <summary>
        /// Remove the listener associated to the <see cref="ParameterOverride" />
        /// </summary>
        /// <param name="parameterOverride">The <see cref="ParameterOverride" /></param>
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
        /// Set the <see cref="IDropInfo.Effects" /> when the payload is an <see cref="Category" />
        /// </summary>
        /// <param name="dropInfo">The <see cref="IDropInfo" /></param>
        /// <param name="category">The <see cref="Category" /> to drop</param>
        protected void DragOver(IDropInfo dropInfo, Category category)
        {
            dropInfo.Effects = CategoryApplicationValidationService.ValidateDragDrop(this.Session.PermissionService, this.Thing, category, logger);
        }

        /// <summary>
        /// Handle the drop of a <see cref="Category" />
        /// </summary>
        /// <param name="dropInfo">The <see cref="IDropInfo" /> containing the payload</param>
        /// <param name="category">The <see cref="Category" /></param>
        protected async Task Drop(IDropInfo dropInfo, Category category)
        {
            try
            {
                var clone = this.Thing.Clone(false);
                clone.Category.Add(category);

                await this.DalWrite(clone, true);

                if (!this.HasError)
                {
                    this.ShowConfirmation("Success", $"The Element {this.Thing.Name} was updated successfully", NotificationKind.INFO);
                }
            }
            catch (Exception exception)
            {
                logger.Error(exception, "An error occured when dropping a Category");
            }
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for highlight of row
        /// on the <see cref="Thing"/> that is being represented by the view-model
        /// </summary>
        protected override void HighlightEventHandler()
        {
            base.HighlightEventHandler();

            if (this.TopContainerViewModel is ElementDefinitionsBrowserViewModel browser)
            {
                browser.ChangeFocusOnThing(this.Thing);
            }
        }
    }
}
