// ------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

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

    using CDP4CommonView.ViewModels;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4ProductTree.Comparers;

    using Microsoft.Practices.ServiceLocation;

    using ReactiveUI;

    /// <summary>
    /// The row representation of a <see cref="ElementDefinition"/>
    /// </summary>
    public class ElementDefinitionRowViewModel : CDP4CommonView.ElementDefinitionRowViewModel, IParameterRowContainer, IDropTarget
    {
        /// <summary>
        /// The <see cref="IComparer{T}"/>
        /// </summary>
        private static readonly IComparer<IRowViewModelBase<Thing>> ChildRowComparer = new ElementDefinitionChildRowComparer();

        /// <summary>
        /// A list of <see cref="ParameteRowViewModel"/> representing all <see cref="Parameter"/> contained by this <see cref="ElementDefinition"/>
        /// </summary>
        private Dictionary<Parameter, ParameterRowViewModel> parameterCache;

        /// <summary>
        /// A Dictionary containing the parameter - group containment
        /// </summary>
        private Dictionary<Parameter, ParameterGroup> parameterContainerMap;

        /// <summary>
        /// A list of all rows representing all <see cref="ParameterGroup"/> contained by this <see cref="ElementDefinition"/>
        /// </summary>
        private Dictionary<ParameterGroup, ParameterGroupRowViewModel> parameterGroupCache;

        /// <summary>
        /// A parameter group - parameter group container mapping
        /// </summary>
        private Dictionary<ParameterGroup, ParameterGroup> parameterGroupContainment;

        /// <summary>
        /// The listener cache associated with a <see cref="ElementUsage"/>
        /// </summary>
        private readonly Dictionary<ElementUsage, IDisposable> elementUsageListenerCache = new Dictionary<ElementUsage, IDisposable>();

        /// <summary>
        /// The selected <see cref="Option"/> for the browser this row is contained in
        /// </summary>
        public readonly Option Option;

        /// <summary>
        /// Backing field for <see cref="ModelCode"/>
        /// </summary>
        private string modelCode;

        /// <summary>
        /// The backing field for <see cref="ThingCreator"/>
        /// </summary>
        private IThingCreator thingCreator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionRowViewModel"/> class
        /// </summary>
        /// <param name="elementDefinition">The <see cref="ElementDefinition"/> associated with this row</param>
        /// <param name="option">The selected <see cref="Option"/> for the browser this row is contained in</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public ElementDefinitionRowViewModel(ElementDefinition elementDefinition, Option option, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(elementDefinition, session, containerViewModel)
        {
            this.parameterCache = new Dictionary<Parameter, ParameterRowViewModel>();
            this.parameterContainerMap = new Dictionary<Parameter, ParameterGroup>();
            this.parameterGroupCache = new Dictionary<ParameterGroup, ParameterGroupRowViewModel>();
            this.parameterGroupContainment = new Dictionary<ParameterGroup, ParameterGroup>();
            this.Option = option;
            this.UpdateProperties();
            this.UpdateTooltip();
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
            var parameter = parameterBase as Parameter;

            if (parameter == null)
            {
                throw new InvalidOperationException("An element definition row may not contain parameter override rows.");
            }

            var oldContainer = this.parameterContainerMap[parameter];
            var newContainer = parameterBase.Group;
            var associatedRow = this.parameterCache[parameter];

            if (newContainer != null && oldContainer == null)
            {
                this.ContainedRows.Remove(associatedRow);
                this.parameterGroupCache[newContainer].ContainedRows.SortedInsert(associatedRow, ParameterGroupRowViewModel.ChildRowComparer);
                this.parameterContainerMap[parameter] = newContainer;
            }
            else if (newContainer == null && oldContainer != null)
            {
                this.parameterGroupCache[oldContainer].ContainedRows.Remove(associatedRow);
                this.ContainedRows.SortedInsert(associatedRow, ChildRowComparer);
                this.parameterContainerMap[parameter] = null;
            }
            else if (newContainer != null && oldContainer != null && newContainer != oldContainer)
            {
                this.parameterGroupCache[oldContainer].ContainedRows.Remove(associatedRow);
                this.parameterGroupCache[newContainer].ContainedRows.SortedInsert(associatedRow, ParameterGroupRowViewModel.ChildRowComparer);
                this.parameterContainerMap[parameter] = newContainer;
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
        /// Update this <see cref="Tooltip"/>
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

            sb.AppendLine($"Model Code: {this.Path()}");

            var definition = this.Thing.Definition.FirstOrDefault();

            sb.AppendLine(
                definition == null
                    ? "Definition : -"
                    : $"Definition [{definition.LanguageCode}]: {definition.Content}");

            this.Tooltip = sb.ToString();
        }

        /// <summary>
        /// Computes the path of the row-view-model
        /// </summary>
        /// <returns></returns>
        public string Path()
        {
            return $"{this.Option.ShortName}.{this.Thing.ShortName}";
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
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// a value indicating whether the class is being disposed of
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            foreach (var parameter in this.parameterCache)
            {
                parameter.Value.Dispose();
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
        /// Update the properties
        /// </summary>
        private void UpdateProperties()
        {
            this.UpdateThingStatus();

            this.UpdateOwnerNameAndShortName();
            this.PopulateElementUsages();
            this.PopulateParameterGroups();
            this.PopulateParameter();
            this.ModelCode = this.Thing.ModelCode();
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
        /// Populate the <see cref="ElementUsage"/> property
        /// </summary>
        private void PopulateElementUsages()
        {
            var existingUsages = this.ContainedRows.OfType<ElementUsageRowViewModel>().Select(x => x.Thing).ToList();
            var currentUsages = this.Thing.ContainedElement;

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
        /// Populates the Parameter group rows
        /// </summary>
        private void PopulateParameterGroups()
        {
            var definedGroups = this.Thing.ParameterGroup;

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
        private void PopulateParameter()
        {
            var definedParameters = this.Thing.Parameter;

            // remove deleted parameters
            var oldparameters = this.parameterCache.Keys.Except(definedParameters).ToList();

            foreach (var parameter in oldparameters)
            {
                if (parameter.Group == null)
                {
                    this.ContainedRows.Remove(this.parameterCache[parameter]);
                }
                else
                {
                    this.parameterGroupCache[parameter.Group].ContainedRows.Remove(this.parameterCache[parameter]);
                }

                this.parameterCache[parameter].Dispose();
                this.parameterCache.Remove(parameter);
                this.parameterContainerMap.Remove(parameter);
            }

            // create new parameters rows
            var newparameters = definedParameters.Except(this.parameterCache.Keys).ToList();

            foreach (var parameter in newparameters)
            {
                var row = new ParameterRowViewModel(parameter, this.Option, this.Session, this);
                this.parameterCache.Add(parameter, row);
                this.parameterContainerMap.Add(parameter, parameter.Group);
            }

            // add the new parameter in the right position in the tree
            foreach (var parameter in newparameters)
            {
                if (parameter.Group == null)
                {
                    this.ContainedRows.SortedInsert(this.parameterCache[parameter], ChildRowComparer);
                }
                else
                {
                    var container = this.parameterGroupCache[parameter.Group];
                    container.ContainedRows.SortedInsert(this.parameterCache[parameter], ParameterGroupRowViewModel.ChildRowComparer);
                }
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

            //checking for cyclic relation between element usages and element definition
            var iteration = this.Thing.Container as Iteration;

            foreach (var element in iteration.Element)
            {
                if (element.Name != null && elementUsage.Name != null)
                {
                    if (element.Name == elementUsage.Name)
                    {
                        if (element.HasUsageOf(this.Thing))
                        {
                            var title = "Error exists in Model";

                            var message = "A cyclic reference problem is encountered with the following items:" +
                                          "\n" +
                                          this.Thing.ClassKind + ": " + this.Thing.Name + "(" + this.Thing.ShortName +
                                          ")" + "\n" + elementUsage.Container.ClassKind + ": " + elementUsage.Name +
                                          "(" + elementUsage.ShortName + ")" + "\n" +
                                          "All cyclic reference problems have to be solved by a person with the appropriate rights.";

                            var dialogOk = new OkDialogViewModel(title, message);
                            var dialogResult = this.dialogNavigationService.NavigateModal(dialogOk);

                            logger.Error(message);

                            if (dialogResult.Result.HasValue)
                            {
                                return;
                            }
                        }
                    }
                }
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
                dropinfo.Effects = elementDefinition.HasUsageOf(this.Thing)
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
                    await this.ThingCreator.CreateElementUsage(this.Thing, elementDefinition, currentDomain, this.Session);
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
