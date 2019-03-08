// -------------------------------------------------------------------------------------------------
// <copyright file="RequirementRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
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
    using CDP4Common.Types;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;
    using Utils;
    using FolderRowViewModel = CDP4Composition.FolderRowViewModel;

    /// <summary>
    /// the row-view-model representing a <see cref="Requirement"/>
    /// </summary>
    public class RequirementRowViewModel : CDP4CommonView.RequirementRowViewModel, IDropTarget
    {
        #region private field
        /// <summary>
        /// The folder row containing the <see cref="SimpleParameterValue"/>
        /// </summary>
        private readonly FolderRowViewModel simpleParameters;

        /// <summary>
        /// The folder row containing the <see cref="parametricConstraints"/>
        /// </summary>
        private readonly FolderRowViewModel parametricConstraints;

        /// <summary>
        /// Backing field for <see cref="Definition"/>
        /// </summary>
        private string definition;

        /// <summary>
        /// Backing field for <see cref="Categories"/>
        /// </summary>
        private string categories;

        /// <summary>
        /// The <see cref="Definition"/> to display
        /// </summary>
        private Definition definitionThing;

        /// <summary>
        /// The subscription on <see cref="definition"/>
        /// </summary>
        private IDisposable definitionSubscription;

        /// <summary>
        /// Backing field for <see cref="CategoryList"/>
        /// </summary>
        private List<Category> categoryList;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementRowViewModel"/> class
        /// </summary>
        /// <param name="req">The requirement</param>
        /// <param name="session">The Session</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public RequirementRowViewModel(Requirement req, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(req, session, containerViewModel)
        {
            this.simpleParameters = new FolderRowViewModel("Simple Parameter Values", "Simple Parameter Values", this.Session, this);
            this.parametricConstraints = new FolderRowViewModel("Parametric Constraints", "Parametric Constraints", this.Session, this);

            this.ContainedRows.Add(this.simpleParameters);
            this.ContainedRows.Add(this.parametricConstraints);

            this.UpdateProperties();
        }

        #region properties
        /// <summary>
        /// Gets the definition of the current requirement
        /// </summary>
        public string Definition
        {
            get { return this.definition; }
            protected set { this.RaiseAndSetIfChanged(ref this.definition, value); }
        }

        /// <summary>
        /// Gets the categories name
        /// </summary>
        public string Categories
        {
            get { return this.categories; }
            private set { this.RaiseAndSetIfChanged(ref this.categories, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Category"/>
        /// </summary>
        public List<Category> CategoryList
        {
            get { return this.categoryList; }
            set { this.RaiseAndSetIfChanged(ref this.categoryList, value); }
        }
        #endregion

        #region Drag, Drop

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
                this.DragOver(category, dropInfo);
                return;
            }

            var tuple = dropInfo.Payload as Tuple<ParameterType, MeasurementScale>;
            if (tuple != null)
            {
                this.DragOver(tuple, dropInfo);
                return;
            }

            var req = dropInfo.Payload as Requirement;
            if (req != null)
            {
                this.DragOver(req, dropInfo);
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
                await this.Drop(category);
                return;
            }

            var tuple = dropInfo.Payload as Tuple<ParameterType, MeasurementScale>;
            if (tuple != null)
            {
                await this.Drop(tuple);
                return;
            }

            var req = dropInfo.Payload as Requirement;
            if (req != null)
            {
                await this.Drop(req, dropInfo);
                return;
            }
        }

        /// <summary>
        /// Handle the drag-over of a <see cref="Category"/>
        /// </summary>
        /// <param name="category">The <see cref="Category"/></param>
        /// <param name="dropInfo">The <see cref="IDropInfo"/></param>
        private void DragOver(Category category, IDropInfo dropInfo)
        {
            if (!this.Session.PermissionService.CanWrite(this.Thing))
            {
                logger.Info("Permission denied to perform the drop.");
                dropInfo.Effects = DragDropEffects.None;
                return;
            }

            if (!category.PermissibleClass.Contains(this.Thing.ClassKind))
            {
                logger.Info("The Category {0} can not be applied to this kind of Thing", category.ShortName);
                dropInfo.Effects = DragDropEffects.None;
                return;
            }

            if (this.Thing.Category.Contains(category))
            {
                logger.Info("The Category {0} has already been applied to this Thing", category.ShortName);
                dropInfo.Effects = DragDropEffects.None;
                return;
            }

            dropInfo.Effects = DragDropEffects.Copy;
        }

        /// <summary>
        /// Handle the drag-over of a <see cref="Category"/>
        /// </summary>
        /// <param name="tuple">The <see cref="Tuple{ParameterType, MeasurementScale}"/></param>
        /// <param name="dropInfo">The <see cref="IDropInfo"/></param>
        private void DragOver(Tuple<ParameterType, MeasurementScale> tuple, IDropInfo dropInfo)
        {
            if (!this.Session.PermissionService.CanWrite(ClassKind.SimpleParameterValue, this.Thing))
            {
                logger.Info("Permission denied to create a SimpleParameterValue.");
                dropInfo.Effects = DragDropEffects.None;
                return;
            }

            if (this.Thing.ParameterValue.Select(x => x.ParameterType).Contains(tuple.Item1))
            {
                logger.Info("A SimpleParameterValue with this ParameterType already exists.");
                dropInfo.Effects = DragDropEffects.None;
                return;
            }

            dropInfo.Effects = DragDropEffects.Copy;
        }

        /// <summary>
        /// Handle the drag-over of a <see cref="Requirement"/>
        /// </summary>
        /// <param name="req">The <see cref="Requirement"/></param>
        /// <param name="dropInfo">The <see cref="IDropInfo"/></param>
        private void DragOver(Requirement req, IDropInfo dropInfo)
        {
            if (req.Container != this.Thing.Container)
            {
                dropInfo.Effects = DragDropEffects.None;
                return;
            }

            // moving requirements before the current one
            dropInfo.Effects = DragDropEffects.Move;
        }

        /// <summary>
        /// Handles the drop action of a <see cref="Category"/>
        /// </summary>
        /// <param name="category">The dropped <see cref="Category"/></param>
        private async Task Drop(Category category)
        {
            var clone = this.Thing.Clone(false);
            clone.Category.Add(category);
            await this.DalWrite(clone);
        }

        /// <summary>
        /// Handles the drop action of a <see cref="Tuple{ParameterType, MeasurementScale}"/>
        /// </summary>
        /// <param name="tuple">The <see cref="Tuple{ParameterType, MeasurementScale}"/></param>
        private async Task Drop(Tuple<ParameterType, MeasurementScale> tuple)
        {
            var clone = this.Thing.Clone(false);

            var parameterValue = new SimpleParameterValue();
            parameterValue.ParameterType = tuple.Item1;
            parameterValue.Scale = tuple.Item2;
            parameterValue.Value = new ValueArray<string>(new [] {"-"});

            clone.ParameterValue.Add(parameterValue);

            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(this.Thing));
            transaction.Create(parameterValue);
            transaction.CreateOrUpdate(clone);

            await this.DalWrite(transaction);
        }

        /// <summary>
        /// Handles the drop action of a <see cref="Requirement"/>
        /// </summary>
        /// <param name="req">The <see cref="Requirement"/></param>
        /// <param name="dropinfo">The dropinfo</param>
        private async Task Drop(Requirement req, IDropInfo dropinfo)
        {
            var model = (EngineeringModel)this.Thing.TopContainer;
            var orderPt = OrderHandlerService.GetOrderParameterType(model);

            if (orderPt == null)
            {
                return;
            }

            var orderService = new RequirementOrderHandlerService(this.Session, orderPt);
            var transaction = orderService.Insert(req, this.Thing, dropinfo.IsDroppedAfter ? InsertKind.InsertAfter : InsertKind.InsertBefore);
            await this.Session.Write(transaction.FinalizeTransaction());
        }

        #endregion Drag, Drop

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

            var requirementsSpecificationRowViewModel = (RequirementsSpecificationRowViewModel)this.ContainerViewModel;
            var containerIsDeprecatedSubscription = requirementsSpecificationRowViewModel.WhenAnyValue(vm => vm.IsDeprecated)
                .Subscribe(_ => this.UpdateIsDeprecatedDerivedFromContainerRequirementsSpecification());
            this.Disposables.Add(containerIsDeprecatedSubscription);
        }

        /// <summary>
        /// Updates the IsDeprecated property based on the value of the container <see cref="RequirementsSpecification"/>
        /// </summary>
        private void UpdateIsDeprecatedDerivedFromContainerRequirementsSpecification()
        {
            var requirementsSpecification = (RequirementsSpecification)this.Thing.Container;
            if (requirementsSpecification.IsDeprecated)
            {
                this.IsDeprecated = true;

                if (this.simpleParameters != null)
                {
                    this.simpleParameters.IsDeprecated = true;
                }

                if (this.parametricConstraints != null)
                {
                    this.parametricConstraints.IsDeprecated = true;
                }
            }
            else
            {
                this.IsDeprecated = this.Thing.IsDeprecated;

                if (this.simpleParameters != null)
                {
                    this.simpleParameters.IsDeprecated = this.Thing.IsDeprecated;
                }

                if (this.parametricConstraints != null)
                {
                    this.parametricConstraints.IsDeprecated = this.Thing.IsDeprecated;
                }
            }
        }

        
        /// <summary>
        /// Update the nodes of this requirement with the current <see cref="ParametricConstraint"/>
        /// </summary>
        private void UpdateParameterConstraintRows()
        {
            var current = this.ContainedRows[1].ContainedRows.Select(x => x.Thing).OfType<ParametricConstraint>().ToList();
            var updated = this.Thing.ParametricConstraint;

            var added = updated.Except(current).ToList();
            var removed = current.Except(updated).ToList();

            foreach (var parametricConstraint in added)
            {
                this.AddConstraint(parametricConstraint);
            }

            foreach (var parametricConstraint in removed)
            {
                this.RemoveConstraint(parametricConstraint);
            }
        }

        /// <summary>
        /// Updates the <see cref="SimpleParameterValue"/>s contained by this <see cref="Requirement"/>.
        /// </summary>
        private void UpdateValues()
        {            
            var current = this.ContainedRows[0].ContainedRows.Select(x => x.Thing).OfType<SimpleParameterValue>().ToList();
            var updated = this.Thing.ParameterValue;

            var added = updated.Except(current).ToList();
            var removed = current.Except(updated).ToList();

            foreach (var value in added)
            {
                this.AddValue(value);
            }

            foreach (var value in removed)
            {
                this.RemoveValue(value);
            }
        }

        /// <summary>
        /// Add a row representing a new <see cref="SimpleParameterValue"/>
        /// </summary>
        /// <param name="value">The associated <see cref="SimpleParameterValue"/></param>
        private void AddValue(SimpleParameterValue value)
        {
            var row = new SimpleParameterValueRowViewModel(value, this.Session, this);
            this.simpleParameters.ContainedRows.Add(row);
        }

        /// <summary>
        /// Removes a value row
        /// </summary>
        /// <param name="value">The associated <see cref="SimpleParameterValue"/> to remove</param>
        private void RemoveValue(SimpleParameterValue value)
        {
            var row = this.simpleParameters.ContainedRows.SingleOrDefault(x => x.Thing == value);
            if (row != null)
            {
                this.simpleParameters.ContainedRows.Remove(row);
                row.Dispose();
            }

            if (this.definitionSubscription != null)
            {
                this.definitionSubscription.Dispose();
            }
        }

        /// <summary>
        /// Add a row representing a new <see cref="ParametricConstraint"/>
        /// </summary>
        /// <param name="constraint">The associated <see cref="ParametricConstraint"/></param>
        private void AddConstraint(ParametricConstraint constraint)
        {
            var row = new ParametricConstraintRowViewModel(constraint, this.Session, this);
            this.parametricConstraints.ContainedRows.Add(row);
        }

        /// <summary>
        /// Removes a row
        /// </summary>
        /// <param name="constraint">The associated <see cref="ParametricConstraint"/> to remove</param>
        private void RemoveConstraint(ParametricConstraint constraint)
        {
            var row = this.parametricConstraints.ContainedRows.SingleOrDefault(x => x.Thing == constraint);
            if (row != null)
            {
                this.parametricConstraints.ContainedRows.Remove(row);
                row.Dispose();
            }
        }

        /// <summary>
        /// Update the values of the row
        /// </summary>
        private void UpdateProperties()
        {
            this.UpdateThingStatus();
            this.SetDefinitionContent();
            this.UpdateParameterConstraintRows();
            this.UpdateValues();
            this.Categories = string.Join(", ", this.Thing.Category.Select(x => x.Name));
            this.CategoryList = new List<Category>(this.Thing.Category.OrderBy(x => x.Name));

            this.UpdateIsDeprecatedDerivedFromContainerRequirementsSpecification();
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent"/> handler for this <see cref="definition"/>
        /// </summary>
        /// <param name="defEvent">The <see cref="ObjectChangedEvent"/></param>
        private void OnDefinitionUpdate(ObjectChangedEvent defEvent)
        {
            if (defEvent.EventKind == EventKind.Removed)
            {
                this.definition = null;
                this.definitionSubscription.Dispose();
            }

            this.SetDefinitionContent();
        }

        /// <summary>
        /// Sets the value of the <see cref="Definition"/> property.
        /// </summary>
        private void SetDefinitionContent()
        {
            if (this.definitionThing == null)
            {
                this.definitionThing = this.Thing.Definition.FirstOrDefault();
                if (this.definitionThing == null)
                {
                    return;
                }

                this.definitionSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.definitionThing)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.OnDefinitionUpdate);
            }

            this.Definition = this.TrunctateRequirementsDefinitionText(this.definitionThing.Content);
        }

        /// <summary>
        /// Truncates the text of the requirement when it contains a carriage return or new line
        /// </summary>
        /// <returns>
        /// A string that is truncated after the first carriage return or new line
        /// </returns>
        private string TrunctateRequirementsDefinitionText(string multilineText = null)
        {
            if (string.IsNullOrEmpty(multilineText))
            {
                return String.Empty;
            }

            var lines = multilineText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            if (lines.Length == 1)
            {
                return lines[0];
            }
            else
            {
                return string.Format("{0}...", lines[0]);
            }
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
    }
}