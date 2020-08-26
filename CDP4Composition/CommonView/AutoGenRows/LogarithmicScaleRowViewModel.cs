// -------------------------------------------------------------------------------------------------
// <copyright file="LogarithmicScaleRowViewModel.cs" company="RHEA System S.A.">
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
    /// Row class representing a <see cref="LogarithmicScale"/>
    /// </summary>
    public partial class LogarithmicScaleRowViewModel : MeasurementScaleRowViewModel<LogarithmicScale>
    {

        /// <summary>
        /// Backing field for <see cref="LogarithmBase"/>
        /// </summary>
        private LogarithmBaseKind logarithmBase;

        /// <summary>
        /// Backing field for <see cref="Factor"/>
        /// </summary>
        private string factor;

        /// <summary>
        /// Backing field for <see cref="Exponent"/>
        /// </summary>
        private string exponent;

        /// <summary>
        /// Backing field for <see cref="ReferenceQuantityKind"/>
        /// </summary>
        private QuantityKind referenceQuantityKind;

        /// <summary>
        /// Backing field for <see cref="ReferenceQuantityKindShortName"/>
        /// </summary>
        private string referenceQuantityKindShortName;

        /// <summary>
        /// Backing field for <see cref="ReferenceQuantityKindName"/>
        /// </summary>
        private string referenceQuantityKindName;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogarithmicScaleRowViewModel"/> class
        /// </summary>
        /// <param name="logarithmicScale">The <see cref="LogarithmicScale"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public LogarithmicScaleRowViewModel(LogarithmicScale logarithmicScale, ISession session, IViewModelBase<Thing> containerViewModel) : base(logarithmicScale, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the LogarithmBase
        /// </summary>
        public LogarithmBaseKind LogarithmBase
        {
            get { return this.logarithmBase; }
            set { this.RaiseAndSetIfChanged(ref this.logarithmBase, value); }
        }

        /// <summary>
        /// Gets or sets the Factor
        /// </summary>
        public string Factor
        {
            get { return this.factor; }
            set { this.RaiseAndSetIfChanged(ref this.factor, value); }
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
        /// Gets or sets the ReferenceQuantityKind
        /// </summary>
        public QuantityKind ReferenceQuantityKind
        {
            get { return this.referenceQuantityKind; }
            set { this.RaiseAndSetIfChanged(ref this.referenceQuantityKind, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="ReferenceQuantityKind"/>
        /// </summary>
        public string ReferenceQuantityKindShortName
        {
            get { return this.referenceQuantityKindShortName; }
            set { this.RaiseAndSetIfChanged(ref this.referenceQuantityKindShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="ReferenceQuantityKind"/>
        /// </summary>
        public string ReferenceQuantityKindName
        {
            get { return this.referenceQuantityKindName; }
            set { this.RaiseAndSetIfChanged(ref this.referenceQuantityKindName, value); }
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
            this.LogarithmBase = this.Thing.LogarithmBase;
            this.Factor = this.Thing.Factor;
            this.Exponent = this.Thing.Exponent;
			if (this.Thing.ReferenceQuantityKind != null)
			{
				this.ReferenceQuantityKindShortName = this.Thing.ReferenceQuantityKind.ShortName;
				this.ReferenceQuantityKindName = this.Thing.ReferenceQuantityKind.Name;
			}			
            this.ReferenceQuantityKind = this.Thing.ReferenceQuantityKind;
        }
    }
}
