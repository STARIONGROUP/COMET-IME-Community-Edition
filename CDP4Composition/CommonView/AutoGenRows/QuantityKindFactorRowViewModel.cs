// -------------------------------------------------------------------------------------------------
// <copyright file="QuantityKindFactorRowViewModel.cs" company="RHEA System S.A.">
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
    /// Row class representing a <see cref="QuantityKindFactor"/>
    /// </summary>
    public partial class QuantityKindFactorRowViewModel : RowViewModelBase<QuantityKindFactor>
    {

        /// <summary>
        /// Backing field for <see cref="Exponent"/>
        /// </summary>
        private string exponent;

        /// <summary>
        /// Backing field for <see cref="QuantityKind"/>
        /// </summary>
        private QuantityKind quantityKind;

        /// <summary>
        /// Backing field for <see cref="QuantityKindShortName"/>
        /// </summary>
        private string quantityKindShortName;

        /// <summary>
        /// Backing field for <see cref="QuantityKindName"/>
        /// </summary>
        private string quantityKindName;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuantityKindFactorRowViewModel"/> class
        /// </summary>
        /// <param name="quantityKindFactor">The <see cref="QuantityKindFactor"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public QuantityKindFactorRowViewModel(QuantityKindFactor quantityKindFactor, ISession session, IViewModelBase<Thing> containerViewModel) : base(quantityKindFactor, session, containerViewModel)
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
        /// Gets or sets the QuantityKind
        /// </summary>
        public QuantityKind QuantityKind
        {
            get { return this.quantityKind; }
            set { this.RaiseAndSetIfChanged(ref this.quantityKind, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="QuantityKind"/>
        /// </summary>
        public string QuantityKindShortName
        {
            get { return this.quantityKindShortName; }
            set { this.RaiseAndSetIfChanged(ref this.quantityKindShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="QuantityKind"/>
        /// </summary>
        public string QuantityKindName
        {
            get { return this.quantityKindName; }
            set { this.RaiseAndSetIfChanged(ref this.quantityKindName, value); }
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
			if (this.Thing.QuantityKind != null)
			{
				this.QuantityKindShortName = this.Thing.QuantityKind.ShortName;
				this.QuantityKindName = this.Thing.QuantityKind.Name;
			}			
            this.QuantityKind = this.Thing.QuantityKind;
        }
    }
}
