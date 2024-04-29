// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MeasurementUnitRowViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
// 
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CommonView;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// A row view model that represents a <see cref="MeasurementUnit"/>
    /// </summary>
    public class MeasurementUnitRowViewModel : MeasurementUnitRowViewModel<MeasurementUnit>
    {
        /// <summary>
        /// Backing field for the <see cref="ContainerRdl"/> property
        /// </summary>
        private string containerRdl;

        /// <summary>
        /// Backing field for the <see cref="IsBaseUnit"/> property
        /// </summary>
        private bool isBaseUnit;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementUnitRowViewModel"/> class. 
        /// </summary>
        /// <param name="measurementUnit">
        /// The <see cref="MeasurementUnit"/> that is represented by the current view-model
        /// </param>
        /// <param name="session">
        /// The session
        /// </param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public MeasurementUnitRowViewModel(MeasurementUnit measurementUnit, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(measurementUnit, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets the Container RDL ShortName.
        /// </summary>
        public string ContainerRdl
        {
            get => this.containerRdl;
            set => this.RaiseAndSetIfChanged(ref this.containerRdl, value);
        }

        /// <summary>
        /// Gets the <see cref="ClassKind"/> of the <see cref="Thing"/> that is represented by the current view-model
        /// </summary>
        public string ClassKind { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="MeasurementUnit"/> is a base unit.
        /// </summary>
        public bool IsBaseUnit
        {
            get => this.isBaseUnit;
            set => this.RaiseAndSetIfChanged(ref this.isBaseUnit, value);
        }

        /// <summary>
        /// Updates the columns values
        /// </summary>
        private void UpdateProperties()
        {
            var container = this.Thing.Container as ReferenceDataLibrary;
            this.IsBaseUnit = container != null && container.BaseUnit.Contains(this.Thing);
            this.ContainerRdl = container == null ? string.Empty : container.ShortName;
            this.ClassKind = this.Thing.ClassKind.ToString();
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
    }
}
