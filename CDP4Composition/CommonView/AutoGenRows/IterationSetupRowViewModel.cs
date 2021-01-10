// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IterationSetupRowViewModel.cs" company="RHEA System S.A.">
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
    /// Row class representing a <see cref="IterationSetup"/>
    /// </summary>
    public partial class IterationSetupRowViewModel : RowViewModelBase<IterationSetup>
    {
        /// <summary>
        /// Backing field for <see cref="CreatedOn"/> property
        /// </summary>
        private DateTime createdOn;

        /// <summary>
        /// Backing field for <see cref="Description"/> property
        /// </summary>
        private string description;

        /// <summary>
        /// Backing field for <see cref="FrozenOn"/> property
        /// </summary>
        private DateTime frozenOn;

        /// <summary>
        /// Backing field for <see cref="IsDeleted"/> property
        /// </summary>
        private bool isDeleted;

        /// <summary>
        /// Backing field for <see cref="IterationIid"/> property
        /// </summary>
        private Guid iterationIid;

        /// <summary>
        /// Backing field for <see cref="IterationNumber"/> property
        /// </summary>
        private int iterationNumber;

        /// <summary>
        /// Backing field for <see cref="SourceIterationSetup"/> property
        /// </summary>
        private IterationSetup sourceIterationSetup;

        /// <summary>
        /// Initializes a new instance of the <see cref="IterationSetupRowViewModel"/> class
        /// </summary>
        /// <param name="iterationSetup">The <see cref="IterationSetup"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public IterationSetupRowViewModel(IterationSetup iterationSetup, ISession session, IViewModelBase<Thing> containerViewModel) : base(iterationSetup, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets the CreatedOn
        /// </summary>
        public DateTime CreatedOn
        {
            get { return this.createdOn; }
            set { this.RaiseAndSetIfChanged(ref this.createdOn, value); }
        }

        /// <summary>
        /// Gets or sets the Description
        /// </summary>
        public string Description
        {
            get { return this.description; }
            set { this.RaiseAndSetIfChanged(ref this.description, value); }
        }

        /// <summary>
        /// Gets or sets the FrozenOn
        /// </summary>
        public DateTime FrozenOn
        {
            get { return this.frozenOn; }
            set { this.RaiseAndSetIfChanged(ref this.frozenOn, value); }
        }

        /// <summary>
        /// Gets or sets the IsDeleted
        /// </summary>
        public bool IsDeleted
        {
            get { return this.isDeleted; }
            set { this.RaiseAndSetIfChanged(ref this.isDeleted, value); }
        }

        /// <summary>
        /// Gets or sets the IterationIid
        /// </summary>
        public Guid IterationIid
        {
            get { return this.iterationIid; }
            set { this.RaiseAndSetIfChanged(ref this.iterationIid, value); }
        }

        /// <summary>
        /// Gets or sets the IterationNumber
        /// </summary>
        public int IterationNumber
        {
            get { return this.iterationNumber; }
            set { this.RaiseAndSetIfChanged(ref this.iterationNumber, value); }
        }

        /// <summary>
        /// Gets or sets the SourceIterationSetup
        /// </summary>
        public IterationSetup SourceIterationSetup
        {
            get { return this.sourceIterationSetup; }
            set { this.RaiseAndSetIfChanged(ref this.sourceIterationSetup, value); }
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
            this.CreatedOn = this.Thing.CreatedOn;
            this.Description = this.Thing.Description;
            if (this.Thing.FrozenOn.HasValue)
            {
                this.FrozenOn = this.Thing.FrozenOn.Value;
            }
            this.IsDeleted = this.Thing.IsDeleted;
            this.IterationIid = this.Thing.IterationIid;
            this.IterationNumber = this.Thing.IterationNumber;
            this.SourceIterationSetup = this.Thing.SourceIterationSetup;
        }
    }
}
