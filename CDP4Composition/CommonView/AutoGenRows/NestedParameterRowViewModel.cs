// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NestedParameterRowViewModel.cs" company="RHEA System S.A.">
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
    /// Row class representing a <see cref="NestedParameter"/>
    /// </summary>
    public partial class NestedParameterRowViewModel : RowViewModelBase<NestedParameter>
    {
        /// <summary>
        /// Backing field for <see cref="ActualState"/> property
        /// </summary>
        private ActualFiniteState actualState;

        /// <summary>
        /// Backing field for <see cref="ActualStateName"/> property
        /// </summary>
        private string actualStateName;

        /// <summary>
        /// Backing field for <see cref="ActualStateShortName"/> property
        /// </summary>
        private string actualStateShortName;

        /// <summary>
        /// Backing field for <see cref="ActualValue"/> property
        /// </summary>
        private string actualValue;

        /// <summary>
        /// Backing field for <see cref="AssociatedParameter"/> property
        /// </summary>
        private ParameterBase associatedParameter;

        /// <summary>
        /// Backing field for <see cref="Formula"/> property
        /// </summary>
        private string formula;

        /// <summary>
        /// Backing field for <see cref="IsVolatile"/> property
        /// </summary>
        private bool isVolatile;

        /// <summary>
        /// Backing field for <see cref="Owner"/> property
        /// </summary>
        private DomainOfExpertise owner;

        /// <summary>
        /// Backing field for <see cref="OwnerName"/> property
        /// </summary>
        private string ownerName;

        /// <summary>
        /// Backing field for <see cref="OwnerShortName"/> property
        /// </summary>
        private string ownerShortName;

        /// <summary>
        /// Backing field for <see cref="Path"/> property
        /// </summary>
        private string path;

        /// <summary>
        /// Initializes a new instance of the <see cref="NestedParameterRowViewModel"/> class
        /// </summary>
        /// <param name="nestedParameter">The <see cref="NestedParameter"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public NestedParameterRowViewModel(NestedParameter nestedParameter, ISession session, IViewModelBase<Thing> containerViewModel) : base(nestedParameter, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets the ActualState
        /// </summary>
        public ActualFiniteState ActualState
        {
            get { return this.actualState; }
            set { this.RaiseAndSetIfChanged(ref this.actualState, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="ActualState"/>
        /// </summary>
        public string ActualStateName
        {
            get { return this.actualStateName; }
            set { this.RaiseAndSetIfChanged(ref this.actualStateName, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="ActualState"/>
        /// </summary>
        public string ActualStateShortName
        {
            get { return this.actualStateShortName; }
            set { this.RaiseAndSetIfChanged(ref this.actualStateShortName, value); }
        }

        /// <summary>
        /// Gets or sets the ActualValue
        /// </summary>
        public string ActualValue
        {
            get { return this.actualValue; }
            set { this.RaiseAndSetIfChanged(ref this.actualValue, value); }
        }

        /// <summary>
        /// Gets or sets the AssociatedParameter
        /// </summary>
        public ParameterBase AssociatedParameter
        {
            get { return this.associatedParameter; }
            set { this.RaiseAndSetIfChanged(ref this.associatedParameter, value); }
        }

        /// <summary>
        /// Gets or sets the Formula
        /// </summary>
        public string Formula
        {
            get { return this.formula; }
            set { this.RaiseAndSetIfChanged(ref this.formula, value); }
        }

        /// <summary>
        /// Gets or sets the IsVolatile
        /// </summary>
        public bool IsVolatile
        {
            get { return this.isVolatile; }
            set { this.RaiseAndSetIfChanged(ref this.isVolatile, value); }
        }

        /// <summary>
        /// Gets or sets the Owner
        /// </summary>
        public DomainOfExpertise Owner
        {
            get { return this.owner; }
            set { this.RaiseAndSetIfChanged(ref this.owner, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="Owner"/>
        /// </summary>
        public string OwnerName
        {
            get { return this.ownerName; }
            set { this.RaiseAndSetIfChanged(ref this.ownerName, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="Owner"/>
        /// </summary>
        public string OwnerShortName
        {
            get { return this.ownerShortName; }
            set { this.RaiseAndSetIfChanged(ref this.ownerShortName, value); }
        }

        /// <summary>
        /// Gets or sets the Path
        /// </summary>
        public string Path
        {
            get { return this.path; }
            set { this.RaiseAndSetIfChanged(ref this.path, value); }
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
            this.ActualState = this.Thing.ActualState;
            if (this.Thing.ActualState != null)
            {
                this.ActualStateName = this.Thing.ActualState.Name;
                this.ActualStateShortName = this.Thing.ActualState.ShortName;
            }
            else
            {
                this.ActualStateName = string.Empty;
                this.ActualStateShortName = string.Empty;
            }
            this.ActualValue = this.Thing.ActualValue;
            this.AssociatedParameter = this.Thing.AssociatedParameter;
            this.Formula = this.Thing.Formula;
            this.IsVolatile = this.Thing.IsVolatile;
            this.Owner = this.Thing.Owner;
            if (this.Thing.Owner != null)
            {
                this.OwnerName = this.Thing.Owner.Name;
                this.OwnerShortName = this.Thing.Owner.ShortName;
            }
            else
            {
                this.OwnerName = string.Empty;
                this.OwnerShortName = string.Empty;
            }
            this.Path = this.Thing.Path;
        }
    }
}
