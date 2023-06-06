// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementsGroupRowViewModel.cs" company="RHEA System S.A.">
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;

    using CDP4Requirements.Utils;

    using ReactiveUI;

    /// <summary>
    /// The row-view-model that represents a <see cref="RequirementsGroup" />
    /// </summary>
    public class RequirementsGroupRowViewModel : RequirementContainerRowViewModel<RequirementsGroup>, IDropTarget,
        IDeprecatableThing
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsGroupRowViewModel" /> class
        /// </summary>
        /// <param name="group">The <see cref="RequirementsGroup" /></param>
        /// <param name="session">The <see cref="ISession" /></param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}" /></param>
        /// <param name="topNode">The top level node for this row</param>
        public RequirementsGroupRowViewModel(
            RequirementsGroup group,
            ISession session,
            IViewModelBase<Thing> containerViewModel,
            RequirementsSpecificationRowViewModel topNode)
            : base(group, session, containerViewModel, topNode)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets the categories name
        /// </summary>
        public string Categories
        {
            get => (this.Thing != null && this.Thing.Category.Any()) ? string.Join(", ", this.Thing.Category.Select(x => x.Name)) : string.Empty;
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
                var canDropRequirement = this.PermissionService.CanWrite(requirement);
                var requirementSpecification = this.Thing.GetContainerOfType<RequirementsSpecification>();

                var canModifyRequirementSpecification = requirementSpecification == requirement.Container ||
                                                        this.PermissionService.CanWrite(
                                                            ClassKind.Requirement,
                                                            requirementSpecification);

                if (!canDropRequirement || !canModifyRequirementSpecification ||
                    requirement.IDalUri.ToString() != this.Session.DataSourceUri)
                {
                    dropInfo.Effects = DragDropEffects.None;
                }

                return;
            }

            var group = dropInfo.Payload as RequirementsGroup;

            if (group != null)
            {
                var canDropRequirementGroup = this.PermissionService.CanWrite(ClassKind.RequirementsGroup, this.Thing);

                if (!canDropRequirementGroup || group.IDalUri.ToString() != this.Session.DataSourceUri
                                             || this.CreatesCycle(this.Thing, group))
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

            var requirementPayload = dropInfo.Payload as Requirement;

            if (requirementPayload != null)
            {
                await this.OnRequirementDrop(requirementPayload);
                return;
            }

            var requirementGroupPayload = dropInfo.Payload as RequirementsGroup;

            if (requirementGroupPayload != null)
            {
                await this.OnRequirementGroupDrop(requirementGroupPayload, dropInfo);
            }
        }

        /// <summary>
        /// Update the properties of this row
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();

            this.UpdateRequirementGroupRows();
            this.UpdateIsDeprecatedDerivedFromContainerRequirementsSpecification();
        }

        /// <summary>
        /// Initializes the subscriptions
        /// </summary>
        protected override void InitializeSubscriptions()
        {
            base.InitializeSubscriptions();

            var requirementsSpecificationRowViewModel =
                this.ContainerViewModel as RequirementsSpecificationRowViewModel;

            if (requirementsSpecificationRowViewModel != null)
            {
                var containerIsDeprecatedSubscription =
                    requirementsSpecificationRowViewModel.WhenAnyValue(vm => vm.IsDeprecated)
                        .Subscribe(_ => this.UpdateIsDeprecatedDerivedFromContainerRequirementsSpecification());

                this.Disposables.Add(containerIsDeprecatedSubscription);
            }
        }

        /// <summary>
        /// Updates the IsDeprecated property based on the value of the container <see cref="RequirementsSpecification" />
        /// </summary>
        private void UpdateIsDeprecatedDerivedFromContainerRequirementsSpecification()
        {
            var requirementsSpecification = this.Thing.GetContainerOfType<RequirementsSpecification>();

            if (requirementsSpecification != null)
            {
                this.IsDeprecated = requirementsSpecification.IsDeprecated;

                var groupRows = this.ContainedRows.OfType<RequirementsGroupRowViewModel>();
                this.RecursivelyUpdateIsDeprecated(groupRows, requirementsSpecification.IsDeprecated);
            }
        }

        /// <summary>
        /// Recursively Update the IsDeprecated property
        /// </summary>
        /// <param name="groupRows">
        /// The collection of rows in the group.
        /// </param>
        /// <param name="isDeprecated">The deprecated state.</param>
        private void RecursivelyUpdateIsDeprecated(
            IEnumerable<RequirementsGroupRowViewModel> groupRows,
            bool isDeprecated)
        {
            foreach (var requirementsGroupRowViewModel in groupRows)
            {
                requirementsGroupRowViewModel.IsDeprecated = isDeprecated;

                var containedGroupRows =
                    requirementsGroupRowViewModel.ContainedRows.OfType<RequirementsGroupRowViewModel>();

                requirementsGroupRowViewModel.RecursivelyUpdateIsDeprecated(containedGroupRows, isDeprecated);
            }
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
        /// Queries whether a drag can be started
        /// </summary>
        /// <param name="dragInfo">
        /// Information about the drag.
        /// </param>
        /// <remarks>
        /// To allow a drag to be started, the <see cref="IDragInfo.Effects" /> property on <paramref name="dragInfo" />
        /// should be set to a value other than <see cref="DragDropEffects.None" />.
        /// </remarks>
        public override void StartDrag(IDragInfo dragInfo)
        {
            dragInfo.Payload = this.Thing;
            dragInfo.Effects = DragDropEffects.Move;
        }

        /// <summary>
        /// Performs the drop operation for a <see cref="Requirement" /> payload
        /// </summary>
        /// <param name="requirement">
        /// The <see cref="Requirement" /> that was dropped into this <see cref="RequirementsGroup" />
        /// </param>
        private async Task OnRequirementDrop(Requirement requirement)
        {
            var firstRow = this.ContainedRows.OfType<RequirementRowViewModel>().FirstOrDefault();

            if (firstRow == null)
            {
                await this.ChangeRequirementGroup(requirement);
            }
            else
            {
                // insert before first
                var model = (EngineeringModel)this.Thing.TopContainer;
                var orderPt = OrderHandlerService.GetOrderParameterType(model);

                if (orderPt == null)
                {
                    await this.ChangeRequirementGroup(requirement);
                    return;
                }

                var orderService = new RequirementOrderHandlerService(this.Session, orderPt);
                var transaction = orderService.Insert(requirement, firstRow.Thing, InsertKind.InsertBefore);
                await this.Session.Write(transaction.FinalizeTransaction());
            }
        }

        /// <summary>
        /// Changes the <see cref="RequirementsGroup" /> of a <see cref="Requirement" />
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement" /></param>
        /// <returns>The async Task</returns>
        private async Task ChangeRequirementGroup(Requirement requirement)
        {
            var context = TransactionContextResolver.ResolveContext(this.Thing);
            var transaction = new ThingTransaction(context);
            var requirementClone = requirement.Clone(false);
            var requirementSpecification = this.Thing.GetContainerOfType<RequirementsSpecification>();
            requirementClone.Group = this.Thing;
            transaction.CreateOrUpdate(requirementClone);

            if (requirement.Container != requirementSpecification)
            {
                var requirementSpecificationClone = requirementSpecification.Clone(false);
                requirementSpecificationClone.Requirement.Add(requirementClone);
                transaction.CreateOrUpdate(requirementSpecificationClone);
            }

            await this.DalWrite(transaction);
        }

        /// <summary>
        /// Performs the drop operation for a <see cref="RequirementsGroup" /> payload
        /// </summary>
        /// <param name="requirementGroup">
        /// The <see cref="RequirementsGroup" /> that was dropped into this <see cref="RequirementsGroup" />
        /// </param>
        /// <param name="dropInfo">The <see cref="IDropInfo" /></param>
        private async Task OnRequirementGroupDrop(RequirementsGroup requirementGroup, IDropInfo dropInfo)
        {
            if (dropInfo.KeyStates == (DragDropKeyStates.LeftMouseButton | DragDropKeyStates.ControlKey))
            {
                // ordered-move
                var model = (EngineeringModel)this.Thing.TopContainer;
                var orderPt = OrderHandlerService.GetOrderParameterType(model);

                if (orderPt == null)
                {
                    return;
                }

                var orderService = new RequirementsGroupOrderHandlerService(this.Session, orderPt);

                var transaction = orderService.Insert(
                    requirementGroup,
                    this.Thing,
                    dropInfo.IsDroppedAfter ? InsertKind.InsertAfter : InsertKind.InsertBefore);

                await this.Session.Write(transaction.FinalizeTransaction());
            }
            else
            {
                var context = TransactionContextResolver.ResolveContext(this.Thing);
                var transaction = new ThingTransaction(context);
                var previousRequirementSpec = requirementGroup.GetContainerOfType<RequirementsSpecification>();
                var currentRequirementSpec = this.Thing.GetContainerOfType<RequirementsSpecification>();

                // Add this RequirementGroup to the RequirementGroup represented by this RowViewModel
                var requirementGroupClone = requirementGroup.Clone(false);
                var containerRequirementGroupClone = this.Thing.Clone(false);
                containerRequirementGroupClone.Group.Add(requirementGroupClone);
                transaction.CreateOrUpdate(containerRequirementGroupClone);

                if (previousRequirementSpec != currentRequirementSpec)
                {
                    // Update the requirements that were inside any of the groups that have been dropped
                    var previousRequirementSpecRow =
                        ((RequirementsBrowserViewModel)this.TopParentRow.ContainerViewModel).ReqSpecificationRows
                        .Single(x => x.Thing == previousRequirementSpec);

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
        }

        /// <summary>
        /// Checks if adding the droppedRequirementsGroup to parentRequirementsGroup would create a cycle
        /// </summary>
        /// <param name="parentRequirementsGroup">
        /// The <see cref="RequirementsGroup" /> to check
        /// </param>
        /// <param name="draggedRequirementsGroup">
        /// The <see cref="RequirementsGroup" /> to that the user is dragging over this row view model
        /// </param>
        private bool CreatesCycle(RequirementsGroup parentRequirementsGroup, RequirementsGroup draggedRequirementsGroup)
        {
            if (parentRequirementsGroup == draggedRequirementsGroup)
            {
                return true;
            }

            parentRequirementsGroup = parentRequirementsGroup.Container as RequirementsGroup;

            return parentRequirementsGroup != null &&
                   this.CreatesCycle(parentRequirementsGroup, draggedRequirementsGroup);
        }
    }
}
