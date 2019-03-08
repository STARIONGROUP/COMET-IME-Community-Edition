// -------------------------------------------------------------------------------------------------
// <copyright file="RequirementsSpecificationRowViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;
    using Utils;

    /// <summary>
    /// A row-view-model that represents a <see cref="RequirementsSpecification"/> in the requirement browser
    /// </summary>
    public class RequirementsSpecificationRowViewModel : RequirementContainerRowViewModel<RequirementsSpecification>, IDropTarget
    {
        /// <summary>
        /// Cache for the <see cref="Requirement"/> currently contained
        /// </summary>
        private Dictionary<Requirement, IRowViewModelBase<Requirement>> requirementCache;

        /// <summary>
        /// Cache for the requirement and their container
        /// </summary>
        internal Dictionary<Requirement, RequirementsGroup> requirementContainerGroupCache;

        /// <summary>
        /// The <see cref="Requirement"/> update listener cache
        /// </summary>
        private Dictionary<Requirement, IDisposable> listenerCache;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsSpecificationRowViewModel"/> class
        /// </summary>
        /// <param name="reqContainer">The <see cref="RequirementsSpecification"/></param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public RequirementsSpecificationRowViewModel(RequirementsSpecification reqContainer, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(reqContainer, session, containerViewModel)
        {
            this.requirementCache = new Dictionary<Requirement, IRowViewModelBase<Requirement>>();
            this.requirementContainerGroupCache = new Dictionary<Requirement, RequirementsGroup>();
            this.listenerCache = new Dictionary<Requirement, IDisposable>();
            this.GroupCache = new Dictionary<RequirementsGroup, IRowViewModelBase<Thing>>();
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets the <see cref="RequirementsGroup"/> cache
        /// </summary>
        public Dictionary<RequirementsGroup, IRowViewModelBase<Thing>> GroupCache { get; private set; }
        
        /// <summary>
        /// Gets the content of the first definition.
        /// </summary>
        public new string Definition
        {
            get
            {
                if (this.Thing == null)
                {
                    return string.Empty;
                }

                var firstOrDefault = this.Thing.Definition.FirstOrDefault();
                return firstOrDefault != null ? firstOrDefault.Content : string.Empty;
            }
        }

        #region Private Methods
        /// <summary>
        /// Update the <see cref="Requirement"/> nodes
        /// </summary>
        private void UpdateRequirementRows()
        {
            var current = this.requirementCache.Keys;
            var updated = this.Thing.Requirement;

            var added = updated.Except(current).ToList();
            var removed = current.Except(updated).ToList();

            foreach (var requirement in added)
            {
                this.AddRequirementRow(requirement);
            }

            foreach (var requirement in removed)
            {
                this.RemoveRequirementRow(requirement);
            }
        }

        /// <summary>
        /// Add a requirement rows
        /// </summary>
        /// <param name="req">The <see cref="Requirement"/> to add</param>
        private void AddRequirementRow(Requirement req)
        {
            var row = new RequirementRowViewModel(req, this.Session, this);
            this.requirementCache[req] = row;
            this.requirementContainerGroupCache[req] = req.Group;

            if (req.Group == null)
            {
                this.ContainedRows.SortedInsert(row, ChildRowComparer);
            }
            else
            {
                IRowViewModelBase<Thing> groupRow;
                if (this.GroupCache.TryGetValue(req.Group, out groupRow))
                {
                    groupRow.ContainedRows.SortedInsert(row, ChildRowComparer);
                }
                else
                {
                    logger.Error("The requirement group with iid {0} could not be found", req.Group.Iid);
                    this.ContainedRows.SortedInsert(row, ChildRowComparer);
                }
            }

            var updateListener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(req)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => this.UpdateRequirementRow((Requirement)x.ChangedThing, false));

            var orderPt = OrderHandlerService.GetOrderParameterType((EngineeringModel)this.Thing.TopContainer);
            if (orderPt != null)
            {
                var orderListener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(SimpleParameterValue))
                    .Where(objectChange => ((SimpleParameterValue)objectChange.ChangedThing).ParameterType == orderPt && this.Thing.Requirement.Any(r => r.ParameterValue.Contains(objectChange.ChangedThing)))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => this.UpdateRequirementRow((Requirement)x.ChangedThing.Container, true));

                this.Disposables.Add(orderListener);
            }

            this.Disposables.Add(updateListener);
            this.listenerCache[req] = updateListener;
        }

        /// <summary>
        /// Remove a requirement row
        /// </summary>
        /// <param name="req">The <see cref="Requirement"/> to remove</param>
        private void RemoveRequirementRow(Requirement req)
        {
            IRowViewModelBase<Requirement> reqRow;
            if (!this.requirementCache.TryGetValue(req, out reqRow))
            {
                return;
            }

            var previousGroup = this.requirementContainerGroupCache[req];
            if (previousGroup != null)
            {
                IRowViewModelBase<Thing> groupRow;
                if (this.GroupCache.TryGetValue(previousGroup, out groupRow))
                {
                    groupRow.ContainedRows.Remove(reqRow);
                }
            }
            else
            {
                this.ContainedRows.Remove(reqRow);
            }

            reqRow.Dispose();
            this.requirementCache.Remove(req);
            this.requirementContainerGroupCache.Remove(req);

            this.listenerCache[req].Dispose();
            this.listenerCache.Remove(req);
        }

        /// <summary>
        /// Update the requirement row within the <see cref="RequirementsGroup"/> rows
        /// </summary>
        /// <param name="req">The <see cref="Requirement"/></param>
        /// <param name="newOrder">A value indicating whether the order may have changed</param>
        private void UpdateRequirementRow(Requirement req, bool newOrder)
        {
            if (req.Container != this.Thing)
            {
                return;
            }

            var currentGroup = this.requirementContainerGroupCache[req];

            var reqRow = this.requirementCache[req];
            var currentContainerRow = currentGroup != null ? this.GroupCache[currentGroup] : this;
            
            IRowViewModelBase<Thing> updatedContainerRow;
            if (req.Group != null)
            {
                if (!this.GroupCache.TryGetValue(req.Group, out updatedContainerRow))
                {
                    logger.Error("The requirement-group {0} could not be found in the Specification {1}", req.Group.Name, this.Thing.Name);
                    return;
                }
            }
            else
            {
                updatedContainerRow = this;
            }

            if (currentContainerRow == updatedContainerRow && !newOrder)
            {
                return;
            }

            currentContainerRow.ContainedRows.Remove(reqRow);
            updatedContainerRow.ContainedRows.SortedInsert(reqRow, ChildRowComparer);
            this.requirementContainerGroupCache[req] = req.Group;
        }

        /// <summary>
        /// Update the properties of this row
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();

            this.IsDeprecated = this.Thing.IsDeprecated;

            this.UpdateRequirementGroupRows();
            this.UpdateRequirementRows();
        }
        #endregion

        #region Overriden Methods
        /// <summary>
        /// The object changed event handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }
        #endregion

        #region IDropTarget

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
            var tuple = dropInfo.Payload as Tuple<ParameterType, MeasurementScale>;
            if (tuple != null)
            {
                this.DragOver(tuple, dropInfo);
                return;
            }

            var requirement = dropInfo.Payload as Requirement;
            if (requirement != null)
            {
                var canDropRequirement = this.PermissionService.CanWrite(ClassKind.Requirement, this.Thing);
                if (!canDropRequirement || requirement.IDalUri.ToString() != this.Session.DataSourceUri)
                {
                    dropInfo.Effects = DragDropEffects.None;
                }

                return;
            }

            var group = dropInfo.Payload as RequirementsGroup;
            if (group != null)
            {
                var canDropRequirementGroup = this.PermissionService.CanWrite(ClassKind.RequirementsGroup, this.Thing);
                if (!canDropRequirementGroup || group.IDalUri.ToString() != this.Session.DataSourceUri || this.Thing.Group.Contains(group))
                {
                    dropInfo.Effects = DragDropEffects.None;
                }

                return;
            }

            var spec = dropInfo.Payload as RequirementsSpecification;
            if (spec != null)
            {
                var canDropSpec = this.PermissionService.CanWrite(ClassKind.RequirementsSpecification, this.Thing);
                if (canDropSpec && spec.Container.Iid == this.Thing.Container.Iid)
                {
                    dropInfo.Effects = DragDropEffects.Move;
                }

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
            var tuple = dropInfo.Payload as Tuple<ParameterType, MeasurementScale>;
            if (tuple != null)
            {
                await this.Drop(tuple);
                return;
            }

            var requirementGroupPayload = dropInfo.Payload as RequirementsGroup;
            if (requirementGroupPayload != null)
            {
                await this.OnRequirementGroupDrop(requirementGroupPayload);
                return;
            }

            var requirementPayload = dropInfo.Payload as Requirement;
            if (requirementPayload != null)
            {
                await this.OnRequirementDrop(requirementPayload);
            }

            var reqSpec = dropInfo.Payload as RequirementsSpecification;
            if (reqSpec != null)
            {
                await this.OnRequirementSpecificationDrop(reqSpec, dropInfo);
            }
        }

        /// <summary>
        /// Performs the drop operation for a <see cref="Requirement"/> payload
        /// </summary>
        /// <param name="requirement">
        /// The <see cref="Requirement"/> that was dropped into this <see cref="RequirementsSpecification"/>
        /// </param>
        private async Task OnRequirementDrop(Requirement requirement)
        {
            var firstRow = this.ContainedRows.OfType<RequirementRowViewModel>().FirstOrDefault();
            if (firstRow == null)
            {
                var context = TransactionContextResolver.ResolveContext(this.Thing);
                var transaction = new ThingTransaction(context);

                var requirementClone = requirement.Clone(false);
                requirementClone.Group = null;
                transaction.CreateOrUpdate(requirementClone);

                if (requirement.Container != this.Thing)
                {
                    // Add the requirement to the RequirementSpecification represented by this RowViewModel
                    var requirementSpecificationClone = this.Thing.Clone(false);
                    requirementSpecificationClone.Requirement.Add(requirement);
                    transaction.CreateOrUpdate(requirementSpecificationClone);
                }

                await this.DalWrite(transaction);
            }
            else
            {
                // insert before first
                var model = (EngineeringModel)this.Thing.TopContainer;
                var orderPt = OrderHandlerService.GetOrderParameterType(model);

                if (orderPt == null)
                {
                    return;
                }

                var orderService = new RequirementOrderHandlerService(this.Session, orderPt);
                var transaction = orderService.Insert(requirement, firstRow.Thing, InsertKind.InsertBefore);
                await this.Session.Write(transaction.FinalizeTransaction());
            }
        }

        /// <summary>
        /// Performs the drop operation for a <see cref="RequirementsGroup"/> payload
        /// </summary>
        /// <param name="requirementGroupPayload">
        /// The <see cref="RequirementsGroup"/> that was dropped into this <see cref="RequirementsSpecification"/>
        /// </param>
        private async Task OnRequirementGroupDrop(RequirementsGroup requirementGroupPayload)
        {
            var firstRow = this.ContainedRows.OfType<RequirementsGroupRowViewModel>().FirstOrDefault();
            if (firstRow == null)
            {
                var context = TransactionContextResolver.ResolveContext(this.Thing);
                var transaction = new ThingTransaction(context);
                var previousRequirementSpec = requirementGroupPayload.GetContainerOfType<RequirementsSpecification>();

                // Add the RequirementGroup to the RequirementsSpecification represented by this RowViewModel
                var requirementsSpecificationClone = this.Thing.Clone(false);
                requirementsSpecificationClone.Group.Add(requirementGroupPayload);
                transaction.CreateOrUpdate(requirementsSpecificationClone);

                if (previousRequirementSpec != this.Thing)
                {
                    // Update the requirements that were inside any of the groups that have been dropped
                    var previousRequirementSpecRow = (RequirementsSpecificationRowViewModel)((RequirementsBrowserViewModel)this.ContainerViewModel).ReqSpecificationRows.Single(x => x.Thing == previousRequirementSpec);
                    var droppedRequirementGroups = requirementGroupPayload.ContainedGroup().ToList();
                    droppedRequirementGroups.Add(requirementGroupPayload);
                    foreach (var keyValuePair in previousRequirementSpecRow.requirementContainerGroupCache)
                    {
                        if (!droppedRequirementGroups.Contains(keyValuePair.Value))
                        {
                            continue;
                        }

                        var requirementClone = keyValuePair.Key.Clone(false);
                        requirementClone.Group = null;
                        transaction.CreateOrUpdate(requirementClone);
                    }
                }

                await this.DalWrite(transaction);
            }
            else
            {
                // insert before first
                var model = (EngineeringModel)this.Thing.TopContainer;
                var orderPt = OrderHandlerService.GetOrderParameterType(model);

                if (orderPt == null)
                {
                    return;
                }

                var orderService = new RequirementsGroupOrderHandlerService(this.Session, orderPt);
                var transaction = orderService.Insert(requirementGroupPayload, firstRow.Thing, InsertKind.InsertBefore);
                await this.Session.Write(transaction.FinalizeTransaction());
            }
        }

        /// <summary>
        /// Performs the drop operation for a <see cref="RequirementsGroup"/> payload
        /// </summary>
        /// <param name="reqSpecPayload">
        /// The <see cref="RequirementsGroup"/> that was dropped into this <see cref="RequirementsSpecification"/>
        /// </param>
        /// <param name="dropinfo">The <see cref="IDropInfo"/></param>
        private async Task OnRequirementSpecificationDrop(RequirementsSpecification reqSpecPayload, IDropInfo dropinfo)
        {
            var model = (EngineeringModel)this.Thing.TopContainer;
            var orderPt = OrderHandlerService.GetOrderParameterType(model);

            if (orderPt == null)
            {
                return;
            }

            var orderService = new RequirementsSpecificationOrderHandlerService(this.Session, orderPt);
            var transaction = orderService.Insert(reqSpecPayload, this.Thing, dropinfo.IsDroppedAfter ? InsertKind.InsertAfter : InsertKind.InsertBefore);
            await this.Session.Write(transaction.FinalizeTransaction());
        }

        #endregion
    }
}