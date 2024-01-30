// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BinaryRelationshipRowViewModel.cs" company="RHEA System S.A.">
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

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// The view-model for the <see cref="BinaryRelationshipRowViewModel"/> row
    /// </summary>
    public class BinaryRelationshipRowViewModel : CDP4CommonView.BinaryRelationshipRowViewModel
    {
        /// <summary>
        /// The folder row containing the <see cref="RelationshipParameterValue"/>
        /// </summary>
        private readonly CDP4Composition.FolderRowViewModel simpleParameters;

        /// <summary>
        /// Backing field for the <see cref="Name"/> property.
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for the <see cref="ShortName"/> property.
        /// </summary>
        private string shortName;

        /// <summary>
        /// Source thing before any update
        /// </summary>
        private Thing oldSource;

        /// <summary>
        /// Source thing subscription
        /// </summary>
        private IDisposable sourceSubscription;

        /// <summary>
        /// Target thing before any update
        /// </summary>
        private Thing oldTarget;

        /// <summary>
        /// Target thing subscription
        /// </summary>
        private IDisposable targetSubscription;

        /// <summary>
        /// Backing field for <see cref="CategoryList"/>
        /// </summary>
        private List<Category> categoryList;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryRelationshipRowViewModel"/> class
        /// </summary>
        /// <param name="relationship">The <see cref="BinaryRelationship"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The container view-model</param>
        public BinaryRelationshipRowViewModel(BinaryRelationship relationship, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(relationship, session, containerViewModel)
        {
            this.simpleParameters = new CDP4Composition.FolderRowViewModel("Simple Parameter Values", "Simple Parameter Values", this.Session, this);
            this.ContainedRows.Add(this.simpleParameters);
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Category"/>
        /// </summary>
        public List<Category> CategoryList
        {
            get => this.categoryList;
            set => this.RaiseAndSetIfChanged(ref this.categoryList, value);
        }

        /// <summary>
        /// Gets or sets the name of the <see cref="BinaryRelationship"/> that is represented by the current row-view-model
        /// </summary>
        public string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>
        /// Gets or sets the short-name of the <see cref="BinaryRelationship"/> that is represented by the current row-view-model
        /// </summary>
        public string ShortName
        {
            get => this.shortName;
            set => this.RaiseAndSetIfChanged(ref this.shortName, value);
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
        /// Updates the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            //In case Source or Target have been updated I dispose and create a new name subscription
            if (this.oldSource != this.Thing.Source)
            {
                if (this.sourceSubscription != null)
                {
                    this.Disposables.Remove(this.sourceSubscription);
                    this.sourceSubscription.Dispose();
                }

                this.oldSource = this.Thing.Source;

                this.sourceSubscription = this.CDPMessageBus.Listen<ObjectChangedEvent>(this.Thing.Source)
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateName());

                this.Disposables.Add(this.sourceSubscription);
            }

            if (this.oldTarget != this.Thing.Target)
            {
                if (this.targetSubscription != null)
                {
                    this.Disposables.Remove(this.targetSubscription);
                    this.targetSubscription.Dispose();
                }

                this.oldTarget = this.Thing.Target;

                this.targetSubscription = this.CDPMessageBus.Listen<ObjectChangedEvent>(this.Thing.Target)
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateName());

                this.Disposables.Add(this.targetSubscription);
            }

            this.UpdateName();
            this.CategoryList = new List<Category>(this.Thing.Category.OrderBy(x => x.Name));
            this.UpdateValues();
        }

        /// <summary>
        /// Updates the relationship name
        /// </summary>
        private void UpdateName()
        {
            this.ShortName = this.Thing.UserFriendlyShortName;
            this.Name = this.Thing.UserFriendlyName;
        }

        /// <summary>
        /// Updates the <see cref="RequirementsContainerParameterValue"/>s contained by this <see cref="RequirementsContainer"/>.
        /// </summary>
        private void UpdateValues()
        {
            var current = this.ContainedRows[0].ContainedRows.Select(x => x.Thing).OfType<RelationshipParameterValue>().ToList();
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
        /// Add a row representing a new <see cref="RelationshipParameterValue"/>
        /// </summary>
        /// <param name="value">The associated <see cref="RelationshipParameterValue"/></param>
        private void AddValue(RelationshipParameterValue value)
        {
            var row = new RelationshipParameterValueRowViewModel(value, this.Session, this);
            this.simpleParameters.ContainedRows.Add(row);
        }

        /// <summary>
        /// Removes a value row
        /// </summary>
        /// <param name="value">The associated <see cref="RequirementsContainerParameterValue"/> to remove</param>
        private void RemoveValue(RelationshipParameterValue value)
        {
            var row = this.simpleParameters.ContainedRows.SingleOrDefault(x => x.Thing == value);

            if (row != null)
            {
                this.simpleParameters.ContainedRows.RemoveAndDispose(row);
            }
        }
    }
}
