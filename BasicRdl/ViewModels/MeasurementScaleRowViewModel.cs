// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MeasurementScaleRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
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
    using System;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    
    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;
    
    using ReactiveUI;

    /// <summary>
    /// A row view model that represents a <see cref="MeasurementScale"/>
    /// </summary>
    public class MeasurementScaleRowViewModel : CDP4CommonView.MeasurementScaleRowViewModel<MeasurementScale>
    {
        /// <summary>
        /// The subscription for the referenced <see cref="MeasurementUnit"/>
        /// </summary>
        private IDisposable measurementUnitSubscription;

        /// <summary>
        /// The <see cref="MeasurementUnit"/> for the current <see cref="MeasurementScale"/>
        /// </summary>
        private MeasurementUnit measurementUnitObject;

        /// <summary>
        /// Backing field for the <see cref="MeasurementUnit"/> property
        /// </summary>
        private string measurementUnit;

        /// <summary>
        /// Backing field for the <see cref="NumberSet"/> property
        /// </summary>
        private string numberSet;

        /// <summary>
        /// Backing field for the <see cref="ContainerRdl"/> property
        /// </summary>
        private string containerRdl;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementScaleRowViewModel"/> class.
        /// </summary>
        /// <param name="measurementScale">
        /// The <see cref="MeasurementScale"/> that is represented by the current view-model
        /// </param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public MeasurementScaleRowViewModel(MeasurementScale measurementScale, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(measurementScale, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets the ShortName of the referenced <see cref="MeasurementUnit"/>
        /// </summary>
        public string MeasurementUnit
        {
            get => this.measurementUnit;
            set => this.RaiseAndSetIfChanged(ref this.measurementUnit, value);
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
        /// Gets or sets the mathematical number set.
        /// </summary>
        public new string NumberSet
        {
            get => this.numberSet;
            set => this.RaiseAndSetIfChanged(ref this.numberSet, value);
        }

        /// <summary>
        /// Gets the <see cref="ClassKind"/> of the <see cref="Thing"/> that is represented by the current view-model
        /// </summary>
        public string ClassKind { get; private set; }

        /// <summary>
        /// Updates the properties of the current view-model
        /// </summary>
        private void UpdateProperties()
        {
            this.CreateMeasurementUnitUpdateSubscription();
            this.NumberSet = this.Thing.NumberSet.ToString();
            this.ClassKind = this.Thing.ClassKind.ToString();
            var container = this.Thing.Container as ReferenceDataLibrary;
            this.ContainerRdl = container == null ? string.Empty : container.ShortName;
        }

        /// <summary>
        /// Create the default scale update subscription
        /// </summary>
        private void CreateMeasurementUnitUpdateSubscription()
        {
            if (this.measurementUnitObject != null && this.Thing.Unit == this.measurementUnitObject)
            {
                return;
            }

            if (this.measurementUnitSubscription != null)
            {
                this.measurementUnitSubscription.Dispose();
            }

            if (this.Thing.Unit == null)
            {
                return;
            }

            this.measurementUnitObject = this.Thing.Unit;
            this.measurementUnitSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.measurementUnitObject)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => { this.MeasurementUnit = this.measurementUnitObject.ShortName; });

            this.MeasurementUnit = this.measurementUnitObject.ShortName;
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
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// a value indicating whether the class is being disposed of
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (this.measurementUnitSubscription != null)
            {
                this.measurementUnitSubscription.Dispose();
            }
        }
    }
}
