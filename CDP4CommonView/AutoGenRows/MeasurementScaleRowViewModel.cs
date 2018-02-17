// -------------------------------------------------------------------------------------------------
// <copyright file="MeasurementScaleRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
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
    /// Row class representing a <see cref="MeasurementScale"/>
    /// </summary>
    public abstract partial class MeasurementScaleRowViewModel<T> : DefinedThingRowViewModel<T> where T : MeasurementScale
    {

        /// <summary>
        /// Backing field for <see cref="NumberSet"/>
        /// </summary>
        private NumberSetKind numberSet;

        /// <summary>
        /// Backing field for <see cref="MinimumPermissibleValue"/>
        /// </summary>
        private string minimumPermissibleValue;

        /// <summary>
        /// Backing field for <see cref="IsMinimumInclusive"/>
        /// </summary>
        private bool isMinimumInclusive;

        /// <summary>
        /// Backing field for <see cref="MaximumPermissibleValue"/>
        /// </summary>
        private string maximumPermissibleValue;

        /// <summary>
        /// Backing field for <see cref="IsMaximumInclusive"/>
        /// </summary>
        private bool isMaximumInclusive;

        /// <summary>
        /// Backing field for <see cref="PositiveValueConnotation"/>
        /// </summary>
        private string positiveValueConnotation;

        /// <summary>
        /// Backing field for <see cref="NegativeValueConnotation"/>
        /// </summary>
        private string negativeValueConnotation;

        /// <summary>
        /// Backing field for <see cref="IsDeprecated"/>
        /// </summary>
        private bool isDeprecated;

        /// <summary>
        /// Backing field for <see cref="Unit"/>
        /// </summary>
        private MeasurementUnit unit;

        /// <summary>
        /// Backing field for <see cref="UnitShortName"/>
        /// </summary>
        private string unitShortName;

        /// <summary>
        /// Backing field for <see cref="UnitName"/>
        /// </summary>
        private string unitName;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementScaleRowViewModel{T}"/> class
        /// </summary>
        /// <param name="measurementScale">The <see cref="MeasurementScale"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        protected MeasurementScaleRowViewModel(T measurementScale, ISession session, IViewModelBase<Thing> containerViewModel) : base(measurementScale, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the NumberSet
        /// </summary>
        public NumberSetKind NumberSet
        {
            get { return this.numberSet; }
            set { this.RaiseAndSetIfChanged(ref this.numberSet, value); }
        }

        /// <summary>
        /// Gets or sets the MinimumPermissibleValue
        /// </summary>
        public string MinimumPermissibleValue
        {
            get { return this.minimumPermissibleValue; }
            set { this.RaiseAndSetIfChanged(ref this.minimumPermissibleValue, value); }
        }

        /// <summary>
        /// Gets or sets the IsMinimumInclusive
        /// </summary>
        public bool IsMinimumInclusive
        {
            get { return this.isMinimumInclusive; }
            set { this.RaiseAndSetIfChanged(ref this.isMinimumInclusive, value); }
        }

        /// <summary>
        /// Gets or sets the MaximumPermissibleValue
        /// </summary>
        public string MaximumPermissibleValue
        {
            get { return this.maximumPermissibleValue; }
            set { this.RaiseAndSetIfChanged(ref this.maximumPermissibleValue, value); }
        }

        /// <summary>
        /// Gets or sets the IsMaximumInclusive
        /// </summary>
        public bool IsMaximumInclusive
        {
            get { return this.isMaximumInclusive; }
            set { this.RaiseAndSetIfChanged(ref this.isMaximumInclusive, value); }
        }

        /// <summary>
        /// Gets or sets the PositiveValueConnotation
        /// </summary>
        public string PositiveValueConnotation
        {
            get { return this.positiveValueConnotation; }
            set { this.RaiseAndSetIfChanged(ref this.positiveValueConnotation, value); }
        }

        /// <summary>
        /// Gets or sets the NegativeValueConnotation
        /// </summary>
        public string NegativeValueConnotation
        {
            get { return this.negativeValueConnotation; }
            set { this.RaiseAndSetIfChanged(ref this.negativeValueConnotation, value); }
        }

        /// <summary>
        /// Gets or sets the IsDeprecated
        /// </summary>
        public bool IsDeprecated
        {
            get { return this.isDeprecated; }
            set { this.RaiseAndSetIfChanged(ref this.isDeprecated, value); }
        }

        /// <summary>
        /// Gets or sets the Unit
        /// </summary>
        public MeasurementUnit Unit
        {
            get { return this.unit; }
            set { this.RaiseAndSetIfChanged(ref this.unit, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="Unit"/>
        /// </summary>
        public string UnitShortName
        {
            get { return this.unitShortName; }
            set { this.RaiseAndSetIfChanged(ref this.unitShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="Unit"/>
        /// </summary>
        public string UnitName
        {
            get { return this.unitName; }
            set { this.RaiseAndSetIfChanged(ref this.unitName, value); }
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
            this.NumberSet = this.Thing.NumberSet;
            this.MinimumPermissibleValue = this.Thing.MinimumPermissibleValue;
            this.IsMinimumInclusive = this.Thing.IsMinimumInclusive;
            this.MaximumPermissibleValue = this.Thing.MaximumPermissibleValue;
            this.IsMaximumInclusive = this.Thing.IsMaximumInclusive;
            this.PositiveValueConnotation = this.Thing.PositiveValueConnotation;
            this.NegativeValueConnotation = this.Thing.NegativeValueConnotation;
            this.IsDeprecated = this.Thing.IsDeprecated;
			if (this.Thing.Unit != null)
			{
				this.UnitShortName = this.Thing.Unit.ShortName;
				this.UnitName = this.Thing.Unit.Name;
			}			
            this.Unit = this.Thing.Unit;
        }
    }
}
