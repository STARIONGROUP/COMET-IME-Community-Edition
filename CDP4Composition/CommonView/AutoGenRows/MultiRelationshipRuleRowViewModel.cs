// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultiRelationshipRuleRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
//
//    This file is part of CDP4-IME Community Edition.
//    This is an auto-generated class. Any manual changes to this file will be overwritten!
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using System;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using ReactiveUI;

    /// <summary>
    /// Row class representing a <see cref="MultiRelationshipRule"/>
    /// </summary>
    public partial class MultiRelationshipRuleRowViewModel : RuleRowViewModel<MultiRelationshipRule>
    {
        /// <summary>
        /// Backing field for <see cref="MaxRelated"/> property
        /// </summary>
        private int maxRelated;

        /// <summary>
        /// Backing field for <see cref="MinRelated"/> property
        /// </summary>
        private int minRelated;

        /// <summary>
        /// Backing field for <see cref="RelationshipCategory"/> property
        /// </summary>
        private Category relationshipCategory;

        /// <summary>
        /// Backing field for <see cref="RelationshipCategoryName"/> property
        /// </summary>
        private string relationshipCategoryName;

        /// <summary>
        /// Backing field for <see cref="RelationshipCategoryShortName"/> property
        /// </summary>
        private string relationshipCategoryShortName;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiRelationshipRuleRowViewModel"/> class
        /// </summary>
        /// <param name="multiRelationshipRule">The <see cref="MultiRelationshipRule"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public MultiRelationshipRuleRowViewModel(MultiRelationshipRule multiRelationshipRule, ISession session, IViewModelBase<Thing> containerViewModel) : base(multiRelationshipRule, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets the MaxRelated
        /// </summary>
        public int MaxRelated
        {
            get { return this.maxRelated; }
            set { this.RaiseAndSetIfChanged(ref this.maxRelated, value); }
        }

        /// <summary>
        /// Gets or sets the MinRelated
        /// </summary>
        public int MinRelated
        {
            get { return this.minRelated; }
            set { this.RaiseAndSetIfChanged(ref this.minRelated, value); }
        }

        /// <summary>
        /// Gets or sets the RelationshipCategory
        /// </summary>
        public Category RelationshipCategory
        {
            get { return this.relationshipCategory; }
            set { this.RaiseAndSetIfChanged(ref this.relationshipCategory, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="RelationshipCategory"/>
        /// </summary>
        public string RelationshipCategoryName
        {
            get { return this.relationshipCategoryName; }
            set { this.RaiseAndSetIfChanged(ref this.relationshipCategoryName, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="RelationshipCategory"/>
        /// </summary>
        public string RelationshipCategoryShortName
        {
            get { return this.relationshipCategoryShortName; }
            set { this.RaiseAndSetIfChanged(ref this.relationshipCategoryShortName, value); }
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Thing"/> that is being represented by the view-model
        /// </summary>
        /// <param name="objectChange">
        /// The payload of the event that is being handled
        /// </param>
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
            this.MaxRelated = this.Thing.MaxRelated;
            this.MinRelated = this.Thing.MinRelated;
            this.RelationshipCategory = this.Thing.RelationshipCategory;
            if (this.Thing.RelationshipCategory != null)
            {
                this.RelationshipCategoryName = this.Thing.RelationshipCategory.Name;
                this.RelationshipCategoryShortName = this.Thing.RelationshipCategory.ShortName;
            }
            else
            {
                this.RelationshipCategoryName = string.Empty;
                this.RelationshipCategoryShortName = string.Empty;
            }
        }
    }
}
