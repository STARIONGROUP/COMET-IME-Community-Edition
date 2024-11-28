﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementContainerRowViewModel.cs" company="Starion Group S.A.">
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
    using CDP4Common.Types;

    using CDP4CommonView;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;

    using CDP4Requirements.Comparers;
    using CDP4Requirements.Extensions;
    using CDP4Requirements.Utils;
    using CDP4Requirements.ViewModels.RequirementBrowser;

    using CDP4RequirementsVerification;

    using ReactiveUI;

    /// <summary>
    /// A Row view model that represents a <see cref="RequirementsContainer"/>
    /// </summary>
    /// <typeparam name="T">
    /// A type of <see cref="RequirementsContainer"/>
    /// </typeparam>
    public abstract class RequirementContainerRowViewModel<T> : RequirementsContainerRowViewModel<T>, IDeprecatableThing, IRequirementBrowserDisplaySettings, IHaveWritableRequirementStateOfCompliance where T : RequirementsContainer
    {
        /// <summary>
        /// Backing field for <see cref="RelationalExpressionRowViewModel.RequirementStateOfCompliance"/>
        /// </summary>
        private RequirementStateOfCompliance requirementStateOfCompliance;

        /// <summary>
        /// The <see cref="IComparer{T}"/>
        /// </summary>
        protected static readonly IComparer<IRowViewModelBase<Thing>> ChildRowComparer = new RequirementContainerChildRowComparer();

        /// <summary>
        /// The folder row containing the <see cref="RequirementsContainerParameterValue"/>
        /// </summary>
        private readonly CDP4Composition.FolderRowViewModel simpleParameters;

        /// <summary>
        /// The top parent node
        /// </summary>
        protected readonly RequirementsSpecificationRowViewModel TopParentRow;

        /// <summary>
        /// Backing field for <see cref="Definition"/> property.
        /// </summary>
        private string definition;

        /// <summary>
        /// Backing field for <see cref="IsDeprecated"/> property.
        /// </summary>
        private bool isDeprecated;

        /// <summary>
        ///Backing field for <see cref="IsSimpleParameterValuesDisplayed"/>
        /// </summary>
        private bool isSimpleParameterValuesDisplayed;

        /// <summary>
        ///Backing field for <see cref="IsParametricConstraintDisplayed"/>
        /// </summary>
        private bool isParametricConstraintDisplayed;

        /// <summary>
        /// Backing field for <see cref="CategoryList"/>
        /// </summary>
        private List<Category> categoryList;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementContainerRowViewModel{T}"/> class
        /// </summary>
        /// <param name="reqContainer">The <see cref="RequirementsSpecification"/></param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        /// <param name="topNode">The top level node for this row</param>
        protected RequirementContainerRowViewModel(T reqContainer, ISession session, IViewModelBase<Thing> containerViewModel, RequirementsSpecificationRowViewModel topNode = null)
            : base(reqContainer, session, containerViewModel)
        {
            this.simpleParameters = new CDP4Composition.FolderRowViewModel("Simple Parameter Values", "Simple Parameter Values", this.Session, this);
            this.ContainedRows.Add(this.simpleParameters);
            this.TopParentRow = topNode ?? this as RequirementsSpecificationRowViewModel;
            this.SetSubscriptions();
        }

        /// <summary>
        /// Gets or sets the <see cref="CDP4RequirementsVerification.RequirementStateOfCompliance"/>
        /// </summary>
        public RequirementStateOfCompliance RequirementStateOfCompliance
        {
            get => this.requirementStateOfCompliance;
            set => this.RaiseAndSetIfChanged(ref this.requirementStateOfCompliance, value);
        }

        /// <summary>
        /// Gets or sets the definition for this <see cref="RequirementsContainer"/>
        /// </summary>
        public string Definition
        {
            get => this.definition;
            protected set => this.RaiseAndSetIfChanged(ref this.definition, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whehter the row is deprecated
        /// </summary>
        public bool IsDeprecated
        {
            get => this.isDeprecated;
            set => this.RaiseAndSetIfChanged(ref this.isDeprecated, value);
        }

        /// <summary>
        /// Gets or sets a value whether SimpleParameterValues things are displayed
        /// </summary>
        public bool IsSimpleParameterValuesDisplayed
        {
            get => this.isSimpleParameterValuesDisplayed;
            set => this.RaiseAndSetIfChanged(ref this.isSimpleParameterValuesDisplayed, value);
        }

        /// <summary>
        /// Gets or sets a value whether Parametric Constraints are displayed
        /// </summary>
        public bool IsParametricConstraintDisplayed
        {
            get => this.isParametricConstraintDisplayed;
            set => this.RaiseAndSetIfChanged(ref this.isParametricConstraintDisplayed, value);
        }

        /// <summary>
        /// Gets or sets the categories
        /// </summary>
        public List<Category> CategoryList
        {
            get => this.categoryList;
            set => this.RaiseAndSetIfChanged(ref this.categoryList, value);
        }

        /// <summary>
        /// Update the <see cref="ThingStatus"/> property
        /// </summary>
        protected override void UpdateThingStatus()
        {
            this.ThingStatus = new ThingStatus(this.Thing);
        }

        /// <summary>
        /// Update the current <see cref="RequirementsContainer"/> with its current <see cref="RequirementsGroup"/>s
        /// </summary>
        protected void UpdateRequirementGroupRows()
        {
            var current = this.ContainedRows.Select(x => x.Thing).OfType<RequirementsGroup>().ToList();
            var updated = this.Thing.Group;

            var added = updated.Except(current).ToList();
            var removed = current.Except(updated).ToList();

            foreach (var grp in added)
            {
                this.AddReqGroupRow(grp);
            }

            foreach (var grp in removed)
            {
                this.RemoveReqGroupRow(grp);
            }

            // if requirement specification, process All contained groups and add to cache
        }

        /// <summary>
        /// Updates the <see cref="RequirementsGroupRowViewModel"/> within the current <see cref="RequirementsGroupRowViewModel"/>
        /// </summary>
        /// <param name="group">The updated <see cref="RequirementsGroup"/></param>
        private void UpdateReqGroupPosition(RequirementsGroup group)
        {
            if (!this.Thing.Group.Contains(group))
            {
                return;
            }

            var groupRow = this.ContainedRows.OfType<RequirementsGroupRowViewModel>().SingleOrDefault(x => x.Thing.Iid == group.Iid);

            if (groupRow == null)
            {
                return;
            }

            this.ContainedRows.RemoveWithoutDispose(groupRow);
            this.ContainedRows.SortedInsert(groupRow, ChildRowComparer);
        }

        /// <summary>
        /// Add a nested <see cref="RequirementsContainer"/> row
        /// </summary>
        /// <param name="group">The <see cref="RequirementsContainer"/> to add</param>
        private void AddReqGroupRow(RequirementsGroup group)
        {
            var row = new RequirementsGroupRowViewModel(group, this.Session, this, this.TopParentRow);
            this.ContainedRows.SortedInsert(row, ChildRowComparer);
            this.TopParentRow.GroupCache[group] = row;

            var orderPt = OrderHandlerService.GetOrderParameterType((EngineeringModel)this.Thing.TopContainer);

            if (orderPt != null)
            {
                var orderListener = this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(RequirementsContainerParameterValue)).Where(objectChange => ((RequirementsContainerParameterValue)objectChange.ChangedThing).ParameterType == orderPt && this.Thing.Group.Any(g => g.ParameterValue.Contains(objectChange.ChangedThing)))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => this.UpdateReqGroupPosition((RequirementsGroup)x.ChangedThing.Container));

                this.Disposables.Add(orderListener);
            }

            var reqSpecRowViewModel = this as RequirementsSpecificationRowViewModel ?? this.TopParentRow;

            var reqSpec = reqSpecRowViewModel.Thing;

            foreach (var req in reqSpec.Requirement)
            {
                if (req.Group == group)
                {
                    reqSpecRowViewModel.TryAddRequirementRow(req);
                }
            }
        }

        /// <summary>
        /// Removes a nested <see cref="RequirementsContainer"/> row
        /// </summary>
        /// <param name="group">The <see cref="RequirementsContainer"/> to remove</param>
        private void RemoveReqGroupRow(RequirementsGroup group)
        {
            if (this.ContainedRows.SingleOrDefault(x => x.Thing == @group) is RequirementsGroupRowViewModel row)
            {
                this.TopParentRow.GroupCache.Remove(group);
                this.RemoveRequirementRowsBeforeRequirementsGroupDisposal(row);
                this.ContainedRows.RemoveAndDispose(row);
            }
        }

        /// <summary>
        /// Remove <see cref="RequirementRowViewModel"/>s from the <param name="row" />'s <see cref="IHaveContainedRows.ContainedRows"/>
        /// and from child <see cref="IHaveContainedRows"/>'s  <see cref="IHaveContainedRows.ContainedRows"/>.
        /// If we don't do this these <see cref="RequirementRowViewModel"/>s will be disposed and <see cref="RequirementsSpecificationRowViewModel"/>
        /// will not be able to reuse them, which is its expected behaviour.
        /// </summary>
        /// <param name="row">The <see cref="IHaveContainedRows"/></param>
        private void RemoveRequirementRowsBeforeRequirementsGroupDisposal(IHaveContainedRows row)
        {
            for (var i = row.ContainedRows.Count - 1; i >= 0; i--)
            {
                if (row.ContainedRows[i] is RequirementRowViewModel requirementRowViewModel)
                {
                    row.ContainedRows.RemoveWithoutDispose(requirementRowViewModel);
                }
                else
                {
                    this.RemoveRequirementRowsBeforeRequirementsGroupDisposal(row.ContainedRows[i]);
                }
            }
        }

        /// <summary>
        /// Updates the <see cref="RequirementsContainerParameterValue"/>s contained by this <see cref="RequirementsContainer"/>.
        /// </summary>
        private void UpdateValues()
        {
            var current = this.simpleParameters.ContainedRows.Select(x => x.Thing).OfType<RequirementsContainerParameterValue>().ToList();
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
        /// Add a row representing a new <see cref="RequirementsContainerParameterValue"/>
        /// </summary>
        /// <param name="value">The associated <see cref="RequirementsContainerParameterValue"/></param>
        private void AddValue(RequirementsContainerParameterValue value)
        {
            var row = new RequirementsContainerParameterValueRowViewModel(value, this.Session, this);
            this.simpleParameters.ContainedRows.Add(row);
        }

        /// <summary>
        /// Removes a value row
        /// </summary>
        /// <param name="value">The associated <see cref="RequirementsContainerParameterValue"/> to remove</param>
        private void RemoveValue(RequirementsContainerParameterValue value)
        {
            var row = this.simpleParameters.ContainedRows.SingleOrDefault(x => x.Thing == value);

            if (row != null)
            {
                this.simpleParameters.ContainedRows.RemoveAndDispose(row);
            }
        }

        /// <summary>
        /// Add the necessary subscriptions 
        /// </summary>
        private void SetSubscriptions()
        {
            this.Disposables.Add(
                this
                    .WhenAnyValue(x => x.IsDeprecated)
                    .Subscribe(x => this.UpdateIsDeprecated()));

            this.Disposables.Add(
                this
                    .WhenAnyValue(x => x.IsParametricConstraintDisplayed, y => y.IsSimpleParameterValuesDisplayed)
                    .Subscribe(x => this.AdjustContainedRows()));

            if (this.ContainerViewModel is IRequirementBrowserDisplaySettings requirementBrowserDisplaySettings)
            {
                this.Disposables.Add(
                    requirementBrowserDisplaySettings
                        .WhenAnyValue(x => x.IsParametricConstraintDisplayed)
                        .Subscribe(y => this.IsParametricConstraintDisplayed = y));

                this.Disposables.Add(
                    requirementBrowserDisplaySettings
                        .WhenAnyValue(x => x.IsSimpleParameterValuesDisplayed)
                        .Subscribe(y => this.IsSimpleParameterValuesDisplayed = y));
            }

            this.SetRequirementStateOfComplianceChangedEventSubscription(this.Thing, this.Disposables, this.CDPMessageBus);
        }

        /// <summary>
        /// Do some manual adjustments to the <see cref="ContainedRows" /> property
        /// </summary>
        private void AdjustContainedRows()
        {
            if (this.IsSimpleParameterValuesDisplayed)
            {
                if (!this.ContainedRows.Contains(this.simpleParameters))
                {
                    this.ContainedRows.Add(this.simpleParameters);
                }
            }
            else
            {
                if (this.ContainedRows.Contains(this.simpleParameters))
                {
                    this.ContainedRows.RemoveWithoutDispose(this.simpleParameters);
                }
            }
        }

        /// <summary>
        /// Update the values of this row
        /// </summary>
        protected virtual void UpdateProperties()
        {
            this.UpdateThingStatus();

            this.Definition = this.Thing.Definition.FirstOrDefault() == null
                ? string.Empty
                : this.Thing.Definition.First().Content;

            this.CategoryList = new List<Category>(this.Thing.Category);
            this.UpdateValues();
        }

        /// <summary>
        /// Update deprecated for <see cref="simpleParameters"/>
        /// </summary>
        private void UpdateIsDeprecated()
        {
            if (this.simpleParameters != null)
            {
                this.simpleParameters.IsDeprecated = this.IsDeprecated;
            }
        }

        /// <summary>
        /// Handle the drag-over of a <see cref="Category"/>
        /// </summary>
        /// <param name="tuple">The <see cref="Tuple{T1,T2}"/></param>
        /// <param name="dropInfo">The <see cref="IDropInfo"/></param>
        protected void DragOver(Tuple<ParameterType, MeasurementScale> tuple, IDropInfo dropInfo)
        {
            if (!this.Session.PermissionService.CanWrite(ClassKind.RequirementsContainerParameterValue, this.Thing))
            {
                logger.Info("Permission denied to create a ParameterValue.");
                dropInfo.Effects = DragDropEffects.None;

                return;
            }

            if (this.Thing.ParameterValue.Select(x => x.ParameterType).Contains(tuple.Item1))
            {
                logger.Info("A ParameterValue with this ParameterType already exists.");
                dropInfo.Effects = DragDropEffects.None;

                return;
            }

            dropInfo.Effects = DragDropEffects.Copy;
        }

        /// <summary>
        /// Handles the drop action of a <see cref="Tuple{ParameterType, MeasurementScale}"/>
        /// </summary>
        /// <param name="tuple">The <see cref="Tuple{ParameterType, MeasurementScale}"/></param>
        protected async Task Drop(Tuple<ParameterType, MeasurementScale> tuple)
        {
            var clone = this.Thing.Clone(false);

            var parameterValue = new RequirementsContainerParameterValue();
            parameterValue.ParameterType = tuple.Item1;
            parameterValue.Scale = tuple.Item2;
            parameterValue.Value = new ValueArray<string>(new[] { "-" });

            clone.ParameterValue.Add(parameterValue);

            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(this.Thing));
            transaction.Create(parameterValue);
            transaction.CreateOrUpdate(clone);

            await this.DalWrite(transaction);
        }
    }
}
