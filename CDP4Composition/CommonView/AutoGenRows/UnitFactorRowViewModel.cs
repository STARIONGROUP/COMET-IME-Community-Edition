// -------------------------------------------------------------------------------------------------
// <copyright file="UnitFactorRowViewModel.cs" company="RHEA System S.A.">
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
    /// Row class representing a <see cref="UnitFactor"/>
    /// </summary>
    public partial class UnitFactorRowViewModel : RowViewModelBase<UnitFactor>
    {

        /// <summary>
        /// Backing field for <see cref="Exponent"/>
        /// </summary>
        private string exponent;

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
        /// Initializes a new instance of the <see cref="UnitFactorRowViewModel"/> class
        /// </summary>
        /// <param name="unitFactor">The <see cref="UnitFactor"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public UnitFactorRowViewModel(UnitFactor unitFactor, ISession session, IViewModelBase<Thing> containerViewModel) : base(unitFactor, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the Exponent
        /// </summary>
        public string Exponent
        {
            get { return this.exponent; }
            set { this.RaiseAndSetIfChanged(ref this.exponent, value); }
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
            this.Exponent = this.Thing.Exponent;
            if (this.Thing.Unit != null)
            {
                this.UnitShortName = this.Thing.Unit.ShortName;
                this.UnitName = this.Thing.Unit.Name;
            }
            this.Unit = this.Thing.Unit;
        }
    }
}
