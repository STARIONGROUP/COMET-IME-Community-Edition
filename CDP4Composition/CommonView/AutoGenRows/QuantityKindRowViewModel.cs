// -------------------------------------------------------------------------------------------------
// <copyright file="QuantityKindRowViewModel.cs" company="Starion Group S.A.">
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
    /// Row class representing a <see cref="QuantityKind"/>
    /// </summary>
    public abstract partial class QuantityKindRowViewModel<T> : ScalarParameterTypeRowViewModel<T> where T : QuantityKind
    {

        /// <summary>
        /// Backing field for <see cref="QuantityDimensionSymbol"/>
        /// </summary>
        private string quantityDimensionSymbol;

        /// <summary>
        /// Backing field for <see cref="QuantityDimensionExpression"/>
        /// </summary>
        private string quantityDimensionExpression;

        /// <summary>
        /// Backing field for <see cref="DefaultScale"/>
        /// </summary>
        private MeasurementScale defaultScale;

        /// <summary>
        /// Backing field for <see cref="DefaultScaleShortName"/>
        /// </summary>
        private string defaultScaleShortName;

        /// <summary>
        /// Backing field for <see cref="DefaultScaleName"/>
        /// </summary>
        private string defaultScaleName;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuantityKindRowViewModel{T}"/> class
        /// </summary>
        /// <param name="quantityKind">The <see cref="QuantityKind"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        protected QuantityKindRowViewModel(T quantityKind, ISession session, IViewModelBase<Thing> containerViewModel) : base(quantityKind, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the QuantityDimensionSymbol
        /// </summary>
        public string QuantityDimensionSymbol
        {
            get { return this.quantityDimensionSymbol; }
            set { this.RaiseAndSetIfChanged(ref this.quantityDimensionSymbol, value); }
        }

        /// <summary>
        /// Gets or sets the QuantityDimensionExpression
        /// </summary>
        public string QuantityDimensionExpression
        {
            get { return this.quantityDimensionExpression; }
            set { this.RaiseAndSetIfChanged(ref this.quantityDimensionExpression, value); }
        }

        /// <summary>
        /// Gets or sets the DefaultScale
        /// </summary>
        public MeasurementScale DefaultScale
        {
            get { return this.defaultScale; }
            set { this.RaiseAndSetIfChanged(ref this.defaultScale, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="DefaultScale"/>
        /// </summary>
        public string DefaultScaleShortName
        {
            get { return this.defaultScaleShortName; }
            set { this.RaiseAndSetIfChanged(ref this.defaultScaleShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="DefaultScale"/>
        /// </summary>
        public string DefaultScaleName
        {
            get { return this.defaultScaleName; }
            set { this.RaiseAndSetIfChanged(ref this.defaultScaleName, value); }
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
            this.QuantityDimensionSymbol = this.Thing.QuantityDimensionSymbol;
            this.QuantityDimensionExpression = this.Thing.QuantityDimensionExpression;
			if (this.Thing.DefaultScale != null)
			{
				this.DefaultScaleShortName = this.Thing.DefaultScale.ShortName;
				this.DefaultScaleName = this.Thing.DefaultScale.Name;
			}			
            this.DefaultScale = this.Thing.DefaultScale;
        }
    }
}
