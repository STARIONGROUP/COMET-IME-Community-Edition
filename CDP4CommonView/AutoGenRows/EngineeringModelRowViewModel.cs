// -------------------------------------------------------------------------------------------------
// <copyright file="EngineeringModelRowViewModel.cs" company="RHEA System S.A.">
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
    /// Row class representing a <see cref="EngineeringModel"/>
    /// </summary>
    public partial class EngineeringModelRowViewModel : TopContainerRowViewModel<EngineeringModel>
    {

        /// <summary>
        /// Backing field for <see cref="EngineeringModelSetup"/>
        /// </summary>
        private EngineeringModelSetup engineeringModelSetup;

        /// <summary>
        /// Backing field for <see cref="EngineeringModelSetupShortName"/>
        /// </summary>
        private string engineeringModelSetupShortName;

        /// <summary>
        /// Backing field for <see cref="EngineeringModelSetupName"/>
        /// </summary>
        private string engineeringModelSetupName;

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineeringModelRowViewModel"/> class
        /// </summary>
        /// <param name="engineeringModel">The <see cref="EngineeringModel"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public EngineeringModelRowViewModel(EngineeringModel engineeringModel, ISession session, IViewModelBase<Thing> containerViewModel) : base(engineeringModel, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the EngineeringModelSetup
        /// </summary>
        public EngineeringModelSetup EngineeringModelSetup
        {
            get { return this.engineeringModelSetup; }
            set { this.RaiseAndSetIfChanged(ref this.engineeringModelSetup, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="EngineeringModelSetup"/>
        /// </summary>
        public string EngineeringModelSetupShortName
        {
            get { return this.engineeringModelSetupShortName; }
            set { this.RaiseAndSetIfChanged(ref this.engineeringModelSetupShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="EngineeringModelSetup"/>
        /// </summary>
        public string EngineeringModelSetupName
        {
            get { return this.engineeringModelSetupName; }
            set { this.RaiseAndSetIfChanged(ref this.engineeringModelSetupName, value); }
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
			if (this.Thing.EngineeringModelSetup != null)
			{
				this.EngineeringModelSetupShortName = this.Thing.EngineeringModelSetup.ShortName;
				this.EngineeringModelSetupName = this.Thing.EngineeringModelSetup.Name;
			}			
            this.EngineeringModelSetup = this.Thing.EngineeringModelSetup;
        }
    }
}
