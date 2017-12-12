// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterRowViewModel.cs" company="RHEA S.A.">
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
    /// Row class representing a <see cref="Parameter"/>
    /// </summary>
    public partial class ParameterRowViewModel : ParameterOrOverrideBaseRowViewModel<Parameter>
    {

        /// <summary>
        /// Backing field for <see cref="AllowDifferentOwnerOfOverride"/>
        /// </summary>
        private bool allowDifferentOwnerOfOverride;

        /// <summary>
        /// Backing field for <see cref="ExpectsOverride"/>
        /// </summary>
        private bool expectsOverride;

        /// <summary>
        /// Backing field for <see cref="RequestedBy"/>
        /// </summary>
        private DomainOfExpertise requestedBy;

        /// <summary>
        /// Backing field for <see cref="RequestedByShortName"/>
        /// </summary>
        private string requestedByShortName;

        /// <summary>
        /// Backing field for <see cref="RequestedByName"/>
        /// </summary>
        private string requestedByName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterRowViewModel"/> class
        /// </summary>
        /// <param name="parameter">The <see cref="Parameter"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public ParameterRowViewModel(Parameter parameter, ISession session, IViewModelBase<Thing> containerViewModel) : base(parameter, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the AllowDifferentOwnerOfOverride
        /// </summary>
        public bool AllowDifferentOwnerOfOverride
        {
            get { return this.allowDifferentOwnerOfOverride; }
            set { this.RaiseAndSetIfChanged(ref this.allowDifferentOwnerOfOverride, value); }
        }

        /// <summary>
        /// Gets or sets the ExpectsOverride
        /// </summary>
        public bool ExpectsOverride
        {
            get { return this.expectsOverride; }
            set { this.RaiseAndSetIfChanged(ref this.expectsOverride, value); }
        }

        /// <summary>
        /// Gets or sets the RequestedBy
        /// </summary>
        public DomainOfExpertise RequestedBy
        {
            get { return this.requestedBy; }
            set { this.RaiseAndSetIfChanged(ref this.requestedBy, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="RequestedBy"/>
        /// </summary>
        public string RequestedByShortName
        {
            get { return this.requestedByShortName; }
            set { this.RaiseAndSetIfChanged(ref this.requestedByShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="RequestedBy"/>
        /// </summary>
        public string RequestedByName
        {
            get { return this.requestedByName; }
            set { this.RaiseAndSetIfChanged(ref this.requestedByName, value); }
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
            this.AllowDifferentOwnerOfOverride = this.Thing.AllowDifferentOwnerOfOverride;
            this.ExpectsOverride = this.Thing.ExpectsOverride;
            this.RequestedBy = this.Thing.RequestedBy;
        }
    }
}
