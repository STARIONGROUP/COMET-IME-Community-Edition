// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterValueRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA System S.A.
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
    /// Row class representing a <see cref="ParameterValue"/>
    /// </summary>
    public abstract partial class ParameterValueRowViewModel<T> : RowViewModelBase<T> where T : ParameterValue
    {

        /// <summary>
        /// Backing field for <see cref="ParameterType"/>
        /// </summary>
        private ParameterType parameterType;

        /// <summary>
        /// Backing field for <see cref="ParameterTypeShortName"/>
        /// </summary>
        private string parameterTypeShortName;

        /// <summary>
        /// Backing field for <see cref="ParameterTypeName"/>
        /// </summary>
        private string parameterTypeName;

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
        /// Initializes a new instance of the <see cref="ParameterValueRowViewModel{T}"/> class
        /// </summary>
        /// <param name="parameterValue">The <see cref="ParameterValue"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        protected ParameterValueRowViewModel(T parameterValue, ISession session, IViewModelBase<Thing> containerViewModel) : base(parameterValue, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the ParameterType
        /// </summary>
        public ParameterType ParameterType
        {
            get { return this.parameterType; }
            set { this.RaiseAndSetIfChanged(ref this.parameterType, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="ParameterType"/>
        /// </summary>
        public string ParameterTypeShortName
        {
            get { return this.parameterTypeShortName; }
            set { this.RaiseAndSetIfChanged(ref this.parameterTypeShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="ParameterType"/>
        /// </summary>
        public string ParameterTypeName
        {
            get { return this.parameterTypeName; }
            set { this.RaiseAndSetIfChanged(ref this.parameterTypeName, value); }
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
            this.ParameterType = this.Thing.ParameterType;
            this.Scale = this.Thing.Scale;
        }
    }
}
