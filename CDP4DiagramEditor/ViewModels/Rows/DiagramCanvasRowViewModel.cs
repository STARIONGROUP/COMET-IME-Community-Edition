// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagramCanvasRowViewModel.cs" company="RHEA System S.A.">
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

namespace CDP4DiagramEditor.ViewModels.Rows
{
    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// Row class representing a <see cref="DiagramCanvas"/>
    /// </summary>
    public class DiagramCanvasRowViewModel : CDP4CommonView.DiagramCanvasRowViewModel
    {
        /// <summary>
        /// Backing field for <see cref="IsLocked"/>
        /// </summary>
        private bool isLocked;

        /// <summary>
        /// Backing field for <see cref="Locker"/>
        /// </summary>
        private string locker;

        /// <summary>
        /// Backing field for <see cref="Description" />
        /// </summary>
        private string description;

        /// <summary>
        /// Backing field for <see cref="IsHidden" />
        /// </summary>
        private bool isHidden;

        /// <summary>
        /// Backing field for <see cref="OwnerShortName"/>
        /// </summary>
        private string ownerShortName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramCanvasRowViewModel" /> class
        /// </summary>
        /// <param name="diagramCanvas">The <see cref="DiagramCanvas" /> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">
        /// The <see cref="IViewModelBase{T}" /> that is the container of this
        /// <see cref="IRowViewModelBase{Thing}" />
        /// </param>
        public DiagramCanvasRowViewModel(DiagramCanvas diagramCanvas, ISession session, IViewModelBase<Thing> containerViewModel) : base(diagramCanvas, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="DiagramCanvas"/> is locked
        /// </summary>
        public bool IsLocked
        {
            get => this.isLocked;
            private set => this.RaiseAndSetIfChanged(ref this.isLocked, value);
        }

        /// <summary>
        /// Gets the name of the person that locked the current <see cref="DiagramCanvas"/>
        /// </summary>
        public string Locker
        {
            get => this.locker;
            private set => this.RaiseAndSetIfChanged(ref this.locker, value);
        }

        /// <summary>
        /// Gets or sets a value that represents the Description
        /// </summary>
        public string Description
        {
            get { return this.description; }
            set { this.RaiseAndSetIfChanged(ref this.description, value); }
        }

        /// <summary>
        /// Gets or sets the IsHidden state
        /// </summary>
        public bool IsHidden
        {
            get { return this.isHidden; }
            set { this.RaiseAndSetIfChanged(ref this.isHidden, value); }
        }

        /// <summary>
        /// Gets or sets the Owner ShortName
        /// </summary>
        public string OwnerShortName
        {
            get { return this.ownerShortName; }
            set { this.RaiseAndSetIfChanged(ref this.ownerShortName, value); }
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
            this.Description = this.Thing.Description;
            this.IsHidden = this.Thing.IsHidden;

            this.IsLocked = this.Thing.LockedBy != null;

            if (this.IsLocked)
            {
                this.Locker = this.Thing.LockedBy?.Name;
            }
            else
            {
                this.locker = string.Empty;
            }
        }

        /// <summary>
        /// Update the <see cref="ThingStatus"/> property
        /// </summary>
        protected override void UpdateThingStatus()
        {
            base.UpdateThingStatus();
            this.ThingStatus = new ThingStatus(this.Thing) { IsLocked = this.Thing.LockedBy != null };
        }
    }
}
