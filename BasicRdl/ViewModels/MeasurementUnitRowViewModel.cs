// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MeasurementUnitRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;

    /// <summary>
    /// A row view model that represents a <see cref="MeasurementUnit"/>
    /// </summary>
    public class MeasurementUnitRowViewModel : CDP4CommonView.MeasurementUnitRowViewModel<MeasurementUnit>
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
            get
            {
                return this.containerRdl;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.containerRdl, value);
            }
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
            get { return this.isBaseUnit; }

            set { this.RaiseAndSetIfChanged(ref this.isBaseUnit, value); }
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
