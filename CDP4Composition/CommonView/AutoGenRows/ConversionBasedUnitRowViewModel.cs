// -------------------------------------------------------------------------------------------------
// <copyright file="ConversionBasedUnitRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2018 Starion Group S.A.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// -------------------------------------------------------------------------------------------------

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
    /// Row class representing a <see cref="ConversionBasedUnit"/>
    /// </summary>
    public abstract partial class ConversionBasedUnitRowViewModel<T> : MeasurementUnitRowViewModel<T> where T : ConversionBasedUnit
    {

        /// <summary>
        /// Backing field for <see cref="ConversionFactor"/>
        /// </summary>
        private string conversionFactor;

        /// <summary>
        /// Backing field for <see cref="ReferenceUnit"/>
        /// </summary>
        private MeasurementUnit referenceUnit;

        /// <summary>
        /// Backing field for <see cref="ReferenceUnitShortName"/>
        /// </summary>
        private string referenceUnitShortName;

        /// <summary>
        /// Backing field for <see cref="ReferenceUnitName"/>
        /// </summary>
        private string referenceUnitName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConversionBasedUnitRowViewModel{T}"/> class
        /// </summary>
        /// <param name="conversionBasedUnit">The <see cref="ConversionBasedUnit"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        protected ConversionBasedUnitRowViewModel(T conversionBasedUnit, ISession session, IViewModelBase<Thing> containerViewModel) : base(conversionBasedUnit, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the ConversionFactor
        /// </summary>
        public string ConversionFactor
        {
            get { return this.conversionFactor; }
            set { this.RaiseAndSetIfChanged(ref this.conversionFactor, value); }
        }

        /// <summary>
        /// Gets or sets the ReferenceUnit
        /// </summary>
        public MeasurementUnit ReferenceUnit
        {
            get { return this.referenceUnit; }
            set { this.RaiseAndSetIfChanged(ref this.referenceUnit, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="ReferenceUnit"/>
        /// </summary>
        public string ReferenceUnitShortName
        {
            get { return this.referenceUnitShortName; }
            set { this.RaiseAndSetIfChanged(ref this.referenceUnitShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="ReferenceUnit"/>
        /// </summary>
        public string ReferenceUnitName
        {
            get { return this.referenceUnitName; }
            set { this.RaiseAndSetIfChanged(ref this.referenceUnitName, value); }
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
            this.ModifiedOn = this.Thing.ModifiedOn;
            this.ConversionFactor = this.Thing.ConversionFactor;
			if (this.Thing.ReferenceUnit != null)
			{
				this.ReferenceUnitShortName = this.Thing.ReferenceUnit.ShortName;
				this.ReferenceUnitName = this.Thing.ReferenceUnit.Name;
			}			
            this.ReferenceUnit = this.Thing.ReferenceUnit;
        }
    }
}
