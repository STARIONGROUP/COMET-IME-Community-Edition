// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementsSpecificationRowViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.DragDrop;
    using CDP4Composition.MessageBus;
    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;

    using CDP4Requirements.Utils;

    using ReactiveUI;

    /// <summary>
    /// A row-view-model that represents a <see cref="RequirementsSpecification" /> in the requirement browser
    /// </summary>
    public class RequirementsSpecificationRowViewModel : RequirementContainerRowViewModel<RequirementsSpecification>,
        IDropTarget
    {
        /// <summary>
        /// The <see cref="Requirement" /> update listener cache
        /// </summary>
        private readonly Dictionary<Requirement, IDisposable> listenerCache;

        /// <summary>
        /// Cache for the <see cref="Requirement" /> currently contained
        /// </summary>
        private readonly Dictionary<Requirement, IRowViewModelBase<Requirement>> requirementCache;

        /// <summary>
        /// Cache for the requirement and their container
        /// </summary>
        internal Dictionary<Requirement, RequirementsGroup> requirementContainerGroupCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsSpecificationRowViewModel" /> class
        /// </summary>
        /// <param name="reqContainer">The <see cref="RequirementsSpecification" /></param>
        /// <param name="session">The <see cref="ISession" /></param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}" /></param>
        public RequirementsSpecificationRowViewModel(
            RequirementsSpecification reqContainer,
            ISession session,
            IViewModelBase<Thing> containerViewModel)
            : base(reqContainer, session, containerViewModel)
        {
            this.requirementCache = new Dictionary<Requirement, IRowViewModelBase<Requirement>>();
            this.requirementContainerGroupCache = new Dictionary<Requirement, RequirementsGroup>();
            this.listenerCache = new Dictionary<Requirement, IDisposable>();
            this.GroupCache = new Dictionary<RequirementsGroup, IRowViewModelBase<Thing>>();
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets the categories name
        /// </summary>
        public string Categories => this.Thing != null && this.Thing.Category.Any() ? string.Join(", ", this.Thing.Category.Select(x => x.Name)) : string.Empty;

        /// <summary>
        /// Gets the <see cref="RequirementsGroup" /> cache
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

        /// <summary>
        /// Updates the current drag state.
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drag operation.
        /// </param>
        /// <remarks>
        /// To allow a drop at the current drag position, the <see cref="DropInfo.Effects" /> property on
        /// <paramref name="dropInfo" /> should be set to a value other than <see cref="DragDropEffects.None" />
        /// and <see cref="DropInfo.Payload" /> should be set to a non-null value.
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

            // Only allow groups to be moved to the same RequirementsSpecification
            if (group != null && this.Thing == group.GetContainerOfType<RequirementsSpecification>())
            {
                var canDropRequirementGroup = this.PermissionService.CanWrite(ClassKind.RequirementsGroup, this.Thing);

                if (!canDropRequirementGroup || group.IDalUri.ToString() != this.Session.DataSourceUri ||
                    this.Thing.Group.Contains(group))
                {
                    dropInfo.Effects = DragDropEffects.None;
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

            // Only allow groups to be moved to the same RequirementsSpecification
            if (requirementGroupPayload != null && this.Thing == requirementGroupPayload.GetContainerOfType<RequirementsSpecification>())
            {
                await this.OnRequirementGroupDrop(requirementGroupPayload);
                return;
            }

            var requirementPayload = dropInfo.Payload as Requirement;

            if (requirementPayload != null)
            {
                await this.OnRequirementDrop(requirementPayload);
            }
        }

        /// <summary>
        /// Update the <see cref="Requirement" /> nodes
        /// </summary>
        private void UpdateRequirementRows()
        {
            var current = this.requirementCache.Keys;
            var updated = this.Thing.Requirement;

            var added = updated.Except(current).ToList();
            var removed = current.Except(updated).ToList();

            foreach (var requirement in added)
            {
                this.TryAddRequirementRow(requirement);
            }

            foreach (var requirement in removed)
            {
                this.RemoveRequirementRow(requirement);
            }
        }

        /// <summary>
        /// Add a requirement rows
        /// </summary>
        /// <param name="req">The <see cref="Requirement" /> to add</param>
        private bool TryAddRequirementRow(Requirement req)
        {
            var row = new RequirementRowViewModel(req, this.Session, this);
            this.requirementCache[req] = row;
            this.requirementContainerGroupCache[req] = req.Group;

            var wasAdded = false;

            if (req.Group == null)
            {
                this.ContainedRows.SortedInsert(row, ChildRowComparer);
                wasAdded = true;
            }
            else
            {
                if (this.GroupCache.TryGetValue(req.Group, out var groupRow))
                {
                    if (groupRow.ContainedRows.All(x => x.Thing != req))
                    {
                        groupRow.ContainedRows.SortedInsert(row, ChildRowComparer);
                        wasAdded = true;
                    }
                }
                else
                {
                    logger.Error("The requirement group with iid {0} could not be found", req.Group.Iid);

                    if (this.ContainedRows.All(x => x.Thing != req))
                    {
                        this.ContainedRows.SortedInsert(row, ChildRowComparer);
                        wasAdded = true;
                    }
                }
            }

            if (wasAdded)
            {
                Func<ObjectChangedEvent, bool> discriminator =
                    objectChange =>
                        objectChange.EventKind == EventKind.Updated &&
                        objectChange.ChangedThing.RevisionNumber > this.RevisionNumber; 

                Action<ObjectChangedEvent> action = x => this.UpdateRequirementRow((Requirement)x.ChangedThing, false);

                if (this.AllowMessageBusSubscriptions)
                {
                    var updateListener = this.CDPMessageBus.Listen<ObjectChangedEvent>(req)
                        .Where(discriminator)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(action);

                    this.Disposables.Add(updateListener);
                    this.listenerCache[req] = updateListener;
                }
                else
                {
                    var observer = this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(Requirement));

                    var handler = this.MessageBusHandler.GetHandler<ObjectChangedEvent>().RegisterEventHandler(observer, new ObjectChangedMessageBusEventHandlerSubscription(req, discriminator, action));
                    this.Disposables.Add(handler);

                    this.listenerCache[req] = handler;
                }

                var orderPt = OrderHandlerService.GetOrderParameterType((EngineeringModel)this.Thing.TopContainer);

                if (orderPt != null)
                {
                    var orderListener = this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(SimpleParameterValue))
                        .Where(
                            objectChange =>
                                ((SimpleParameterValue)objectChange.ChangedThing).ParameterType == orderPt &&
                                this.Thing.Requirement.Any(r => r.ParameterValue.Contains(objectChange.ChangedThing)))
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(x => this.UpdateRequirementRow((Requirement)x.ChangedThing.Container, true));

                    this.Disposables.Add(orderListener);
                }
            }

            return wasAdded;
        }

        /// <summary>
        /// Remove a requirement row
        /// </summary>
        /// <param name="req">The <see cref="Requirement" /> to remove</param>
        private void RemoveRequirementRow(Requirement req)
        {
            if (!this.requirementCache.TryGetValue(req, out var reqRow))
            {
                return;
            }

            var previousGroup = this.requirementContainerGroupCache[req];

            if (previousGroup != null)
            {
                if (this.GroupCache.TryGetValue(previousGroup, out var groupRow))
                {
                    groupRow.ContainedRows.RemoveWithoutDispose(reqRow);
                }
            }
            else
            {
                this.ContainedRows.RemoveWithoutDispose(reqRow);
            }

            reqRow.Dispose();
            this.requirementCache.Remove(req);
            this.requirementContainerGroupCache.Remove(req);

            this.listenerCache[req].Dispose();
            this.listenerCache.Remove(req);
        }

        /// <summary>
        /// Update the requirement row within the <see cref="RequirementsGroup" /> rows
        /// </summary>
        /// <param name="req">The <see cref="Requirement" /></param>
        /// <param name="newOrder">A value indicating whether the order may have changed</param>
        public void UpdateRequirementRow(Requirement req, bool newOrder)
        {
            if (req.Container != this.Thing || !this.requirementContainerGroupCache.ContainsKey(req) ||
                !this.requirementCache.ContainsKey(req))
            {
                return;
            }

            var currentGroup = this.requirementContainerGroupCache[req];

            var reqRow = this.requirementCache[req];

            var currentContainerRow =
                currentGroup == null
                    ? this
                    : this.GroupCache.ContainsKey(currentGroup)
                        ? this.GroupCache[currentGroup]
                        : null;

            IRowViewModelBase<Thing> updatedContainerRow;

            if (req.Group != null)
            {
                if (!this.GroupCache.TryGetValue(req.Group, out updatedContainerRow))
                {
                    logger.Error(
                        "The requirement-group {0} could not be found in the Specification {1}",
                        req.Group.Name,
                        this.Thing.Name);

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

            currentContainerRow?.ContainedRows.RemoveWithoutDispose(reqRow);
            updatedContainerRow?.ContainedRows.SortedInsert(reqRow, ChildRowComparer);
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

        /// <summary>
        /// The object changed event handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent" /></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Performs the drop operation for a <see cref="Requirement" /> payload
        /// </summary>
        /// <param name="requirement">
        /// The <see cref="Requirement" /> that was dropped into this <see cref="RequirementsSpecification" />
        /// </param>
        private async Task OnRequirementDrop(Requirement requirement)
        {
            var firstRow = this.ContainedRows.OfType<RequirementRowViewModel>().FirstOrDefault();

            if (firstRow == null)
            {
                await this.ChangeRequirementContainer(requirement);
            }
            else
            {
                // insert before first
                var model = (EngineeringModel)this.Thing.TopContainer;
                var orderPt = OrderHandlerService.GetOrderParameterType(model);

                if (orderPt == null)
                {
                    await this.ChangeRequirementContainer(requirement);
                    return;
                }

                var orderService = new RequirementOrderHandlerService(this.Session, orderPt);
                var transaction = orderService.Insert(requirement, firstRow.Thing, InsertKind.InsertBefore);
                await this.Session.Write(transaction.FinalizeTransaction());
            }
        }

        /// <summary>
        /// Changes the <see cref="RequirementsContainer" /> of a <see cref="Requirement" />
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /></param>
        /// <returns>The async Task</returns>
        private async Task ChangeRequirementContainer(Requirement requirement)
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

        /// <summary>
        /// Performs the drop operation for a <see cref="RequirementsGroup" /> payload
        /// </summary>
        /// <param name="requirementGroupPayload">
        /// The <see cref="RequirementsGroup" /> that was dropped into this <see cref="RequirementsSpecification" />
        /// </param>
        private async Task OnRequirementGroupDrop(RequirementsGroup requirementGroupPayload)
        {
            var firstRow = this.ContainedRows.OfType<RequirementsGroupRowViewModel>().FirstOrDefault();

            if (firstRow == null)
            {
                await this.MoveGroup(requirementGroupPayload);
            }
            else
            {
                // insert before first ???
                var model = (EngineeringModel)this.Thing.TopContainer;
                var orderPt = OrderHandlerService.GetOrderParameterType(model);

                if (orderPt == null)
                {
                    await this.MoveGroup(requirementGroupPayload);
                    return;
                }

                var orderService = new RequirementsGroupOrderHandlerService(this.Session, orderPt);
                var transaction = orderService.Insert(requirementGroupPayload, firstRow.Thing, InsertKind.InsertBefore);
                await this.Session.Write(transaction.FinalizeTransaction());
            }
        }

        /// <summary>
        /// Move a group to a <see cref="RequirementsSpecification"/>
        /// </summary>
        /// <param name="requirementGroup"></param>
        /// <returns></returns>
        private async Task MoveGroup(RequirementsGroup requirementGroup)
        {
            var context = TransactionContextResolver.ResolveContext(this.Thing);
            var transaction = new ThingTransaction(context);
            var previousRequirementSpec = requirementGroup.GetContainerOfType<RequirementsSpecification>();

            // Add the RequirementGroup to the RequirementsSpecification represented by this RowViewModel
            var requirementsSpecificationClone = this.Thing.Clone(false);
            requirementsSpecificationClone.Group.Add(requirementGroup);
            transaction.CreateOrUpdate(requirementsSpecificationClone);

            if (previousRequirementSpec != this.Thing)
            {
                // Update the requirements that were inside any of the groups that have been dropped
                var previousRequirementSpecRow =
                    ((RequirementsBrowserViewModel)this.ContainerViewModel)
                    .ReqSpecificationRows.Single(x => x.Thing == previousRequirementSpec);

                var droppedRequirementGroups = requirementGroup.ContainedGroup().ToList();
                droppedRequirementGroups.Add(requirementGroup);

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

        /// <summary>
        /// Performs the drop operation for a <see cref="RequirementsGroup" /> payload
        /// </summary>
        /// <param name="reqSpecPayload">
        /// The <see cref="RequirementsGroup" /> that was dropped into this <see cref="RequirementsSpecification" />
        /// </param>
        /// <param name="dropinfo">The <see cref="IDropInfo" /></param>
        private async Task OnRequirementSpecificationDrop(RequirementsSpecification reqSpecPayload, IDropInfo dropinfo)
        {
            var model = (EngineeringModel)this.Thing.TopContainer;
            var orderPt = OrderHandlerService.GetOrderParameterType(model);

            if (orderPt == null)
            {
                return;
            }

            var orderService = new RequirementsSpecificationOrderHandlerService(this.Session, orderPt);

            var transaction = orderService.Insert(
                reqSpecPayload,
                this.Thing,
                dropinfo.IsDroppedAfter ? InsertKind.InsertAfter : InsertKind.InsertBefore);

            await this.Session.Write(transaction.FinalizeTransaction());
        }
    }
}
