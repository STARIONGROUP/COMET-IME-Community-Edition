﻿// -------------------------------------------------------------------------------------------------
// <copyright file="ScaleReferenceQuantityValueRowViewModel.cs" company="Starion Group S.A.">
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
    /// Row class representing a <see cref="ScaleReferenceQuantityValue"/>
    /// </summary>
    public partial class ScaleReferenceQuantityValueRowViewModel : RowViewModelBase<ScaleReferenceQuantityValue>
    {

        /// <summary>
        /// Backing field for <see cref="Value"/>
        /// </summary>
        private string value;

        /// <summary>
        /// Backing field for <see cref="Scale"/>
        /// </summary>
        private MeasurementScale scale;

        /// <summary>
        /// Backing field for <see cref="ScaleShortName"/>
        /// </summary>
        private string scaleShortName;

        /// <summary>
        /// Backing field for <see cref="ScaleName"/>
        /// </summary>
        private string scaleName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScaleReferenceQuantityValueRowViewModel"/> class
        /// </summary>
        /// <param name="scaleReferenceQuantityValue">The <see cref="ScaleReferenceQuantityValue"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public ScaleReferenceQuantityValueRowViewModel(ScaleReferenceQuantityValue scaleReferenceQuantityValue, ISession session, IViewModelBase<Thing> containerViewModel) : base(scaleReferenceQuantityValue, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the Value
        /// </summary>
        public string Value
        {
            get { return this.value; }
            set { this.RaiseAndSetIfChanged(ref this.value, value); }
        }

        /// <summary>
        /// Gets or sets the Scale
        /// </summary>
        public MeasurementScale Scale
        {
            get { return this.scale; }
            set { this.RaiseAndSetIfChanged(ref this.scale, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="Scale"/>
        /// </summary>
        public string ScaleShortName
        {
            get { return this.scaleShortName; }
            set { this.RaiseAndSetIfChanged(ref this.scaleShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="Scale"/>
        /// </summary>
        public string ScaleName
        {
            get { return this.scaleName; }
            set { this.RaiseAndSetIfChanged(ref this.scaleName, value); }
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
            this.Value = this.Thing.Value;
			if (this.Thing.Scale != null)
			{
				this.ScaleShortName = this.Thing.Scale.ShortName;
				this.ScaleName = this.Thing.Scale.Name;
			}			
            this.Scale = this.Thing.Scale;
        }
    }
}
