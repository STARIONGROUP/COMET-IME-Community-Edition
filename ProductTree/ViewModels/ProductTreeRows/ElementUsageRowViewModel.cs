// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementUsageRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of CDP4-COMET-IME Community Edition.
//    The CDP4-COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ProductTree.ViewModels
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
    using CDP4Composition.Services;
    using CDP4Composition.Services.NestedElementTreeService;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4ProductTree.Comparers;

    using CommonServiceLocator;

    using ReactiveUI;

    /// <summary>
    /// A row-view-model class representing a <see cref="ElementUsage"/>
    /// </summary>
    public class ElementUsageRowViewModel : CDP4CommonView.ElementUsageRowViewModel, IParameterRowContainer, IDropTarget
    {
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

        /// <summary>
        /// The <see cref="INestedElementTreeService"/>
        /// </summary>
        private readonly INestedElementTreeService nestedElementTreeService = ServiceLocator.Current.GetInstance<INestedElementTreeService>();

        /// <summary>
        /// The backing field for <see cref="ThingCreator"/>
        /// </summary>
        private IThingCreator thingCreator;

        /// <summary>
        /// Backing field for <see cref="Category"/>
        /// </summary>
        private IEnumerable<Category> category;

        /// <summary>
        /// Backing field for <see cref="DisplayCategory"/>
        /// </summary>
        private string displayCategory;

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
        public ElementUsageRowViewModel(ElementUsage elementUsage, Option option, ISession session, IViewModelBase<Thing> containerRow)
            : base(elementUsage, session, containerRow)
        {
            this.Option = option;
            this.parameterOrOverrideBaseCache = new Dictionary<ParameterBase, IRowViewModelBase<ParameterOrOverrideBase>>();
            this.parameterOrOverrideContainerMap = new Dictionary<ParameterBase, ParameterGroup>();
            this.parameterGroupCache = new Dictionary<ParameterGroup, ParameterGroupRowViewModel>();
            this.parameterGroupContainment = new Dictionary<ParameterGroup, ParameterGroup>();

            this.UpdateModelCode();
            this.UpdateElementDefinitionProperties();
            this.UpdateProperties();
            this.UpdateTooltip();
        }

        /// <summary>
        /// Gets the Category
        /// </summary>
        public IEnumerable<Category> Category
        {
            get => this.category;
            private set => this.RaiseAndSetIfChanged(ref this.category, value);
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
        /// Gets the model-code
        /// </summary>
        public string ModelCode
        {
            get => this.modelCode;
            private set => this.RaiseAndSetIfChanged(ref this.modelCode, value);
        }

        /// <summary>
        /// Calculates the Path
        /// </summary>
        public string GetPath()
        {
            return this.nestedElementTreeService.GetNestedElementPath(this.Thing, this.Option);
        }

        /// <summary>
        /// Update the model code property of itself and all contained rows recursively
        /// </summary>
        public void UpdateModelCode()
        {
            this.ModelCode = this.Thing.ModelCode();
            foreach (var containedRow in this.ContainedRows)
            {
                var modelCodeRow = containedRow as IHavePath;
                modelCodeRow?.UpdateModelCode();

                if (containedRow is IHaveContainedModelCodes groupRow)
                {
                    groupRow.UpdateModelCode();
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IThingCreator"/> that is used to create different <see cref="Things"/>.
        /// </summary>
        public IThingCreator ThingCreator
        {
            get => this.thingCreator = this.thingCreator ?? ServiceLocator.Current.GetInstance<IThingCreator>();
            set => this.thingCreator = value;
        }

        /// <summary>
        /// Update the row containment associated to a <see cref="ParameterBase"/>
        /// </summary>
        /// <param name="parameterBase">The <see cref="ParameterBase"/></param>
        public void UpdateParameterBasePosition(ParameterBase parameterBase)
        {
            var oldContainer = this.parameterOrOverrideContainerMap[parameterBase];
            var newContainer = parameterBase.Group;
            var associatedRow = this.parameterOrOverrideBaseCache[parameterBase];

            if ((newContainer != null) && (oldContainer == null))
            {
                this.ContainedRows.RemoveWithoutDispose(associatedRow);
                this.parameterGroupCache[newContainer].ContainedRows.SortedInsert(associatedRow, ParameterGroupRowViewModel.ChildRowComparer);
                this.parameterOrOverrideContainerMap[parameterBase] = newContainer;
            }
            else if ((newContainer == null) && (oldContainer != null))
            {
                this.parameterGroupCache[oldContainer].ContainedRows.RemoveWithoutDispose(associatedRow);
                this.ContainedRows.SortedInsert(associatedRow, ChildRowComparer);
                this.parameterOrOverrideContainerMap[parameterBase] = null;
            }
            else if ((newContainer != null) && (oldContainer != null) && (newContainer != oldContainer))
            {
                this.parameterGroupCache[oldContainer].ContainedRows.RemoveWithoutDispose(associatedRow);
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

            if ((newContainer != null) && (oldContainer == null))
            {
                this.ContainedRows.RemoveWithoutDispose(associatedRow);
                this.parameterGroupCache[newContainer].ContainedRows.SortedInsert(associatedRow, ParameterGroupRowViewModel.ChildRowComparer);
                this.parameterGroupContainment[parameterGroup] = newContainer;
            }
            else if ((newContainer == null) && (oldContainer != null))
            {
                this.parameterGroupCache[oldContainer].ContainedRows.RemoveWithoutDispose(associatedRow);
                this.ContainedRows.SortedInsert(associatedRow, ChildRowComparer);
                this.parameterGroupContainment[parameterGroup] = null;
            }
            else if ((newContainer != null) && (oldContainer != null) && (newContainer != oldContainer))
            {
                this.parameterGroupCache[oldContainer].ContainedRows.RemoveWithoutDispose(associatedRow);
                this.parameterGroupCache[newContainer].ContainedRows.SortedInsert(associatedRow, ParameterGroupRowViewModel.ChildRowComparer);
                this.parameterGroupContainment[parameterGroup] = newContainer;
            }
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
            if (dropInfo.Payload is ElementDefinition elementDefinition)
            {
                this.DragOver(dropInfo, elementDefinition);
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
            if (dropInfo.Payload is ElementDefinition elementDefinition)
            {
                await this.Drop(dropInfo, elementDefinition);
            }
        }

        /// <summary>
        /// Update the <see cref="ThingStatus"/> property
        /// </summary>
        protected override void UpdateThingStatus()
        {
            this.ThingStatus = new ThingStatus(this.Thing);
        }

        /// <summary>
        /// Initializes the subscriptions
        /// </summary>
        protected override void InitializeSubscriptions()
        {
            base.InitializeSubscriptions();
            
            Func<ObjectChangedEvent, bool> discriminator = objectChange => objectChange.EventKind == EventKind.Updated;

            Action<ObjectChangedEvent> elementDefAction = _ =>
            {
                this.UpdateElementDefinitionProperties();
                this.UpdateModelCode();
            };

            Action<ElementUsageHighlightEvent> highlightUsageAction = _ => this.HighlightEventHandler();

            if (this.AllowMessageBusSubscriptions)
            {
                var elementDefListener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing.ElementDefinition)
                    .Where(discriminator)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(elementDefAction);

                this.Disposables.Add(elementDefListener);

                var highlightUsageListener = CDPMessageBus.Current.Listen<ElementUsageHighlightEvent>(this.Thing.ElementDefinition)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(highlightUsageAction);

                this.Disposables.Add(highlightUsageListener);
            }
            else
            {
                var elementDefObserver = CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(ElementDefinition));
                this.Disposables.Add(
                    this.MessageBusHandler.GetHandler<ObjectChangedEvent>()
                    .RegisterEventHandler(elementDefObserver, new ObjectChangedMessageBusEventHandlerSubscription(this.Thing.ElementDefinition, discriminator, elementDefAction)));

                var highlightUsageObserver = CDPMessageBus.Current.Listen<ElementUsageHighlightEvent>();
                this.Disposables.Add(
                    this.MessageBusHandler.GetHandler<ElementUsageHighlightEvent>()
                    .RegisterEventHandler(highlightUsageObserver, new MessageBusEventHandlerSubscription<ElementUsageHighlightEvent>(x => x.ElementDefinition.Equals(this.Thing.ElementDefinition), highlightUsageAction)));
            }
        }

        /// <summary>
        /// Update the tooltip
        /// </summary>
        protected override void UpdateTooltip()
        {
            if (this.Option == null)
            {
                return;
            }

            var sb = new StringBuilder();

            string owner;

            if (this.Thing.Owner != null)
            {
                owner = this.Thing.Owner.ShortName;
            }
            else
            {
                owner = "NA";
                logger.Debug($"Owner if {this.Thing.ClassKind} null");
            }

            sb.AppendLine($"Owner: {owner}");

            var categories = this.Thing.Category.Any() ? string.Join(" ", this.Thing.Category.OrderBy(x => x.ShortName).Select(x => x.ShortName)) : "-";
            sb.AppendLine($"Category: {categories}");

            var elementDefCategories = this.Thing.ElementDefinition.Category.OrderBy(x => x.ShortName).Select(x => x.ShortName);
            var elementDefCategoriesText = this.Thing.ElementDefinition.Category.Any() ? string.Join(" ", elementDefCategories) : "-";
            sb.AppendLine($"ED Category: {elementDefCategoriesText }");

            sb.AppendLine($"Model Code: {this.Path()}");

            var definition = this.Thing.Definition.FirstOrDefault();

            sb.AppendLine(
                definition == null
                    ? $"Definition : -"
                    : $"Definition [{definition.LanguageCode}]: {definition.Content}");

            this.Tooltip = sb.ToString();
        }

        /// <summary>
        /// Computes the path of the row-view-model
        /// </summary>
        /// <returns></returns>
        public string Path()
        {
            if (this.ContainerViewModel is ElementDefinitionRowViewModel elementDefinitionRowViewModel)
            {
                return $"{elementDefinitionRowViewModel.Path()}.{this.Thing.ShortName}";
            }

            if (this.ContainerViewModel is ElementUsageRowViewModel elementUsageRowViewModel)
            {
                return $"{elementUsageRowViewModel.Path()}.{this.Thing.ShortName}";
            }

            return string.Empty;
        }

        /// <summary>
        /// The object changed event handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
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

        /// <summary>
        /// Update the properties related to this <see cref="ElementUsage"/>
        /// </summary>
        private void UpdateProperties()
        {
            this.UpdateThingStatus();
            this.Name = this.Thing.Name + " : " + this.ElementDefinition.Name;
            this.ShortName = this.Thing.ShortName + " : " + this.ElementDefinition.ShortName;

            var builder = new CategoryStringBuilder()
                                .AddCategories("EU", this.Thing.Category)
                                .AddCategories("ED", this.Thing.ElementDefinition.Category);

            this.Category = builder.GetCategories().Distinct();
            this.DisplayCategory = builder.Build();

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
            this.UpdateProperties();
            this.UpdateTooltip();
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
                IDisposable listener;
                
                Func<ObjectChangedEvent, bool> discriminator = 
                    objectChange => 
                    objectChange.EventKind == EventKind.Updated 
                    && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber;

                Action<ObjectChangedEvent> action = x => this.UpdateOptionDependentElementUsage((ElementUsage)x.ChangedThing);

                if (this.AllowMessageBusSubscriptions)
                {
                    listener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(elementUsage)
                        .Where(discriminator)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(action);
                }
                else
                {
                    var elementUsageObserver = CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(ElementUsage));
                    listener = this.MessageBusHandler.GetHandler<ObjectChangedEvent>().RegisterEventHandler(elementUsageObserver, new ObjectChangedMessageBusEventHandlerSubscription(elementUsage, discriminator, action));
                }

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
                this.ContainedRows.RemoveAndDispose(row);
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
                    this.ContainedRows.RemoveAndDispose(row);
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
                    this.ContainedRows.RemoveWithoutDispose(this.parameterOrOverrideBaseCache[parameterOrOverride]);
                }
                else
                {
                    this.parameterGroupCache[group].ContainedRows.RemoveWithoutDispose(this.parameterOrOverrideBaseCache[parameterOrOverride]);
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

                if (parameterOrOverride is ParameterOverride parameterOverride)
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

        /// <summary>
        /// Set the <see cref="IDropInfo.Effects"/> when the payload is an <see cref="ElementDefinition"/>
        /// </summary>
        /// <param name="dropinfo">The <see cref="IDropInfo"/></param>
        /// <param name="elementDefinition">The <see cref="ElementDefinition"/> in the payload</param>
        private void DragOver(IDropInfo dropinfo, ElementDefinition elementDefinition)
        {
            if (!this.PermissionService.CanWrite(ClassKind.ElementUsage, this.Thing.Container))
            {
                dropinfo.Effects = DragDropEffects.None;
                return;
            }

            if (elementDefinition.TopContainer == this.Thing.TopContainer)
            {
                // prevent circular model
                dropinfo.Effects = elementDefinition.HasUsageOf(this.Thing.ElementDefinition)
                    ? DragDropEffects.None
                    : DragDropEffects.Copy;

                return;
            }

            dropinfo.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// Handle the drop of a <see cref="ElementDefinition"/>
        /// </summary>
        /// <param name="dropInfo">The <see cref="IDropInfo"/> containing the payload</param>
        /// <param name="elementDefinition">The <see cref="ElementDefinition"/></param>
        private async Task Drop(IDropInfo dropInfo, ElementDefinition elementDefinition)
        {
            try
            {
                var currentDomain = this.Session.QuerySelectedDomainOfExpertise(this.Thing.GetContainerOfType<Iteration>());

                if (elementDefinition.TopContainer == this.Thing.TopContainer)
                {
                    await this.ThingCreator.CreateElementUsage(this.Thing.ElementDefinition, elementDefinition, currentDomain, this.Session);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                this.ErrorMsg = ex.Message;
            }
        }
    }
}
